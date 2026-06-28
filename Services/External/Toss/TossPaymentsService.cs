using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace 홍달.Services.External.Toss
{
    public interface ITossPaymentsService
    {
        Task<TossConfirmResult> ConfirmAsync(TossConfirmApiRequest request);
    }

    public sealed class TossPaymentsService : ITossPaymentsService
    {
        private readonly HttpClient _httpClient;
        private readonly TossPaymentsOptions _options;

        public TossPaymentsService(HttpClient httpClient, IOptions<TossPaymentsOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<TossConfirmResult> ConfirmAsync(TossConfirmApiRequest request)
        {
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes(_options.SecretKey + ":"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);

            var response = await _httpClient.PostAsJsonAsync("/v1/payments/confirm", new
            {
                paymentKey = request.PaymentKey,
                orderId = request.OrderId,
                amount = request.Amount
            });

            var body = await response.Content.ReadAsStringAsync();

            string? method = null;
            if (response.IsSuccessStatusCode)
            {
                try
                {
                    using var doc = JsonDocument.Parse(body);
                    if (doc.RootElement.TryGetProperty("method", out var methodElement))
                    {
                        method = methodElement.GetString();
                    }
                }
                catch
                {
                }
            }

            return new TossConfirmResult(response.IsSuccessStatusCode, body, method);
        }
    }

    public sealed record TossConfirmApiRequest(string PaymentKey, string OrderId, int Amount);
    public sealed record TossConfirmResult(bool IsSuccess, string ResponseJson, string? PaymentMethod);
}



