using simple_rank_backend.Application.Common;
using simple_rank_backend.Application.Services.Interfaces;
using simple_rank_backend.DTOs;
using simple_rank_backend.DTOs.Statistic;
using Supabase;
using Supabase.Interfaces;
using System.Security.Claims;

namespace simple_rank_backend.Application.Services
{
    public class StatisticService : IStatisticService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserCacheService _userCache;
        private readonly Client _supabase;
        private readonly Serilog.ILogger _log;

        public StatisticService(IHttpContextAccessor httpAccessor, IUserCacheService userCache, Client client, Serilog.ILogger log)
        {
            _httpContextAccessor = httpAccessor;
            _userCache = userCache;
            _supabase = client;
            _log = log;
        }

        public async Task<Result<RankStatisticResponse>> GetRankStatistics(string rankingId)
        {
            var result = await _supabase.From<TableModels.Stats>()
                .Where(s => s.RankingId == rankingId)
                .Get();

            if (result.ResponseMessage.IsSuccessStatusCode)
            {
                if (result.Models.Count == 0) 
                { 
                    return Result<RankStatisticResponse>.Success(new RankStatisticResponse());
                }
                Dictionary<Statistics, int> statCollection = new Dictionary<Statistics, int>();
                var enumValues = Enum.GetValues(typeof(Statistics)).Cast<Statistics>();
                foreach( var enumValue in enumValues )
                {
                    statCollection.Add(enumValue, 0);
                }

                foreach(var s in result.Models)
                {
                    Statistics currentValue = Enum.Parse<Statistics>(s.Name, true);
                    if (statCollection.ContainsKey(currentValue))
                    {
                        statCollection[currentValue]++;
                    }
                }
                
                return Result<RankStatisticResponse>.Success(new RankStatisticResponse(statCollection));
                
            }

            Error supabaseError = await Error.SupabaseError(result.ResponseMessage, "Error getting stat");
            return Result<RankStatisticResponse>.Failure(supabaseError);

        }

        public async Task<Result<RankStatisticResponse>> GetSharedRankStatistics(string rankingId)
        {
            var shareLinkResult = await _supabase.From<TableModels.ShareableLink>()
                .Where(l => l.SharedLinkId == rankingId)
                .Get();

            if (shareLinkResult.ResponseMessage.IsSuccessStatusCode && shareLinkResult.Models.Any())
            {
                var result = await GetRankStatistics(shareLinkResult.Model.RankingId);
                return result;
            }
            else
            {
                Error supabaseError = await Error.SupabaseError(shareLinkResult.ResponseMessage, "Error getting stat", _log);
                return Result<RankStatisticResponse>.Failure(supabaseError);
            }
        }

        public async Task<Result<ResponseBase>> RecordReaction(ReactDto rq, string rankingId)
        {
            Statistics reaction =  Enum.Parse<Statistics>(rq.ReactionId.ToUpper());
            string callerId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            string callerIp = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress.ToString();

            var checkReactionRecord = await _supabase.From<TableModels.ReactionHistory>()
                .Where(h => h.IpAddress == callerIp)
                .Where(h => h.RankingId == rankingId || h.SharedId == rankingId)
                .Get();

            if(checkReactionRecord.ResponseMessage.IsSuccessStatusCode && checkReactionRecord.Models.Count == 0)
            {
                if(rq.IsShared)
                {
                    var getRankingId = await _supabase.From<TableModels.ShareableLink>().Where(s => s.SharedLinkId == rankingId).Get();
                    if(getRankingId.ResponseMessage.IsSuccessStatusCode && getRankingId.Models.Any())
                    {
                        rankingId = getRankingId.Model.RankingId;
                    }
                    else
                    {
                        Error supabaseError = await Error.SupabaseError(getRankingId.ResponseMessage, "Error getting link", _log);
                        return Result<ResponseBase>.Failure(supabaseError);
                    }

                }
                var stat = new TableModels.Stats(reaction, callerId, rq.RankingId);
                var result = await _supabase.From<TableModels.Stats>().Insert(stat);

                if (!result.ResponseMessage.IsSuccessStatusCode)
                {
                    Error supabaseError = await Error.SupabaseError(result.ResponseMessage, "Error recording stat", _log);
                    return Result<ResponseBase>.Failure(supabaseError);
                }

                bool isRealUserId = false;

                if (String.IsNullOrEmpty(callerId))
                {
                    callerId = Guid.NewGuid().ToString();
                }
                else
                {
                    isRealUserId = true;
                }

                TableModels.ReactionHistory record = 
                    rq.IsShared ? 
                        TableModels.ReactionHistory.RecordWithsharedId(sharedId: rankingId, ipAddress: callerIp, callerId, isReal: isRealUserId) 
                        :
                        TableModels.ReactionHistory.RecordWithRankingId(rankingId: rankingId, ipAddress: callerIp, callerId, isReal: isRealUserId);

                var updateResult = await _supabase.From<TableModels.ReactionHistory>().Insert(record);
              

                if (!updateResult.ResponseMessage.IsSuccessStatusCode)
                {
                    Error supabaseError = await Error.SupabaseError(result.ResponseMessage, "Error updating records", _log);
                    return Result<ResponseBase>.Failure(supabaseError);
                }

                ResponseBase response = new ResponseBase("Successfully recorded view");
                return Result<ResponseBase>.Success(response);
            }
            else
            {
                Error supabaseError = await Error.SupabaseError(checkReactionRecord.ResponseMessage, "Error checking records", _log);
                return Result<ResponseBase>.Failure(supabaseError);
            }
        }

