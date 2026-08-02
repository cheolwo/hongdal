using System.Text.Json;
using 살뜰.Services.Images;

namespace Ssalddel.Tests.Services.Images;

public sealed class AppContextImageBatchPromptPackTests
{
    [Fact]
    public async Task CatalogContainsThirteenApprovedPilotPacksAndSixtyFiveScenes()
    {
        var repositoryRoot = FindRepositoryRoot();
        var promptRoot = Path.Combine(
            repositoryRoot,
            "docs",
            "Content",
            "AppContextImagePrompts");
        using var catalog = JsonDocument.Parse(
            await File.ReadAllTextAsync(Path.Combine(
                promptRoot,
                "catalog.v1.json")));
        var root = catalog.RootElement;
        var entries = root.GetProperty("packs").EnumerateArray().ToArray();

        Assert.Equal(13, root.GetProperty("packCount").GetInt32());
        Assert.Equal(65, root.GetProperty("totalPilotScenes").GetInt32());
        Assert.Equal(13, entries.Length);
        var sceneCount = 0;
        foreach (var entry in entries)
        {
            var pack = 앱문맥이미지BatchPromptPackCompiler.Parse(
                await File.ReadAllTextAsync(Path.Combine(
                    promptRoot,
                    entry.GetProperty("path").GetString()!)));
            Assert.Equal(
                entry.GetProperty("packId").GetString(),
                pack.PackId);
            Assert.Equal(
                앱문맥이미지BatchPromptPack상태Codes.ApprovedForBatch,
                pack.Status);
            Assert.Equal(5, pack.Scenes.Count);
            sceneCount += pack.Scenes.Count;
        }

        Assert.Equal(65, sceneCount);
    }

    [Fact]
    public async Task ExpansionCatalogContainsThirteenApprovedPacksAndFiveHundredEightyFiveScenes()
    {
        var repositoryRoot = FindRepositoryRoot();
        var promptRoot = Path.Combine(
            repositoryRoot,
            "docs",
            "Content",
            "AppContextImagePrompts");
        using var catalog = JsonDocument.Parse(
            await File.ReadAllTextAsync(Path.Combine(
                promptRoot,
                "catalog.expansion.v2.json")));
        var root = catalog.RootElement;
        var entries = root.GetProperty("packs").EnumerateArray().ToArray();

        Assert.Equal(13, root.GetProperty("packCount").GetInt32());
        Assert.Equal(585, root.GetProperty("totalScenes").GetInt32());
        Assert.Equal(13, entries.Length);
        var sceneCount = 0;
        foreach (var entry in entries)
        {
            var pack = 앱문맥이미지BatchPromptPackCompiler.Parse(
                await File.ReadAllTextAsync(Path.Combine(
                    promptRoot,
                    entry.GetProperty("path").GetString()!)));
            Assert.Equal(
                앱문맥이미지BatchPromptPack상태Codes.ApprovedForBatch,
                pack.Status);
            var plan =
                앱문맥이미지BatchPromptPackCompiler.CompileApproved(pack);
            Assert.Equal(45, plan.Items.Count);
            Assert.Equal(
                $"{pack.PackId}--scene-06",
                plan.Items[0].Key);
            Assert.Equal(
                $"{pack.PackId}--scene-50",
                plan.Items[^1].Key);
            Assert.All(plan.Items, item =>
            {
                Assert.DoesNotContain("앱 팩:", item.Prompt);
                Assert.DoesNotContain("장면 06/", item.Prompt);
                Assert.Contains("읽을 수 있는 문자", item.Prompt);
            });
            sceneCount += plan.Items.Count;
        }

        Assert.Equal(585, sceneCount);
    }

    [Fact]
    public void ResearchDraft_CanPreviewButCannotCompileForSubmission()
    {
        var pack = 앱문맥이미지BatchPromptPackCompiler.Parse(
            BuildPackJson(
                앱문맥이미지BatchPromptPack상태Codes.ResearchDraft));

        var preview =
            앱문맥이미지BatchPromptPackCompiler.CompilePreview(pack);

        Assert.Equal(5, preview.Items.Count);
        var exception = Assert.Throws<InvalidOperationException>(() =>
            앱문맥이미지BatchPromptPackCompiler.CompileApproved(pack));
        Assert.Contains("ApprovedForBatch", exception.Message);
    }

    [Fact]
    public void ApprovedPack_CompilesStableKeysAndSafetyPrompt()
    {
        var pack = 앱문맥이미지BatchPromptPackCompiler.Parse(
            BuildPackJson(
                앱문맥이미지BatchPromptPack상태Codes.ApprovedForBatch));

        var plan =
            앱문맥이미지BatchPromptPackCompiler.CompileApproved(pack);

        Assert.Equal("community-shipper", plan.PackId);
        Assert.Equal("gemini-3.1-flash-lite-image", plan.Model);
        Assert.Equal("community-shipper--scene-01", plan.Items[0].Key);
        Assert.Equal("community-shipper--scene-05", plan.Items[4].Key);
        Assert.All(plan.Items, item =>
        {
            Assert.Equal("1K", item.Resolution);
            Assert.Contains("AI 표현물", item.Prompt);
            Assert.Contains("실제 거래", item.Prompt);
            Assert.Contains("식별 가능한 실존 인물", item.Prompt);
        });
    }

