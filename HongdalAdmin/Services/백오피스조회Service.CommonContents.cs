using System.Net.Http.Json;
using Hongdal.Contracts.CommonContents;

namespace HongdalAdmin.Services;

public sealed partial class 백오피스조회Service
{
    public async Task<IReadOnlyList<관리자공통콘텐츠요약응답>> 공통콘텐츠목록조회Async(CancellationToken cancellationToken = default)
    {
        ApplyAuthorizationHeader();

        var result = await _httpClient.GetFromJsonAsync<List<관리자공통콘텐츠요약응답>>("api/v1/admin/common-contents", cancellationToken);
        return result ?? [];
    }

    public async Task<관리자공통콘텐츠상세응답?> 공통콘텐츠상세조회Async(long id, CancellationToken cancellationToken = default)
    {
        ApplyAuthorizationHeader();

        return await _httpClient.GetFromJsonAsync<관리자공통콘텐츠상세응답>($"api/v1/admin/common-contents/{id}", cancellationToken);
    }

    public async Task<관리자공통콘텐츠상세응답?> 공통콘텐츠등록Async(관리자공통콘텐츠저장요청 request, CancellationToken cancellationToken = default)
    {
        ApplyAuthorizationHeader();

        var response = await _httpClient.PostAsJsonAsync("api/v1/admin/common-contents", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<관리자공통콘텐츠상세응답>(cancellationToken: cancellationToken);
    }

    public async Task<관리자공통콘텐츠상세응답?> 공통콘텐츠수정Async(long id, 관리자공통콘텐츠저장요청 request, CancellationToken cancellationToken = default)
    {
        ApplyAuthorizationHeader();

        var response = await _httpClient.PutAsJsonAsync($"api/v1/admin/common-contents/{id}", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<관리자공통콘텐츠상세응답>(cancellationToken: cancellationToken);
    }

    public async Task 공통콘텐츠활성화변경Async(long id, bool enabled, CancellationToken cancellationToken = default)
    {
        ApplyAuthorizationHeader();

        var response = await _httpClient.PatchAsync($"api/v1/admin/common-contents/{id}/active?enabled={enabled.ToString().ToLowerInvariant()}", null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<공통콘텐츠보상정책Dto>> 공통콘텐츠보상정책목록조회Async(CancellationToken cancellationToken = default)
    {
        ApplyAuthorizationHeader();

        var result = await _httpClient.GetFromJsonAsync<List<공통콘텐츠보상정책Dto>>("api/v1/admin/common-contents/reward-policies", cancellationToken);
        return result ?? [];
    }

    public async Task<공통콘텐츠보상정책Dto?> 공통콘텐츠보상정책등록Async(공통콘텐츠보상정책Dto request, CancellationToken cancellationToken = default)
    {
        ApplyAuthorizationHeader();

        var response = await _httpClient.PostAsJsonAsync("api/v1/admin/common-contents/reward-policies", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<공통콘텐츠보상정책Dto>(cancellationToken: cancellationToken);
    }
}
