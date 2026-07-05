using System.Globalization;
using System.Xml.Linq;
using Microsoft.Extensions.Options;

namespace 홍달.Services.External.Opinet
{
    public interface IOpinetAveragePriceService
    {
        Task<IReadOnlyList<OpinetAveragePriceItem>> GetAveragePricesAsync(CancellationToken cancellationToken = default);
    }

    public sealed class OpinetAveragePriceService : IOpinetAveragePriceService
    {
        private readonly HttpClient _httpClient;
        private readonly OpinetOptions _options;

        public OpinetAveragePriceService(HttpClient httpClient, IOptions<OpinetOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<IReadOnlyList<OpinetAveragePriceItem>> GetAveragePricesAsync(CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_options.CertKey))
            {
                throw new InvalidOperationException("Opinet:CertKey configuration is required.");
            }

            var output = string.IsNullOrWhiteSpace(_options.OutputFormat) ? "xml" : _options.OutputFormat.Trim().ToLowerInvariant();
            var requestUrl = $"{_options.AveragePricePath.TrimStart('/')}?out={Uri.EscapeDataString(output)}&certkey={Uri.EscapeDataString(_options.CertKey)}";

            using var response = await _httpClient.GetAsync(requestUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (output == "json")
            {
                return ParseJson(content);
            }

            return ParseXml(content);
        }

        private static IReadOnlyList<OpinetAveragePriceItem> ParseXml(string content)
        {
            var document = XDocument.Parse(content);
            return document
                .Descendants("OIL")
                .Select(x => new OpinetAveragePriceItem
                {
                    TradeDate = GetString(x, "TRADE_DT"),
                    ProductCode = GetString(x, "PRODCD"),
                    ProductName = GetString(x, "PRODNM"),
                    Price = GetDecimal(x, "PRICE"),
                    Difference = GetDecimal(x, "DIFF")
                })
                .ToList();
        }

        private static IReadOnlyList<OpinetAveragePriceItem> ParseJson(string content)
        {
            using var document = System.Text.Json.JsonDocument.Parse(content);
            var root = document.RootElement;
            var list = new List<OpinetAveragePriceItem>();

            if (root.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (var item in root.EnumerateArray())
                {
                    list.Add(MapJsonItem(item));
                }

                return list;
            }

            if (root.ValueKind == System.Text.Json.JsonValueKind.Object)
            {
                if (root.TryGetProperty("RESULT", out var result) && result.ValueKind == System.Text.Json.JsonValueKind.Object && result.TryGetProperty("OIL", out var oils))
                {
                    if (oils.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        foreach (var item in oils.EnumerateArray())
                        {
                            list.Add(MapJsonItem(item));
                        }

                        return list;
                    }
                }

                if (root.TryGetProperty("OIL", out var oilArray) && oilArray.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    foreach (var item in oilArray.EnumerateArray())
                    {
                        list.Add(MapJsonItem(item));
                    }
                }
            }

            return list;
        }

        private static OpinetAveragePriceItem MapJsonItem(System.Text.Json.JsonElement element)
        {
            return new OpinetAveragePriceItem
            {
                TradeDate = GetJsonString(element, "TRADE_DT"),
                ProductCode = GetJsonString(element, "PRODCD"),
                ProductName = GetJsonString(element, "PRODNM"),
                Price = GetJsonDecimal(element, "PRICE"),
                Difference = GetJsonDecimal(element, "DIFF")
            };
        }

        private static string? GetString(XElement element, string name)
        {
            return element.Element(name)?.Value;
        }

        private static decimal? GetDecimal(XElement element, string name)
        {
            var value = element.Element(name)?.Value;
            return decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
        }

        private static string? GetJsonString(System.Text.Json.JsonElement element, string name)
        {
            return element.TryGetProperty(name, out var value) ? value.GetString() : null;
        }

        private static decimal? GetJsonDecimal(System.Text.Json.JsonElement element, string name)
        {
            if (!element.TryGetProperty(name, out var value))
            {
                return null;
            }

            return value.ValueKind switch
            {
                System.Text.Json.JsonValueKind.Number when value.TryGetDecimal(out var parsed) => parsed,
                System.Text.Json.JsonValueKind.String when decimal.TryParse(value.GetString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) => parsed,
                _ => null
            };
        }
    }

    public sealed class OpinetAveragePriceItem
    {
        public string? TradeDate { get; set; }
        public string? ProductCode { get; set; }
        public string? ProductName { get; set; }
        public decimal? Price { get; set; }
        public decimal? Difference { get; set; }
    }
}


