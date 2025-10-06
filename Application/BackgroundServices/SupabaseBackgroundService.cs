
using simple_rank_backend.Application.Common;
using Supabase;

namespace simple_rank_backend.Application.BackgroundServices
{
    public class SupabaseBackgroundService : BackgroundService
    {
        private readonly Serilog.ILogger _logger;
        private readonly Client _supabase;

        private DateTime _nextExecution;

        public SupabaseBackgroundService(Serilog.ILogger logger)
        {
            _logger = logger;
            _supabase = new Client(SecretManager.GetSecret(SecretKeys.supabaseUrl), SecretManager.GetSecret(SecretKeys.supabaseServiceKey),
                new SupabaseOptions
                {
                    AutoRefreshToken = true,
                    AutoConnectRealtime = true,
                }
            );
            _nextExecution = DateTime.UtcNow.AddMinutes(5);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Information("Supabase background service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                if (DateTime.UtcNow >= _nextExecution)
                {
                    // Perform background tasks here
                    _logger.Information("Supabase background service is running.");
                    // Wait for a specified interval before the next iteration
                    //Task.Delay(TimeSpan.FromMinutes(5), stoppingToken).Wait(stoppingToken);
                    var insertResult = await _supabase.From<TableModels.Heartbeat>().Insert(new TableModels.Heartbeat() { LastHeartbeat = DateTime.UtcNow }, cancellationToken: stoppingToken);

                    if (insertResult.ResponseMessage.IsSuccessStatusCode)
                    {
                        _logger.Information("Heartbeat updated successfully at {Time}", DateTime.UtcNow);
                    }
                    else
                    {
                        _logger.Error("Failed to insert heartbeat due to {Message}. Status Code: {StatusCode}", insertResult.ResponseMessage, insertResult.ResponseMessage.StatusCode);
                    }

                    _nextExecution = _nextExecution.AddMinutes(5);
                }
            }
        }
    }
}
