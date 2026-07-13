using Hongdal.Services.Community;
using MediatR;

namespace Hongdal.Application.Community;

public sealed class 커뮤니티게시글등록됨EventHandler : INotificationHandler<커뮤니티게시글등록됨Event>
{
    private readonly I커뮤니티게시글음성작업신호 _작업신호;

    public 커뮤니티게시글등록됨EventHandler(I커뮤니티게시글음성작업신호 작업신호)
    {
        _작업신호 = 작업신호;
    }

    public Task Handle(커뮤니티게시글등록됨Event notification, CancellationToken cancellationToken)
    {
        _작업신호.알림();
        return Task.CompletedTask;
    }
}
