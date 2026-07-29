using Ssalddel.Application.Community;
using Ssalddel.Contracts.Common.Community;

namespace Ssalddel.Services.Community;

public sealed class CommunityActivityPostPublisher(
    I커뮤니티활동공개ProjectionRecorder projectionRecorder) : ICommunityActivityPostPublisher
{
    public Task PublishAsync(
        CommunityActivityBoardDefinition definition,
        object occurrence,
        CancellationToken cancellationToken = default)
        => projectionRecorder.RecordAsync(definition, occurrence, cancellationToken);
}
