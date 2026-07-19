using System.Net.Http.Headers;
using System.Net.Http.Json;
using Ssalddel.Contracts.Admin.Customs;

namespace SsalddelAdmin.Services;

public sealed class HsCodeOperationsService
{
    private readonly HttpClient _httpClient;
    private readonly 관리자인증세션Service _session;
    private readonly bool _useMemoryFallback;

    public HsCodeOperationsService(HttpClient httpClient, 관리자인증세션Service session, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _session = session;
        _useMemoryFallback = configuration.GetValue("AdminData:UseMemory", false);
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
        if (_useMemoryFallback)
        {
            var items = BuildMemoryItems();
            return new AdminHsCodeListResponse
            {
                Items = items,
                TotalCount = items.Count,
                Page = page,
                PageSize = pageSize
            };
        }

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
        if (_useMemoryFallback)
        {
            var item = BuildMemoryItems().FirstOrDefault(x => x.Id == entryId) ?? BuildMemoryItems()[0];
            item.BusinessCategory = payload.BusinessCategory;
            item.BusinessCategoryLabel = payload.BusinessCategory switch
            {
                10 => "식품 관련",
                20 => "일반 화물",
                30 => "복합",
                _ => "미분류"
            };
            item.BusinessCategoryReason = payload.Reason;
            return item;
        }

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
        if (_useMemoryFallback)
        {
            var item = BuildMemoryItems().FirstOrDefault(x => x.Id == entryId) ?? BuildMemoryItems()[0];
            item.RiskTags = item.RiskTags
                .Concat([BuildMemoryTag(900 + entryId, payload)])
                .ToArray();
            return item;
        }

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
        if (_useMemoryFallback)
        {
            var item = BuildMemoryItems().First();
            item.RiskTags = item.RiskTags
                .Select(x => x.Id == tagId ? BuildMemoryTag(tagId, payload) : x)
                .ToArray();
            return item;
        }

        using var request = CreateRequest(HttpMethod.Put, $"api/v1/admin/hs-codes/risk-tags/{tagId}");
        request.Content = JsonContent.Create(payload);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<AdminHsCodeEntryResponse>(cancellationToken: cancellationToken);
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path)
    {
        var request = new HttpRequestMessage(method, path);
        if (!string.IsNullOrWhiteSpace(_session.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _session.AccessToken);
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

    private static IReadOnlyList<AdminHsCodeEntryResponse> BuildMemoryItems()
        =>
        [
            new()
            {
                Id = 1,
                Code = "8504.40",
                NormalizedCode = "850440",
                KoreanName = "정지형 변환기",
                EnglishName = "Static converters",
                BusinessCategory = 20,
                BusinessCategoryLabel = "일반 화물",
                BusinessCategoryReason = "전자부품 배송 검토 대상",
                IsActive = true,
                RiskTags =
                [
                    new()
                    {
                        Id = 101,
                        TagType = 60,
                        TagTypeLabel = "전기/인증 확인",
                        Label = "KC 인증 검토",
                        Reason = "전기용품 인증 여부를 확인합니다.",
                        Source = 10,
                        SourceLabel = "운영자",
                        IsActive = true
                    }
                ]
            },
            new()
            {
                Id = 2,
                Code = "2106.90",
                NormalizedCode = "210690",
                KoreanName = "조제 식료품",
                EnglishName = "Food preparations",
                BusinessCategory = 10,
                BusinessCategoryLabel = "식품 관련",
                BusinessCategoryReason = "수입식품 신고와 검역 검토 필요",
                IsActive = true,
                RiskTags =
                [
                    new()
                    {
                        Id = 201,
                        TagType = 20,
                        TagTypeLabel = "검역/식품신고 확인",
                        Label = "수입식품 신고",
                        Reason = "식품위생법 신고 대상일 수 있습니다.",
                        Source = 10,
                        SourceLabel = "운영자",
                        IsActive = true
                    },
                    new()
                    {
                        Id = 202,
                        TagType = 30,
                        TagTypeLabel = "조제식품/보충제 검토",
                        Label = "성분표 확인",
                        Reason = "성분에 따라 통관 보류 가능성이 있습니다.",
                        Source = 20,
                        SourceLabel = "관세사",
                        IsActive = true
                    }
                ]
            }
        ];

    private static AdminHsCodeRiskTagResponse BuildMemoryTag(long tagId, AdminHsCodeRiskTagUpdateRequest payload)
        => new()
        {
            Id = tagId,
            TagType = payload.TagType,
            TagTypeLabel = payload.TagType switch
            {
                20 => "검역/식품신고 확인",
                30 => "조제식품/보충제 검토",
                50 => "화학물질 확인",
                60 => "전기/인증 확인",
                70 => "배터리 포함 가능",
                _ => "주의 태그"
            },
            Label = payload.Label,
            Reason = payload.Reason,
            Source = 10,
            SourceLabel = "운영자",
            IsActive = payload.IsActive
        };
}
