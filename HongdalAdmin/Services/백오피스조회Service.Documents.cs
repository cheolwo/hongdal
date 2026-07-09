using System.Net.Http.Json;

namespace HongdalAdmin.Services;

public sealed partial class 백오피스조회Service
{
    public async Task<IReadOnlyList<파일POD응답>> 파일POD목록조회Async(string? fileType = null, string? requestId = null, CancellationToken cancellationToken = default)
    {
        var query = BuildQuery("api/v1/admin/files/pod", ("fileType", fileType), ("requestId", requestId));
        return await 서버목록조회Async<파일POD응답>(
            query,
            cancellationToken);
    }

    public async Task<파일POD응답?> 파일POD상태변경Async(Guid id, string uploadStatus, CancellationToken cancellationToken = default)
    {
        ApplyAuthorizationHeader();

        var response = await _httpClient.PatchAsJsonAsync($"api/v1/admin/files/pod/{id}/status", new 파일POD상태변경요청
        {
            UploadStatus = uploadStatus
        }, cancellationToken);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<파일POD응답>(cancellationToken: cancellationToken);
    }

    public async Task<파일POD응답?> 파일POD업로드Async(Stream fileStream, string fileName, string contentType, string fileType, string? requestId, CancellationToken cancellationToken = default)
    {
        ApplyAuthorizationHeader();

        using var content = new MultipartFormDataContent();
        using var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

        content.Add(fileContent, "File", fileName);
        content.Add(new StringContent(fileType), "FileType");
        if (!string.IsNullOrWhiteSpace(requestId))
        {
            content.Add(new StringContent(requestId.Trim()), "RequestId");
        }

        var response = await _httpClient.PostAsync("api/v1/admin/files/pod/upload", content, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<파일POD응답>(cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<문서정책요약응답>> 문서정책목록조회Async(CancellationToken cancellationToken = default)
    {
        ApplyAuthorizationHeader();

        var result = await _httpClient.GetFromJsonAsync<List<문서정책요약응답>>("api/v1/admin/documents/policies", cancellationToken);
        return result ?? [];
    }

    public async Task<문서정책요약응답?> 문서정책수정Async(string documentCode, 문서정책수정요청 request, CancellationToken cancellationToken = default)
    {
        ApplyAuthorizationHeader();

        var response = await _httpClient.PutAsJsonAsync($"api/v1/admin/documents/policies/{Uri.EscapeDataString(documentCode.Trim())}", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<문서정책요약응답>(cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<문서조회요약응답>> 문서목록조회Async(string? documentCode = null, string? requestId = null, string? status = null, CancellationToken cancellationToken = default)
    {
        ApplyAuthorizationHeader();

        var query = BuildQuery(
            "api/v1/admin/documents",
            ("documentCode", documentCode),
            ("requestId", requestId),
            ("status", status));

        var result = await _httpClient.GetFromJsonAsync<List<문서조회요약응답>>(query, cancellationToken);
        return result ?? [];
    }

    public async Task<IReadOnlyList<문서조회로그요약응답>> 문서로그목록조회Async(long? documentId = null, CancellationToken cancellationToken = default)
    {
        ApplyAuthorizationHeader();

        var query = documentId.HasValue
            ? $"api/v1/admin/documents/logs?documentId={documentId.Value}"
            : "api/v1/admin/documents/logs";

        var result = await _httpClient.GetFromJsonAsync<List<문서조회로그요약응답>>(query, cancellationToken);
        return result ?? [];
    }

    public async Task<문서조회요약응답?> 문서업로드Async(Stream fileStream, string fileName, string contentType, string documentCode, string documentName, string requestId, long? transportId = null, bool? encrypt = null, bool? allowDownload = null, CancellationToken cancellationToken = default)
    {
        ApplyAuthorizationHeader();

        using var content = new MultipartFormDataContent();
        using var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);

        content.Add(fileContent, "File", fileName);
        content.Add(new StringContent(requestId ?? string.Empty), "의뢰Id");
        if (transportId.HasValue)
        {
            content.Add(new StringContent(transportId.Value.ToString()), "배송운송Id");
        }

        content.Add(new StringContent(documentCode), "문서코드");
        content.Add(new StringContent(documentName), "문서명");
        if (encrypt.HasValue)
        {
            content.Add(new StringContent(encrypt.Value.ToString().ToLowerInvariant()), "암호화여부");
        }

        if (allowDownload.HasValue)
        {
            content.Add(new StringContent(allowDownload.Value.ToString().ToLowerInvariant()), "다운로드허용여부");
        }

        var response = await _httpClient.PostAsync("api/v1/admin/documents", content, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<문서조회요약응답>(cancellationToken: cancellationToken);
    }

    public async Task<byte[]?> 문서다운로드Async(long id, CancellationToken cancellationToken = default)
    {
        ApplyAuthorizationHeader();

        return await _httpClient.GetByteArrayAsync($"api/v1/admin/documents/{id}/download", cancellationToken);
    }
}
