using Hongdal.Services.Content;
using Quartz;

namespace Hongdal.Infrastructure.BackgroundJobs.Content;

[DisallowConcurrentExecution]
public sealed class YouTube채널동기화Job : IJob
{
    private readonly IYouTube채널감시Service _service;
    private readonly ILogger<YouTube채널동기화Job> _logger;

    public YouTube채널동기화Job(
        IYouTube채널감시Service service,
        ILogger<YouTube채널동기화Job> logger)
    {
        _service = service;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var result = await _service.동기화Async(null, context.CancellationToken);
        _logger.LogInformation(
            "Action={Action} Executed={Executed} ChannelCount={ChannelCount} ReceivedVideoCount={ReceivedVideoCount} AddedVideoCount={AddedVideoCount} NewUploadCount={NewUploadCount}",
            "YouTubeChannelsSynced",
            result.실행됨,
            result.처리채널수,
            result.수신영상수,
            result.추가영상수,
            result.신규업로드수);
    }
}
