using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace 홍달.Services.External.Nts
{
    public interface INtsBusinessRegistrationService
    {
        Task<NtsBusinessRegistrationStatusResponse> GetStatusAsync(string businessRegistrationNumber, CancellationToken cancellationToken = default);
        Task<NtsBusinessRegistrationCheckResult> CheckStatusAsync(string businessRegistrationNumber, CancellationToken cancellationToken = default);
    }

    public sealed class NtsBusinessRegistrationService : INtsBusinessRegistrationService
    {
        private readonly HttpClient _httpClient;
        private readonly NtsBusinessRegistrationOptions _options;

        public NtsBusinessRegistrationService(HttpClient httpClient, IOptions<NtsBusinessRegistrationOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<NtsBusinessRegistrationStatusResponse> GetStatusAsync(string businessRegistrationNumber, CancellationToken cancellationToken = default)
        {
            var normalized = NormalizeBusinessRegistrationNumber(businessRegistrationNumber);
            if (string.IsNullOrWhiteSpace(normalized) || normalized.Length != 10)
            {
                throw new ArgumentException("?ъ뾽?먮벑濡앸쾲?몃뒗 ?レ옄 10?먮━?ъ빞 ?⑸땲??", nameof(businessRegistrationNumber));
            }

            var payload = new NtsBusinessRegistrationStatusRequest(new[] { normalized });
            var requestUri = BuildRequestUri(_options.StatusPath);

            using var response = await _httpClient.PostAsJsonAsync(requestUri, payload, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<NtsBusinessRegistrationStatusResponse>(content, JsonOptions);

            return result ?? new NtsBusinessRegistrationStatusResponse();
        }

        public async Task<NtsBusinessRegistrationCheckResult> CheckStatusAsync(string businessRegistrationNumber, CancellationToken cancellationToken = default)
        {
            var response = await GetStatusAsync(businessRegistrationNumber, cancellationToken);
            var normalized = NormalizeBusinessRegistrationNumber(businessRegistrationNumber);
            var item = response.Data.FirstOrDefault();

            if (response.IsOk && item != null && string.Equals(item.BusinessStatusCode, "01", StringComparison.OrdinalIgnoreCase))
            {
                return new NtsBusinessRegistrationCheckResult
                {
                    IsValid = true,
                    BusinessRegistrationNumber = normalized,
                    StatusCode = response.StatusCode,
                    Message = string.IsNullOrWhiteSpace(item.BusinessStatus) ? "?뺤긽 ?ъ뾽?먯엯?덈떎." : item.BusinessStatus,
                    Status = item
                };
            }

            return new NtsBusinessRegistrationCheckResult
            {
                IsValid = false,
                BusinessRegistrationNumber = normalized,
                StatusCode = response.StatusCode,
                Message = item?.BusinessStatus ?? response.StatusCode ?? "?ъ뾽?먮벑濡??곹깭瑜??뺤씤?????놁뒿?덈떎.",
                Status = item
            };
        }

        private string BuildRequestUri(string path)
        {
            var relativePath = path.TrimStart('/');
            if (string.IsNullOrWhiteSpace(_options.ServiceKey))
            {
                return relativePath;
            }

            return $"{relativePath}?serviceKey={Uri.EscapeDataString(_options.ServiceKey)}";
        }

        private static string NormalizeBusinessRegistrationNumber(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return new string(value.Where(char.IsDigit).ToArray());
        }

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public sealed class NtsBusinessRegistrationStatusRequest
    {
        public NtsBusinessRegistrationStatusRequest(IReadOnlyList<string> b_no)
        {
            BNo = b_no;
        }

        [System.Text.Json.Serialization.JsonPropertyName("b_no")]
        public IReadOnlyList<string> BNo { get; }
    }

    public sealed class NtsBusinessRegistrationStatusResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("status_code")]
        public string? StatusCode { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("match_cnt")]
        public int MatchCnt { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("request_cnt")]
        public int RequestCnt { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public List<NtsBusinessRegistrationStatusItem> Data { get; set; } = [];

        public bool IsOk => string.Equals(StatusCode, "OK", StringComparison.OrdinalIgnoreCase);
    }

    public sealed class NtsBusinessRegistrationStatusItem
    {
        [System.Text.Json.Serialization.JsonPropertyName("b_no")]
        public string? BusinessRegistrationNumber { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("b_stt")]
        public string? BusinessStatus { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("b_stt_cd")]
        public string? BusinessStatusCode { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("tax_type")]
        public string? TaxType { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("tax_type_cd")]
        public string? TaxTypeCode { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("end_dt")]
        public string? EndDate { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("utcc_yn")]
        public string? UtccYn { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("tax_type_change_dt")]
        public string? TaxTypeChangeDate { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("invoice_apply_dt")]
        public string? InvoiceApplyDate { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("rbf_tax_type")]
        public string? RbfTaxType { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("rbf_tax_type_cd")]
        public string? RbfTaxTypeCode { get; set; }
    }

    public sealed class NtsBusinessRegistrationCheckResult
    {
        public bool IsValid { get; set; }
        public string BusinessRegistrationNumber { get; set; } = string.Empty;
        public string? StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public NtsBusinessRegistrationStatusItem? Status { get; set; }
    }
}


