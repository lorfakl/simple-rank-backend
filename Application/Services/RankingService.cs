using Serilog;
using simple_rank_backend.Application.Common;
using simple_rank_backend.Application.Services.Interfaces;
using simple_rank_backend.DTOs;
using simple_rank_backend.DTOs.Ranking;
using simple_rank_backend.DTOs.Ranking.Responses;
using simple_rank_backend.Models;
using Supabase;
using Supabase.Postgrest.Responses;
using System.Linq;
using System.Security.Claims;

namespace simple_rank_backend.Application.Services
{
    public class RankingService : IRankService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserCacheService _userCache;
        private readonly Client _supabase;
        private readonly Serilog.ILogger _log;

        public RankingService(IHttpContextAccessor httpContextAccessor, IUserCacheService userCache, Client supabaseService, Serilog.ILogger logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _supabase = supabaseService;
            _userCache = userCache;
            _log = logger;
        }

        public async Task<Result<CreateRankingResponse>> CreateRankingAsync(CreateRankingRequest rq, string ownerId)
        {
            string userId = ownerId;
            TableModels.Ranking newRanking = new TableModels.Ranking(rq, userId);

            var result = await _supabase.From<TableModels.Ranking>().Insert(newRanking);

            if (result?.ResponseMessage?.IsSuccessStatusCode == true)
            {

                if (rq.Items.Any())
                {
                    var rankItems = TableModels.RankingItems.CreateItems(result.Model.RankingId, rq.Items.ToList());
                    var insertItemsResult = await _supabase.From<TableModels.RankingItems>().Insert(rankItems);

                    if (insertItemsResult.ResponseMessage.IsSuccessStatusCode && insertItemsResult.Models.Any())
                    {
                        var itemPlacement = TableModels.Ranking.CreateItemPlacement(insertItemsResult.Models.OrderBy(i => i.Rank).ToList());

                        var updateItemPlacementResult = await _supabase.From<TableModels.Ranking>()
                                                        .Where(r => r.RankingId == result.Model.RankingId)
                                                        .Where(r => r.Owner == ownerId)
                                                        .Set(r => r.ItemPlacement, itemPlacement)
                                                        .Update();

                        if(updateItemPlacementResult.ResponseMessage.IsSuccessStatusCode)
                        {
                            var successResponse = new CreateRankingResponse(result.Model.RankingId, "Successfully created ranking");
                            return Result<CreateRankingResponse>.Success(successResponse);
                        }
                        else
                        {
                            var errorResponse = new CreateRankingResponse("", "Data integritial error detected, aborting");
                            return Result<CreateRankingResponse>.HandleSupabase(insertItemsResult.ResponseMessage, errorResponse);
                        }
                    }
                    else
                    {
                        var errorResponse = new CreateRankingResponse("", "Data integritial error detected, aborting");
                        return Result<CreateRankingResponse>.HandleSupabase(insertItemsResult.ResponseMessage, errorResponse);
                    }

                }

                var response = new CreateRankingResponse(result.Model.RankingId, "Successfully created ranking");
                return Result<CreateRankingResponse>.Success(response);
            }
            else
            {
                return Result<CreateRankingResponse>.Failure(new Error(
                    Code: result?.ResponseMessage?.StatusCode.ToString() ?? "Unknown",
                    Message: result?.ResponseMessage?.ToString() ?? "No response message"));
            }
        }

