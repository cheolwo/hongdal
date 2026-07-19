using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace 살뜰.Services.External.Customs;

public sealed class Unipass개인통관부호검증Service : I개인통관부호검증Service
{
    private readonly HttpClient _httpClient;
    private readonly CustomsOptions _options;
    private readonly ILogger<Unipass개인통관부호검증Service> _logger;

    public Unipass개인통관부호검증Service(
        HttpClient httpClient,
        IOptions<CustomsOptions> options,
        ILogger<Unipass개인통관부호검증Service> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<개인통관부호검증Result> 검증Async(
        개인통관부호검증Request request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return new 개인통관부호검증Result
            {
                성공여부 = false,
                결과코드 = "MissingApiKey",
                메시지 = "Customs:ApiKey 설정이 필요합니다."
            };
        }

        var query = new Dictionary<string, string?>
        {
            ["crkyCn"] = _options.ApiKey,
            ["persEcm"] = request.개인통관고유부호,
            ["pltxNm"] = request.이름,
            ["cralTelno"] = request.휴대폰번호,
            ["zip"] = request.우편번호
        };

        var path = _options.PersonalCodeValidationPath.TrimStart('/');
        var relative = QueryHelpers.AddQueryString(path, query);

        try
        {
            using var response = await _httpClient.GetAsync(relative, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return new 개인통관부호검증Result
                {
                    성공여부 = false,
                    결과코드 = ((int)response.StatusCode).ToString(),
                    메시지 = "개인통관고유부호 검증 API 호출에 실패했습니다."
                };
            }

            return ParseResult(body);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "개인통관부호 검증 중 예외가 발생했습니다.");
            return new 개인통관부호검증Result
            {
                성공여부 = false,
                결과코드 = "Exception",
                메시지 = "개인통관고유부호 검증 중 오류가 발생했습니다."
            };
        }
    }

    private static 개인통관부호검증Result ParseResult(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return new 개인통관부호검증Result
            {
                성공여부 = false,
                결과코드 = "EmptyBody",
                메시지 = "응답 본문이 비어 있습니다."
            };
        }

        try
        {
            using var json = JsonDocument.Parse(body);
            var root = json.RootElement;
            var success = root.TryGetProperty("success", out var successProp) && successProp.GetBoolean();
            var code = root.TryGetProperty("code", out var codeProp) ? codeProp.ToString() : (success ? "OK" : "UNKNOWN");
            var message = root.TryGetProperty("message", out var messageProp) ? messageProp.ToString() : (success ? "검증되었습니다." : "검증에 실패했습니다.");

            return new 개인통관부호검증Result
            {
                성공여부 = success,
                결과코드 = code ?? "UNKNOWN",
                메시지 = message ?? string.Empty
            };
        }
        catch
        {
            var normalized = body.Contains("정상", StringComparison.OrdinalIgnoreCase)
                || body.Contains("유효", StringComparison.OrdinalIgnoreCase)
                || body.Contains("OK", StringComparison.OrdinalIgnoreCase);

            return new 개인통관부호검증Result
            {
                성공여부 = normalized,
                결과코드 = normalized ? "OK" : "PARSE_FAILED",
                메시지 = normalized ? "검증되었습니다." : "검증 응답 해석에 실패했습니다."
            };
        }
    }
}
