using System.Text.Json;
using 살뜰.Services.Images;

namespace Ssalddel.Tests.Services.Images;

public sealed class RegionalCultureImageBatchPromptPackTests
{
    [Fact]
    public async Task 서울로컬Pack은_장면10개를가진검토초안이다()
    {
        var repositoryRoot = FindRepositoryRoot();
        var json = await File.ReadAllTextAsync(Path.Combine(
            repositoryRoot,
            "docs",
            "Content",
            "RegionalCultureImagePrompts",
            "packs",
            "kr-seoul.v1.json"));

        var pack = 지역문화이미지BatchPromptPackCompiler.Parse(json);

        Assert.Equal("kr-seoul-v1", pack.PackId);
        Assert.Equal(지역문화이미지BatchPromptPack상태Codes.ResearchDraft, pack.Status);
        Assert.Equal(10, pack.Scenes.Count);
        Assert.Throws<InvalidOperationException>(
            () => 지역문화이미지BatchPromptPackCompiler.CompileApproved(pack));
    }

    [Fact]
    public void ResearchDraft는_Batch요청으로컴파일하지않는다()
    {
        var pack = 지역문화이미지BatchPromptPackCompiler.Parse(
            BuildPackJson(지역문화이미지BatchPromptPack상태Codes.ResearchDraft));

        var exception = Assert.Throws<InvalidOperationException>(
            () => 지역문화이미지BatchPromptPackCompiler.CompileApproved(pack));

        Assert.Contains("ApprovedForBatch", exception.Message);
    }

    [Fact]
    public void 승인된Pack은_장면10개를안정된Key와완성Prompt로컴파일한다()
    {
        var pack = 지역문화이미지BatchPromptPackCompiler.Parse(
            BuildPackJson(지역문화이미지BatchPromptPack상태Codes.ApprovedForBatch));

        var items =
            지역문화이미지BatchPromptPackCompiler.CompileApproved(pack);

        Assert.Equal(10, items.Count);
        Assert.Equal("seoul--scene-01", items[0].Key);
        Assert.Equal("seoul--scene-10", items[9].Key);
        Assert.All(items, item =>
        {
            Assert.Equal("gemini-3-pro-image-preview", item.Model);
            Assert.Equal("16:9", item.AspectRatio);
            Assert.Equal("1K", item.Resolution);
            Assert.Contains("서울특별시", item.Prompt);
            Assert.Contains("문화적 이해를 돕는 표현물", item.Prompt);
        });
    }

    [Fact]
    public void 누락된장면이있는Pack은_파싱단계에서거부한다()
    {
        using var document = JsonDocument.Parse(
            BuildPackJson(지역문화이미지BatchPromptPack상태Codes.ResearchDraft));
        var source = document.RootElement;
        var invalid = JsonSerializer.Serialize(new
        {
            schemaVersion = 1,
            packId = "kr-seoul-v1",
            status = "ResearchDraft",
            countryCode = "KR",
            regionKey = "seoul",
            regionNameKo = "서울특별시",
            promptVersion = 1,
            model = "gemini-3-pro-image-preview",
            aspectRatio = "16:9",
            resolution = "1K",
            basePromptKo = "충분히 구체적인 공통 스타일 프롬프트입니다.",
            evidenceChecklist = new[] { "공식 자료 확인" },
            avoidExpressions = new[] { "고정관념" },
            scenes = source.GetProperty("scenes")
                .EnumerateArray()
                .Take(9)
                .Select(item => item.Clone())
                .ToArray()
        });

        Assert.Throws<InvalidOperationException>(
            () => 지역문화이미지BatchPromptPackCompiler.Parse(invalid));
    }

    private static string BuildPackJson(string status)
        => JsonSerializer.Serialize(new
        {
            schemaVersion = 1,
            packId = "kr-seoul-v1",
            status,
            countryCode = "KR",
            regionKey = "seoul",
            regionNameKo = "서울특별시",
            promptVersion = 1,
            model = "gemini-3-pro-image-preview",
            aspectRatio = "16:9",
            resolution = "1K",
            basePromptKo = "따뜻한 시네마틱 3D 애니메이션 필름 스틸로 표현합니다.",
            evidenceChecklist = new[] { "서울시 공식 문화 자료 확인" },
            avoidExpressions = new[] { "관광 엽서식 고정관념" },
            scenes = Enumerable.Range(1, 10).Select(sequence => new
            {
                sequence,
                code = $"scene-{sequence:00}",
                titleKo = $"장면 {sequence:00}",
                promptKo =
                    $"서울의 현재 생활을 보여 주는 장면 {sequence:00}입니다. 주민의 자연스러운 활동과 공간의 깊이를 구체적으로 표현합니다."
            })
        });

    private static string FindRepositoryRoot()
    {
        foreach (var start in new[]
                 {
                     Directory.GetCurrentDirectory(),
                     AppContext.BaseDirectory
                 })
        {
            var directory = new DirectoryInfo(start);
            while (directory is not null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "Ssalddel.sln")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }
        }

        throw new DirectoryNotFoundException("Hongdal 저장소 루트를 찾을 수 없습니다.");
    }
}