        public async Task<Result<ResponseBase>> CreateRankItemAsync(CreateRankingItemRequest rq, string ownerId)
        {
            var checkOwnerResult = await _supabase.From<TableModels.Ranking>()
                .Where(r => r.RankingId == rq.RankingId)
                .Where(r => r.Owner == ownerId)
                .Get();

            if(checkOwnerResult.ResponseMessage.IsSuccessStatusCode && checkOwnerResult.Models.Any())
            {
                TableModels.RankingItems newItem = new TableModels.RankingItems(rq.Item, rq.RankingId);
                
                var rankItemInsertResult = await _supabase.From<TableModels.RankingItems>().Insert(newItem);

                if(rankItemInsertResult.ResponseMessage.IsSuccessStatusCode && rankItemInsertResult.Models.Any())
                {
                    var itemPlacementDict = checkOwnerResult.Model.ItemPlacement;
                    itemPlacementDict.Add(rankItemInsertResult.Model.ItemId, (int)rq.Item.Rank);

                    var updateRankingPlacement = await _supabase.From<TableModels.Ranking>()
                        .Where(r => r.RankingId == rq.RankingId)
                        .Where(r => r.Owner == ownerId)
                        .Set(r => r.ItemPlacement, itemPlacementDict)
                        .Set(r => r.ItemCount, checkOwnerResult.Model.ItemCount + 1)
                        .Set(r => r.LastUpdated, DateTime.UtcNow)
                        .Update();

                    if(updateRankingPlacement.ResponseMessage.IsSuccessStatusCode)
                    {
                        ResponseBase response = new ResponseBase("Create rank item success");
                        return Result<ResponseBase>.Success(response);
                    }
                    else
                    {
                        var supaBaseError = await Error.SupabaseError(updateRankingPlacement.ResponseMessage, "Unable to create ranking", _log);
                        return Result<ResponseBase>.Failure(supaBaseError);
                    }
                }
                else
                {
                    var supaBaseError = await Error.SupabaseError(rankItemInsertResult.ResponseMessage, "Unable to create ranking", _log);
                    return Result<ResponseBase>.Failure(supaBaseError);
                }
            }
            else
            {
                var supaBaseError = await Error.SupabaseError(checkOwnerResult.ResponseMessage, "Unable to create ranking", _log);
                return Result<ResponseBase>.Failure(supaBaseError);
            }
        }


        public async Task<Result<Ranking>> GetRankingAsync(GetRankingByIdRequest rq)
        {
            var queryResult = await _supabase.From<TableModels.Ranking>()
                .Where(r => r.RankingId == rq.Id)
                .Get();


            if(queryResult.Model != default(TableModels.Ranking) && queryResult.Model != null)
            {
                var ranking = new Ranking(queryResult.Model);
                UserMetadata ownerInfo = await _userCache.GetUserAsync(ranking.Owner);

                ranking.CreatedBy = ownerInfo;

                return Result<Ranking>.Success(ranking, $"Successfully grabbed ranking: {rq.Id}");
            }
            else
            {
                return Result<Ranking>.Failure(Error.NullValue);
            }
        }

        public async Task<Result<ShareableLinkResponse>> GetShareableLinkAsync(string rankingId)
        {
            //get the ownerId if not throw error
            //check to see if a shareable link has been created
            //
            string ownerId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (String.IsNullOrEmpty(ownerId))
            {
                return Result<ShareableLinkResponse>.Failure(Error.NullValue, "Unable to determine owner Id");
            }

            var rankingQueryResult = await _supabase.From<TableModels.Ranking>()
                .Where(r => r.RankingId == rankingId)
                .Where(r => r.Owner == ownerId).Get();

            //handle supabase error
            if(!rankingQueryResult.ResponseMessage.IsSuccessStatusCode)
            {
                var supaBaseError = await Error.SupabaseError(rankingQueryResult.ResponseMessage, "Unable to get shareable link");
                return Result<ShareableLinkResponse>.Failure(supaBaseError);
            }

            if(rankingQueryResult.Models.Count > 0)
            {
                var ranking = rankingQueryResult.Model;
                if(ranking.IsShared)
                {
                    string shareableId = ranking.ShareableId.SharedLinkId;
                    if(string.IsNullOrEmpty(shareableId))
                    {
                        shareableId = HashService.GenerateRobustHash(shareableId);
                    }
                    
                    var shareableResponse = new ShareableLinkResponse(shareableId);
                    return Result<ShareableLinkResponse>.Success(shareableResponse, "successfully retrieved shareable id");
                }
                else
                {
                    string shareable = HashService.GenerateRobustHash(rankingId);
                    var createShareableQuery = await _supabase.From<TableModels.ShareableLink>()
                        .Insert(new TableModels.ShareableLink
                        {
                            RankingId = rankingId,
                            SharedLinkId = shareable
                        });

                    if(!createShareableQuery.ResponseMessage.IsSuccessStatusCode)
                    {
                        var supaBaseError = await Error.SupabaseError(createShareableQuery.ResponseMessage, "Unable to get shareable link");
                        return Result<ShareableLinkResponse>.Failure(supaBaseError);
                    }

                    var updateRankingQueryResult = await _supabase.From<TableModels.Ranking>()
                        .Where(r => r.RankingId == rankingId)
                        .Where(r => r.Owner == ownerId)
                        .Set(r => r.IsShared, true)
                        .Update();

                    if(updateRankingQueryResult.ResponseMessage.IsSuccessStatusCode)
                    {
                        var shareableResponse = new ShareableLinkResponse(shareable);
                        return Result<ShareableLinkResponse>.Failure(Error.NotFound);
                    }
                    else
                    {
                        var errorResult = await Error.SupabaseError(updateRankingQueryResult.ResponseMessage, "Failed to updating ranking as shared");
                        return Result<ShareableLinkResponse>.Failure(errorResult);
                    }
                }
            }
            else
            {
                //weird this happened tbh
                return Result<ShareableLinkResponse>.Failure(Error.NotFound);
            }


            //throw new NotImplementedException();

        }

