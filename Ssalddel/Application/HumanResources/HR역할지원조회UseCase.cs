using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Ssalddel.ApiMetadata;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Contracts.Common.Hr;
using 살뜰.Data;

namespace Ssalddel.Application.HumanResources;

public interface IHR역할지원조회UseCase
{
    Task<Result<HrRoleApplicationPageResponse>> 내지원목록Async(CancellationToken cancellationToken);
}

[SsalddelApiWorkflow(SsalddelWorkflow.HrParticipation)]
[SsalddelUseCase(
    "내 HR 역할 지원 조회",
    Summary = "로그인 사용자가 직접 제출한 역할 지원·철회 원장과 지원 가능한 역할만 조회합니다.")]
[SsalddelUseCaseActor(SsalddelActor.CommunityMember)]
public sealed class HR역할지원조회UseCase(
    SsalddelContext db,
    ICurrentUserAccessor currentUserAccessor) : IHR역할지원조회UseCase
{
    public async Task<Result<HrRoleApplicationPageResponse>> 내지원목록Async(
        CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.UserId?.Trim();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return HrRoleApplicationResults.Unauthorized<HrRoleApplicationPageResponse>();
        }

        var applications = await db.HrRoleApplications
            .AsNoTracking()
            .Where(item => item.ApplicantUserId == userId)
            .OrderByDescending(item => item.SubmittedAtUtc)
            .ThenByDescending(item => item.Id)
            .ToArrayAsync(cancellationToken);

        return Result.Ok(new HrRoleApplicationPageResponse
        {
            Options = HrRoleApplicationCatalog.Items,
            Applications = applications.Select(HrRoleApplicationMapper.ToResponse).ToArray()
        });
    }
}
