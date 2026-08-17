using Ssalddel.Unity.Application;

namespace Ssalddel.Unity.Tests;

public sealed class LastSuccessfulLoadRuntimeTests
{
    [Fact]
    public async Task 최초실패는_초기오류이고_상태사본이없다()
    {
        var runtime = new LastSuccessfulLoadRuntime<string, int>();

        var result = await runtime.LoadAsync(
            _ => Task.FromException<string>(new InvalidOperationException("offline")),
            (_, _) => 0);

        Assert.Equal(ZoneRuntimeStateCode.InitialError, result.StateCode);
        Assert.Null(result.Snapshot);
        Assert.IsType<InvalidOperationException>(result.Error);
    }

    [Fact]
    public async Task 새로고침실패는_마지막성공상태를유지한다()
    {
        var runtime = new LastSuccessfulLoadRuntime<string, int>();
        var first = await runtime.LoadAsync(
            _ => Task.FromResult("revision-1"),
            (_, _) => 1);

        var failed = await runtime.LoadAsync(
            _ => Task.FromException<string>(new InvalidOperationException("offline")),
            (_, _) => 2);

        Assert.Equal(ZoneRuntimeStateCode.Ready, first.StateCode);
        Assert.Equal(ZoneRuntimeStateCode.RefreshError, failed.StateCode);
        Assert.Equal("revision-1", failed.Snapshot);
        Assert.Equal(0, failed.Changes);
    }

    [Fact]
    public async Task 취소는_오류상태로바꾸지않고_호출자에게전파한다()
    {
        var runtime = new LastSuccessfulLoadRuntime<string, int>();
        using var source = new CancellationTokenSource();
        source.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            runtime.LoadAsync(
                _ => Task.FromCanceled<string>(source.Token),
                (_, _) => 0,
                source.Token));

        Assert.Equal(ZoneRuntimeStateCode.InitialLoading, runtime.StateCode);
    }
}
