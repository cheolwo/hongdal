using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Application.Community;

public interface ICommunityActivityPostPublisher
{
    Task PublishAsync(
        CommunityActivityBoardDefinition definition,
        object occurrence,
        CancellationToken cancellationToken = default);
}
