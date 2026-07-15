using Hongdal.Services.Community;
using MediatR;

namespace Hongdal.Application.Community;

public sealed class 커뮤니티키워드알림EventHandler : INotificationHandler<커뮤니티게시글등록됨Event>
{
    private readonly ICommunityKeywordNotificationSignal _signal;

    public 커뮤니티키워드알림EventHandler(ICommunityKeywordNotificationSignal signal)
    {
        _signal = signal;
    }

    public Task Handle(커뮤니티게시글등록됨Event notification, CancellationToken cancellationToken)
    {
        _signal.Notify();
        return Task.CompletedTask;
    }
}
