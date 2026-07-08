using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using 홍달.Services.Options;

namespace 홍달.Services.Notifications;

public sealed class KakaoAlimTalkService : IKakaoAlimTalkService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly KakaoAlimTalkOptions _options;
    private readonly ILogger<KakaoAlimTalkService> _logger;

    public KakaoAlimTalkService(
        HttpClient httpClient,
        IOptions<KakaoAlimTalkOptions> options,
        ILogger<KakaoAlimTalkService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<bool> SendAsync(KakaoAlimTalkMessage message, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            _logger.LogDebug("카카오 알림톡 발송이 비활성화되어 있습니다. TemplateCode={TemplateCode}", message.TemplateCode);
            return false;
        }

        if (string.IsNullOrWhiteSpace(_options.BaseUrl)
            || string.IsNullOrWhiteSpace(_options.ApiKey)
            || string.IsNullOrWhiteSpace(_options.SenderKey)
            || string.IsNullOrWhiteSpace(message.RecipientPhoneNumber))
        {
            _logger.LogWarning(
                "카카오 알림톡 발송 설정 또는 수신 번호가 부족합니다. TemplateCode={TemplateCode}",
                message.TemplateCode);
            return false;
        }

        var payload = new
        {
            senderKey = _options.SenderKey,
            templateCode = message.TemplateCode,
            recipientPhoneNumber = NormalizePhoneNumber(message.RecipientPhoneNumber),
            title = message.Title,
            message = message.Body,
            variables = message.Variables
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.SendPath)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        if (response.IsSuccessStatusCode)
        {
            return true;
        }

        var errorBody = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogWarning(
            "카카오 알림톡 발송 실패. StatusCode={StatusCode} TemplateCode={TemplateCode} Reason={Reason}",
            response.StatusCode,
            message.TemplateCode,
            errorBody);
        return false;
    }

    private static string NormalizePhoneNumber(string value)
        => new(value.Where(char.IsDigit).ToArray());
}
