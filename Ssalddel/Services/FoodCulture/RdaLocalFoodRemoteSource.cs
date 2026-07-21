using System.Globalization;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.Content;
using 살뜰.Services.Options;

namespace Ssalddel.Services.FoodCulture;

public sealed class RdaLocalFoodRemoteSource : IOfficialFoodRecipeRemoteSource
{
    private const string DocumentationUrl = "https://www.data.go.kr/data/15101449/openapi.do";
    private readonly HttpClient _httpClient;
    private readonly RdaLocalFoodOptions _options;

    public RdaLocalFoodRemoteSource(
        HttpClient httpClient,
        IOptions<PublicDataOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value.RdaLocalFood;
    }

    public string SourceKey => OfficialFoodRecipeSourceKeys.RdaLocalFood;

    public async Task<IReadOnlyList<OfficialFoodRecipeCollectedRecord>> FetchAsync(
        int maxPages,
        int maxItems,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException(
                "PublicData:RdaLocalFood:ApiKey가 필요합니다. 키는 appsettings.Local.json 또는 환경 변수에만 설정하세요.");
        }

        var pageSize = Math.Clamp(_options.PageSize, 1, 1000);
        var records = new List<OfficialFoodRecipeCollectedRecord>();
        var totalCount = int.MaxValue;
        for (var page = 1;
             page <= maxPages && records.Count < maxItems && (page - 1) * pageSize < totalCount;
             page++)
        {
            var listPath = BuildPath(
                _options.ListPath,
                new Dictionary<string, string>
                {
                    ["apiKey"] = _options.ApiKey.Trim(),
                    ["pageNo"] = page.ToString(CultureInfo.InvariantCulture),
                    ["schType"] = "A",
                    ["schText"] = string.Empty,
                    ["order"] = "ASC",
                    ["numOfRows"] = pageSize.ToString(CultureInfo.InvariantCulture)
                });
            var listDocument = await GetXmlAsync(listPath, cancellationToken);
            ThrowIfApiError(listDocument);
            totalCount = ReadInt(listDocument.Root, "totalCount") ?? 0;

            var listItems = Descendants(listDocument.Root, "item").ToArray();
            if (listItems.Length == 0)
            {
                break;
            }

            foreach (var item in listItems)
            {
                if (records.Count >= maxItems)
                {
                    break;
                }

                var externalId = Read(item, "cntntsNo");
                if (string.IsNullOrWhiteSpace(externalId))
                {
                    continue;
                }

                var detailPath = BuildPath(
                    _options.DetailPath,
                    new Dictionary<string, string>
                    {
                        ["apiKey"] = _options.ApiKey.Trim(),
                        ["cntntsNo"] = externalId
                    });
                var detailDocument = await GetXmlAsync(detailPath, cancellationToken);
                ThrowIfApiError(detailDocument);
                var detail = Descendants(detailDocument.Root, "item").FirstOrDefault()
                             ?? detailDocument.Root;
                var record = ToRecord(item, detail, detailDocument);
                if (record is not null)
                {
                    records.Add(record);
                }
            }
        }