    [Fact]
    public void ExpansionPack_CompilesSceneNumbersSixThroughFifty()
    {
        var json = JsonSerializer.Serialize(new
        {
            schemaVersion = 1,
            packId = "community-shipper",
            status = 앱문맥이미지BatchPromptPack상태Codes.ApprovedForBatch,
            promptVersion = 2,
            model = "gemini-3.1-flash-lite-image",
            expectedSceneCount = 45,
            sceneNumberStart = 6,
            basePromptKo = "문자가 없는 일관된 커뮤니티 편집 이미지로 표현합니다.",
            contextChecklist = new[] { "사용 route 확인" },
            avoidExpressions = new[] { "읽을 수 있는 문자와 숫자" },
            scenes = Enumerable.Range(6, 45).Select(sequence => new
            {
                sequence,
                code = $"expansion-{sequence:00}",
                titleKo = $"확장 장면 {sequence:00}",
                promptKo = $"세계 여러 지역의 농수산물 정보를 함께 이해하는 커뮤니티 장면 {sequence:00}을 문자 없이 생활감 있는 구도로 구체적으로 표현합니다.",
                aspectRatio = "16:9",
                resolution = "1K",
                routeRefs = new[] { "/community/regions" }
            })
        });

        var pack = 앱문맥이미지BatchPromptPackCompiler.Parse(json);
        var plan = 앱문맥이미지BatchPromptPackCompiler.CompileApproved(pack);

        Assert.Equal(45, plan.Items.Count);
        Assert.Equal("community-shipper--scene-06", plan.Items[0].Key);
        Assert.Equal("community-shipper--scene-50", plan.Items[^1].Key);
        Assert.DoesNotContain("장면 06/45", plan.Items[0].Prompt);
    }

    [Fact]
    public void MissingRouteReference_IsRejected()
    {
        using var document = JsonDocument.Parse(BuildPackJson(
            앱문맥이미지BatchPromptPack상태Codes.ResearchDraft));
        var source = document.RootElement;
        var scenes = source.GetProperty("scenes")
            .EnumerateArray()
            .Select((scene, index) => new
            {
                sequence = scene.GetProperty("sequence").GetInt32(),
                code = scene.GetProperty("code").GetString(),
                titleKo = scene.GetProperty("titleKo").GetString(),
                promptKo = scene.GetProperty("promptKo").GetString(),
                aspectRatio = scene.GetProperty("aspectRatio").GetString(),
                resolution = scene.GetProperty("resolution").GetString(),
                routeRefs = index == 0
                    ? Array.Empty<string>()
                    : ["/community/regions"]
            })
            .ToArray();
        var invalid = JsonSerializer.Serialize(new
        {
            schemaVersion = 1,
            packId = "community-shipper",
            status = "ResearchDraft",
            promptVersion = 1,
            model = "gemini-3.1-flash-lite-image",
            expectedSceneCount = 5,
            basePromptKo = "앱 화면에 쓰이는 일관된 문화 편집 이미지로 자연스럽게 표현합니다.",
            contextChecklist = new[] { "사용 route 확인" },
            avoidExpressions = new[] { "과장된 문화적 고정관념" },
            scenes
        });

        Assert.Throws<InvalidOperationException>(() =>
            앱문맥이미지BatchPromptPackCompiler.Parse(invalid));
    }

    private static string BuildPackJson(string status)
        => JsonSerializer.Serialize(new
        {
            schemaVersion = 1,
            packId = "community-shipper",
            status,
            promptVersion = 1,
            model = "gemini-3.1-flash-lite-image",
            expectedSceneCount = 5,
            basePromptKo = "앱 화면에 쓰이는 일관된 문화 편집 이미지로 자연스럽게 표현합니다.",
            contextChecklist = new[]
            {
                "사용 route와 카드 crop 확인",
                "공공데이터 증거로 오인하지 않는지 확인"
            },
            avoidExpressions = new[]
            {
                "과장된 문화적 고정관념",
                "실제 거래 증빙처럼 보이는 문서"
            },
            scenes = Enumerable.Range(1, 5).Select(sequence => new
            {
                sequence,
                code = $"pilot-{sequence:00}",
                titleKo = $"파일럿 장면 {sequence:00}",
                promptKo = $"세계의 서로 다른 지역 문화와 농수산물을 배우는 커뮤니티 상황 {sequence:00}을 생활감 있고 안전한 구도로 자세히 표현합니다.",
                aspectRatio = sequence == 5 ? "3:4" : "16:9",
                resolution = "1K",
                routeRefs = new[] { "/community/regions" }
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

        throw new DirectoryNotFoundException(
            "Hongdal 저장소 루트를 찾을 수 없습니다.");
    }
}
