using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Driver.Transport;
using Microsoft.AspNetCore.Components.Forms;

namespace Ssalddel.Ui.Common.Areas.App.Services;

public static class CommunityLedgerEvidencePolicy
{
    public const long MaxFileBytes = 8 * 1024 * 1024;
}

public interface ICommunityLedgerNodeActionService
{
    Task<CommunityLedgerEvidenceUploadResult> 상차증빙업로드Async(
        PlatformCommunityLedgerNodeActionResponse action,
        IBrowserFile file,
        CancellationToken cancellationToken = default);

    Task<기사운송상태변경응답> 실행Async(
        PlatformCommunityLedgerNodeActionResponse action,
        CommunityLedgerEvidenceUploadResult? evidence = null,
        CancellationToken cancellationToken = default);
}

public sealed class CommunityLedgerNodeActionService : ICommunityLedgerNodeActionService
{
    private readonly SsalddelProtectedApiClient protectedApiClient;

    public CommunityLedgerNodeActionService(SsalddelProtectedApiClient protectedApiClient)
    {
        this.protectedApiClient = protectedApiClient;
    }

    public async Task<CommunityLedgerEvidenceUploadResult> 상차증빙업로드Async(
        PlatformCommunityLedgerNodeActionResponse action,
        IBrowserFile file,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentNullException.ThrowIfNull(file);
        var transportId = ResolveTransportId(action);

        await using var stream = file.OpenReadStream(CommunityLedgerEvidencePolicy.MaxFileBytes, cancellationToken);
        using var content = new MultipartFormDataContent();
        using var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(
            string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType);
        content.Add(fileContent, "file", file.Name);
        content.Add(new StringContent("TransportPickupComplete"), "commandName");
        content.Add(new StringContent(transportId.ToString()), "referenceId");

        using var response = await protectedApiClient.PostAsync("api/v1/files/upload", content, cancellationToken);
        await EnsureSuccessAsync(response, "상차 증빙 업로드", cancellationToken);
        return await response.Content.ReadFromJsonAsync<CommunityLedgerEvidenceUploadResult>(cancellationToken)
               ?? throw new InvalidOperationException("상차 증빙 업로드 응답을 읽을 수 없습니다.");
    }

    public async Task<기사운송상태변경응답> 실행Async(
        PlatformCommunityLedgerNodeActionResponse action,
        CommunityLedgerEvidenceUploadResult? evidence = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (!action.실행가능여부)
        {
            throw new InvalidOperationException(action.비활성사유 ?? "현재 실행할 수 없는 업무입니다.");
        }

        var transportId = ResolveTransportId(action);
        using var response = action.행동Code switch
        {
            CommunityLedgerNodeActionCodes.TransportArrivePickup =>
                await protectedApiClient.PostAsProtectedJsonAsync(
                    $"api/v1/driver/transports/{transportId}/arrive-pickup",
                    new { },
                    cancellationToken),
            CommunityLedgerNodeActionCodes.TransportCompletePickup when evidence is not null =>
                await protectedApiClient.PostAsProtectedJsonAsync(
                    $"api/v1/driver/transports/{transportId}/pickup-complete",
                    new 기사운송상차완료요청
                    {
                        상차사진ObjectName = evidence.ObjectName,
                        상차사진Url = evidence.Url,
                        인수증증빙방식 = "문서사진",
                        인수증확인완료 = true
                    },
                    cancellationToken),
            CommunityLedgerNodeActionCodes.TransportCompletePickup =>
                throw new InvalidOperationException("상차 완료에는 현장 증빙 사진이 필요합니다."),
            _ => throw new InvalidOperationException("서버에서 허용하지 않은 원장 업무입니다.")
        };

        await EnsureSuccessAsync(response, action.표시명, cancellationToken);
        return await response.Content.ReadFromJsonAsync<기사운송상태변경응답>(cancellationToken)
               ?? throw new InvalidOperationException($"{action.표시명} 응답을 읽을 수 없습니다.");
    }

    private static long ResolveTransportId(PlatformCommunityLedgerNodeActionResponse action)
        => long.TryParse(action.실행대상Id, out var transportId) && transportId > 0
            ? transportId
            : throw new InvalidOperationException("연결된 운송 실행 정보를 확인할 수 없습니다.");

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        string actionName,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new InvalidOperationException(BuildErrorMessage(actionName, response, body));
    }

    private static string BuildErrorMessage(string actionName, HttpResponseMessage response, string body)
    {
        if (!string.IsNullOrWhiteSpace(body))
        {
            try
            {
                using var document = JsonDocument.Parse(body);
                var root = document.RootElement;
                if (root.TryGetProperty("detail", out var detail) && !string.IsNullOrWhiteSpace(detail.GetString()))
                {
                    return detail.GetString()!;
                }

                if (root.TryGetProperty("title", out var title) && !string.IsNullOrWhiteSpace(title.GetString()))
                {
                    return title.GetString()!;
                }
            }
            catch (JsonException)
            {
                // 구조화된 오류가 아니면 아래의 HTTP 상태 메시지를 사용합니다.
            }
        }

        return $"{actionName} 실패: HTTP {(int)response.StatusCode}";
    }
}

public sealed class CommunityLedgerEvidenceUploadResult
{
    public string BucketName { get; set; } = string.Empty;
    public string ObjectName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
