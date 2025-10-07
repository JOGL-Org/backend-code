namespace Jogl.Server.API.Services
{
    public class TaskBackgroundService : BackgroundService
    {
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly ILogger<TaskBackgroundService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public TaskBackgroundService(
            IBackgroundTaskQueue taskQueue,
            ILogger<TaskBackgroundService> logger,
            IServiceScopeFactory serviceScopeFactory)
        {
            _taskQueue = taskQueue;
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Task Background Service is running");

            await BackgroundProcessing(stoppingToken);
        }

        private async Task BackgroundProcessing(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var workItem = await _taskQueue.DequeueAsync(stoppingToken);

                    // Create a scope for this operation to get fresh dependencies
                    using var scope = _serviceScopeFactory.CreateScope();

                    try
                    {
                        await workItem(stoppingToken);
                        _logger.LogInformation("Background task completed successfully");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error occurred executing background work item");
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing the background queue");
                }
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Task Background Service is stopping");
            await base.StopAsync(stoppingToken);
        }
    }
}
