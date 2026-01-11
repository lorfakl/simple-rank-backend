
using simple_rank_backend.Application.Common;
using simple_rank_backend.Application.Services;
using Supabase;
using ILogger = Serilog.ILogger;

namespace simple_rank_backend.Application.BackgroundServices
{
    public class SupabaseBackgroundService : BackgroundService
    {
        private readonly Serilog.ILogger _logger;
        private readonly Client _supabase;
        private readonly Random _random;
        private DateTime _nextExecution;

        public SupabaseBackgroundService(Serilog.ILogger logger, Client supabaseService)
        {
            _logger = logger;
            _supabase = supabaseService;
            _random = new Random();

            _nextExecution = DateTime.UtcNow.AddSeconds(15); //AddMinutes(_random.NextInt64(1, 365));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Information("Supabase background service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                if (DateTime.UtcNow >= _nextExecution)
                {
                    _logger.Information("Supabase background service is running.");

                    var insertResult = await _supabase.From<TableModels.Heartbeat>().Insert(new TableModels.Heartbeat() { LastHeartbeat = DateTime.UtcNow }, cancellationToken: stoppingToken);

                    if (insertResult.ResponseMessage.IsSuccessStatusCode)
                    {
                        _logger.Information("Heartbeat updated successfully at {Time}", DateTime.UtcNow);
                    }
                    else
                    {
                        _logger.Error("Failed to insert heartbeat due to {Message}. Status Code: {StatusCode}", insertResult.ResponseMessage, insertResult.ResponseMessage.StatusCode);
                    }

                    _nextExecution = _nextExecution.AddMinutes(_random.NextInt64(1, 1440)); // Next between 1 minute to 24 hours
                }
            }
        }
    }
}
