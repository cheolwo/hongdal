using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Ssalddel.Application.Behaviors;
using Ssalddel.Application.CommonContents.Commands;
using Ssalddel.Application.Community;
using Ssalddel.Application.Community.Handlers;
using Ssalddel.Application.Driver.Transport;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.CommonContents;

namespace Ssalddel.Tests.Application.Community;

public sealed class CommunityActivityPostHandlersTests
{
    [Fact]
    public async Task EventHandler_PublishesSelectedEvent()
    {
        var publisher = new RecordingActivityPublisher();
        var handler = new CommunityActivityEventPostHandler<운송상차완료됨Event>(
            publisher,
            NullLogger<CommunityActivityEventPostHandler<운송상차완료됨Event>>.Instance);
        var notification = new 운송상차완료됨Event(
            "driver",
            1,
            "TR-1",
            "출발지",
            "도착지",
            "배차완료",
            "상차완료",
            DateTime.UtcNow,
            "trace",
            null);

        await handler.Handle(notification, CancellationToken.None);

        var publication = Assert.Single(publisher.Publications);
        Assert.Equal("activity-transport", publication.Definition.Board.Key);
        Assert.Same(notification, publication.Occurrence);
    }

    [Fact]
    public async Task EventHandler_IgnoresUnselectedEvent()
    {
        var publisher = new RecordingActivityPublisher();
        var handler = new CommunityActivityEventPostHandler<UnselectedEvent>(
            publisher,
            NullLogger<CommunityActivityEventPostHandler<UnselectedEvent>>.Instance);

        await handler.Handle(new UnselectedEvent(), CancellationToken.None);

        Assert.Empty(publisher.Publications);
    }

    [Fact]
    public void OpenGenericEventHandlerRegistration_ResolvesSelectedEventHandler()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<ICommunityActivityPostPublisher>(new RecordingActivityPublisher());
        services.AddScoped(
            typeof(INotificationHandler<>),
            typeof(CommunityActivityEventPostHandler<>));

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
        using var scope = provider.CreateScope();

        Assert.IsType<CommunityActivityEventPostHandler<운송상차완료됨Event>>(
            scope.ServiceProvider.GetRequiredService<INotificationHandler<운송상차완료됨Event>>());
    }

    [Fact]
    public async Task CommandBehavior_PublishesOnlyCompletedCommand()
    {
        var publisher = new RecordingActivityPublisher();
        var behavior = new CommunityActivityCommandPostBehavior<
            콘텐츠시청완료Command,
            콘텐츠시청완료Result?>(
                publisher,
                NullLogger<CommunityActivityCommandPostBehavior<
                    콘텐츠시청완료Command,
                    콘텐츠시청완료Result?>>.Instance);
        var command = new 콘텐츠시청완료Command(42);

        var incomplete = await behavior.Handle(
            command,
            _ => Task.FromResult<콘텐츠시청완료Result?>(new()
            {
                완료여부 = false,
                메시지 = "아직 완료되지 않았습니다."
            }),
            CancellationToken.None);
        var completed = await behavior.Handle(
            command,
            _ => Task.FromResult<콘텐츠시청완료Result?>(new()
            {
                완료여부 = true,
                메시지 = "완료되었습니다."
            }),
            CancellationToken.None);

        Assert.False(incomplete!.완료여부);
        Assert.True(completed!.완료여부);
        var publication = Assert.Single(publisher.Publications);
        Assert.Equal("activity-foundation", publication.Definition.Board.Key);
        Assert.Same(command, publication.Occurrence);
    }

    private sealed record UnselectedEvent : INotification;

    private sealed class RecordingActivityPublisher : ICommunityActivityPostPublisher
    {
        public List<Publication> Publications { get; } = [];

        public Task PublishAsync(
            CommunityActivityBoardDefinition definition,
            object occurrence,
            CancellationToken cancellationToken = default)
        {
            Publications.Add(new Publication(definition, occurrence));
            return Task.CompletedTask;
        }
    }

    private sealed record Publication(
        CommunityActivityBoardDefinition Definition,
        object Occurrence);
}
