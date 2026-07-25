using FluentResults;
using Microsoft.AspNetCore.Http;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Application.HumanResources;
using Ssalddel.Contracts.Common.Hr;
using Ssalddel.Contracts.Common.Warehouse;

namespace Ssalddel.Application.Warehouse;

public interface I창고작업진입UseCase
{
    Task<Result<창고작업진입확인응답>> 확인Async(
        창고작업진입확인요청? request,
        CancellationToken cancellationToken = default);
}

public sealed class 창고작업진입UseCase(
    ICurrentUserAccessor currentUser,
    IHrRoleAssignmentStore roleAssignments) : I창고작업진입UseCase
{
    public async Task<Result<창고작업진입확인응답>> 확인Async(
        창고작업진입확인요청? request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(currentUser.UserId))
        {
            return Result.Fail<창고작업진입확인응답>(
                new Error("창고 작업자 로그인이 필요합니다.")
                    .WithMetadata("StatusCode", StatusCodes.Status401Unauthorized));
        }

        if (request is null || string.IsNullOrWhiteSpace(request.ProcessCode))
        {
            return Result.Fail<창고작업진입확인응답>("processCode is required");
        }

        var allowedRoles = ResolveRoles(request.ProcessCode);
        var assignments = await roleAssignments.ListAsync(
            currentUser.UserId,
            scopeType: null,
            scopeId: null,
            cancellationToken);
        var matched = assignments.FirstOrDefault(item =>
            allowedRoles.Contains(item.RoleCode, StringComparer.Ordinal));
        if (matched is null)
        {
            return Result.Ok(new 창고작업진입확인응답
            {
                IsAllowed = false,
                OperatorName = currentUser.UserId,
                Message = "현재 계정에 이 공정을 수행할 활성 HR 역할이 없습니다."
            });
        }

        return Result.Ok(new 창고작업진입확인응답
        {
            IsAllowed = true,
            OperatorName = currentUser.UserId,
            RoleName = matched.RoleName,
            Message = "서버의 활성 HR 역할로 작업자 진입을 확인했습니다."
        });
    }

    private static IReadOnlyList<string> ResolveRoles(string processCode)
        => processCode.Trim().ToLowerInvariant() switch
        {
            "inbound" or "mart-inbound" =>
                [HrDetailedRoleCodes.WarehouseManager, HrDetailedRoleCodes.WarehouseInboundOperator],
            "mart-replenishment" =>
                [HrDetailedRoleCodes.WarehouseManager, HrDetailedRoleCodes.WarehouseInventoryOperator],
            "international-forwarding" or "delivery-agency" =>
                [HrDetailedRoleCodes.WarehouseManager, HrDetailedRoleCodes.ShippingAgencyOperator],
            _ =>
                [HrDetailedRoleCodes.WarehouseManager, HrDetailedRoleCodes.WarehouseDispatchOperator]
        };
}