        public async Task<Result<SharedRankResponse>> GetSharedRankAsync(string rankingId)
        {
            var shareLinkResult = await _supabase.From<TableModels.ShareableLink>()
                .Where(l => l.SharedLinkId == rankingId)
                .Get();

            if(shareLinkResult.ResponseMessage.IsSuccessStatusCode && shareLinkResult.Models.Any())
            {
                var rankingResult = await _supabase.From<TableModels.Ranking>()
                    .Where(r => r.RankingId == shareLinkResult.Model.RankingId)
                    .Get();
                
                if(rankingResult.ResponseMessage.IsSuccessStatusCode && rankingResult.Models.Any())
                {
                    var user = await _userCache.GetUserAsync(rankingResult.Model.Owner);
                    SharedRankResponse properResponse = new SharedRankResponse(rankingResult.Model, user);

                    try
                    {
                        string callerId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? Guid.NewGuid().ToString();

                        TableModels.Stats statEntry = new TableModels.Stats(Statistics.VIEW, callerId, rankingResult.Model.RankingId);
                        var result = await _supabase.From<TableModels.Stats>().Insert(statEntry);

                        if (!result.ResponseMessage.IsSuccessStatusCode)
                        {
                            Error supabaseError = await Error.SupabaseError(result.ResponseMessage, "Error recording stat");
                            _log.Error<Error>("Supabase api error response", supabaseError);
                        }
                        else
                        {
                            var updateResult = await _supabase.From<TableModels.Statistics>()
                            .Where(s => s.Id == rankingId)
                            .Set(s => s.LastUpdate, DateTime.UtcNow)
                            .Update();
                        }
                    }
                    catch(Exception ex)
                    {
                        _log.Error(ex, "Exception thrown while attempting update statistics");
                    }
                    
                    return Result<SharedRankResponse>.Success(properResponse, "Successfully gotten ranking info");
                }
                else
                {
                    SharedRankResponse noRankingResponse = new SharedRankResponse();
                    return Result<SharedRankResponse>.HandleSupabase(rankingResult.ResponseMessage, noRankingResponse);
                }
            }

            SharedRankResponse response = new SharedRankResponse();
            return Result<SharedRankResponse>.HandleSupabase(shareLinkResult.ResponseMessage, response);
        }

        public async Task<Result<List<Ranking>>> GetUserRankingsAsync(string userId)
        {
            var queryResult = await _supabase.From<TableModels.Ranking>()
                .Where(r => r.Owner == userId)
                .Get();

            var rankings = queryResult.Models.Select(r => new Ranking(r))
                .OrderByDescending(r => r.IsPinned)
                .ThenBy(r => r.IsPinned ? r.PinnedTime : r.LastUpdated)
                .ToList();

            foreach(Ranking r in rankings)
            {
                UserMetadata ownerInfo = await _userCache.GetUserAsync(r.Owner);
                r.CreatedBy = ownerInfo;
            }
            

            return Result<List<Ranking>>.Success(rankings, $"Successfully pulled {rankings.Count} rankings for user");
        }

