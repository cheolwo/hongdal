using System.Text.Json;
using System.Text.RegularExpressions;
using 살뜰.Services.External.Gemini;

namespace 살뜰.Services.Images;

public static class 앱문맥이미지BatchPromptPack상태Codes
{
    public const string ResearchDraft = "ResearchDraft";
    public const string ApprovedForBatch = "ApprovedForBatch";
}

public sealed record 앱문맥이미지Batch장면Prompt(
    int Sequence,
    string Code,
    string TitleKo,
    string PromptKo,
    string AspectRatio,
    string Resolution,
    IReadOnlyList<string> RouteRefs);

public sealed record 앱문맥이미지BatchPromptPack(
    int SchemaVersion,
    string PackId,
    string Status,
    int PromptVersion,
    string Model,
    int ExpectedSceneCount,
    int? SceneNumberStart,
    string BasePromptKo,
    IReadOnlyList<string> ContextChecklist,
    IReadOnlyList<string> AvoidExpressions,
    IReadOnlyList<앱문맥이미지Batch장면Prompt> Scenes);

public sealed record 앱문맥이미지BatchPlan(
    string PackId,
    string Status,
    int PromptVersion,
    string Model,
    IReadOnlyList<AppContextImageBatchRequestItem> Items);

public static partial class 앱문맥이미지BatchPromptPackCompiler
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

    public static 앱문맥이미지BatchPromptPack Parse(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        var pack = JsonSerializer.Deserialize<앱문맥이미지BatchPromptPack>(
            json,
            JsonOptions)
            ?? throw new InvalidOperationException(
                "앱 문맥 이미지 Batch 프롬프트 팩을 읽을 수 없습니다.");
        Validate(pack);
        return pack;
    }

    public static 앱문맥이미지BatchPlan CompilePreview(
        앱문맥이미지BatchPromptPack pack)
    {
        ArgumentNullException.ThrowIfNull(pack);
        Validate(pack);
        return Compile(pack);
    }

    public static 앱문맥이미지BatchPlan CompileApproved(
        앱문맥이미지BatchPromptPack pack)
    {
        ArgumentNullException.ThrowIfNull(pack);
        Validate(pack);
        if (!string.Equals(
                pack.Status,
                앱문맥이미지BatchPromptPack상태Codes.ApprovedForBatch,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "앱 문맥 이미지 프롬프트 팩이 ApprovedForBatch 상태가 아니므로 외부 Batch 요청을 만들지 않습니다.");
        }

        return Compile(pack);
    }

    private static 앱문맥이미지BatchPlan Compile(
        앱문맥이미지BatchPromptPack pack)
        => new(
            pack.PackId,
            pack.Status,
            pack.PromptVersion,
            pack.Model,
            pack.Scenes
                .OrderBy(scene => scene.Sequence)
                .Select(scene => new AppContextImageBatchRequestItem(
                    $"{pack.PackId}--scene-{scene.Sequence:00}",
                    BuildPrompt(pack, scene),
                    scene.AspectRatio,
                    scene.Resolution))
                .ToArray());

    private static string BuildPrompt(
        앱문맥이미지BatchPromptPack pack,
        앱문맥이미지Batch장면Prompt scene)
        => $"""
            {pack.BasePromptKo.Trim()}

            {scene.PromptKo.Trim()}

            피해야 할 표현:
            - {string.Join("\n- ", pack.AvoidExpressions.Select(item => item.Trim()))}

            화면 안에 읽을 수 있는 문자, 가격, 숫자, QR 코드, 로고, 관인, 인증서, 주소, 전화번호, 차량번호판과 식별 가능한 실존 인물을 넣지 않는다.
            생성 이미지는 앱 문맥을 돕는 AI 표현물이며 실제 거래, 배송, 계약, 검수, 통관, 가격 또는 공공기관 보증의 증거가 아니다.
            """;

    private static void Validate(
        앱문맥이미지BatchPromptPack pack)
    {
        if (pack.SchemaVersion != 1)
        {
            throw new InvalidOperationException(
                "지원하지 않는 앱 문맥 이미지 Batch schemaVersion입니다.");
        }

        Require(pack.PackId, nameof(pack.PackId));
        Require(pack.Status, nameof(pack.Status));
        Require(pack.Model, nameof(pack.Model));
        Require(pack.BasePromptKo, nameof(pack.BasePromptKo));
        if (!PackIdRegex().IsMatch(pack.PackId))
        {
            throw new InvalidOperationException(
                "packId는 영문 소문자, 숫자와 하이픈만 사용해야 합니다.");
        }

        if (pack.Status is not (
                앱문맥이미지BatchPromptPack상태Codes.ResearchDraft
                or 앱문맥이미지BatchPromptPack상태Codes.ApprovedForBatch))
        {
            throw new InvalidOperationException(
                "프롬프트 팩 상태는 ResearchDraft 또는 ApprovedForBatch여야 합니다.");
        }

        if (pack.PromptVersion < 1)
        {
            throw new InvalidOperationException(
                "promptVersion은 1 이상이어야 합니다.");
        }

        if (pack.ExpectedSceneCount is < 1 or > 50)
        {
            throw new InvalidOperationException(
                "expectedSceneCount는 1 이상 50 이하여야 합니다.");
        }

        if (pack.ContextChecklist.Count == 0
            || pack.ContextChecklist.Any(string.IsNullOrWhiteSpace))
        {
            throw new InvalidOperationException(
                "앱 화면과 업무 문맥 검토 목록이 필요합니다.");
        }

        if (pack.AvoidExpressions.Count == 0
            || pack.AvoidExpressions.Any(string.IsNullOrWhiteSpace))
        {
            throw new InvalidOperationException(
                "오표현과 안전 위험을 막는 표현 목록이 필요합니다.");
        }

        var sceneNumberStart = pack.SceneNumberStart ?? 1;
        if (sceneNumberStart < 1
            || sceneNumberStart + pack.ExpectedSceneCount - 1 > 50)
        {
            throw new InvalidOperationException(
                "sceneNumberStart부터 expectedSceneCount까지의 장면 번호는 1~50 안에 있어야 합니다.");
        }

        var expectedSequences = Enumerable.Range(
            sceneNumberStart,
            pack.ExpectedSceneCount);
        if (pack.Scenes.Count != pack.ExpectedSceneCount
            || !pack.Scenes.Select(scene => scene.Sequence)
                .Order()
                .SequenceEqual(expectedSequences))
        {
            throw new InvalidOperationException(
                "장면 sequence는 sceneNumberStart부터 expectedSceneCount만큼 정확히 한 번씩 포함해야 합니다.");
        }

        if (pack.Scenes.Select(scene => scene.Code)
                .Distinct(StringComparer.Ordinal)
                .Count()
            != pack.Scenes.Count)
        {
            throw new InvalidOperationException(
                "장면 code는 팩 안에서 중복될 수 없습니다.");
        }

        foreach (var scene in pack.Scenes)
        {
            Require(scene.Code, nameof(scene.Code));
            Require(scene.TitleKo, nameof(scene.TitleKo));
            Require(scene.PromptKo, nameof(scene.PromptKo));
            Require(scene.AspectRatio, nameof(scene.AspectRatio));
            Require(scene.Resolution, nameof(scene.Resolution));
            if (scene.PromptKo.Trim().Length < 40)
            {
                throw new InvalidOperationException(
                    "각 장면 promptKo는 40자 이상의 구체적인 문장이어야 합니다.");
            }

            if (scene.RouteRefs.Count == 0
                || scene.RouteRefs.Any(string.IsNullOrWhiteSpace))
            {
                throw new InvalidOperationException(
                    "각 장면에는 사용 화면 route 또는 component 참조가 필요합니다.");
            }
        }
    }

    private static void Require(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"{name}이(가) 필요합니다.");
        }
    }

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex PackIdRegex();
}
