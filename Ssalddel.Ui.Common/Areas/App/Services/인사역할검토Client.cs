using Ssalddel.Contracts.Common.Hr;

namespace Ssalddel.Ui.Common.Areas.App.Services;

public interface I인사역할검토읽기Service
{
    Task<HrRoleReviewListResponse> 목록Async(
        HrRoleReviewListRequest request,
        CancellationToken cancellationToken = default);

    Task<HrRoleReviewDetailResponse?> 상세Async(
        Guid reviewId,
        CancellationToken cancellationToken = default);
}

public sealed class 인사역할검토Client(ISsalddelJsonApiClient apiClient)
    : I인사역할검토읽기Service
{
    public async Task<HrRoleReviewListResponse> 목록Async(
        HrRoleReviewListRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var query = new List<string>
        {
            $"page={request.Page}",
            $"pageSize={request.PageSize}"
        };
        AddQuery(query, "search", request.Search);
        AddQuery(query, "sourceCode", request.SourceCode);
        AddQuery(query, "statusCode", request.StatusCode);
        AddQuery(query, "participantCategory", request.ParticipantCategory);
        AddQuery(query, "scopeType", request.ScopeType);

        return await apiClient.GetAsync<HrRoleReviewListResponse>(
                   $"api/v1/admin/hr-role-reviews?{string.Join('&', query)}",
                   "HR 역할 검토 목록 조회",
                   allowNotFound: false,
                   cancellationToken)
               ?? throw new InvalidOperationException("HR 역할 검토 목록 응답이 비어 있습니다.");
    }

    public Task<HrRoleReviewDetailResponse?> 상세Async(
        Guid reviewId,
        CancellationToken cancellationToken = default)
        => apiClient.GetAsync<HrRoleReviewDetailResponse>(
            $"api/v1/admin/hr-role-reviews/{reviewId:D}",
            "HR 역할 검토 상세 조회",
            allowNotFound: true,
            cancellationToken);

    private static void AddQuery(ICollection<string> query, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            query.Add($"{name}={Uri.EscapeDataString(value.Trim())}");
        }
    }
}
