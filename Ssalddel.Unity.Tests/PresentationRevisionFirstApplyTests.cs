using Ssalddel.Unity.PresentationContracts.Reconciliation;

namespace Ssalddel.Unity.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "공통 상태 비교기의 최초 판본 누락과 실패 조기중단을 회귀 검증한다.",
    Boundary = "순수 코드 회귀이며 실제 표현 조립·해제·Game View 증거가 아니다.")]
public sealed class PresentationRevisionFirstApplyTests
{
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void 최초추가에서도_표현판본누락을_거부한다(string? revision)
    {
        var reconciler = Create();
        var error = Assert.Throws<StableIdReconciliationException>(() =>
            reconciler.Reconcile(Array.Empty<Item>(), new[] { new Item("object:first", revision) }));
        Assert.Equal("PresentationRevisionMissing", error.ErrorCode);
        Assert.Equal("incoming", error.CollectionName);
        Assert.Equal("object:first", error.StableId);
    }

    [Fact]
    public void 첫항목이_잘못되면_뒤항목_판본조회도_실행하지않는다()
    {
        var readCount = 0;
        var reconciler = new StableIdReconciler<Item>(new StableIdReconciliationPolicy<Item>(
            x => x.Id, presentationRevision: x => { readCount++; return x.Revision!; }));
        Assert.Throws<StableIdReconciliationException>(() => reconciler.Reconcile(
            Array.Empty<Item>(), new[] { new Item("object:first", ""), new Item("object:second", "valid") }));
        Assert.Equal(1, readCount);
    }

    [Fact]
    public void 삭제되는_기존항목도_판본누락을_거부한다()
    {
        var error = Assert.Throws<StableIdReconciliationException>(() => Create().Reconcile(
            new[] { new Item("object:old", "") }, Array.Empty<Item>()));
        Assert.Equal("current", error.CollectionName);
    }

    [Fact]
    public void 판본대신_기존동등성함수를_쓰는소비자는_유지한다()
    {
        var item = new Item("object:one", null);
        var reconciler = new StableIdReconciler<Item>(new StableIdReconciliationPolicy<Item>(
            x => x.Id, presentationEquivalent: (a, b) => a.Id == b.Id));
        Assert.Same(item, Assert.Single(reconciler.Reconcile(new[] { item }, new[] { item }).Unchanged));
    }

    private static StableIdReconciler<Item> Create() => new(new StableIdReconciliationPolicy<Item>(
        x => x.Id, presentationRevision: x => x.Revision!));
    private sealed record Item(string Id, string? Revision);
}