        public async Task<Result> UpdateBasicRankingInfoAsync(UpdateRankingRequest rq)
        {
            string ownerId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (String.IsNullOrEmpty(ownerId))
            {
                return Result.Failure(Error.NullValue, "Unable to determine owner Id");
            }

            var updateResult = await _supabase.From<TableModels.Ranking>()
                .Where(r => r.RankingId == rq.Id)
                .Where(r => r.Owner == ownerId)
                .Set(r => r.Description, rq.Description)
                .Set(r => r.Name, rq.Title)
                .Set(r => r.IsPublic, rq.IsPublic)
                .Set( r=> r.LastUpdated, DateTime.UtcNow)
                .Update();

            string msg = updateResult.ResponseMessage.IsSuccessStatusCode ? $"successfully updated {updateResult.Models.Count} rankings" : "failed to update basic ranking information";
            return Result.HandleSupabase(updateResult.ResponseMessage, msg);
        }

        public async Task<Result> UpdateRankingAsync(UpdateRankingRequest rq)
        {
            string ownerId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if(String.IsNullOrEmpty(ownerId))
            {
                return Result.Failure(Error.NullValue, "Unable to determine owner Id");
            }

            var getOwningRankResult = await _supabase.From<TableModels.Ranking>()
                .Where(r => r.RankingId == rq.Id)
                .Where(r => r.Owner == ownerId)
                .Get();

            if(!getOwningRankResult.ResponseMessage.IsSuccessStatusCode)
            {
                return Result.HandleSupabase(getOwningRankResult.ResponseMessage, "error occurred when executing supabase call");
            }

            if (getOwningRankResult.Models.Count != 1)
            {
                return Result.Failure(Error.ValidationFailed);
            }

            List<TableModels.RankingItems> itemsToUpsert = rq.Items.Select(i => new TableModels.RankingItems(i.Id, rq.Id, i.Name, i.Description, i.Rank)).ToList();
            var updateResult = await _supabase.From<TableModels.RankingItems>()
                .Upsert(itemsToUpsert);

            if(!updateResult.ResponseMessage.IsSuccessStatusCode)
            {
                return Result.HandleSupabase(updateResult.ResponseMessage, "failed to update rank items");
            }

            var updateItemCountResult = await _supabase.From<TableModels.Ranking>()
                .Where(r => r.RankingId == rq.Id)
                .Where(r => r.Owner == ownerId)
                .Set(r => r.ItemCount, (uint)rq.Items.Count)
                .Set(r => r.LastUpdated, DateTime.UtcNow)
                .Update();

            if(!updateItemCountResult.ResponseMessage.IsSuccessStatusCode)
            {
                return Result.HandleSupabase(updateItemCountResult.ResponseMessage, "failed to update ranking ids");
            }

            return Result.Success("successfully updated rank items");
        }

        public async Task<Result> UpdateRankItemPlacement(UpdateRankItemPlacementRequest rq)
        {
            string ownerId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (String.IsNullOrEmpty(ownerId))
            {
                return Result.Failure(Error.NullValue, "Unable to determine owner Id");
            }
            rq.ItemIdToRank = rq.ItemIdToRank.OrderBy(p => p.Value).ToDictionary();
            var updateItemPlacement = await _supabase.From<TableModels.Ranking>()
                .Where(r => r.RankingId == rq.RankingId)
                .Where(r => r.Owner == ownerId)
                .Set(r => r.ItemPlacement, rq.ItemIdToRank)
                .Set(r => r.LastUpdated, DateTime.UtcNow)
                .Update();

            if (updateItemPlacement.ResponseMessage.IsSuccessStatusCode)
            {
                return Result.Success("successfully updated rank item placements");
            }
            else
            {
                return Result.HandleSupabase(updateItemPlacement.ResponseMessage, "unable to update item placement, invalid");
            }
        }

