using System.Text.Json;
using Hongdal.Contracts.Common.Speech;
using Hongdal.Domain.Speech;
using Hongdal.Services.External.Typecast;
using Microsoft.Extensions.Options;
using 홍달.Services.Options;

namespace Hongdal.Services.Speech;

public interface ITypecast음성카탈로그Service
{
    Task<Typecast음성카탈로그동기화결과Dto> 동기화Async(CancellationToken cancellationToken);

    Task<IReadOnlyList<Typecast음성캐릭터Dto>> 목록조회Async(
        Typecast음성카탈로그검색조건 조건,
        CancellationToken cancellationToken);

    Task<Typecast음성캐릭터Dto?> 단건조회Async(string voiceId, CancellationToken cancellationToken);
}

public sealed class Typecast음성카탈로그Service : ITypecast음성카탈로그Service
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly ITypecastClient _client;
    private readonly ITypecast음성카탈로그저장소 _저장소;
    private readonly TypecastOptions _options;

    public Typecast음성카탈로그Service(
        ITypecastClient client,
        ITypecast음성카탈로그저장소 저장소,
        IOptions<TypecastOptions> options)
    {
        _client = client;
        _저장소 = 저장소;
        _options = options.Value;
    }

    public async Task<Typecast음성카탈로그동기화결과Dto> 동기화Async(CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            return new Typecast음성카탈로그동기화결과Dto(
                false, 0, 0, 0, 0, null, "Typecast API가 비활성화되어 있습니다.");
        }

        var 원격목록 = (await _client.음성목록조회Async(null, cancellationToken))
            .Where(x => !string.IsNullOrWhiteSpace(x.VoiceId))
            .GroupBy(x => x.VoiceId, StringComparer.Ordinal)
            .Select(x => x.Last())
            .ToArray();

        if (원격목록.Length == 0)
        {
            throw new InvalidOperationException(
                "Typecast 음성 목록이 비어 있어 기존 카탈로그를 변경하지 않았습니다.");
        }

        var now = DateTime.UtcNow;
        var 기존목록 = await _저장소.전체추적조회Async(cancellationToken);
        var 기존Map = 기존목록.ToDictionary(x => x.VoiceId, StringComparer.Ordinal);
        var 수신VoiceIds = 원격목록.Select(x => x.VoiceId).ToHashSet(StringComparer.Ordinal);
        var 추가수 = 0;
        var 수정수 = 0;

        foreach (var 원격 in 원격목록)
        {
            if (!기존Map.TryGetValue(원격.VoiceId, out var 음성))
            {
                음성 = new Typecast음성
                {
                    VoiceId = 원격.VoiceId,
                    생성일시Utc = now
                };
                _저장소.추가(음성);
                추가수++;
            }
            else if (HasChanged(음성, 원격))
            {
                수정수++;
            }

            Apply(음성, 원격, now);
        }

        var 비활성화수 = 0;
        foreach (var 기존 in 기존목록.Where(x => x.활성화여부 && !수신VoiceIds.Contains(x.VoiceId)))
        {
            기존.활성화여부 = false;
            기존.마지막동기화일시Utc = now;
            기존.수정일시Utc = now;
            비활성화수++;
        }

        await _저장소.저장Async(cancellationToken);

        return new Typecast음성카탈로그동기화결과Dto(
            true,
            원격목록.Length,
            추가수,
            수정수,
            비활성화수,
            now,
            "Typecast 음성 캐릭터 카탈로그를 동기화했습니다.");
    }

    public async Task<IReadOnlyList<Typecast음성캐릭터Dto>> 목록조회Async(
        Typecast음성카탈로그검색조건 조건,
        CancellationToken cancellationToken)
        => (await _저장소.검색Async(조건, cancellationToken)).Select(ToDto).ToArray();

    public async Task<Typecast음성캐릭터Dto?> 단건조회Async(
        string voiceId,
        CancellationToken cancellationToken)
    {
        var 음성 = await _저장소.단건조회Async(voiceId, cancellationToken);
        return 음성 is null ? null : ToDto(음성);
    }

    private static bool HasChanged(Typecast음성 현재, Typecast음성응답 원격)
    {
        if (!string.Equals(현재.이름, Normalize(원격.이름), StringComparison.Ordinal)
            || !string.Equals(현재.성별, Normalize(원격.성별), StringComparison.Ordinal)
            || !string.Equals(현재.연령대, Normalize(원격.연령대), StringComparison.Ordinal)
            || !string.Equals(현재.음성유형, Normalize(원격.음성유형), StringComparison.Ordinal)
            || !현재.활성화여부)
        {
            return true;
        }

        var 현재모델 = 현재.지원모델
            .OrderBy(x => x.버전)
            .Select(x => (x.버전, 감정: x.지원감정Json))
            .ToArray();
        var 원격모델 = NormalizeModels(원격.지원모델)
            .Select(x => (x.버전, 감정: SerializeValues(x.지원감정)))
            .ToArray();

        var 현재용도 = 현재.용도.Select(x => x.이름).OrderBy(x => x).ToArray();
        var 원격용도 = NormalizeValues(원격.용도);

        return !현재모델.SequenceEqual(원격모델) || !현재용도.SequenceEqual(원격용도);
    }

    private static void Apply(Typecast음성 대상, Typecast음성응답 원격, DateTime now)
    {
        대상.이름 = Normalize(원격.이름);
        대상.성별 = Normalize(원격.성별);
        대상.연령대 = Normalize(원격.연령대);
        대상.음성유형 = Normalize(원격.음성유형);
        대상.활성화여부 = true;
        대상.마지막동기화일시Utc = now;
        대상.수정일시Utc = now;

        var 모델Map = 대상.지원모델.ToDictionary(x => x.버전, StringComparer.OrdinalIgnoreCase);
        var 수신모델 = NormalizeModels(원격.지원모델);
        var 수신모델버전 = 수신모델.Select(x => x.버전).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var 모델 in 수신모델)
        {
            if (!모델Map.TryGetValue(모델.버전, out var 저장모델))
            {
                저장모델 = new Typecast음성모델 { 버전 = 모델.버전 };
                대상.지원모델.Add(저장모델);
            }

            저장모델.지원감정Json = SerializeValues(모델.지원감정);
        }

        foreach (var 제거대상 in 대상.지원모델.Where(x => !수신모델버전.Contains(x.버전)).ToArray())
        {
            대상.지원모델.Remove(제거대상);
        }

        var 용도Map = 대상.용도.ToDictionary(x => x.이름, StringComparer.OrdinalIgnoreCase);
        var 수신용도 = NormalizeValues(원격.용도);
        var 수신용도Set = 수신용도.ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var 용도 in 수신용도.Where(x => !용도Map.ContainsKey(x)))
        {
            대상.용도.Add(new Typecast음성용도 { 이름 = 용도 });
        }

        foreach (var 제거대상 in 대상.용도.Where(x => !수신용도Set.Contains(x.이름)).ToArray())
        {
            대상.용도.Remove(제거대상);
        }
    }

    private static Typecast음성캐릭터Dto ToDto(Typecast음성 음성)
        => new(
            음성.VoiceId,
            음성.이름,
            음성.성별,
            음성.연령대,
            음성.음성유형,
            음성.용도.Select(x => x.이름).OrderBy(x => x).ToArray(),
            음성.지원모델
                .OrderBy(x => x.버전)
                .Select(x => new Typecast음성모델Dto(x.버전, DeserializeValues(x.지원감정Json)))
                .ToArray(),
            음성.활성화여부,
            음성.마지막동기화일시Utc);

    private static IReadOnlyList<Typecast음성모델응답> NormalizeModels(
        IReadOnlyList<Typecast음성모델응답>? models)
        => (models ?? [])
            .Where(x => !string.IsNullOrWhiteSpace(x.버전))
            .GroupBy(x => Normalize(x.버전), StringComparer.OrdinalIgnoreCase)
            .Select(x => new Typecast음성모델응답(x.Key, NormalizeValues(x.SelectMany(y => y.지원감정))))
            .OrderBy(x => x.버전)
            .ToArray();

    private static string[] NormalizeValues(IEnumerable<string>? values)
        => (values ?? [])
            .Select(Normalize)
            .Where(x => x.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToArray();

    private static string SerializeValues(IEnumerable<string>? values)
        => JsonSerializer.Serialize(NormalizeValues(values), JsonOptions);

    private static IReadOnlyList<string> DeserializeValues(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<string[]>(json, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static string Normalize(string? value)
        => value?.Trim() ?? string.Empty;
}
