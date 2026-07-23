using Microsoft.Extensions.Logging;
using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Application.Community.Handlers;

public sealed class CommunityActivityEventPostHandler<TEvent>(
    ICommunityActivityPostPublisher publisher,
    ILogger<CommunityActivityEventPostHandler<TEvent>> logger)
    : INotificationHandler<TEvent>
    where TEvent : INotification
{
    public async Task Handle(TEvent notification, CancellationToken cancellationToken)
    {
        var definition = CommunityActivityBoardCatalog.FindSource(
            CommunityActivitySourceKinds.Event,
            typeof(TEvent).Name);
        if (definition is null)
        {
            return;
        }

        try
        {
            await publisher.PublishAsync(definition, notification, cancellationToken);
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(
                exception,
                "Event 활동 게시글 발행에 실패했습니다. EventName={EventName} BoardKey={BoardKey}",
                typeof(TEvent).Name,
                definition.Board.Key);
        }
    }
}
