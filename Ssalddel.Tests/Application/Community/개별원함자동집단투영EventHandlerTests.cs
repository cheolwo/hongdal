using Microsoft.Extensions.Logging.Abstractions;
using Ssalddel.Application.Community.Events;
using Ssalddel.Application.Community.Handlers;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Services.Community;
using Ssalddel.Services.Orderer;

namespace Ssalddel.Tests.Application.Community;

public sealed class 개별원함자동집단투영EventHandlerTests
{
    [Fact]
    public async Task 투영대상원장이변경되면_개별원함투영한관심사에_그원장을위임한다()
    {
        var projection = new RecordingProjection(target: true);
        var handler = new 개별원함자동집단투영EventHandler(
            projection,
            NullLogger<개별원함자동집단투영EventHandler>.Instance);
        var ledger = new 커뮤니티원장Dto
        {
            원장Id = "individual-demand-ledger-1",
            Revision = 3
        };
        var notification = new 커뮤니티원장변경됨Event(
            ledger,
            커뮤니티원장변경유형.저장,
            "orderer-1",
            null,
            DateTime.UtcNow,
            "event-1");

        await handler.Handle(notification, CancellationToken.None);

        Assert.Equal(1, projection.TargetCheckCount);
        Assert.Equal(1, projection.ProjectionCount);
        Assert.Same(ledger, projection.ProjectedLedger);
    }

    [Fact]
    public async Task 투영대상이아닌원장변경은_투영Service를호출하지않는다()
    {
        var projection = new RecordingProjection(target: false);
        var handler = new 개별원함자동집단투영EventHandler(
            projection,
            NullLogger<개별원함자동집단투영EventHandler>.Instance);

        await handler.Handle(
            new 커뮤니티원장변경됨Event(
                new 커뮤니티원장Dto { 원장Id = "other-ledger", Revision = 1 },
                커뮤니티원장변경유형.저장,
                "system",
                null,
                DateTime.UtcNow,
                "event-2"),
            CancellationToken.None);

        Assert.Equal(1, projection.TargetCheckCount);
        Assert.Equal(0, projection.ProjectionCount);
        Assert.Null(projection.ProjectedLedger);
    }

    private sealed class RecordingProjection(bool target)
        : I공동구매개별원함자동집단투영Service
    {
        public int TargetCheckCount { get; private set; }
        public int ProjectionCount { get; private set; }
        public 커뮤니티원장Dto? ProjectedLedger { get; private set; }

        public bool 투영대상(커뮤니티원장Dto ledger)
        {
            TargetCheckCount++;
            return target;
        }

        public Task<공동구매개별원함자동집단투영결과> 투영Async(
            커뮤니티원장Dto ledger,
            CancellationToken cancellationToken = default)
        {
            ProjectionCount++;
            ProjectedLedger = ledger;
            return Task.FromResult(
                new 공동구매개별원함자동집단투영결과(null, null));
        }
    }
}
