using Hongdal.Contracts.Common.Orderer;
using 홍달.Services.External.Customs;

namespace Hongdal.Services.Orderer;

public interface IGroupPurchaseOverseasShipmentCustomsSyncService
{
    Task<GroupPurchaseOverseasShipmentCustomsSyncResult> SyncAsync(
        GroupPurchaseOverseasShipmentCustomsSyncRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default);
}

public sealed class GroupPurchaseOverseasShipmentCustomsSyncService : IGroupPurchaseOverseasShipmentCustomsSyncService
{
    private readonly IGroupPurchaseOverseasShipmentTrackingStore _store;
    private readonly I화물통관진행조회Service _customsTrackingService;

    public GroupPurchaseOverseasShipmentCustomsSyncService(
        IGroupPurchaseOverseasShipmentTrackingStore store,
        I화물통관진행조회Service customsTrackingService)
    {
        _store = store;
        _customsTrackingService = customsTrackingService;
    }

    public async Task<GroupPurchaseOverseasShipmentCustomsSyncResult> SyncAsync(
        GroupPurchaseOverseasShipmentCustomsSyncRequest request,
        string updatedBy,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.DocumentManagementNumber))
        {
            throw new InvalidOperationException("documentManagementNumber is required.");
        }

        var shipment = await _store.GetByDocumentManagementNumberAsync(
            request.DocumentManagementNumber,
            cancellationToken);
        if (shipment is null)
        {
            return new GroupPurchaseOverseasShipmentCustomsSyncResult
            {
                Synced = false,
                Message = "공동주문 해외 선적 추적 원장을 찾을 수 없습니다.",
                QueriedAtUtc = DateTimeOffset.UtcNow
            };
        }

        var lookup = await _customsTrackingService.조회Async(
            new 화물통관진행조회Request
            {
                화물관리번호 = TrimToNull(request.CustomsCargoManagementNumber),
                MasterBl = ResolveMasterBl(request, shipment),
                HouseBl = TrimToNull(request.HouseBillOfLadingNumber),
                BlYear = request.BillOfLadingYear ?? ResolveBillOfLadingYear(shipment)
            },
            cancellationToken);

        if (!lookup.조회성공여부)
        {
            return new GroupPurchaseOverseasShipmentCustomsSyncResult
            {
                Synced = false,
                Message = lookup.오류메시지 ?? "관세청 화물통관 진행정보 조회에 실패했습니다.",
                QueriedAtUtc = lookup.조회시각,
                Shipment = shipment
            };
        }

        var eventCode = GroupPurchaseOverseasShipmentCustomsStageMapper.ToShipmentStatusCode(lookup.진행단계);
        var location = GroupPurchaseOverseasShipmentCustomsStageMapper.ResolveLocationSummary(lookup.장치장명);

        if (string.Equals(shipment.CurrentStatusCode, eventCode, StringComparison.Ordinal)
            && string.Equals(shipment.CurrentLocationSummary, location, StringComparison.Ordinal))
        {
            return new GroupPurchaseOverseasShipmentCustomsSyncResult
            {
                Synced = true,
                Message = "관세청 조회 결과가 이미 원장 최신 상태와 같습니다.",
                CustomsStageName = lookup.처리단계명 ?? string.Empty,
                CustomsLocationSummary = location,
                QueriedAtUtc = lookup.조회시각,
                Shipment = shipment
            };
        }

        var updated = await _store.AppendEventAsync(
            shipment.DocumentManagementNumber,
            GroupPurchaseOverseasShipmentCustomsStageMapper.ToShipmentEvent(lookup, request.IsOrdererVisible),
            updatedBy,
            cancellationToken);

        return new GroupPurchaseOverseasShipmentCustomsSyncResult
        {
            Synced = true,
            Message = "관세청 화물통관 진행정보를 공동구매 해외 선적 원장에 반영했습니다.",
            CustomsStageName = lookup.처리단계명 ?? string.Empty,
            CustomsLocationSummary = location,
            QueriedAtUtc = lookup.조회시각,
            Shipment = updated
        };
    }

    private static string? ResolveMasterBl(
        GroupPurchaseOverseasShipmentCustomsSyncRequest request,
        GroupPurchaseOverseasShipmentTrackingDto shipment)
    {
        var explicitValue = TrimToNull(request.MasterBillOfLadingNumber);
        if (explicitValue is not null)
        {
            return explicitValue;
        }

        return string.Equals(shipment.TransportDocumentType, GroupPurchaseShipmentDocumentTypeCode.BillOfLading, StringComparison.Ordinal)
            ? TrimToNull(shipment.TransportDocumentNumber)
            : null;
    }

    private static int ResolveBillOfLadingYear(GroupPurchaseOverseasShipmentTrackingDto shipment)
        => (shipment.ActualDepartureAtUtc
            ?? shipment.EstimatedDepartureAtUtc
            ?? shipment.CreatedAtUtc).Year;

    private static string? TrimToNull(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
