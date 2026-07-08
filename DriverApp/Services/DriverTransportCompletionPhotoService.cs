using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Hongdal.Contracts.Driver.Transport;

namespace DriverApp.Services;

public enum DriverTransportCompletionPhotoKind
{
    Pickup,
    Dropoff
}

public sealed record DriverTransportCompletionPhoto(
    DriverTransportCompletionPhotoKind Kind,
    long TransportId,
    string FileName,
    string ContentType,
    byte[] Bytes,
    DriverTransportPickupReceiptEvidence? ReceiptEvidence = null);

public sealed record DriverTransportPickupReceiptEvidence(
    string EvidenceMethod,
    bool Signed,
    bool SignatureOmitted,
    string? SignatureOmissionReason,
    string? RecipientName,
    string? RecipientOrganization,
    string? RecipientSignature,
    string? DriverSignature);

public sealed record DriverTransportCompletionPhotoResult(
    bool Uploaded,
    string Message,
    string? Url = null,
    string? ObjectName = null);

public interface IDriverTransportCompletionPhotoService
{
    Task<DriverTransportCompletionPhotoResult> CompleteWithPhotoAsync(
        DriverTransportCompletionPhoto photo,
        CancellationToken cancellationToken = default);
}

public sealed class SampleDriverTransportCompletionPhotoService : IDriverTransportCompletionPhotoService
{
    public Task<DriverTransportCompletionPhotoResult> CompleteWithPhotoAsync(
        DriverTransportCompletionPhoto photo,
        CancellationToken cancellationToken = default)
    {
        var stepName = photo.Kind == DriverTransportCompletionPhotoKind.Pickup ? "상차 완료" : "하차 완료";
        var message = $"{stepName} 사진이 완료 Command에 첨부될 준비가 끝났습니다. 파일: {photo.FileName}";

        return Task.FromResult(new DriverTransportCompletionPhotoResult(
            Uploaded: false,
            Message: message,
            ObjectName: BuildSampleObjectName(photo)));
    }

    private static string BuildSampleObjectName(DriverTransportCompletionPhoto photo)
    {
        var folder = photo.Kind == DriverTransportCompletionPhotoKind.Pickup
            ? "pickup-complete"
            : "dropoff-complete";

        return $"driver-transports/{photo.TransportId}/{folder}/{photo.FileName}";
    }
}

public sealed class HttpDriverTransportCompletionPhotoService : IDriverTransportCompletionPhotoService
{
    private readonly HttpClient _httpClient;
    private readonly IAuthSession _authSession;

    public HttpDriverTransportCompletionPhotoService(HttpClient httpClient, IAuthSession authSession)
    {
        _httpClient = httpClient;
        _authSession = authSession;
    }

    public async Task<DriverTransportCompletionPhotoResult> CompleteWithPhotoAsync(
        DriverTransportCompletionPhoto photo,
        CancellationToken cancellationToken = default)
    {
        await _authSession.RestoreAsync(cancellationToken);

        var upload = await UploadPhotoAsync(photo, cancellationToken);
        await CompleteTransportAsync(photo, upload, cancellationToken);

        var stepName = photo.Kind == DriverTransportCompletionPhotoKind.Pickup ? "상차 완료" : "하차 완료";
        return new DriverTransportCompletionPhotoResult(
            Uploaded: true,
            Message: $"{stepName} 사진 업로드와 완료 처리가 끝났습니다.",
            Url: upload.Url,
            ObjectName: upload.ObjectName);
    }

    private async Task<FileUploadResponse> UploadPhotoAsync(
        DriverTransportCompletionPhoto photo,
        CancellationToken cancellationToken)
    {
        using var content = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(photo.Bytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(photo.ContentType);

        content.Add(fileContent, "file", photo.FileName);
        content.Add(new StringContent(ResolveCommandName(photo.Kind)), "commandName");
        content.Add(new StringContent(photo.TransportId.ToString(CultureInfo.InvariantCulture)), "referenceId");

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/files/upload")
        {
            Content = content
        };
        ApplyAuthorization(request);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var upload = await response.Content.ReadFromJsonAsync<FileUploadResponse>(cancellationToken: cancellationToken);
        return upload ?? throw new InvalidOperationException("파일 업로드 응답을 읽지 못했습니다.");
    }

    private async Task CompleteTransportAsync(
        DriverTransportCompletionPhoto photo,
        FileUploadResponse upload,
        CancellationToken cancellationToken)
    {
        var path = photo.Kind == DriverTransportCompletionPhotoKind.Pickup
            ? $"api/v1/driver/transports/{photo.TransportId}/pickup-complete"
            : $"api/v1/driver/transports/{photo.TransportId}/complete";

        using var request = new HttpRequestMessage(HttpMethod.Post, path);
        if (photo.Kind == DriverTransportCompletionPhotoKind.Pickup)
        {
            request.Content = JsonContent.Create(new 기사운송상차완료요청
            {
                상차사진ObjectName = upload.ObjectName,
                상차사진Url = upload.Url,
                인수증증빙방식 = photo.ReceiptEvidence?.EvidenceMethod,
                인수자명 = photo.ReceiptEvidence?.RecipientName,
                인수자소속 = photo.ReceiptEvidence?.RecipientOrganization,
                인수자서명 = photo.ReceiptEvidence?.RecipientSignature,
                기사서명 = photo.ReceiptEvidence?.DriverSignature,
                인수증확인완료 = photo.ReceiptEvidence?.Signed == true,
                인수증서명생략확인 = photo.ReceiptEvidence?.SignatureOmitted == true,
                인수증서명생략사유 = photo.ReceiptEvidence?.SignatureOmissionReason
            });
        }
        else
        {
            request.Content = JsonContent.Create(new 기사운송하차완료요청
            {
                하차사진ObjectName = upload.ObjectName,
                하차사진Url = upload.Url
            });
        }

        ApplyAuthorization(request);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private void ApplyAuthorization(HttpRequestMessage request)
    {
        if (!string.IsNullOrWhiteSpace(_authSession.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authSession.AccessToken);
        }
    }

    private static string ResolveCommandName(DriverTransportCompletionPhotoKind kind)
    {
        return kind == DriverTransportCompletionPhotoKind.Pickup
            ? "TransportPickupComplete"
            : "TransportDropoffComplete";
    }

    private sealed class FileUploadResponse
    {
        public string BucketName { get; set; } = string.Empty;
        public string ObjectName { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}
