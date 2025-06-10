using Microsoft.Extensions.Caching.Memory;
using simple_rank_backend.Application.Common;
using simple_rank_backend.Application.Services.Interfaces;
using simple_rank_backend.DTOs;
using simple_rank_backend.DTOs.User;
using simple_rank_backend.Models;
using Supabase;


namespace simple_rank_backend.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserCacheService _userCache;
        private readonly Client _supabase;
        private readonly Serilog.ILogger _logger;

        public UserService(IHttpContextAccessor httpContextAccessor, Client supabaseService, IUserCacheService cache, Serilog.ILogger log)
        {
            _httpContextAccessor = httpContextAccessor;
            _supabase = supabaseService;
            _userCache = cache;
            _logger = log;
        }

        public async Task<Result<ResponseBase>> SetUserMetadata(SetMetadataDTO rq)
        {
            Supabase.Postgrest.QueryOptions insertOptions = new Supabase.Postgrest.QueryOptions();
            insertOptions.DuplicateResolution = Supabase.Postgrest.QueryOptions.DuplicateResolutionType.IgnoreDuplicates;
            
            TableModels.UserMetadata metadata = new TableModels.UserMetadata(rq);

            try
            {
                var insertResult = await _supabase.From<TableModels.UserMetadata>().Insert(model: metadata);
                if (insertResult.ResponseMessage.IsSuccessStatusCode)
                {
                    await _userCache.SetUserAsync(rq.Id, new UserMetadata(metadata));
                    ResponseBase response = new ResponseBase("Successfully updated user mmetadata");
                    return Result<ResponseBase>.Success(response);
                }
                else
                {
                    var error = await Error.SupabaseError(insertResult.ResponseMessage, "Supabase error");
                    return Result<ResponseBase>.Failure(error);
                }
            }
            catch(Exception ex)
            {
                _logger.Warning("Unable to insert metadata", ex);
                if(ex.Message.ToLower().Contains("violates unique constraint"))
                {
                    ResponseBase alreadySavedResponse = new ResponseBase("Successfully updated metadata");
                    _logger.Information("metadata already saved");
                    return Result<ResponseBase>.Success(alreadySavedResponse);
                }
                else
                {
                    _logger.Error($"Unexpected insert error {ex.Message}", ex);
                    return Result<ResponseBase>.Failure(Error.Exception("500", ex));
                }
            }
            
        }

        public async Task<UserMetadata> GetUserMetadataAsync(string userId)
        {
            var result = await _supabase.From<TableModels.UserMetadata>().Where(u => u.UserId == userId).Get();
            if(result.ResponseMessage.IsSuccessStatusCode && result.Models.Any())
            {
                var response = new UserMetadata(result.Models.First());
                return response;
            }
            else
            {
                return default(UserMetadata);
            }
        }

        
    }
}
