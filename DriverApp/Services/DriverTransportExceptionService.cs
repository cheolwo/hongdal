using System.Net.Http.Headers;
using System.Net.Http.Json;
using Hongdal.Contracts.Driver.Transport;

namespace DriverApp.Services;

public sealed record DriverTransportExceptionReport(
    long TransportId,
    string Stage,
    string ExceptionCode,
    string Reason,
    string? Memo,
    bool RequestAdminReview = true);

public sealed record DriverTransportExceptionReportResult(
    bool Reported,
    string Message,
    기사운송요약응답? Transport = null);

public interface IDriverTransportExceptionService
{
    Task<DriverTransportExceptionReportResult> ReportAsync(
        DriverTransportExceptionReport report,
        CancellationToken cancellationToken = default);
}

public sealed class HttpDriverTransportExceptionService : IDriverTransportExceptionService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthSession _authSession;

    public HttpDriverTransportExceptionService(HttpClient httpClient, IAuthSession authSession)
    {
        _httpClient = httpClient;
        _authSession = authSession;
    }

    public async Task<DriverTransportExceptionReportResult> ReportAsync(
        DriverTransportExceptionReport report,
        CancellationToken cancellationToken = default)
    {
        await _authSession.RestoreAsync(cancellationToken);

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"api/v1/driver/transports/{report.TransportId}/report-exception");
        request.Content = JsonContent.Create(new 기사운송문제신고요청
        {
            단계 = report.Stage,
            예외코드 = report.ExceptionCode,
            사유 = report.Reason,
            메모 = report.Memo,
            관리자확인요청 = report.RequestAdminReview
        });

        if (!string.IsNullOrWhiteSpace(_authSession.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authSession.AccessToken);
        }

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            return new DriverTransportExceptionReportResult(false, $"서버 연결 실패로 문제 신고가 저장되지 않았습니다. 네트워크 확인 후 다시 시도해 주세요. {ex.Message}");
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new DriverTransportExceptionReportResult(false, "서버 응답 지연으로 문제 신고가 저장되지 않았습니다. 잠시 후 다시 시도해 주세요.");
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                var suffix = string.IsNullOrWhiteSpace(body) ? string.Empty : $" {body}";
                return new DriverTransportExceptionReportResult(false, $"문제 신고 처리에 실패했습니다. HTTP {(int)response.StatusCode}.{suffix}");
            }

            var transport = await response.Content.ReadFromJsonAsync<기사운송요약응답>(cancellationToken: cancellationToken);
            return new DriverTransportExceptionReportResult(
                true,
                transport?.관리자확인필요 == true
                    ? "현장 문제가 서버에 신고되었고 관리자 확인 대상으로 표시되었습니다."
                    : "현장 문제가 서버에 신고되었습니다.",
                transport);
        }
    }
}
