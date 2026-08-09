using System;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Unity.Npcs;

namespace Ssalddel.Unity.Transport
{
    public static class LogisticsFacilityAreaCodes
    {
        public const string VehicleGate = "VehicleGate";
        public const string InboundDock = "InboundDock";
        public const string Inspection = "Inspection";
        public const string Storage = "Storage";
    }

    public static class LogisticsFacilityAreaStateCodes
    {
        public const string Idle = "Idle";
        public const string Next = "Next";
        public const string Active = "Active";
        public const string Completed = "Completed";
    }

    public sealed class LogisticsFacilityAreaPresentationModel
    {
        public string AreaCode { get; set; } = string.Empty;
        public string LabelText { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string ColorToken { get; set; } = string.Empty;
    }

    public sealed class LogisticsFacilityOverviewPresentationModel
    {
        public string StableId { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string HandoffStateCode { get; set; } = string.Empty;
        public string CargoStableId { get; set; } = string.Empty;
        public string TransportTaskStableId { get; set; } = string.Empty;
        public string InboundTaskStableId { get; set; } = string.Empty;
        public string CurrentAreaCode { get; set; } = string.Empty;
        public string SummaryText { get; set; } = string.Empty;
        public string BoundaryText { get; set; } = string.Empty;
        public LogisticsFacilityAreaPresentationModel[] Areas { get; set; } =
            Array.Empty<LogisticsFacilityAreaPresentationModel>();
    }

    public sealed class LogisticsFacilityOverviewProjector
    {
        public LogisticsFacilityOverviewPresentationModel? Project(
            CargoWarehouseHandoffSnapshot? handoff)
        {
            if (handoff == null) return null;
            var currentArea = CurrentArea(handoff.HandoffStateCode);
            return new LogisticsFacilityOverviewPresentationModel
            {
                StableId = "logistics-facility-overview:" + handoff.StableId,
                Revision = handoff.Revision,
                HandoffStateCode = handoff.HandoffStateCode,
                CargoStableId = handoff.CargoStableId,
                TransportTaskStableId = handoff.TransportTaskStableId,
                InboundTaskStableId = handoff.InboundTaskStableId,
                CurrentAreaCode = currentArea,
                SummaryText = "화물 " + handoff.CargoStableId + " · "
                    + StateLabel(handoff.HandoffStateCode),
                BoundaryText = "서버 handoff Projection · NPC 도착만으로 입고 완료되지 않음",
                Areas = new[]
                {
                    Area(LogisticsFacilityAreaCodes.VehicleGate, "차량 접근",
                        AreaState(handoff.HandoffStateCode, 0)),
                    Area(LogisticsFacilityAreaCodes.InboundDock, "입고 Dock",
                        AreaState(handoff.HandoffStateCode, 1)),
                    Area(LogisticsFacilityAreaCodes.Inspection, "검수·입고 처리",
                        AreaState(handoff.HandoffStateCode, 2)),
                    Area(LogisticsFacilityAreaCodes.Storage, "보관 위치",
                        AreaState(handoff.HandoffStateCode, 3)),
                },
            };
        }

        private static LogisticsFacilityAreaPresentationModel Area(
            string code,
            string label,
            string state)
            => new LogisticsFacilityAreaPresentationModel
            {
                AreaCode = code,
                LabelText = label,
                StateCode = state,
                ColorToken = state,
            };

        private static string CurrentArea(string state)
            => state switch
            {
                CargoHandoffStateCodes.InTransit => LogisticsFacilityAreaCodes.VehicleGate,
                CargoHandoffStateCodes.ArrivedAtWarehouse => LogisticsFacilityAreaCodes.InboundDock,
                CargoHandoffStateCodes.ReceivingCompleted => LogisticsFacilityAreaCodes.Storage,
                _ => throw new InvalidOperationException("LogisticsFacilityHandoffStateInvalid:" + state),
            };

        private static string StateLabel(string state)
            => state switch
            {
                CargoHandoffStateCodes.InTransit => "창고로 운송 중",
                CargoHandoffStateCodes.ArrivedAtWarehouse => "Dock 도착·입고 처리 대기",
                CargoHandoffStateCodes.ReceivingCompleted => "입고 완료·보관 위치 이동",
                _ => throw new InvalidOperationException("LogisticsFacilityHandoffStateInvalid:" + state),
            };

        private static string AreaState(string handoffState, int areaIndex)
        {
            var currentIndex = handoffState switch
            {
                CargoHandoffStateCodes.InTransit => 0,
                CargoHandoffStateCodes.ArrivedAtWarehouse => 1,
                CargoHandoffStateCodes.ReceivingCompleted => 3,
                _ => throw new InvalidOperationException(
                    "LogisticsFacilityHandoffStateInvalid:" + handoffState),
            };
            if (areaIndex < currentIndex) return LogisticsFacilityAreaStateCodes.Completed;
            if (areaIndex == currentIndex) return LogisticsFacilityAreaStateCodes.Active;
            if (areaIndex == currentIndex + 1) return LogisticsFacilityAreaStateCodes.Next;
            return LogisticsFacilityAreaStateCodes.Idle;
        }
    }

    public sealed class UrbanLogisticsCenterPresentationReadModel
    {
        public LogisticsFacilityOverviewPresentationModel? Facility { get; set; }
        public TruckMovementPresentationModel? Truck { get; set; }
    }

    /// <summary>같은 handoff 조회에서 시설 개요와 운송 corridor를 함께 만듭니다.</summary>
    public sealed class UrbanLogisticsCenterPresentationQueryUseCase
    {
        private readonly CargoWarehouseHandoffQueryUseCase handoffQuery;
        private readonly LogisticsFacilityOverviewProjector facilityProjector;
        private readonly TransportCorridorProjector corridorProjector;
        private readonly TransportCorridorPresenter corridorPresenter;

        public UrbanLogisticsCenterPresentationQueryUseCase(
            CargoWarehouseHandoffQueryUseCase handoffQuery,
            LogisticsFacilityOverviewProjector facilityProjector,
            TransportCorridorProjector corridorProjector,
            TransportCorridorPresenter corridorPresenter)
        {
            this.handoffQuery = handoffQuery ?? throw new ArgumentNullException(nameof(handoffQuery));
            this.facilityProjector = facilityProjector ?? throw new ArgumentNullException(nameof(facilityProjector));
            this.corridorProjector = corridorProjector ?? throw new ArgumentNullException(nameof(corridorProjector));
            this.corridorPresenter = corridorPresenter ?? throw new ArgumentNullException(nameof(corridorPresenter));
        }

        public async Task<UrbanLogisticsCenterPresentationReadModel> ExecuteAsync(
            CancellationToken cancellationToken = default)
        {
            var handoff = await handoffQuery.실행Async(cancellationToken).ConfigureAwait(false);
            return new UrbanLogisticsCenterPresentationReadModel
            {
                Facility = facilityProjector.Project(handoff),
                Truck = corridorPresenter.Present(corridorProjector.Project(handoff)),
            };
        }
    }
}
