using FluentResults;
using Ssalddel.Contracts.Common.VehicleLoading;
using Ssalddel.Contracts.Common.WorldProjection;
using Ssalddel.Services.LogisticsProcessing.VehicleLoading;

namespace Ssalddel.Application.WorldProjection;

public interface IResidentialPickupPerspectiveUseCase
{
    Task<Result<ResidentialPickupPerspectiveResponse>> QueryOrdererAsync(
        CancellationToken cancellationToken = default);

    Task<Result<ResidentialPickupPerspectiveResponse>> QueryTransporterAsync(
        CancellationToken cancellationToken = default);
}

public sealed class ResidentialPickupPerspectiveUseCase(
    IUnloadingPerspectiveReadService unloadingReader)
    : IResidentialPickupPerspectiveUseCase
{
    public Task<Result<ResidentialPickupPerspectiveResponse>> QueryOrdererAsync(
        CancellationToken cancellationToken = default)
        => QueryAsync(
            하차업무관점코드.주문자,
            ResidentialPickupRoleCodes.Orderer,
            cancellationToken);

    public Task<Result<ResidentialPickupPerspectiveResponse>> QueryTransporterAsync(
        CancellationToken cancellationToken = default)
        => QueryAsync(
            하차업무관점코드.운송담당자,
            ResidentialPickupRoleCodes.Transporter,
            cancellationToken);

    private async Task<Result<ResidentialPickupPerspectiveResponse>> QueryAsync(
        string unloadingPerspective,
        string roleCode,
        CancellationToken cancellationToken)
    {
        var result = await unloadingReader.QueryAsync(
            unloadingPerspective,
            null,
            new 하차관점목록조회요청
            {
                Page = 0,
                PageSize = 50,
                SortBy = nameof(하차관점항목응답.수정시각Utc),
                SortDescending = true,
            },
            cancellationToken);
        if (result.IsFailed)
        {
            return Result.Fail<ResidentialPickupPerspectiveResponse>(result.Errors);
        }

        var items = result.Value.Items;
        var revision = items.Count == 0
            ? 0L
            : items.Max(item => AsUtc(item.수정시각Utc).Ticks);
        var roleKey = roleCode.ToLowerInvariant();
        return Result.Ok(new ResidentialPickupPerspectiveResponse
        {
            StableId = $"role-perspective:residential-pickup.{roleKey}",
            Revision = revision,
            AuthorizedRoleCode = roleCode,
            AuthorizationDecisionId =
                $"authorized-residential-pickup:{roleKey}.{revision}.{items.Count}",
            GeneratedAt = DateTimeOffset.UtcNow,
            PickupPoints = items.Select(item => Map(item, roleCode)).ToArray(),
        });
    }

    private static ResidentialPickupPointResponse Map(
        하차관점항목응답 source,
        string roleCode)
    {
        var status = source.하차상태 switch
        {
            하차작업상태코드.도착 => ResidentialPickupStatusCodes.Arrived,
            하차작업상태코드.완료 => ResidentialPickupStatusCodes.Completed,
            _ => ResidentialPickupStatusCodes.Waiting,
        };

        return new ResidentialPickupPointResponse
        {
            StableId = $"residential-pickup:{source.출고예정Id}",
            CanonicalTaskStableId =
                $"unloading-task:{source.운송원장Id}.{source.출고예정Id}",
            PickupPointLabel = source.창고입고연결여부
                ? "공동 수령지"
                : "지정 수령지",
            ProductLabel = source.상품명,
            Quantity = Math.Max(0, source.수량),
            StatusCode = status,
            RoleLabel = string.Equals(
                roleCode,
                ResidentialPickupRoleCodes.Orderer,
                StringComparison.Ordinal)
                ? "내 수령 상품"
                : "내 하차 대상",
            CanInspect = true,
            UpdatedAt = new DateTimeOffset(AsUtc(source.수정시각Utc)),
        };
    }

    private static DateTime AsUtc(DateTime value)
        => value.Kind == DateTimeKind.Utc
            ? value
            : DateTime.SpecifyKind(value, DateTimeKind.Utc);
}
