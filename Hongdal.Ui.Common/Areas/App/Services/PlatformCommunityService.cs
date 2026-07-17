using System.Net.Http.Json;
using Hongdal.Contracts.Common.Community;
using Hongdal.Contracts.Common.Customs;
using Hongdal.Contracts.Common.Orderer;
using Hongdal.Contracts.Common.PublicData;
using Hongdal.Contracts.Common.Versioning;
using Microsoft.AspNetCore.Components.Forms;

namespace Hongdal.Ui.Common.Areas.App.Services;

public sealed class PlatformCommunityService
{
    private readonly HttpClient _httpClient;
    private readonly HongdalProtectedApiClient _protectedApiClient;

    public PlatformCommunityService(
        HttpClient httpClient,
        HongdalProtectedApiClient protectedApiClient)
    {
        _httpClient = httpClient;
        _protectedApiClient = protectedApiClient;
    }

    public async Task<PlatformCommunityPostListResponse> GetPostsAsync(
        string appKey,
        CancellationToken cancellationToken = default)
    {
        var path = $"api/v1/community/posts?appKey={Uri.EscapeDataString(appKey)}&page=1&pageSize=20";
        return await _httpClient.GetFromJsonAsync<PlatformCommunityPostListResponse>(path, cancellationToken)
               ?? new PlatformCommunityPostListResponse();
    }

    public async Task<PlatformCommunityPostListResponse> GetBoardPostsAsync(
        string appKey,
        string? boardKey = null,
        string? category = null,
        string? workflowTag = null,
        string? roleTag = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>
        {
            $"appKey={Uri.EscapeDataString(appKey)}",
            $"page={Math.Max(1, page)}",
            $"pageSize={Math.Clamp(pageSize, 1, 50)}"
        };
        AddQueryValue(query, "boardKey", boardKey);
        AddQueryValue(query, "category", category);
        AddQueryValue(query, "workflowTag", workflowTag);
        AddQueryValue(query, "roleTag", roleTag);

        return await _httpClient.GetFromJsonAsync<PlatformCommunityPostListResponse>(
                   $"api/v1/community/posts?{string.Join("&", query)}",
                   cancellationToken)
               ?? new PlatformCommunityPostListResponse();
    }

    public async Task<IReadOnlyList<CommunityBoardSummaryResponse>> GetBoardSummariesAsync(
        string appKey,
        CancellationToken cancellationToken = default)
        => await _httpClient.GetFromJsonAsync<IReadOnlyList<CommunityBoardSummaryResponse>>(
               $"api/v1/community/posts/board-summaries?appKey={Uri.EscapeDataString(appKey)}",
               cancellationToken)
           ?? [];

    public async Task<PlatformCommunityPostResponse?> GetPostAsync(
        long postId,
        CancellationToken cancellationToken = default)
        => await _httpClient.GetFromJsonAsync<PlatformCommunityPostResponse>(
            $"api/v1/community/posts/{postId}",
            cancellationToken);

