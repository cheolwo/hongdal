using Microsoft.Extensions.Logging;

namespace Ssalddel.BackOffice.Client.Diagnostics;

public sealed class ApiClientDiagnostics(ILogger<ApiClientDiagnostics> logger)
{
    public void RequestConfigured(string clientName, string baseUrl)
    {
        logger.LogInformation("API 클라이언트 설정 완료: {ClientName} -> {BaseUrl}", clientName, baseUrl);
    }
}
