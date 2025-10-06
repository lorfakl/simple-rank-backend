using Microsoft.Extensions.Caching.Memory;
using simple_rank_backend.Application.Services;
using simple_rank_backend.Application.Services.Interfaces;

namespace simple_rank_backend.Application.BackgroundServices
{
    public class ConfigureMemoryCache : IHostedService
    {
        private IMemoryCache? _memoryCache;
        private readonly Serilog.ILogger _logger;
        private ISecretClientService? _secretClientService;
        private readonly IServiceScopeFactory _scopeFactory;
        public ConfigureMemoryCache(IServiceScopeFactory scopeFactory, Serilog.ILogger logger)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.Information("Configuring Memory Cache with secrets from Key Vault.");

            _logger.Information("Loading the memory cache from scoped services");
            using (var scope = _scopeFactory.CreateScope())
            {
                _memoryCache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();
                _secretClientService = scope.ServiceProvider.GetRequiredService<ISecretClientService>();
                var secretResult = await _secretClientService.GetSecretAsync(SecretNames.MasterKey.ToString(), cancellationToken);
                if (secretResult.IsSuccess)
                {
                    var options = new MemoryCacheEntryOptions()
                        .SetPriority(CacheItemPriority.High)
                        .SetSize(1); // If using size limits
                    
                    _memoryCache.Set("MasterKey", secretResult.Value, options);
                    _logger.Information("MasterKey cached successfully.");
                }
                else
                {
                    _logger.Error("Failed to retrieve MasterKey from Key Vault: {Error}", secretResult.Error);
                    throw new Exception("Failed to retrieve MasterKey from Key Vault");
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
