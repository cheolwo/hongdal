using Microsoft.Extensions.Options;
using 홍달.Services.Options;

namespace 홍달.Services.Images;

public sealed class KieAiTaskPollingWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<KieAiTaskPollingWorker> _logger;
    private readonly KieAiOptions _options;

    public KieAiTaskPollingWorker(
        IServiceProvider serviceProvider,
        ILogger<KieAiTaskPollingWorker> logger,
        IOptions<KieAiOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var intervalSeconds = Math.Max(5, _options.PollingIntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<I샘플이미지생성Service>();
                await service.미완료작업처리Async(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kie.AI task polling worker failed.");
            }

            await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken);
        }
    }
}
