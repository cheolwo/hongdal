using Ssalddel.Contracts.Common.Hr;

namespace Ssalddel.Ui.Common.Areas.App.Services;

public interface I인사역할지원Service
{
    Task<HrRoleApplicationPageResponse> 내지원목록Async(
        CancellationToken cancellationToken = default);

    Task<HrRoleApplicationResponse> 제출Async(
        HrRoleApplicationSubmitRequest request,
        CancellationToken cancellationToken = default);

    Task<HrRoleApplicationResponse> 철회Async(
        Guid applicationId,
        CancellationToken cancellationToken = default);
}

public sealed class 인사역할지원Client(ISsalddelJsonApiClient apiClient) : I인사역할지원Service
{
    private const string BasePath = "api/v1/hr/role-applications";

    public async Task<HrRoleApplicationPageResponse> 내지원목록Async(
        CancellationToken cancellationToken = default)
        => await apiClient.GetAsync<HrRoleApplicationPageResponse>(
               BasePath,
               "내 역할 지원 목록 조회",
               allowNotFound: false,
               cancellationToken)
           ?? throw new InvalidOperationException("내 역할 지원 목록 응답이 비어 있습니다.");

    public async Task<HrRoleApplicationResponse> 제출Async(
        HrRoleApplicationSubmitRequest request,
        CancellationToken cancellationToken = default)
        => await apiClient.SendAsync<HrRoleApplicationSubmitRequest, HrRoleApplicationResponse>(
               HttpMethod.Post,
               BasePath,
               request,
               "역할 지원 제출",
               cancellationToken: cancellationToken)
           ?? throw new InvalidOperationException("역할 지원 제출 응답이 비어 있습니다.");

    public async Task<HrRoleApplicationResponse> 철회Async(
        Guid applicationId,
        CancellationToken cancellationToken = default)
        => await apiClient.SendAsync<HrRoleApplicationResponse>(
               HttpMethod.Post,
               $"{BasePath}/{applicationId:D}/withdraw",
               "역할 지원 철회",
               cancellationToken: cancellationToken)
           ?? throw new InvalidOperationException("역할 지원 철회 응답이 비어 있습니다.");
}