    public async Task<PlatformCommunityPostTranslationResponse?> TranslatePostAsync(
        long postId,
        string targetLanguageCode,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync(
            $"api/v1/community/posts/{postId}/translations/{Uri.EscapeDataString(targetLanguageCode)}",
            content: null,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PlatformCommunityPostTranslationResponse>(
            cancellationToken: cancellationToken);
    }

    public async Task<CommunityPostOpportunityListResponse?> GetPostOpportunitiesAsync(
        long postId,
        string displayLanguageCode = CommunityDisplayLanguageCodes.Korean,
        CancellationToken cancellationToken = default)
        => await _httpClient.GetFromJsonAsync<CommunityPostOpportunityListResponse>(
            $"api/v1/community/posts/{postId}/opportunities?displayLanguage={Uri.EscapeDataString(displayLanguageCode)}",
            cancellationToken);

    public async Task<StartCommunityPostParticipationResponse?> StartPostParticipationAsync(
        long postId,
        StartCommunityPostParticipationRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PostAsProtectedJsonAsync(
            $"api/v1/community/posts/{postId}/opportunities/participation/start",
            request,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<StartCommunityPostParticipationResponse>(
            cancellationToken: cancellationToken);
    }

    public async Task<PromoteCommunityPostParticipationResponse?> PromotePostParticipationAsync(
        long postId,
        PromoteCommunityPostParticipationRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PostAsProtectedJsonAsync(
            $"api/v1/community/posts/{postId}/opportunities/participation/provisional-ledger",
            request,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PromoteCommunityPostParticipationResponse>(
            cancellationToken: cancellationToken);
    }

    public async Task<JoinCommunityPostProfessionalResponse?> JoinPostProfessionalRoleAsync(
        long postId,
        JoinCommunityPostProfessionalRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PostAsProtectedJsonAsync(
            $"api/v1/community/posts/{postId}/opportunities/participation/professionals",
            request,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JoinCommunityPostProfessionalResponse>(
            cancellationToken: cancellationToken);
    }

    public async Task<JoinCommunityPostPartyRoleResponse?> JoinPostPartyRoleAsync(
        long postId,
        JoinCommunityPostPartyRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PostAsProtectedJsonAsync(
            $"api/v1/community/posts/{postId}/opportunities/participation/party-roles",
            request,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JoinCommunityPostPartyRoleResponse>(
            cancellationToken: cancellationToken);
    }

    public async Task<CommunityVoteResponse?> CastCommunityVoteAsync(
        Guid voteId,
        CommunityVoteCastRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PostAsProtectedJsonAsync(
            $"api/v1/community/votes/{voteId}/votes",
            request,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CommunityVoteResponse>(
            cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<PlatformCommunityPostLedgerChoiceResponse>> GetMyLedgersAsync(
        string? workflowTag = null,
        CancellationToken cancellationToken = default)
    {
        var path = "api/v1/community/posts/my-ledgers";
        if (!string.IsNullOrWhiteSpace(workflowTag))
        {
            path += $"?workflowTag={Uri.EscapeDataString(workflowTag)}";
        }

        using var response = await _protectedApiClient.GetAsync(path, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<PlatformCommunityPostLedgerChoiceResponse>>(
                   cancellationToken: cancellationToken)
               ?? [];
    }

    public async Task<IReadOnlyList<PlatformCommunityPostLedgerChoiceResponse>> GetSharedLedgersAsync(
        string? workflowTag = null,
        CancellationToken cancellationToken = default)
    {
        var path = "api/v1/community/posts/shared-ledgers";
        if (!string.IsNullOrWhiteSpace(workflowTag))
        {
            path += $"?workflowTag={Uri.EscapeDataString(workflowTag)}";
        }

        using var response = await _protectedApiClient.GetAsync(path, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<PlatformCommunityPostLedgerChoiceResponse>>(
                   cancellationToken: cancellationToken)
               ?? [];
    }

    public async Task<PlatformCommunityPostLedgerContextResponse?> GetLedgerContextAsync(
        string ledgerId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.GetAsync(
            $"api/v1/community/posts/ledgers/{Uri.EscapeDataString(ledgerId)}/context",
            cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PlatformCommunityPostLedgerContextResponse>(
            cancellationToken: cancellationToken);
    }

    public async Task<커뮤니티원장공개설정Response?> GetLedgerSharingSettingsAsync(
        string ledgerId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.GetAsync(
            $"api/v1/community/ledgers/{Uri.EscapeDataString(ledgerId)}/sharing",
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<커뮤니티원장공개설정Response>(cancellationToken: cancellationToken);
    }

    public async Task<커뮤니티원장공개설정Response?> UpdateLedgerSharingSettingsAsync(
        string ledgerId,
        커뮤니티원장공개설정변경Request request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PutAsProtectedJsonAsync(
            $"api/v1/community/ledgers/{Uri.EscapeDataString(ledgerId)}/sharing",
            request,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<커뮤니티원장공개설정Response>(cancellationToken: cancellationToken);
    }

    public async Task<CommunityLedgerRoleAccessSettingsResponse?> GetLedgerRoleAccessSettingsAsync(
        string ledgerId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.GetAsync(
            $"api/v1/community/ledgers/{Uri.EscapeDataString(ledgerId)}/role-access",
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CommunityLedgerRoleAccessSettingsResponse>(
            cancellationToken: cancellationToken);
    }

    public async Task<CommunityLedgerRoleAccessSettingsResponse?> UpdateLedgerRoleAccessSettingsAsync(
        string ledgerId,
        CommunityLedgerRoleAccessUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PutAsProtectedJsonAsync(
            $"api/v1/community/ledgers/{Uri.EscapeDataString(ledgerId)}/role-access",
            request,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CommunityLedgerRoleAccessSettingsResponse>(
            cancellationToken: cancellationToken);
    }

    public async Task<CommunityLedgerBlockAssignmentSettingsResponse?> GetLedgerBlockAssignmentsAsync(
        string ledgerId,
        string blockId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.GetAsync(
            $"api/v1/community/ledgers/{Uri.EscapeDataString(ledgerId)}/blocks/{Uri.EscapeDataString(blockId)}/assignees",
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CommunityLedgerBlockAssignmentSettingsResponse>(
            cancellationToken: cancellationToken);
    }

    public async Task<CommunityLedgerBlockAssignmentSettingsResponse?> UpdateLedgerBlockAssignmentsAsync(
        string ledgerId,
        string blockId,
        CommunityLedgerBlockAssignmentUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PutAsProtectedJsonAsync(
            $"api/v1/community/ledgers/{Uri.EscapeDataString(ledgerId)}/blocks/{Uri.EscapeDataString(blockId)}/assignees",
            request,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CommunityLedgerBlockAssignmentSettingsResponse>(
            cancellationToken: cancellationToken);
    }

    public async Task<커뮤니티원장재사용Response?> ReuseSharedLedgerAsync(
        string ledgerId,
        string? newTitle = null,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PostAsProtectedJsonAsync(
            $"api/v1/community/ledgers/{Uri.EscapeDataString(ledgerId)}/sharing/reuse",
            new 커뮤니티원장재사용Request { 새제목 = newTitle },
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<커뮤니티원장재사용Response>(cancellationToken: cancellationToken);
    }

    public async Task<PlatformCommunityBoardListResponse> GetBoardsAsync(
        string appKey,
        string status = PlatformCommunityBoardRequestStatuses.Approved,
        CancellationToken cancellationToken = default)
    {
        var isPublicList = string.Equals(
            status,
            PlatformCommunityBoardRequestStatuses.Approved,
            StringComparison.OrdinalIgnoreCase);
        var path = isPublicList
            ? $"api/v1/community/boards?appKey={Uri.EscapeDataString(appKey)}"
            : $"api/v1/community/boards/requests?appKey={Uri.EscapeDataString(appKey)}&status={Uri.EscapeDataString(status)}";
        if (isPublicList)
        {
            return await _httpClient.GetFromJsonAsync<PlatformCommunityBoardListResponse>(path, cancellationToken)
                   ?? new PlatformCommunityBoardListResponse();
        }

        using var response = await _protectedApiClient.GetAsync(path, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PlatformCommunityBoardListResponse>(cancellationToken: cancellationToken)
               ?? new PlatformCommunityBoardListResponse();
    }

    public async Task<VersionFeatureFlagsResponse> GetVersionWorkflowMetadataAsync(
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<VersionFeatureFlagsResponse>(
                   "api/v1/version-feature-flags",
                   cancellationToken)
               ?? new VersionFeatureFlagsResponse();
    }

    public async Task<GroupImportHsCodeSearchResponse> SearchGroupImportHsCodesAsync(
        string? query = null,
        int? businessCategory = null,
        int page = 1,
        int pageSize = 30,
        CancellationToken cancellationToken = default)
    {
        var queryValues = new List<string>();
        AddQueryValue(queryValues, "query", query);
        if (businessCategory.HasValue)
        {
            queryValues.Add($"businessCategory={businessCategory.Value}");
        }

        queryValues.Add($"page={Math.Max(1, page)}");
        queryValues.Add($"pageSize={Math.Clamp(pageSize, 10, 50)}");
        var path = $"api/v1/customs/hs-codes?{string.Join("&", queryValues)}";

        return await _httpClient.GetFromJsonAsync<GroupImportHsCodeSearchResponse>(path, cancellationToken)
               ?? new GroupImportHsCodeSearchResponse();
    }

    public async Task<HsCountryImportUnitPriceSimulationResult?> GetGroupImportUnitPriceAsync(
        HsCountryMonthlyTradeUnitPriceRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "api/v1/orderer/public-data/customs/hs-country-import-unit-price-simulation",
            request,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<HsCountryImportUnitPriceSimulationResult>(
            cancellationToken: cancellationToken);
    }

    public async Task<CommunityDriverAvailabilityListResponse> GetCommunityDriverAvailabilityAsync(
        string? operatingArea = null,
        CancellationToken cancellationToken = default)
    {
        var suffix = string.IsNullOrWhiteSpace(operatingArea)
            ? string.Empty
            : $"?operatingArea={Uri.EscapeDataString(operatingArea.Trim())}";
        return await _httpClient.GetFromJsonAsync<CommunityDriverAvailabilityListResponse>(
                   $"api/v1/community/driver-availability{suffix}",
                   cancellationToken)
               ?? new CommunityDriverAvailabilityListResponse();
    }

    public async Task<CommunityDriverInquiryResponse?> CreateCommunityDriverInquiryAsync(
        Guid postId,
        CommunityDriverInquiryCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PostAsProtectedJsonAsync(
            $"api/v1/community/driver-availability/{postId}/inquiries",
            request,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CommunityDriverInquiryResponse>(
            cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<CommunityDriverInquiryResponse>> GetMyCommunityDriverInquiriesAsync(
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.GetAsync(
            "api/v1/community/driver-availability/my-inquiries",
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IReadOnlyList<CommunityDriverInquiryResponse>>(
                   cancellationToken: cancellationToken)
               ?? [];
    }

    public async Task<DomesticProducerCandidateQueryResponse> GetDomesticProducerCandidatesAsync(
        Guid campaignId,
        string? search = null,
        string? regionCode = null,
        string? product = null,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        AddQueryValue(query, "search", search);
        AddQueryValue(query, "regionCode", regionCode);
        AddQueryValue(query, "product", product);
        var suffix = query.Count == 0 ? string.Empty : $"?{string.Join("&", query)}";

        using var response = await _protectedApiClient.GetAsync(
            $"api/v1/orderer/domestic-group-purchases/{campaignId}/producer-connections/candidates{suffix}",
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DomesticProducerCandidateQueryResponse>(
                   cancellationToken: cancellationToken)
               ?? new DomesticProducerCandidateQueryResponse();
    }

    public async Task<DomesticProducerContactRequestDraftResponse?> CreateDomesticProducerContactDraftAsync(
        Guid campaignId,
        DomesticProducerContactRequestDraftRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PostAsProtectedJsonAsync(
            $"api/v1/orderer/domestic-group-purchases/{campaignId}/producer-connections/contact-request-drafts",
            request,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DomesticProducerContactRequestDraftResponse>(
            cancellationToken: cancellationToken);
    }

    public async Task<DomesticGroupPurchaseRepresentativeCandidateQueryResponse> GetDomesticGroupPurchaseRepresentativesAsync(
        Guid campaignId,
        string? search = null,
        string? operatingAreaCode = null,
        string? product = null,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        AddQueryValue(query, "search", search);
        AddQueryValue(query, "operatingAreaCode", operatingAreaCode);
        AddQueryValue(query, "product", product);
        var suffix = query.Count == 0 ? string.Empty : $"?{string.Join("&", query)}";

        using var response = await _protectedApiClient.GetAsync(
            $"api/v1/orderer/domestic-group-purchases/{campaignId}/producer-connections/representatives{suffix}",
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DomesticGroupPurchaseRepresentativeCandidateQueryResponse>(
                   cancellationToken: cancellationToken)
               ?? new DomesticGroupPurchaseRepresentativeCandidateQueryResponse();
    }

    public async Task<DomesticProducerSupplyOfferDraftResponse?> CreateDomesticProducerSupplyOfferDraftAsync(
        Guid campaignId,
        DomesticProducerSupplyOfferDraftRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PostAsProtectedJsonAsync(
            $"api/v1/orderer/domestic-group-purchases/{campaignId}/producer-connections/supply-offer-drafts",
            request,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DomesticProducerSupplyOfferDraftResponse>(
            cancellationToken: cancellationToken);
    }

    public async Task<DomesticGroupPurchaseSupplyCompatibilityPreviewResponse?> PreviewDomesticSupplyCompatibilityAsync(
        Guid campaignId,
        DomesticGroupPurchaseSupplyCompatibilityPreviewRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PostAsProtectedJsonAsync(
            $"api/v1/orderer/domestic-group-purchases/{campaignId}/producer-connections/compatibility-previews",
            request,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DomesticGroupPurchaseSupplyCompatibilityPreviewResponse>(
            cancellationToken: cancellationToken);
    }

    public async Task<DomesticGroupPurchaseFulfillmentPlanResponse?> PreviewDomesticGroupPurchaseFulfillmentPlanAsync(
        Guid campaignId,
        DomesticGroupPurchaseFulfillmentPlanRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PostAsProtectedJsonAsync(
            $"api/v1/orderer/domestic-group-purchases/{campaignId}/fulfillment-plans/preview",
            request,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DomesticGroupPurchaseFulfillmentPlanResponse>(
            cancellationToken: cancellationToken);
    }

    public async Task<DomesticGroupPurchaseFulfillmentOrderDraftResponse?> CreateDomesticGroupPurchaseFulfillmentOrderDraftAsync(
        Guid campaignId,
        DomesticGroupPurchaseFulfillmentPlanRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PostAsProtectedJsonAsync(
            $"api/v1/orderer/domestic-group-purchases/{campaignId}/fulfillment-plans/order-drafts",
            request,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DomesticGroupPurchaseFulfillmentOrderDraftResponse>(
            cancellationToken: cancellationToken);
    }

    public async Task<DomesticGroupPurchaseNegotiationTimelineResponse> GetDomesticGroupPurchaseNegotiationTimelineAsync(
        Guid campaignId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.GetAsync(
            $"api/v1/orderer/domestic-group-purchases/{campaignId}/negotiation",
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DomesticGroupPurchaseNegotiationTimelineResponse>(
                   cancellationToken: cancellationToken)
               ?? new DomesticGroupPurchaseNegotiationTimelineResponse { GroupPurchaseCampaignId = campaignId };
    }

    public async Task<DomesticGroupPurchaseNegotiationEventResponse?> AppendDomesticGroupPurchaseNegotiationEventAsync(
        Guid campaignId,
        DomesticGroupPurchaseNegotiationEventRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PostAsProtectedJsonAsync(
            $"api/v1/orderer/domestic-group-purchases/{campaignId}/negotiation/events",
            request,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DomesticGroupPurchaseNegotiationEventResponse>(
            cancellationToken: cancellationToken);
    }

    public async Task<DomesticGroupPurchaseNegotiationIssueResponse?> OpenDomesticGroupPurchaseNegotiationIssueAsync(
        Guid campaignId,
        DomesticGroupPurchaseNegotiationIssueRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PostAsProtectedJsonAsync(
            $"api/v1/orderer/domestic-group-purchases/{campaignId}/negotiation/issues",
            request,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DomesticGroupPurchaseNegotiationIssueResponse>(
            cancellationToken: cancellationToken);
    }

    public async Task<DomesticGroupPurchaseNegotiationIssueResponse?> AddDomesticGroupPurchaseDeliberationPositionAsync(
        Guid campaignId,
        Guid issueId,
        DomesticGroupPurchaseDeliberationPositionRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PostAsProtectedJsonAsync(
            $"api/v1/orderer/domestic-group-purchases/{campaignId}/negotiation/issues/{issueId}/positions",
            request,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DomesticGroupPurchaseNegotiationIssueResponse>(
            cancellationToken: cancellationToken);
    }

    public async Task<DomesticGroupPurchaseNegotiationIssueResponse?> ResolveDomesticGroupPurchaseNegotiationIssueAsync(
        Guid campaignId,
        Guid issueId,
        DomesticGroupPurchaseNegotiationResolutionRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PostAsProtectedJsonAsync(
            $"api/v1/orderer/domestic-group-purchases/{campaignId}/negotiation/issues/{issueId}/resolution",
            request,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<DomesticGroupPurchaseNegotiationIssueResponse>(
            cancellationToken: cancellationToken);
    }

    private static void AddQueryValue(List<string> query, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            query.Add($"{name}={Uri.EscapeDataString(value.Trim())}");
        }
    }

    public async Task<PlatformCommunityBoardResponse?> CreateBoardRequestAsync(
        PlatformCommunityBoardCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PostAsProtectedJsonAsync("api/v1/community/boards", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PlatformCommunityBoardResponse>(cancellationToken: cancellationToken);
    }

    public async Task<PlatformCommunityBoardResponse?> ApproveBoardAsync(
        long boardRequestId,
        string operatorMemo,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PostAsProtectedJsonAsync(
            $"api/v1/community/boards/{boardRequestId}/approve",
            new PlatformCommunityBoardReviewRequest { OperatorMemo = operatorMemo },
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PlatformCommunityBoardResponse>(cancellationToken: cancellationToken);
    }

    public async Task<PlatformCommunityBoardResponse?> RejectBoardAsync(
        long boardRequestId,
        string operatorMemo,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PostAsProtectedJsonAsync(
            $"api/v1/community/boards/{boardRequestId}/reject",
            new PlatformCommunityBoardReviewRequest { OperatorMemo = operatorMemo },
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PlatformCommunityBoardResponse>(cancellationToken: cancellationToken);
    }

    public async Task<PlatformCommunityPostResponse?> CreatePostAsync(
        PlatformCommunityPostCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PostAsProtectedJsonAsync("api/v1/community/posts", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PlatformCommunityPostResponse>(cancellationToken: cancellationToken);
    }

    public async Task<PlatformCommunityPostResponse?> UpdatePostAsync(
        long postId,
        PlatformCommunityPostUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PutAsProtectedJsonAsync($"api/v1/community/posts/{postId}", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PlatformCommunityPostResponse>(cancellationToken: cancellationToken);
    }

    public async Task<PlatformCommunityPostResponse?> SetOperatorPinAsync(
        long postId,
        bool isOperatorPinned,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PostAsProtectedJsonAsync(
            $"api/v1/community/posts/{postId}/operator-pin",
            new PlatformCommunityPostOperatorPinRequest { IsOperatorPinned = isOperatorPinned },
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PlatformCommunityPostResponse>(cancellationToken: cancellationToken);
    }

    public async Task<PlatformCommunityPostResponse?> RecommendAsync(
        long postId,
        string recommenderKey,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PostAsProtectedJsonAsync(
            $"api/v1/community/posts/{postId}/recommendations",
            new PlatformCommunityPostRecommendationRequest { RecommenderKey = recommenderKey },
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PlatformCommunityPostResponse>(cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<PlatformCommunityPostCommentResponse>> GetCommentsAsync(
        long postId,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<IReadOnlyList<PlatformCommunityPostCommentResponse>>(
                   $"api/v1/community/posts/{postId}/comments",
                   cancellationToken)
               ?? [];
    }

    public async Task<PlatformCommunityPostCommentResponse?> CreateCommentAsync(
        long postId,
        PlatformCommunityPostCommentCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PostAsProtectedJsonAsync($"api/v1/community/posts/{postId}/comments", request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PlatformCommunityPostCommentResponse>(cancellationToken: cancellationToken);
    }

    public async Task DeleteCommentAsync(
        long postId,
        long commentId,
        string password,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.SendAsProtectedJsonAsync(
            HttpMethod.Delete,
            $"api/v1/community/posts/{postId}/comments/{commentId}",
            new PlatformCommunityPostPasswordRequest { Password = password },
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task ReportCommentAsync(long commentId, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync($"api/v1/community/posts/comments/{commentId}/reports", null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyList<PlatformCommunityPostAttachmentCommentResponse>> GetAttachmentCommentsAsync(
        long attachmentId,
        CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetFromJsonAsync<IReadOnlyList<PlatformCommunityPostAttachmentCommentResponse>>(
                   $"api/v1/community/posts/attachments/{attachmentId}/comments",
                   cancellationToken)
               ?? [];
    }

    public async Task<PlatformCommunityPostAttachmentCommentResponse?> CreateAttachmentCommentAsync(
        long attachmentId,
        PlatformCommunityPostAttachmentCommentCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PostAsProtectedJsonAsync(
            $"api/v1/community/posts/attachments/{attachmentId}/comments",
            request,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PlatformCommunityPostAttachmentCommentResponse>(cancellationToken: cancellationToken);
    }

    public async Task DeleteAttachmentCommentAsync(
        long attachmentId,
        long commentId,
        string password,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.SendAsProtectedJsonAsync(
            HttpMethod.Delete,
            $"api/v1/community/posts/attachments/{attachmentId}/comments/{commentId}",
            new PlatformCommunityPostPasswordRequest { Password = password },
            cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task ReportAttachmentCommentAsync(long commentId, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsync($"api/v1/community/posts/attachments/comments/{commentId}/reports", null, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<CommunityVoteListResponse> GetGroupPurchaseVotesAsync(
        string? communityScope = null,
        string? hsCode = null,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        AddQueryValue(query, "communityScope", communityScope);
        AddQueryValue(query, "hsCode", hsCode);
        var suffix = query.Count == 0 ? string.Empty : $"?{string.Join("&", query)}";
        var path = $"api/v1/orderer/group-purchase-demand-votes{suffix}";

        using var response = await _protectedApiClient.GetAsync(path, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CommunityVoteListResponse>(cancellationToken: cancellationToken)
               ?? new CommunityVoteListResponse();
    }

    public async Task<CommunityVoteResponse?> GetGroupPurchaseVoteAsync(
        Guid voteId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.GetAsync(
            $"api/v1/orderer/group-purchase-demand-votes/{voteId}",
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CommunityVoteResponse>(cancellationToken: cancellationToken);
    }

    public async Task<CommunityVoteResponse?> CreateGroupPurchaseVoteAsync(
        CommunityVoteCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PostAsProtectedJsonAsync(
            "api/v1/orderer/group-purchase-demand-votes",
            request,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CommunityVoteResponse>(cancellationToken: cancellationToken);
    }

    public async Task<CommunityVoteResponse?> CastGroupPurchaseVoteAsync(
        Guid voteId,
        CommunityVoteCastRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PostAsProtectedJsonAsync(
            $"api/v1/orderer/group-purchase-demand-votes/{voteId}/votes",
            request,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CommunityVoteResponse>(cancellationToken: cancellationToken);
    }

    public async Task<CommunityVoteResponse?> CloseVoteAsync(
        Guid voteId,
        CommunityVoteCloseRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PostAsProtectedJsonAsync(
            $"api/v1/community/votes/{voteId}/close",
            request,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CommunityVoteResponse>(cancellationToken: cancellationToken);
    }

    public async Task<CommunityVoteResolutionDocumentResponse?> CreateVoteResolutionAsync(
        Guid voteId,
        CommunityVoteResolutionDraftRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PostAsProtectedJsonAsync(
            $"api/v1/community/votes/{voteId}/resolution-documents",
            request,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CommunityVoteResolutionDocumentResponse>(cancellationToken: cancellationToken);
    }

    public async Task<CommunityVoteResolutionDocumentResponse?> MarkVoteResolutionReadyToSignAsync(
        Guid voteId,
        CommunityVoteResolutionReadyToSignRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PostAsProtectedJsonAsync(
            $"api/v1/community/votes/{voteId}/resolution-documents/ready-to-sign",
            request,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CommunityVoteResolutionDocumentResponse>(cancellationToken: cancellationToken);
    }

    public async Task<CommunityVoteResolutionDocumentResponse?> SignVoteResolutionAsync(
        Guid voteId,
        CommunityVoteResolutionSignRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _protectedApiClient.PostAsProtectedJsonAsync(
            $"api/v1/community/votes/{voteId}/resolution-documents/signatures",
            request,
            cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CommunityVoteResolutionDocumentResponse>(cancellationToken: cancellationToken);
    }

    public async Task<PlatformCommunityPostAttachmentResponse?> UploadAttachmentAsync(
        long postId,
        string password,
        IBrowserFile file,
        long maxAllowedSize,
        CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(password), "Password");

        await using var stream = file.OpenReadStream(maxAllowedSize);
        using var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
        content.Add(streamContent, "File", file.Name);

        using var response = await _httpClient.PostAsync($"api/v1/community/posts/{postId}/attachments", content, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PlatformCommunityPostAttachmentResponse>(cancellationToken: cancellationToken);
    }
}