        return records;
    }

    internal static OfficialFoodRecipeCollectedRecord? ToRecord(
        XElement listItem,
        XElement? detail,
        XDocument rawDetail)
    {
        var externalId = Read(listItem, "cntntsNo");
        var name = FirstNonEmpty(Read(detail, "trditfdNm"), Read(listItem, "trditfdNm"));
        if (string.IsNullOrWhiteSpace(externalId) || string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        var region = FirstNonEmpty(Read(detail, "atptCodeNm"), Read(listItem, "atptCodeNm"));
        var category = FirstNonEmpty(
            Read(detail, "foodTyCodeFullname"),
            Read(listItem, "foodTyCodeFullname"));
        var cookingMethod = FirstNonEmpty(
            Read(detail, "ckryCodeFullname"),
            Read(listItem, "ckryCodeFullname"));
        var origin = Read(detail, "originDtl");
        var provider = Read(detail, "infoOfferInfo");
        var ingredients = new[]
            {
                (Label: "주재료", Value: Read(detail, "fdmtInfo")),
                (Label: "부재료", Value: Read(detail, "asstnMatrlInfo"))
            }
            .Where(item => !string.IsNullOrWhiteSpace(item.Value))
            .SelectMany(item => SplitRecipeText(item.Value, item.Label))
            .ToArray();
        var instructions = SplitRecipeText(Read(detail, "stdCkryDtl"), null);
        var tags = new[] { region, category, cookingMethod, "향토 음식" }
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var tips = string.Join(
            Environment.NewLine,
            new[]
            {
                Read(detail, "referMatterDtl"),
                string.IsNullOrWhiteSpace(provider) ? string.Empty : $"정보제공자: {provider}",
                Read(detail, "ckngDmprDtl")
            }.Where(value => !string.IsNullOrWhiteSpace(value)));

        return new OfficialFoodRecipeCollectedRecord(
            externalId,
            name,
            name,
            string.Empty,
            origin,
            region,
            category,
            string.Empty,
            ingredients,
            instructions,
            new Dictionary<string, string>(),
            tags,
            tips,
            DocumentationUrl,
            BuildImageReference(listItem, detail),
            rawDetail.ToString(SaveOptions.DisableFormatting));
    }

    private async Task<XDocument> GetXmlAsync(
        string path,
        CancellationToken cancellationToken)
    {
        using var response = await SendAsync(path, cancellationToken);
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var settings = new XmlReaderSettings
        {
            Async = true,
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        };
        using var reader = XmlReader.Create(stream, settings);
        return await XDocument.LoadAsync(reader, LoadOptions.None, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendAsync(
        string path,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            var response = await _httpClient.GetAsync(
                path,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
            if (attempt >= 3 || !IsTransient(response.StatusCode))
            {
                response.EnsureSuccessStatusCode();
                return response;
            }

            response.Dispose();
            await Task.Delay(TimeSpan.FromMilliseconds(200 * attempt), cancellationToken);
        }
    }

    private static string BuildPath(
        string path,
        IReadOnlyDictionary<string, string> query)
    {
        var separator = path.Contains('?', StringComparison.Ordinal) ? '&' : '?';
        return path + separator + string.Join(
            '&',
            query.Select(item =>
                $"{Uri.EscapeDataString(item.Key)}={Uri.EscapeDataString(item.Value)}"));
    }

    private static void ThrowIfApiError(XDocument document)
    {
        var resultCode = FirstNonEmpty(
            Read(document.Root, "resultCode"),
            Read(document.Root, "code"));
        if (string.IsNullOrWhiteSpace(resultCode)
            || resultCode is "00" or "0" or "SUCCESS")
        {
            return;
        }

        var message = FirstNonEmpty(
            Read(document.Root, "resultMsg"),
            Read(document.Root, "message"));
        throw new InvalidOperationException(
            $"농사로 향토 음식 API 응답 오류입니다. Code={resultCode}, Message={message}");
    }

    private static IEnumerable<XElement> Descendants(XContainer? container, string name)
        => container?.Descendants().Where(element => element.Name.LocalName == name)
           ?? [];

    private static string Read(XContainer? container, string name)
        => container?.Descendants()
               .FirstOrDefault(element => element.Name.LocalName == name)
               ?.Value
               .Trim()
           ?? string.Empty;

    private static int? ReadInt(XContainer? container, string name)
        => int.TryParse(
            Read(container, name),
            NumberStyles.Integer,
            CultureInfo.InvariantCulture,
            out var value)
            ? value
            : null;

    private static IReadOnlyList<string> SplitRecipeText(string value, string? label)
    {
        var lines = value
            .Replace("<br />", "\n", StringComparison.OrdinalIgnoreCase)
            .Replace("<br/>", "\n", StringComparison.OrdinalIgnoreCase)
            .Replace("<br>", "\n", StringComparison.OrdinalIgnoreCase)
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();
        if (lines.Length == 0)
        {
            return [];
        }

        if (string.IsNullOrWhiteSpace(label))
        {
            return lines;
        }

        lines[0] = $"[{label}] {lines[0]}";
        return lines;
    }

    private static string BuildImageReference(XElement listItem, XElement? detail)
    {
        var codes = FirstNonEmpty(Read(detail, "rtnImgSeCode"), Read(listItem, "rtnImgSeCode"))
            .Split('|', StringSplitOptions.TrimEntries);
        var courses = FirstNonEmpty(Read(detail, "rtnFileCours"), Read(listItem, "rtnFileCours"))
            .Split('|', StringSplitOptions.TrimEntries);
        var names = FirstNonEmpty(Read(detail, "rtnStreFileNm"), Read(listItem, "rtnThumbFileNm"))
            .Split('|', StringSplitOptions.TrimEntries);
        if (courses.Length == 0 || names.Length == 0)
        {
            return string.Empty;
        }

        var max = Math.Min(codes.Length, Math.Min(courses.Length, names.Length));
        var index = Array.FindIndex(codes, 0, max, code => code == "209006");
        if (index < 0)
        {
            index = Array.FindIndex(codes, 0, max, code => code is "209005" or "209007");
        }

        if (index < 0 || string.IsNullOrWhiteSpace(courses[index]) || string.IsNullOrWhiteSpace(names[index]))
        {
            return string.Empty;
        }

        var relative = $"{courses[index].Trim('/')}/{names[index].Trim('/')}";
        return new Uri(new Uri("https://www.nongsaro.go.kr/"), relative).AbsoluteUri;
    }

    private static bool IsTransient(HttpStatusCode statusCode)
        => statusCode is HttpStatusCode.RequestTimeout
            or HttpStatusCode.TooManyRequests
            or HttpStatusCode.InternalServerError
            or HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.GatewayTimeout;

    private static string FirstNonEmpty(params string[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim()
           ?? string.Empty;
}