        public async Task<Result<ResponseBase>> RecordView(string rankingId)
        {
            var result = await _supabase.Rpc("increment_statistics_column", new { p_id = rankingId, p_column_name = "views" });

            if (!result.ResponseMessage.IsSuccessStatusCode)
            {
                Error supabaseError = await Error.SupabaseError(result.ResponseMessage, "Error recording stat");
                return Result<ResponseBase>.Failure(supabaseError);
            }


            var updateResult = await _supabase.From<TableModels.Statistics>()
                .Where(s => s.Id == rankingId)
                .Set(s => s.LastUpdate, DateTime.UtcNow)
                .Update();

            if (!updateResult.ResponseMessage.IsSuccessStatusCode)
            {
                Error supabaseError = await Error.SupabaseError(result.ResponseMessage, "Error recording stat");
                return Result<ResponseBase>.Failure(supabaseError);
            }

            ResponseBase response = new ResponseBase("Successfully recorded view");
            return Result<ResponseBase>.Success(response);
        }

        public async Task<Result<RankingsCountResponse>> GetTotalRankingsCountAsync()
        {
            try
            {
                // Get total count of all rankings in the system
                var countResult = await _supabase.From<TableModels.Ranking>()
                    .Count(Supabase.Postgrest.Constants.CountType.Exact);

                if (countResult != null)
                {
                    var response = new RankingsCountResponse
                    {
                        TotalCount = (int)countResult,
                        Message = "Successfully retrieved total rankings count"
                    };
                    return Result<RankingsCountResponse>.Success(response);
                }
                else
                {
                    return Result<RankingsCountResponse>.Failure(Error.NullValue, "Unable to retrieve rankings count");
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error retrieving total rankings count");
                return Result<RankingsCountResponse>.Failure(new Error("CountError", "Failed to retrieve rankings count"));
            }
        }

        public async Task<Result<UsersCountResponse>> GetTotalUsersCountAsync()
        {
            try
            {
                // Get total count of all users in the system
                // Note: This assumes you have a Users table. Adjust table name as needed.
                var countResult = await _supabase.From<TableModels.UserMetadata>()
                    .Count(Supabase.Postgrest.Constants.CountType.Exact);

                if (countResult != null)
                {
                    var response = new UsersCountResponse
                    {
                        TotalCount = (int)countResult,
                        Message = "Successfully retrieved total users count"
                    };
                    return Result<UsersCountResponse>.Success(response);
                }
                else
                {
                    return Result<UsersCountResponse>.Failure(Error.NullValue, "Unable to retrieve users count");
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error retrieving total users count");
                return Result<UsersCountResponse>.Failure(new Error("CountError", "Failed to retrieve users count"));
            }
        }
    }
}
