namespace Ssalddel.WebApp.Services;

public sealed record OrdererBetaReadiness(
    bool IsReady,
    string Message,
    DateTime CheckedAtUtc);

public sealed class OrdererBetaReadinessService(HttpClient httpClient)
{
    public async Task<OrdererBetaReadiness> CheckAsync(CancellationToken cancellationToken = default)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(5));

        try
        {
            using var response = await httpClient.GetAsync("health/ready", timeout.Token);
            return response.IsSuccessStatusCode
                ? new(true, "서버와 데이터 저장소가 응답하고 있습니다.", DateTime.UtcNow)
                : new(false, $"서버 준비 확인이 HTTP {(int)response.StatusCode}로 응답했습니다.", DateTime.UtcNow);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new(false, "서버 준비 확인 시간이 초과되었습니다.", DateTime.UtcNow);
        }
        catch (HttpRequestException)
        {
            return new(false, "서버에 연결할 수 없습니다. 잠시 뒤 다시 확인해 주세요.", DateTime.UtcNow);
        }
    }
}
