using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using simple_rank_backend.Application.Configuration;
using simple_rank_backend.Application.Services.Interfaces;
using simple_rank_backend.Models;

namespace simple_rank_backend.Application.Services
{
    public class UserCacheService : IUserCacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<UserCacheService> _logger;
        private readonly UserCacheOptions _options;
        private readonly IServiceScopeFactory _scopeFactory; // Your service to fetch user data

        public UserCacheService(IMemoryCache cache, ILogger<UserCacheService> logger, IOptions<UserCacheOptions> options, IServiceScopeFactory scopeFactory)
        {
            _cache = cache;
            _logger = logger;
            _options = options.Value;
            _scopeFactory = scopeFactory;
        }

        public async Task<UserMetadata> GetUserAsync(string userId)
        {
            return await GetUserAsync(userId, false);
        }

        public async Task<UserMetadata> GetUserAsync(string userId, bool forceRefresh)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            var cacheKey = GetCacheKey(userId);

            // Try to get from cache first (unless force refresh)
            if (!forceRefresh && _cache.TryGetValue<UserMetadata>(cacheKey, out var cachedUser))
            {
                _logger.LogDebug("User {UserId} found in cache", userId);
                return cachedUser;
            }

            using(var scope =  _scopeFactory.CreateScope())
            {
                var userService = scope.ServiceProvider.GetService<IUserService>();

                // Fetch from data source
                _logger.LogDebug("Fetching user {UserId} from data source", userId);
                var user = await userService.GetUserMetadataAsync(userId);

                if (user != null)
                {
                    // Store in cache
                    await SetUserAsync(userId, user);
                }

                return user;
            }
        }

        public Task SetUserAsync(string userId, UserMetadata metadata)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            var cacheKey = GetCacheKey(userId);
            metadata.LastUpdated = DateTime.UtcNow;

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(_options.SlidingExpirationMinutes))
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(_options.AbsoluteExpirationMinutes))
                .SetPriority(CacheItemPriority.High)
                .SetSize(1); // If using size limits

            _cache.Set(cacheKey, metadata, cacheEntryOptions);
            _logger.LogDebug("User {UserId} cached successfully", userId);

            return Task.CompletedTask;
        }

        public Task RemoveUserAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            var cacheKey = GetCacheKey(userId);
            _cache.Remove(cacheKey);
            _logger.LogDebug("User {UserId} removed from cache", userId);

            return Task.CompletedTask;
        }

        public Task ClearCacheAsync()
        {
            // Note: IMemoryCache doesn't have a built-in clear method
            // You might need to track keys or use a different approach
            _logger.LogWarning("Cache clear requested - consider implementing key tracking for full clear functionality");
            return Task.CompletedTask;
        }

        private string GetCacheKey(string userId)
        {
            return $"user:{userId}";
        }
    }

}
