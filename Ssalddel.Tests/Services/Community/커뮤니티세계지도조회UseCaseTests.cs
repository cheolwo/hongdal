using Ssalddel.Contracts.Common.Community;
using Ssalddel.Services.Community;

namespace Ssalddel.Tests.Services.Community;

public sealed class 커뮤니티세계지도조회UseCaseTests
{
    private readonly 커뮤니티세계지도조회UseCase _useCase = new();

    [Fact]
    public async Task 낮Snapshot은_문화와가격을_서로다른Layer로제공한다()
    {
        var snapshot = await _useCase.조회Async(CommunityPageRoutes.WorldMapDayWorkDataset);

        Assert.Equal(CommunityPageRoutes.WorldMapDayWorkDataset, snapshot.DatasetCode);
        Assert.Contains(snapshot.Layers, layer => layer.Code == 커뮤니티세계지도LayerCodes.RegionalCulture);
        Assert.Contains(snapshot.Layers, layer => layer.Code == 커뮤니티세계지도LayerCodes.PublicPrice);
        Assert.Contains(snapshot.Observations, item => item.StableId == "culture:us-maine");
        Assert.Contains(snapshot.Observations, item => item.StableId == "price:kr");
        Assert.All(snapshot.Observations, item => Assert.False(string.IsNullOrWhiteSpace(item.SourceName)));
    }

    [Fact]
    public async Task 밤Snapshot은_배움과경전공부Layer를_구분한다()
    {
        var snapshot = await _useCase.조회Async(CommunityPageRoutes.WorldMapNightLearningDataset);

        Assert.Contains(snapshot.Observations, item => item.LayerCode == 커뮤니티세계지도LayerCodes.LearningChannel);
        Assert.Contains(snapshot.Observations, item => item.LayerCode == 커뮤니티세계지도LayerCodes.ScriptureAndClassics);
        Assert.All(snapshot.Observations, item => Assert.StartsWith("https://", item.DetailHref, StringComparison.Ordinal));
    }

    [Fact]
    public async Task 같은공개자료는_생성시각이달라도_같은Revision을유지한다()
    {
        var first = await _useCase.조회Async(CommunityPageRoutes.WorldMapDayWorkDataset);
        var second = await _useCase.조회Async(CommunityPageRoutes.WorldMapDayWorkDataset);

        Assert.Equal(first.Revision, second.Revision);
        Assert.NotEmpty(first.Revision);
    }

    [Fact]
    public async Task 알수없는Dataset은_거부한다()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _useCase.조회Async("unknown"));

        Assert.Contains("day-work", exception.Message);
    }
}
