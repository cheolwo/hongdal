using System.Security.Cryptography;
using System.Text;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Ssalddel.ApiMetadata;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Contracts.Common.Hr;
using Ssalddel.Domain.HumanResources;
using 살뜰.Data;

namespace Ssalddel.Application.HumanResources;

public interface IHR역할지원CommandUseCase
{
    Task<Result<HrRoleApplicationResponse>> 제출Async(
        HrRoleApplicationSubmitRequest request,
        CancellationToken cancellationToken);

    Task<Result<HrRoleApplicationResponse>> 철회Async(
        Guid applicationId,
        CancellationToken cancellationToken);
}

[SsalddelApiWorkflow(SsalddelWorkflow.HrParticipation)]
[SsalddelUseCase(
    "HR 역할 지원 제출·철회",
    Summary = "로그인 사용자의 자발적 역할 관심 표시를 동의 버전과 함께 멱등 저장하고 본인 지원만 철회합니다.")]
[SsalddelUseCaseActor(SsalddelActor.CommunityMember)]
public sealed class HR역할지원CommandUseCase(
    SsalddelContext db,
    ICurrentUserAccessor currentUserAccessor) : IHR역할지원CommandUseCase
{
    public async Task<Result<HrRoleApplicationResponse>> 제출Async(
        HrRoleApplicationSubmitRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userId = currentUserAccessor.UserId?.Trim();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return HrRoleApplicationResults.Unauthorized<HrRoleApplicationResponse>();
        }

        if (request.SubmissionRequestId == Guid.Empty)
        {
            return HrRoleApplicationResults.BadRequest<HrRoleApplicationResponse>(
                "역할 지원 요청 ID를 확인해 주세요.");
        }

        var role = HrRoleApplicationCatalog.Find(request.RoleCode);
        if (role is null)
        {
            return HrRoleApplicationResults.BadRequest<HrRoleApplicationResponse>(
                "현재 지원할 수 있는 역할을 선택해 주세요.");
        }

        if (!HrRoleApplicationConsent.IsValid(request))
        {
            return HrRoleApplicationResults.BadRequest<HrRoleApplicationResponse>(
                "현재 역할 지원 안내를 확인하고 세 가지 항목에 모두 동의해 주세요.");
        }

        var existingRequest = await db.HrRoleApplications
            .AsNoTracking()
            .SingleOrDefaultAsync(item =>
                item.ApplicantUserId == userId
                && item.SubmissionRequestId == request.SubmissionRequestId,
                cancellationToken);
        if (existingRequest is not null)
        {
            return string.Equals(existingRequest.RequestedRoleCode, role.RoleCode, StringComparison.Ordinal)
                ? Result.Ok(HrRoleApplicationMapper.ToResponse(existingRequest))
                : HrRoleApplicationResults.Conflict<HrRoleApplicationResponse>(
                    "같은 요청 ID를 다른 역할 지원에 다시 사용할 수 없습니다.");
        }

        var activeApplicationKey = CreateActiveApplicationKey(userId, role.RoleCode, role.ScopeType, role.ScopeId);
        var activeApplication = await db.HrRoleApplications
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.ActiveApplicationKey == activeApplicationKey, cancellationToken);
        if (activeApplication is not null)
        {
            return Result.Ok(HrRoleApplicationMapper.ToResponse(activeApplication));
        }

        var now = DateTime.UtcNow;
        var application = new HrRoleApplicationRecord
        {
            Id = Guid.NewGuid(),
            ApplicantUserId = userId,
            ParticipantCategory = role.ParticipantCategory,
            RequestedRoleCode = role.RoleCode,
            RequestedRoleName = role.RoleName,
            ScopeType = role.ScopeType,
            ScopeId = role.ScopeId,
            StatusCode = HrRoleApplicationStatusCodes.Submitted,
            SubmissionRequestId = request.SubmissionRequestId,
            ActiveApplicationKey = activeApplicationKey,
            ConfirmedVoluntaryApplication = true,
            ConfirmedNoRoleOrEmploymentGuarantee = true,
            ConfirmedReviewDataUse = true,
            ConsentVersion = HrRoleApplicationConsent.CurrentVersion,
            SubmittedAtUtc = now,
            CreatedAt = now,
            UpdatedAt = now
        };
        db.HrRoleApplications.Add(application);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            db.Entry(application).State = EntityState.Detached;
            var concurrentlyCreatedByRequest = await db.HrRoleApplications
                .AsNoTracking()
                .SingleOrDefaultAsync(item =>
                    item.ApplicantUserId == userId
                    && item.SubmissionRequestId == request.SubmissionRequestId,
                    cancellationToken);
            if (concurrentlyCreatedByRequest is not null)
            {
                return string.Equals(
                    concurrentlyCreatedByRequest.RequestedRoleCode,
                    role.RoleCode,
                    StringComparison.Ordinal)
                    ? Result.Ok(HrRoleApplicationMapper.ToResponse(concurrentlyCreatedByRequest))
                    : HrRoleApplicationResults.Conflict<HrRoleApplicationResponse>(
                        "같은 요청 ID를 다른 역할 지원에 다시 사용할 수 없습니다.");
            }

            var concurrentlyCreatedByRole = await db.HrRoleApplications
                .AsNoTracking()
                .SingleOrDefaultAsync(item => item.ActiveApplicationKey == activeApplicationKey, cancellationToken);
            if (concurrentlyCreatedByRole is null)
            {
                throw;
            }

            return Result.Ok(HrRoleApplicationMapper.ToResponse(concurrentlyCreatedByRole));
        }

        return Result.Ok(HrRoleApplicationMapper.ToResponse(application));
    }

    public async Task<Result<HrRoleApplicationResponse>> 철회Async(
        Guid applicationId,
        CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.UserId?.Trim();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return HrRoleApplicationResults.Unauthorized<HrRoleApplicationResponse>();
        }

        if (applicationId == Guid.Empty)
        {
            return HrRoleApplicationResults.BadRequest<HrRoleApplicationResponse>(
                "철회할 역할 지원 ID를 확인해 주세요.");
        }

        var application = await db.HrRoleApplications.SingleOrDefaultAsync(item =>
            item.Id == applicationId && item.ApplicantUserId == userId,
            cancellationToken);
        if (application is null)
        {
            return HrRoleApplicationResults.NotFound<HrRoleApplicationResponse>();
        }

        if (string.Equals(application.StatusCode, HrRoleApplicationStatusCodes.Withdrawn, StringComparison.Ordinal))
        {
            return Result.Ok(HrRoleApplicationMapper.ToResponse(application));
        }

        var now = DateTime.UtcNow;
        application.StatusCode = HrRoleApplicationStatusCodes.Withdrawn;
        application.ActiveApplicationKey = null;
        application.WithdrawnAtUtc = now;
        application.UpdatedAt = now;
        await db.SaveChangesAsync(cancellationToken);

        return Result.Ok(HrRoleApplicationMapper.ToResponse(application));
    }

    private static string CreateActiveApplicationKey(
        string userId,
        string roleCode,
        string scopeType,
        string scopeId)
    {
        var value = $"{userId}\n{roleCode}\n{scopeType}\n{scopeId}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    }
}

