using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using 홍달.Services.Options;

namespace Hongdal.Services.External.Typecast;

public interface ITypecastClient
{
    Task<IReadOnlyList<Typecast음성응답>> 음성목록조회Async(
        Typecast음성조회필터? 필터,
        CancellationToken cancellationToken);

    Task<Typecast음성합성결과> 음성합성Async(
        Typecast음성합성요청 요청,
        CancellationToken cancellationToken);
}

public sealed record Typecast음성조회필터(
    string? 모델 = null,
    string? 성별 = null,
    string? 연령대 = null,
    string? 용도 = null,
    string? 음성유형 = null);

public sealed record Typecast음성응답(
    [property: JsonPropertyName("voice_id")] string VoiceId,
    [property: JsonPropertyName("voice_name")] string 이름,
    [property: JsonPropertyName("models")] IReadOnlyList<Typecast음성모델응답> 지원모델,
    [property: JsonPropertyName("gender")] string 성별,
    [property: JsonPropertyName("age")] string 연령대,
    [property: JsonPropertyName("use_cases")] IReadOnlyList<string> 용도,
    [property: JsonPropertyName("voice_type")] string 음성유형);

public sealed record Typecast음성모델응답(
    [property: JsonPropertyName("version")] string 버전,
    [property: JsonPropertyName("emotions")] IReadOnlyList<string> 지원감정);

public sealed class Typecast음성합성요청
{
    public string VoiceId { get; init; } = string.Empty;

    public string 텍스트 { get; init; } = string.Empty;

    public string 모델 { get; init; } = "ssfm-v30";

    public string? 언어코드 { get; init; } = "kor";

    public int 음량 { get; init; } = 100;

    public int 음높이 { get; init; }

    public decimal 속도 { get; init; } = 1m;

    public string 오디오형식 { get; init; } = "wav";
}

public sealed record Typecast음성합성결과(
    byte[] 오디오,
    string ContentType,
    string 오디오형식);

public sealed class TypecastClient : ITypecastClient
{
    private readonly HttpClient _httpClient;
    private readonly TypecastOptions _options;

    public TypecastClient(HttpClient httpClient, IOptions<TypecastOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<Typecast음성응답>> 음성목록조회Async(
        Typecast음성조회필터? 필터,
        CancellationToken cancellationToken)
    {
        EnsureConfigured();

        using var request = new HttpRequestMessage(HttpMethod.Get, BuildVoicesPath(필터));
        request.Headers.Add("X-API-KEY", _options.ApiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<List<Typecast음성응답>>(cancellationToken)
            ?? [];
    }

    public async Task<Typecast음성합성결과> 음성합성Async(
        Typecast음성합성요청 요청,
        CancellationToken cancellationToken)
    {
        EnsureConfigured();
        Validate(요청);

        var body = new
        {
            voice_id = 요청.VoiceId,
            text = 요청.텍스트,
            model = 요청.모델,
            language = string.IsNullOrWhiteSpace(요청.언어코드) ? null : 요청.언어코드,
            output = new
            {
                volume = 요청.음량,
                audio_pitch = 요청.음높이,
                audio_tempo = 요청.속도,
                audio_format = 요청.오디오형식
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.TextToSpeechPath)
        {
            Content = JsonContent.Create(body)
        };
        request.Headers.Add("X-API-KEY", _options.ApiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        return new Typecast음성합성결과(
            await response.Content.ReadAsByteArrayAsync(cancellationToken),
            response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream",
            요청.오디오형식);
    }

    private string BuildVoicesPath(Typecast음성조회필터? 필터)
    {
        if (필터 is null)
        {
            return _options.VoicesPath;
        }

        var values = new Dictionary<string, string?>
        {
            ["model"] = 필터.모델,
            ["gender"] = 필터.성별,
            ["age"] = 필터.연령대,
            ["use_cases"] = 필터.용도,
            ["voice_type"] = 필터.음성유형
        };
        var query = string.Join("&", values
            .Where(x => !string.IsNullOrWhiteSpace(x.Value))
            .Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value!)}"));

        return string.IsNullOrEmpty(query) ? _options.VoicesPath : $"{_options.VoicesPath}?{query}";
    }

    private void EnsureConfigured()
    {
        if (!_options.Enabled)
        {
            throw new InvalidOperationException("Typecast API가 비활성화되어 있습니다.");
        }

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("Typecast:ApiKey 설정이 필요합니다.");
        }
    }

    private static void Validate(Typecast음성합성요청 요청)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(요청.VoiceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(요청.텍스트);
        ArgumentException.ThrowIfNullOrWhiteSpace(요청.모델);

        if (요청.텍스트.Length > 2000)
        {
            throw new ArgumentOutOfRangeException(nameof(요청), "Typecast 음성 합성 텍스트는 2,000자 이하여야 합니다.");
        }
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var detail = await response.Content.ReadAsStringAsync(cancellationToken);
        if (detail.Length > 1000)
        {
            detail = detail[..1000];
        }

        throw new HttpRequestException(
            $"Typecast API 호출 실패: {(int)response.StatusCode} {response.ReasonPhrase}. {detail}",
            null,
            response.StatusCode);
    }
}
