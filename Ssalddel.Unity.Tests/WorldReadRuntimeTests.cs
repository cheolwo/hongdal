using Ssalddel.Unity.Application;
using Ssalddel.Unity.InterpretationContracts;

namespace Ssalddel.Tests.UnityData;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class WorldReadRuntimeTests
{
    [Fact]
    public async Task Refresh는_Data해석표현Diff를순서대로실행하고_성공후에만교체한다()
    {
        var fixture = new RuntimeFixture();

        var result = await fixture.Runtime.RefreshDataAsync(
            "request", "rule-v1", "producer", "desktop", "role:producer");

        Assert.Equal(ZoneRuntimeStateCode.Ready, result.Status.StateCode);
        Assert.Equal("data-1|rule-v1|producer|desktop", result.Presentation!.Value);
        Assert.Equal("added", result.Changes);
        Assert.Equal(1, fixture.Query.CallCount);
        Assert.Equal(1, fixture.SharedInterpreter.CallCount);
        Assert.Equal(1, fixture.PerspectiveInterpreter.CallCount);
        Assert.Equal(1, fixture.Projector.CallCount);
    }

    [Fact]
    public async Task Reinterpret는_Data를재조회하지않고_해석부터다시실행한다()
    {
        var fixture = new RuntimeFixture();
        await fixture.Runtime.RefreshDataAsync("request", "rule-v1", "producer", "desktop", "role:producer");

        var result = fixture.Runtime.ReinterpretShared("rule-v2", "producer", "desktop");

        Assert.Equal("data-1|rule-v2|producer|desktop", result.Presentation!.Value);
        Assert.Equal(1, fixture.Query.CallCount);
        Assert.Equal(2, fixture.SharedInterpreter.CallCount);
        Assert.Equal(2, fixture.PerspectiveInterpreter.CallCount);
        Assert.Equal(2, fixture.Projector.CallCount);
    }

    [Fact]
    public async Task Perspective재해석은_SharedWorld를재계산하지않고_역할목적의미만바꾼다()
    {
        var fixture = new RuntimeFixture();
        await fixture.Runtime.RefreshDataAsync("request", "rule-v1", "producer", "desktop", "role:producer");

        var result = fixture.Runtime.ReinterpretPerspective("driver", "desktop");

        Assert.Equal("data-1|rule-v1|driver|desktop", result.Presentation!.Value);
        Assert.Equal(1, fixture.Query.CallCount);
        Assert.Equal(1, fixture.SharedInterpreter.CallCount);
        Assert.Equal(2, fixture.PerspectiveInterpreter.CallCount);
        Assert.Equal(2, fixture.Projector.CallCount);
    }

    [Fact]
    public async Task Reproject는_Data와두WorldState를재계산하지않고_표현만바꾼다()
    {
        var fixture = new RuntimeFixture();
        await fixture.Runtime.RefreshDataAsync("request", "rule-v1", "producer", "desktop", "role:producer");

        var result = fixture.Runtime.Reproject("mobile");

        Assert.Equal("data-1|rule-v1|producer|mobile", result.Presentation!.Value);
        Assert.Equal(1, fixture.Query.CallCount);
        Assert.Equal(1, fixture.SharedInterpreter.CallCount);
        Assert.Equal(1, fixture.PerspectiveInterpreter.CallCount);
        Assert.Equal(2, fixture.Projector.CallCount);
    }

    [Fact]
    public async Task 같은AuthorizationScope의갱신실패는_마지막성공표현을유지한다()
    {
        var fixture = new RuntimeFixture();
        var success = await fixture.Runtime.RefreshDataAsync(
            "request", "rule-v1", "producer", "desktop", "role:producer");
        fixture.Query.Error = new TimeoutException();

        var failed = await fixture.Runtime.RefreshDataAsync(
            "request", "rule-v1", "producer", "desktop", "role:producer");

        Assert.Equal(ZoneRuntimeStateCode.RefreshError, failed.Status.StateCode);
        Assert.True(failed.Status.IsShowingLastSuccess);
        Assert.Equal("Timeout", failed.Status.SafeErrorCode);
        Assert.Same(success.Presentation, failed.Presentation);
    }

    [Fact]
    public async Task AuthorizationScope변경후실패하면_이전역할의LastSuccess를노출하지않는다()
    {
        var fixture = new RuntimeFixture();
        await fixture.Runtime.RefreshDataAsync("request", "rule-v1", "producer", "desktop", "role:producer");
        fixture.Query.Error = new InvalidOperationException("private detail");

        var failed = await fixture.Runtime.RefreshDataAsync(
            "request", "rule-v1", "driver", "desktop", "role:driver");

        Assert.Equal(ZoneRuntimeStateCode.InitialError, failed.Status.StateCode);
        Assert.False(failed.Status.IsShowingLastSuccess);
        Assert.Equal("UnexpectedError", failed.Status.SafeErrorCode);
        Assert.Null(failed.Data);
        Assert.Null(failed.SharedWorld);
        Assert.Null(failed.PerspectiveWorld);
        Assert.Null(failed.Presentation);
    }

