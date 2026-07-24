using Microsoft.Extensions.Options;
using Quartz;
using 살뜰.Services.Options;

namespace Ssalddel.Infrastructure.BackgroundJobs.Community;

[DisallowConcurrentExecution]
public sealed class CommunityCultureTransportEditorialJob : IJob
{
    private readonly CommunityEditorialBatchRunner _runner;
    private readonly CommunityEditorialBatchOptions _options;
    private readonly ILogger<CommunityCultureTransportEditorialJob> _logger;

    public CommunityCultureTransportEditorialJob(
        CommunityEditorialBatchRunner runner,
        IOptions<CommunityEditorialBatchOptions> options,
        ILogger<CommunityCultureTransportEditorialJob> logger)
    {
        _runner = runner;
        _options = options.Value;
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context)
        => CommunityEditorialJobExecution.RunAsync(
            "CommunityCultureTransportEditorial",
            _runner.RunCultureTransportAsync,
            context,
            _options,
            _logger);
}
