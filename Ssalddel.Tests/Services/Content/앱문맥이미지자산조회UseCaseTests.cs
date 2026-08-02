using Microsoft.EntityFrameworkCore;
using Ssalddel.Domain.Content;
using Ssalddel.Services.Content;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;

namespace Ssalddel.Tests.Services.Content;

public sealed class 앱문맥이미지자산조회UseCaseTests
{
    [Fact]
    public async Task 팩조회는_활성자산만_장면순으로_반환한다()
    {
        await using var db = CreateContext();
        db.앱문맥이미지자산들.AddRange(
            CreateAsset("community-shipper--scene-02", 2, true),
            CreateAsset("community-shipper--scene-01", 1, true),
            CreateAsset("community-shipper--scene-03", 3, false),
            CreateAsset("seller--scene-01", 1, true, "seller"));
        await db.SaveChangesAsync();

        var useCase = new 앱문맥이미지자산조회UseCase(db);

        var result = await useCase.팩조회Async("COMMUNITY-SHIPPER");

        Assert.Equal("community-shipper", result.AppPackId);
        Assert.Equal(2, result.Count);
        Assert.Equal([1, 2], result.Items.Select(item => item.SceneNumber));
        Assert.Equal(["/community"], result.Items[0].RouteRefs);
    }

    [Theory]
    [InlineData("../seller")]
    [InlineData("community_shipper")]
    [InlineData(" ")]
    public async Task 팩조회는_안전하지않은_팩Id를_거부한다(string packId)
    {
        await using var db = CreateContext();
        var useCase = new 앱문맥이미지자산조회UseCase(db);

        await Assert.ThrowsAsync<ArgumentException>(
            () => useCase.팩조회Async(packId));
    }

    private static 앱문맥이미지자산 CreateAsset(
        string sceneKey,
        int sceneNumber,
        bool active,
        string packId = "community-shipper")
        => new()
        {
            장면Key = sceneKey,
            앱PackId = packId,
            장면번호 = sceneNumber,
            PromptVersion = 2,
            제목 = $"장면 {sceneNumber}",
            대체Text = $"장면 {sceneNumber} 설명",
            이미지Url = $"https://cdn.example.test/{sceneKey}.jpg",
            StorageContainer = "public",
            StorageObjectName = $"{sceneKey}.jpg",
            화면비율 = "4:3",
            Sha256 = new string('a', 64),
            RouteRefsJson = "[\"/community\"]",
            활성화여부 = active
        };

    private static SsalddelContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase($"app-context-images-{Guid.NewGuid():N}")
            .Options;
        return new SsalddelContext(
            options,
            new DummyPersonalDataEncryptionService());
    }

    private sealed class DummyPersonalDataEncryptionService
        : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;
        public string? Unprotect(string? value) => value;
    }
}
