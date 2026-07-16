using System.Net;
using System.Net.Http.Json;

namespace Hongdal.Ui.Common.Areas.App.Services;

/// <summary>
/// 역할별 타입드 API 서비스가 공통으로 사용하는 JSON 호출 계층입니다.
/// 인증 헤더와 ISMS-P 요청 암호화는 <see cref="HongdalProtectedApiClient"/>가 담당합니다.
/// </summary>
public interface IHongdalJsonApiClient
{
    Task<TResponse?> GetAsync<TResponse>(
        string path,
        string operationName,
        bool allowNotFound = true,
        CancellationToken cancellationToken = default);

    Task<TResponse?> SendAsync<TResponse>(
        HttpMethod method,
        string path,
        string operationName,
        bool allowNotFound = false,
        CancellationToken cancellationToken = default);

    Task<TResponse?> SendAsync<TRequest, TResponse>(
        HttpMethod method,
        string path,
        TRequest request,
        string operationName,
        bool allowNotFound = false,
        CancellationToken cancellationToken = default);

    Task SendAsync(
        HttpMethod method,
        string path,
        string operationName,
        CancellationToken cancellationToken = default);

    Task SendAsync<TRequest>(
        HttpMethod method,
        string path,
        TRequest request,
        string operationName,
        CancellationToken cancellationToken = default);
}

public sealed class HongdalJsonApiClient : IHongdalJsonApiClient
{
    private readonly HongdalProtectedApiClient _protectedClient;

    public HongdalJsonApiClient(HongdalProtectedApiClient protectedClient)
    {
        _protectedClient = protectedClient;
    }

    public Task<TResponse?> GetAsync<TResponse>(
        string path,
        string operationName,
        bool allowNotFound = true,
        CancellationToken cancellationToken = default)
        => SendAsync<TResponse>(HttpMethod.Get, path, operationName, allowNotFound, cancellationToken);

    public async Task<TResponse?> SendAsync<TResponse>(
        HttpMethod method,
        string path,
        string operationName,
        bool allowNotFound = false,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendCoreAsync(method, path, operationName, cancellationToken);
        if (allowNotFound && response.StatusCode == HttpStatusCode.NotFound)
        {
            return default;
        }

        await EnsureSuccessAsync(response, operationName, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return default;
        }

        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
    }

    public async Task<TResponse?> SendAsync<TRequest, TResponse>(
        HttpMethod method,
        string path,
        TRequest request,
        string operationName,
        bool allowNotFound = false,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendCoreAsync(method, path, request, operationName, cancellationToken);
        if (allowNotFound && response.StatusCode == HttpStatusCode.NotFound)
        {
            return default;
        }

        await EnsureSuccessAsync(response, operationName, cancellationToken);
        if (response.StatusCode == HttpStatusCode.NoContent)
        {
            return default;
        }

        return await response.Content.ReadFromJsonAsync<TResponse>(cancellationToken: cancellationToken);
    }

    public async Task SendAsync(
        HttpMethod method,
        string path,
        string operationName,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendCoreAsync(method, path, operationName, cancellationToken);
        await EnsureSuccessAsync(response, operationName, cancellationToken);
    }

    public async Task SendAsync<TRequest>(
        HttpMethod method,
        string path,
        TRequest request,
        string operationName,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendCoreAsync(method, path, request, operationName, cancellationToken);
        await EnsureSuccessAsync(response, operationName, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendCoreAsync(
        HttpMethod method,
        string path,
        string operationName,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _protectedClient.SendAsync(method, path, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"{operationName} API에 연결할 수 없습니다.", ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new InvalidOperationException($"{operationName} API 응답 시간이 초과되었습니다.", ex);
        }
    }

    private async Task<HttpResponseMessage> SendCoreAsync<TRequest>(
        HttpMethod method,
        string path,
        TRequest request,
        string operationName,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _protectedClient.SendAsProtectedJsonAsync(method, path, request, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException($"{operationName} API에 연결할 수 없습니다.", ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new InvalidOperationException($"{operationName} API 응답 시간이 초과되었습니다.", ex);
        }
    }

    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response,
        string operationName,
        CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var detail = string.IsNullOrWhiteSpace(body) ? string.Empty : $": {body}";
        throw new InvalidOperationException(
            $"{operationName} API 실패: HTTP {(int)response.StatusCode}{detail}");
    }
}
