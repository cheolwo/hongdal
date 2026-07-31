using System.Text.Json;

namespace 살뜰.Services.Images;

public static class 지역문화이미지BatchPromptPack상태Codes
{
    public const string ResearchDraft = "ResearchDraft";
    public const string ApprovedForBatch = "ApprovedForBatch";
}

public sealed record 지역문화이미지Batch장면Prompt(
    int Sequence,
    string Code,
    string TitleKo,
    string PromptKo);

public sealed record 지역문화이미지BatchPromptPack(
    int SchemaVersion,
    string PackId,
    string Status,
    string CountryCode,
    string RegionKey,
    string RegionNameKo,
    int PromptVersion,
    string Model,
    string AspectRatio,
    string Resolution,
    string BasePromptKo,
    IReadOnlyList<string> EvidenceChecklist,
    IReadOnlyList<string> AvoidExpressions,
    IReadOnlyList<지역문화이미지Batch장면Prompt> Scenes);

public sealed record 지역문화이미지Batch요청항목(
    string Key,
    string Prompt,
    string Model,
    string AspectRatio,
    string Resolution);

public static class 지역문화이미지BatchPromptPackCompiler
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

    public static 지역문화이미지BatchPromptPack Parse(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        var pack = JsonSerializer.Deserialize<지역문화이미지BatchPromptPack>(
            json,
            JsonOptions)
            ?? throw new InvalidOperationException(
                "지역문화 이미지 Batch 프롬프트 팩을 읽을 수 없습니다.");
        Validate(pack);
        return pack;
    }

    public static IReadOnlyList<지역문화이미지Batch요청항목> CompileApproved(
        지역문화이미지BatchPromptPack pack)
    {
        ArgumentNullException.ThrowIfNull(pack);
        Validate(pack);
        if (!string.Equals(
                pack.Status,
                지역문화이미지BatchPromptPack상태Codes.ApprovedForBatch,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "로컬 프롬프트 팩이 ApprovedForBatch 상태가 아니므로 외부 Batch 요청을 만들지 않습니다.");
        }

        return pack.Scenes
            .OrderBy(scene => scene.Sequence)
            .Select(scene => new 지역문화이미지Batch요청항목(
                $"{pack.RegionKey}--scene-{scene.Sequence:00}",
                BuildPrompt(pack, scene),
                pack.Model,
                pack.AspectRatio,
                pack.Resolution))
            .ToArray();
    }

    private static string BuildPrompt(
        지역문화이미지BatchPromptPack pack,
        지역문화이미지Batch장면Prompt scene)
        => $"""
            {pack.BasePromptKo.Trim()}

            지역: {pack.RegionNameKo}
            장면 {scene.Sequence:00}/10 · {scene.TitleKo}
            {scene.PromptKo.Trim()}

            피해야 할 표현:
            - {string.Join("\n- ", pack.AvoidExpressions.Select(item => item.Trim()))}

            화면 안에 문자, 가격, 로고, 국기, 지도, 정치 상징, 관인, 워터마크와 식별 가능한 실존 인물을 넣지 않는다.
            생성 이미지는 문화적 이해를 돕는 표현물이며 역사적 사실, 상품 원산지 또는 공공기관 보증의 증거가 아니다.
            """;

    private static void Validate(지역문화이미지BatchPromptPack pack)
    {
        if (pack.SchemaVersion != 1)
        {
            throw new InvalidOperationException(
                "지원하지 않는 지역문화 이미지 Batch 프롬프트 팩 schemaVersion입니다.");
        }

        Require(pack.PackId, nameof(pack.PackId));
        Require(pack.CountryCode, nameof(pack.CountryCode));
        Require(pack.RegionKey, nameof(pack.RegionKey));
        Require(pack.RegionNameKo, nameof(pack.RegionNameKo));
        Require(pack.Model, nameof(pack.Model));
        Require(pack.BasePromptKo, nameof(pack.BasePromptKo));
        if (pack.PromptVersion < 1)
        {
            throw new InvalidOperationException("promptVersion은 1 이상이어야 합니다.");
        }

        if (pack.EvidenceChecklist.Count == 0)
        {
            throw new InvalidOperationException(
                "공식 근거 검토 목록이 없는 프롬프트 팩은 사용할 수 없습니다.");
        }

        if (pack.AvoidExpressions.Count == 0)
        {
            throw new InvalidOperationException(
                "고정관념 방지 표현이 없는 프롬프트 팩은 사용할 수 없습니다.");
        }

        if (pack.Scenes.Count != 10
            || pack.Scenes.Select(scene => scene.Sequence)
                .Order()
                .SequenceEqual(Enumerable.Range(1, 10)) is false)
        {
            throw new InvalidOperationException(
                "지역문화 이미지 Batch 프롬프트 팩은 장면 01~10을 정확히 한 번씩 포함해야 합니다.");
        }

        if (pack.Scenes.Any(scene =>
                string.IsNullOrWhiteSpace(scene.Code)
                || string.IsNullOrWhiteSpace(scene.TitleKo)
                || scene.PromptKo.Trim().Length < 40))
        {
            throw new InvalidOperationException(
                "각 장면은 code, titleKo와 40자 이상의 구체적인 promptKo가 필요합니다.");
        }
    }

    private static void Require(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"{fieldName} 값이 필요합니다.");
        }
    }
}