        public async Task<Result<ResponseBase>> PinRankingAsync(string rankingId, bool isPinned = true)
        {
            string ownerId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (String.IsNullOrEmpty(ownerId))
            {
                return Result<ResponseBase>.Failure(Error.NullValue, "Unable to determine owner Id");
            }

            var updateResult = await _supabase.From<TableModels.Ranking>()
                .Where(r => r.RankingId == rankingId)
                .Where(r => r.Owner == ownerId)
                .Set(r => r.IsPinned, isPinned)
                .Set(r => r.LastUpdated, DateTime.UtcNow)
                .Set(r => r.PinnedTime, DateTime.UtcNow)
                .Update();

            if (updateResult.ResponseMessage.IsSuccessStatusCode)
            {
                string action = isPinned ? "pinned" : "unpinned";
                ResponseBase response = new ResponseBase($"Successfully {action} ranking");
                return Result<ResponseBase>.Success(response);
            }
            else
            {
                var supaBaseError = await Error.SupabaseError(updateResult.ResponseMessage, "Unable to update ranking pin status", _log);
                return Result<ResponseBase>.Failure(supaBaseError);
            }
        }

        public async Task<Result<ResponseBase>> DeleteRankingAsync(string rankingId)
        {
            string ownerId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (String.IsNullOrEmpty(ownerId))
            {
                return Result<ResponseBase>.Failure(Error.NullValue, "Unable to determine owner Id");
            }

            // First verify ownership
            var checkOwnerResult = await _supabase.From<TableModels.Ranking>()
                .Where(r => r.RankingId == rankingId)
                .Where(r => r.Owner == ownerId)
                .Get();

            if (checkOwnerResult.ResponseMessage.IsSuccessStatusCode && checkOwnerResult.Models.Any())
            {
                try
                {
                    await _supabase.From<TableModels.Ranking>()
                    .Where(r => r.RankingId == rankingId)
                    .Where(r => r.Owner == ownerId)
                    .Delete();

                    ResponseBase successResponse = new ResponseBase($"Successfully deleted ranking: {rankingId}");
                    return Result<ResponseBase>.Success(successResponse);
                }
                catch (Exception ex)
                {
                    var exceptionError = Error.Exception("500", ex, _log);
                    return Result<ResponseBase>.Failure(exceptionError);
                }
            }
            else
            {
                var supaBaseError = await Error.SupabaseError(checkOwnerResult.ResponseMessage, "Unable to delete ranking", _log);
                return Result<ResponseBase>.Failure(supaBaseError);
            } 
        }

        public async Task<Result> DeleteRankItem(DeleteRankItemRequest rq)
        {
            string ownerId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            if (String.IsNullOrEmpty(ownerId))
            {
                return Result.Failure(Error.NullValue, "Unable to determine owner Id");
            }

            var checkRankOwnerResult = await _supabase.From<TableModels.Ranking>()
                .Where(r => r.RankingId == rq.RankingId)
                .Where(r => r.Owner == ownerId)
                .Get();

            if(!checkRankOwnerResult.ResponseMessage.IsSuccessStatusCode)
            {
                return Result.HandleSupabase(checkRankOwnerResult.ResponseMessage, "unable to delete item, invalid");
            }

            if(checkRankOwnerResult.Models.Count != 1)
            {
                return Result.Failure(Error.ValidationFailed);
            }

             await _supabase.From<TableModels.RankingItems>()
                .Where(item => item.RankingId == rq.RankingId)
                .Where(item => item.ItemId == rq.RankItemId)
                .Delete();

            var itemPlacementDict = checkRankOwnerResult.Model.ItemPlacement;
            itemPlacementDict.Remove(rq.RankItemId);

            var updateRanking = await _supabase.From<TableModels.Ranking>()
                .Where(r => r.RankingId == rq.RankingId)
                .Where(r => r.Owner == ownerId)
                .Set(r => r.ItemPlacement, itemPlacementDict)
                .Set(r => r.ItemCount, checkRankOwnerResult.Model.ItemCount - 1)
                .Set(r => r.LastUpdated, DateTime.UtcNow)
                .Update();
            return Result.Success("removed rank item");

        }

        
    }
}
