using System.Globalization;
using System.Xml.Linq;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using 홍달.도메인.통관;

namespace 홍달.Services.External.Customs;

public sealed class 공공데이터화물통관진행조회Service : I화물통관진행조회Service
{
    private readonly HttpClient _httpClient;
    private readonly CustomsOptions _options;

    public 공공데이터화물통관진행조회Service(HttpClient httpClient, IOptions<CustomsOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<화물통관진행조회Result> 조회Async(
        화물통관진행조회Request request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return 실패("Customs:ApiKey 설정이 필요합니다.");
        }

        if (string.IsNullOrWhiteSpace(request.화물관리번호)
            && string.IsNullOrWhiteSpace(request.MasterBl)
            && string.IsNullOrWhiteSpace(request.HouseBl))
        {
            return 실패("화물관리번호 또는 MBL/HBL 중 하나는 필요합니다.");
        }

        var query = new Dictionary<string, string?>
        {
            ["serviceKey"] = _options.ApiKey,
            ["type"] = "xml",
            ["cargMtNo"] = request.화물관리번호,
            ["mblNo"] = request.MasterBl,
            ["hblNo"] = request.HouseBl
        };

        var path = _options.CargoTrackingPath.TrimStart('/');
        var relative = QueryHelpers.AddQueryString(path, query);

        try
        {
            using var response = await _httpClient.GetAsync(relative, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return 실패($"HTTP {(int)response.StatusCode}");
            }

            return ParseXml(body);
        }
        catch (Exception ex)
        {
            return 실패($"예외 발생: {ex.Message}");
        }
    }

    private static 화물통관진행조회Result ParseXml(string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
        {
            return 실패("응답 본문이 비어 있습니다.");
        }

        try
        {
            var doc = XDocument.Parse(xml);
            var item = doc.Descendants().FirstOrDefault(x => string.Equals(x.Name.LocalName, "item", StringComparison.OrdinalIgnoreCase));
            if (item is null)
            {
                return 실패("통관 진행정보가 없습니다.");
            }

            var 처리단계명 = GetValue(item, "csclPrgsStts");
            var 장치장명 = GetValue(item, "shedNm");
            var 단계 = MapStage(처리단계명);

            return new 화물통관진행조회Result
            {
                조회성공여부 = true,
                진행단계 = 단계,
                장치장명 = 장치장명,
                처리단계명 = 처리단계명,
                조회시각 = DateTimeOffset.UtcNow
            };
        }
        catch (Exception ex)
        {
            return 실패($"응답 파싱 실패: {ex.Message}");
        }
    }

    private static string? GetValue(XElement parent, string localName)
    {
        return parent.Elements().FirstOrDefault(x => string.Equals(x.Name.LocalName, localName, StringComparison.OrdinalIgnoreCase))?.Value;
    }

    private static 통관진행단계 MapStage(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return 통관진행단계.알수없음;
        }

        return status.Trim() switch
        {
            var s when s.Contains("반입", StringComparison.OrdinalIgnoreCase) && s.Contains("완료", StringComparison.OrdinalIgnoreCase) => 통관진행단계.반입완료,
            var s when s.Contains("신고", StringComparison.OrdinalIgnoreCase) && s.Contains("수리", StringComparison.OrdinalIgnoreCase) => 통관진행단계.신고수리,
            var s when s.Contains("신고", StringComparison.OrdinalIgnoreCase) => 통관진행단계.신고진행중,
            var s when s.Contains("검사", StringComparison.OrdinalIgnoreCase) => 통관진행단계.검사대상,
            var s when s.Contains("반출", StringComparison.OrdinalIgnoreCase) && s.Contains("완료", StringComparison.OrdinalIgnoreCase) => 통관진행단계.반출완료,
            var s when s.Contains("반출", StringComparison.OrdinalIgnoreCase) && s.Contains("가능", StringComparison.OrdinalIgnoreCase) => 통관진행단계.반출가능,
            var s when s.Contains("보류", StringComparison.OrdinalIgnoreCase) => 통관진행단계.보류,
            _ => 통관진행단계.알수없음
        };
    }

    private static 화물통관진행조회Result 실패(string message)
    {
        return new 화물통관진행조회Result
        {
            조회성공여부 = false,
            진행단계 = 통관진행단계.알수없음,
            오류메시지 = message,
            조회시각 = DateTimeOffset.UtcNow
        };
    }
}