internal static class HrRoleApplicationMapper
{
    internal static HrRoleApplicationResponse ToResponse(HrRoleApplicationRecord application)
        => new()
        {
            ApplicationId = application.Id,
            RoleCode = application.RequestedRoleCode,
            RoleName = application.RequestedRoleName,
            ParticipantCategory = application.ParticipantCategory,
            ParticipantCategoryName = HrParticipantCategoryCodes.GetDisplayName(application.ParticipantCategory),
            ScopeType = application.ScopeType,
            ScopeId = application.ScopeId,
            StatusCode = application.StatusCode,
            StatusName = HrRoleApplicationStatusCodes.GetDisplayName(application.StatusCode),
            ConsentVersion = application.ConsentVersion,
            SubmittedAtUtc = application.SubmittedAtUtc,
            WithdrawnAtUtc = application.WithdrawnAtUtc,
            UpdatedAtUtc = application.UpdatedAt,
            CanWithdraw = string.Equals(
                application.StatusCode,
                HrRoleApplicationStatusCodes.Submitted,
                StringComparison.Ordinal)
        };
}

internal static class HrRoleApplicationResults
{
    internal static Result<T> Unauthorized<T>()
        => Failure<T>("로그인 사용자 인증 정보가 필요합니다.", StatusCodes.Status401Unauthorized);

    internal static Result<T> BadRequest<T>(string message)
        => Failure<T>(message, StatusCodes.Status400BadRequest);

    internal static Result<T> NotFound<T>()
        => Failure<T>("역할 지원을 찾을 수 없거나 현재 계정의 지원이 아닙니다.", StatusCodes.Status404NotFound);

    internal static Result<T> Conflict<T>(string message)
        => Failure<T>(message, StatusCodes.Status409Conflict);

    private static Result<T> Failure<T>(string message, int statusCode)
        => Result.Fail<T>(new Error(message).WithMetadata("StatusCode", statusCode));
}
