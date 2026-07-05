using System.Net.Http.Headers;
using System.Net.Http.Json;
using Hongdal.Contracts.Admin.Customs;

namespace CustomsBrokerApp.Services;

public sealed class HsCodeCorrectionService
{
    private readonly HttpClient _httpClient;
    private readonly CustomsBrokerAuthService _authService;

    public HsCodeCorrectionService(HttpClient httpClient, CustomsBrokerAuthService authService)
    {
        _httpClient = httpClient;
        _authService = authService;
    }

    public async Task<AdminHsCodeListResponse> SearchAsync(
        string? query,
        string? businessCategory,
        string? tagType,
        bool includeInactive,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Get, BuildSearchPath(query, businessCategory, tagType, includeInactive, page, pageSize));
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<AdminHsCodeListResponse>(cancellationToken: cancellationToken)
               ?? new AdminHsCodeListResponse();
    }

    public async Task<AdminHsCodeEntryResponse?> UpdateBusinessCategoryAsync(
        long entryId,
        AdminHsCodeBusinessCategoryUpdateRequest payload,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Put, $"api/v1/admin/hs-codes/{entryId}/business-category");
        request.Content = JsonContent.Create(payload);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<AdminHsCodeEntryResponse>(cancellationToken: cancellationToken);
    }

    public async Task<AdminHsCodeEntryResponse?> SaveRiskTagAsync(
        long entryId,
        AdminHsCodeRiskTagUpdateRequest payload,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Post, $"api/v1/admin/hs-codes/{entryId}/risk-tags");
        request.Content = JsonContent.Create(payload);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<AdminHsCodeEntryResponse>(cancellationToken: cancellationToken);
    }

    public async Task<AdminHsCodeEntryResponse?> UpdateRiskTagAsync(
        long tagId,
        AdminHsCodeRiskTagUpdateRequest payload,
        CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Put, $"api/v1/admin/hs-codes/risk-tags/{tagId}");
        request.Content = JsonContent.Create(payload);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<AdminHsCodeEntryResponse>(cancellationToken: cancellationToken);
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path)
    {
        var request = new HttpRequestMessage(method, path);
        if (!string.IsNullOrWhiteSpace(_authService.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authService.AccessToken);
        }

        return request;
    }

    private static string BuildSearchPath(
        string? query,
        string? businessCategory,
        string? tagType,
        bool includeInactive,
        int page,
        int pageSize)
    {
        var parameters = new List<string>();
        Add(parameters, "query", query);
        Add(parameters, "businessCategory", businessCategory);
        Add(parameters, "tagType", tagType);
        parameters.Add($"includeInactive={includeInactive.ToString().ToLowerInvariant()}");
        parameters.Add($"page={page}");
        parameters.Add($"pageSize={pageSize}");

        return $"api/v1/admin/hs-codes?{string.Join("&", parameters)}";
    }

    private static void Add(ICollection<string> parameters, string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            parameters.Add($"{key}={Uri.EscapeDataString(value.Trim())}");
        }
    }
}
