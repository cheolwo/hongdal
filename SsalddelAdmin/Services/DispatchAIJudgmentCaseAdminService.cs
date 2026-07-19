using System.Net.Http.Headers;
using System.Net.Http.Json;
using Ssalddel.Contracts.Admin.Dispatch;

namespace SsalddelAdmin.Services;

public sealed class DispatchAIJudgmentCaseAdminService
{
    private const string Endpoint = "api/v1/admin/dispatch/ai-judgment-cases";

    private readonly HttpClient _httpClient;
    private readonly 관리자인증세션Service _session;
    private readonly bool _useMemoryFallback;

    public DispatchAIJudgmentCaseAdminService(HttpClient httpClient, 관리자인증세션Service session, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _session = session;
        _useMemoryFallback = configuration.GetValue("AdminData:UseMemory", false);
    }

    public async Task<DispatchAIJudgmentCaseCatalogDto> GetCatalogAsync(CancellationToken cancellationToken = default)
    {
        if (_useMemoryFallback)
        {
            return BuildMemoryCatalog();
        }

        using var request = CreateRequest(HttpMethod.Get, Endpoint);
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<DispatchAIJudgmentCaseCatalogDto>(cancellationToken: cancellationToken)
               ?? new DispatchAIJudgmentCaseCatalogDto();
    }

    public async Task<DispatchAIJudgmentCaseDto> CreateAsync(
        DispatchAIJudgmentCaseCreateRequest createRequest,
        CancellationToken cancellationToken = default)
    {
        if (_useMemoryFallback)
        {
            return BuildCreatedCase(createRequest);
        }

        using var request = CreateRequest(HttpMethod.Post, Endpoint);
        request.Content = JsonContent.Create(createRequest);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<DispatchAIJudgmentCaseDto>(cancellationToken: cancellationToken)
               ?? new DispatchAIJudgmentCaseDto();
    }

    public async Task<DispatchAIJudgmentCaseDto> PromoteSuggestionAsync(
        string suggestionKey,
        DispatchAIJudgmentCasePromoteSuggestionRequest promoteRequest,
        CancellationToken cancellationToken = default)
    {
        if (_useMemoryFallback)
        {
            var suggestion = BuildMemoryCatalog().Suggestions.FirstOrDefault(x =>
                string.Equals(x.SuggestionKey, suggestionKey, StringComparison.OrdinalIgnoreCase));

            return new DispatchAIJudgmentCaseDto
            {
                CaseId = $"MEM-CASE-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
                Title = suggestion?.Title ?? suggestionKey,
                RelatedOS = suggestion?.RelatedOS ?? "배차 운영",
                Keywords = suggestion?.Keywords.ToList() ?? [],
                SituationSummary = suggestion?.SituationSummary ?? string.Empty,
                JudgmentSummary = promoteRequest.JudgmentSummary ?? suggestion?.SuggestedJudgmentSummary ?? string.Empty,
                UserDecision = promoteRequest.UserDecision ?? suggestion?.DefaultUserDecision ?? "승인",
                BalancedDecision = promoteRequest.BalancedDecision ?? suggestion?.DefaultBalancedDecision ?? "운영자 승인",
                Source = "memory",
                Active = promoteRequest.Active,
                CreatedBy = _session.UserName
            };
        }

        using var request = CreateRequest(HttpMethod.Post, $"{Endpoint}/suggestions/{Uri.EscapeDataString(suggestionKey)}/promote");
        request.Content = JsonContent.Create(promoteRequest);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<DispatchAIJudgmentCaseDto>(cancellationToken: cancellationToken)
               ?? new DispatchAIJudgmentCaseDto();
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

    private DispatchAIJudgmentCaseDto BuildCreatedCase(DispatchAIJudgmentCaseCreateRequest createRequest)
        => new()
        {
            CaseId = $"MEM-CASE-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
            Title = createRequest.Title,
            RelatedOS = createRequest.RelatedOS,
            Keywords = createRequest.Keywords.ToList(),
            SituationSummary = createRequest.SituationSummary,
            JudgmentSummary = createRequest.JudgmentSummary,
            UserDecision = createRequest.UserDecision,
            BalancedDecision = createRequest.BalancedDecision,
            Source = createRequest.Source ?? "memory",
            Active = createRequest.Active,
            CreatedBy = _session.UserName
        };

    private static DispatchAIJudgmentCaseCatalogDto BuildMemoryCatalog()
        => new()
        {
            Suggestions =
            [
                new()
                {
                    SuggestionKey = "domestic-bundle-nearby-driver",
                    Title = "근접 기사에게 국내화물 묶음 제안",
                    RelatedOS = "국내 화물 운송 OS",
                    Keywords = ["국내화물", "묶음", "기사위치", "운임"],
                    SituationSummary = "상차지가 가까운 두 의뢰가 같은 권역 하차지로 이동하고 근처 기사가 대기 중입니다.",
                    SuggestedJudgmentSummary = "운임 합계와 예상 추가 이동거리가 맞으면 AI 묶음을 승인하고, 상차 시간 충돌만 확인합니다.",
                    DefaultUserDecision = "승인",
                    DefaultBalancedDecision = "상차 시간 조건부 승인",
                    Source = "memory"
                },
                new()
                {
                    SuggestionKey = "food-neighbor-scope-manual-hold",
                    Title = "음식배달 인접권 묶음 보류",
                    RelatedOS = "음식배달 OS",
                    Keywords = ["음식배달", "인접권", "조리완료", "보류"],
                    SituationSummary = "픽업지는 가깝지만 고객 전달지가 서로 다른 배달권이고 조리 완료 시간이 20분 이상 벌어집니다.",
                    SuggestedJudgmentSummary = "고객 전달 지연 위험이 커서 자동 묶음은 보류하고 운영자가 단건 배차로 전환합니다.",
                    DefaultUserDecision = "보류",
                    DefaultBalancedDecision = "단건 배차 권고",
                    Source = "memory"
                }
            ],
            Cases =
            [
                new()
                {
                    CaseId = "MEM-CASE-001",
                    Title = "상차권역 일치 국내화물 2건 승인",
                    RelatedOS = "국내 화물 운송 OS",
                    Keywords = ["국내화물", "묶음", "상차권역"],
                    SituationSummary = "서울 동남권 상차 2건과 경기 남부 하차 2건을 같은 기사에게 묶어 검토했습니다.",
                    JudgmentSummary = "픽업 시간과 적재 가능량이 맞아 조건부 승인했습니다.",
                    UserDecision = "조건부 승인",
                    BalancedDecision = "운영자 확인 후 승인",
                    Source = "memory",
                    Active = true,
                    CreatedBy = "memory"
                }
            ]
        };
}
