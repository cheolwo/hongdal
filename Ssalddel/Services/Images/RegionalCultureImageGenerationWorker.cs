using Microsoft.Extensions.Options;
using 살뜰.Services.Options;

namespace 살뜰.Services.Images;

public sealed class 지역문화이미지생성Worker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<지역문화이미지생성Worker> _logger;
    private readonly RegionalCultureImageGenerationOptions _options;

    public 지역문화이미지생성Worker(
        IServiceProvider serviceProvider,
        ILogger<지역문화이미지생성Worker> logger,
        IOptions<RegionalCultureImageGenerationOptions> options)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation(
                "Regional culture image generation worker is disabled. "
                + "Set RegionalCultureImageGeneration:Enabled=true after evidence review and cost approval.");
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Max(1, _options.IntervalMinutes));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var service = scope.ServiceProvider
                    .GetRequiredService<I지역문화이미지순차생성Service>();
                var result = await service.다음배치생성Async(
                    _options.MaxNewJobsPerCycle,
                    includeFailed: false,
                    stoppingToken);

                if (result.Accepted)
                {
                    _logger.LogInformation(
                        "Regional culture image generation submitted {Count} job(s): {JobCodes}.",
                        result.Jobs.Count,
                        string.Join(",", result.Jobs.Select(item => item.작업코드)));
                }
                else if (result.ResultCode is not "ActiveJobExists")
                {
                    _logger.LogInformation(
                        "Regional culture image generation skipped. Code={Code}, Message={Message}",
                        result.ResultCode,
                        result.Message);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Regional culture image generation worker failed. The next cycle will retry without duplicating active jobs.");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }
}
