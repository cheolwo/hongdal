using Microsoft.Extensions.Logging.Abstractions;
using Ssalddel.Application.Behaviors;
using Ssalddel.Application.CommonContents.Commands;
using Ssalddel.Application.Community;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.CommonContents;

namespace Ssalddel.Tests.Application.Community;

public sealed class CommunityActivityPublicationPolicyTests
{
    [Fact]
    public async Task CommandBehavior_DoesNotPublishRelationshipOnlyCommand()
    {
        var publisher = new RecordingPublisher();
        var behavior = new CommunityActivityCommandPostBehavior<
            콘텐츠시청시작Command,
            콘텐츠시청시작Result?>(
                publisher,
                NullLogger<CommunityActivityCommandPostBehavior<
                    콘텐츠시청시작Command,
                    콘텐츠시청시작Result?>>.Instance);
        var command = new 콘텐츠시청시작Command(11, 120);

        var result = await behavior.Handle(
            command,
            _ => Task.FromResult<콘텐츠시청시작Result?>(new() { 세션Id = 7 }),
            CancellationToken.None);

        Assert.Equal(7, result?.세션Id);
        Assert.Empty(publisher.Publications);
        var definition = Assert.IsType<CommunityActivityBoardDefinition>(
            CommunityActivityBoardCatalog.FindSource(
                CommunityActivitySourceKinds.Command,
                nameof(콘텐츠시청시작Command)));
        Assert.False(definition.PublishesActivityPost);
    }

    private sealed class RecordingPublisher : ICommunityActivityPostPublisher
    {
        public List<CommunityActivityBoardDefinition> Publications { get; } = [];

        public Task PublishAsync(
            CommunityActivityBoardDefinition definition,
            object occurrence,
            CancellationToken cancellationToken = default)
        {
            Publications.Add(definition);
            return Task.CompletedTask;
        }
    }
}
