using MediatR;
using Microsoft.Extensions.Options;
using Ssalddel.Services.Community;
using 살뜰.Services.Options;

namespace Ssalddel.Application.Community;

public sealed class 커뮤니티게시글이메일알림EventHandler
    : INotificationHandler<커뮤니티게시글등록됨Event>
{
    private readonly ICommunityPostEmailNotificationOutboxStore _outbox;
    private readonly IOptionsMonitor<CommunityPostEmailNotificationOptions> _options;
    private readonly ILogger<커뮤니티게시글이메일알림EventHandler> _logger;

    public 커뮤니티게시글이메일알림EventHandler(
        ICommunityPostEmailNotificationOutboxStore outbox,
        IOptionsMonitor<CommunityPostEmailNotificationOptions> options,
        ILogger<커뮤니티게시글이메일알림EventHandler> logger)
    {
        _outbox = outbox;
        _options = options;
        _logger = logger;
    }

    public async Task Handle(
        커뮤니티게시글등록됨Event notification,
        CancellationToken cancellationToken)
    {
        if (!_options.CurrentValue.Enabled)
        {
            return;
        }

        try
        {
            await _outbox.EnqueueAsync(notification.게시글Id, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "게시글 {PostId} Gmail 알림 DB outbox 적재에 실패했습니다.",
                notification.게시글Id);
            throw;
        }
    }
}