    [Fact]
    public async Task Projector실패는_새Data와World를LastSuccess로교체하지않는다()
    {
        var fixture = new RuntimeFixture();
        var success = await fixture.Runtime.RefreshDataAsync(
            "request", "rule-v1", "producer", "desktop", "role:producer");
        fixture.Query.NextValue = "data-2";
        fixture.Projector.ShouldFail = true;

        var failed = await fixture.Runtime.RefreshDataAsync(
            "request", "rule-v2", "producer", "desktop", "role:producer");

        Assert.Equal(ZoneRuntimeStateCode.RefreshError, failed.Status.StateCode);
        Assert.Equal("data-1", failed.Data!.Value);
        Assert.Same(success.SharedWorld, failed.SharedWorld);
        Assert.Same(success.PerspectiveWorld, failed.PerspectiveWorld);
        Assert.Same(success.Presentation, failed.Presentation);
    }

    [Fact]
    public async Task 취소는오류로변환하지않고_기존성공상태로복귀한다()
    {
        var fixture = new RuntimeFixture();
        await fixture.Runtime.RefreshDataAsync("request", "rule-v1", "producer", "desktop", "role:producer");
        fixture.Query.Error = new OperationCanceledException();

        await Assert.ThrowsAsync<OperationCanceledException>(() => fixture.Runtime.RefreshDataAsync(
            "request", "rule-v1", "producer", "desktop", "role:producer"));

        Assert.Equal(ZoneRuntimeStateCode.Ready, fixture.Runtime.CurrentStatus.StateCode);
        Assert.True(fixture.Runtime.CurrentStatus.IsShowingLastSuccess);
    }

    [Fact]
    public void Selection은_World목록에서유지하고_Scope변경이나대상제거시해제한다()
    {
        var store = new SelectionStateStore();
        var selected = new WorldStableId("inventory:item-17");
        store.SetAuthorizationScope("role:manager");
        store.Select(selected);

        Assert.True(store.RetainIfPresent(new[] { selected }));
        Assert.False(store.RetainIfPresent(new[] { new WorldStableId("inventory:item-18") }));
        Assert.Null(store.SelectedWorldId);

        store.Select(new WorldStableId("inventory:item-18"));
        store.SetAuthorizationScope("role:observer");
        Assert.Null(store.SelectedWorldId);
    }

    private sealed class RuntimeFixture
    {
        public RuntimeFixture()
        {
            Query = new FakeQuery();
            SharedInterpreter = new FakeSharedInterpreter();
            PerspectiveInterpreter = new FakePerspectiveInterpreter();
            Projector = new FakeProjector();
            Runtime = new WorldReadRuntime<string, DataValue, string, SharedWorldValue, string, PerspectiveWorldValue, string, PresentationValue, string>(
                Query, SharedInterpreter, PerspectiveInterpreter, Projector, new FakeChangeSetCalculator());
        }

        public FakeQuery Query { get; }
        public FakeSharedInterpreter SharedInterpreter { get; }
        public FakePerspectiveInterpreter PerspectiveInterpreter { get; }
        public FakeProjector Projector { get; }
        public WorldReadRuntime<string, DataValue, string, SharedWorldValue, string, PerspectiveWorldValue, string, PresentationValue, string> Runtime { get; }
    }

    private sealed class DataValue
    {
        public DataValue(string value) => Value = value;
        public string Value { get; }
    }

    private sealed class SharedWorldValue
    {
        public SharedWorldValue(string value) => Value = value;
        public string Value { get; }
    }

    private sealed class PerspectiveWorldValue
    {
        public PerspectiveWorldValue(string value) => Value = value;
        public string Value { get; }
    }

    private sealed class PresentationValue
    {
        public PresentationValue(string value) => Value = value;
        public string Value { get; }
    }

    private sealed class FakeQuery : IWorldDataQuery<string, DataValue>
    {
        public int CallCount { get; private set; }
        public string NextValue { get; set; } = "data-1";
        public Exception? Error { get; set; }

        public Task<DataValue> QueryAsync(string query, CancellationToken cancellationToken = default)
        {
            CallCount++;
            if (Error != null) return Task.FromException<DataValue>(Error);
            return Task.FromResult(new DataValue(NextValue));
        }
    }

    private sealed class FakeSharedInterpreter : ISharedWorldInterpreter<DataValue, string, SharedWorldValue>
    {
        public int CallCount { get; private set; }
        public SharedWorldValue Interpret(DataValue data, string context)
        {
            CallCount++;
            return new SharedWorldValue(data.Value + "|" + context);
        }
    }

    private sealed class FakePerspectiveInterpreter : IPerspectiveInterpreter<SharedWorldValue, string, PerspectiveWorldValue>
    {
        public int CallCount { get; private set; }
        public PerspectiveWorldValue Interpret(SharedWorldValue world, string context)
        {
            CallCount++;
            return new PerspectiveWorldValue(world.Value + "|" + context);
        }
    }

    private sealed class FakeProjector : IPresentationProjector<PerspectiveWorldValue, string, PresentationValue>
    {
        public int CallCount { get; private set; }
        public bool ShouldFail { get; set; }
        public PresentationValue Project(PerspectiveWorldValue world, string context)
        {
            CallCount++;
            if (ShouldFail) throw new InvalidOperationException("projection failed");
            return new PresentationValue(world.Value + "|" + context);
        }
    }

    private sealed class FakeChangeSetCalculator : IPresentationChangeSetCalculator<PresentationValue, string>
    {
        public string Calculate(PresentationValue? current, PresentationValue incoming)
            => current == null ? "added" : "updated";
    }
}
