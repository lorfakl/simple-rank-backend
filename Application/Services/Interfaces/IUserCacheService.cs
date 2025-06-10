using simple_rank_backend.Models;

namespace simple_rank_backend.Application.Services.Interfaces
{
    public interface IUserCacheService
    {
        Task<UserMetadata> GetUserAsync(string userId);
        Task<UserMetadata> GetUserAsync(string userId, bool forceRefresh);
        Task SetUserAsync(string userId, UserMetadata metadata);
        Task RemoveUserAsync(string userId);
        Task ClearCacheAsync();
    }
}
