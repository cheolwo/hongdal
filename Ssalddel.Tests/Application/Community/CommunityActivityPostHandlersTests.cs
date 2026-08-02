using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Ssalddel.Application.Behaviors;
using Ssalddel.Application.CommonContents.Commands;
using Ssalddel.Application.Community;
using Ssalddel.Application.Community.Handlers;
using Ssalddel.Application.Driver.Transport;
using Ssalddel.Application.Food.Events;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Food;
using Ssalddel.Contracts.CommonContents;

namespace Ssalddel.Tests.Application.Community;

public sealed class CommunityActivityPostHandlersTests
{
    [Fact]
    public async Task EventHandler_PublishesSelectedEvent()
    {
        var publisher = new RecordingActivityPublisher();
        var handler = new CommunityActivityEventPostEventHandler<운송상차완료됨Event>(
            publisher,
            NullLogger<CommunityActivityEventPostEventHandler<운송상차완료됨Event>>.Instance);
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
        Assert.Equal(CommunityActivityBoardKeys.LoadingJourney, publication.Definition.Board.Key);
        Assert.Same(notification, publication.Occurrence);
    }

    [Fact]
    public async Task EventHandler_IgnoresUnselectedEvent()
    {
        var publisher = new RecordingActivityPublisher();
        var handler = new CommunityActivityEventPostEventHandler<UnselectedEvent>(
            publisher,
            NullLogger<CommunityActivityEventPostEventHandler<UnselectedEvent>>.Instance);

        await handler.Handle(new UnselectedEvent(), CancellationToken.None);

        Assert.Empty(publisher.Publications);
    }

    [Fact]
    public async Task EventHandler_PublishesFoodDeliveryReceiptToHandoffBoard()
    {
        var publisher = new RecordingActivityPublisher();
        var handler = new CommunityActivityEventPostEventHandler<주문자음식주문수령확인됨Event>(
            publisher,
            NullLogger<CommunityActivityEventPostEventHandler<주문자음식주문수령확인됨Event>>.Instance);
        var notification = new 주문자음식주문수령확인됨Event(
            new 음식주문응답 { 주문번호 = "FOOD-PRIVATE" },
            "orderer-private",
            "문 앞 수령",
            DateTime.UtcNow,
            "event-private");

        await handler.Handle(notification, CancellationToken.None);

        var publication = Assert.Single(publisher.Publications);
        Assert.Equal(CommunityActivityBoardKeys.FoodDeliveryHandoff, publication.Definition.Board.Key);
        Assert.Same(notification, publication.Occurrence);
    }

    [Fact]
    public async Task EventHandler_PublishesFoodDeliveryHandoffStatusChange()
    {
        var publisher = new RecordingActivityPublisher();
        var handler = new CommunityActivityEventPostEventHandler<음식배달인계상태변경됨Event>(
            publisher,
            NullLogger<CommunityActivityEventPostEventHandler<음식배달인계상태변경됨Event>>.Instance);
        var notification = new 음식배달인계상태변경됨Event(
            "delivery-41",
            "order-41",
            "픽업완료",
            DateTime.UtcNow,
            "event-41");

        await handler.Handle(notification, CancellationToken.None);

        var publication = Assert.Single(publisher.Publications);
        Assert.Equal(CommunityActivityBoardKeys.FoodDeliveryHandoff, publication.Definition.Board.Key);
        Assert.Same(notification, publication.Occurrence);
    }

    [Fact]
    public void OpenGenericEventHandlerRegistration_ResolvesSelectedEventHandler()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<ICommunityActivityPostPublisher>(new RecordingActivityPublisher());
        services.AddScoped(
            typeof(INotificationHandler<>),
            typeof(CommunityActivityEventPostEventHandler<>));

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });
        using var scope = provider.CreateScope();

        Assert.IsType<CommunityActivityEventPostEventHandler<운송상차완료됨Event>>(
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
        Assert.Equal(CommunityActivityBoardKeys.FoundationEvidence, publication.Definition.Board.Key);
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
