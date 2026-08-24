using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Unity.Farm;

namespace Ssalddel.Unity.PotatoJourney
{
    public static class PotatoJourneyReadStateCodes
    {
        public const string InitialLoading = "InitialLoading";
        public const string Refreshing = "Refreshing";
        public const string Ready = "Ready";
        public const string Partial = "Partial";
        public const string Stale = "Stale";
        public const string Error = "Error";
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public static class PotatoJourneyApiRequestBuilder
    {
        public static string Build(
            string? cultivationStableId,
            string? referenceDate = null,
            int lookbackDays = 14)
        {
            if (lookbackDays < 1 || lookbackDays > 90)
                throw new ArgumentOutOfRangeException(nameof(lookbackDays), "PotatoJourneyLookbackDaysInvalid");
            var normalizedDate = referenceDate?.Trim() ?? string.Empty;
            if (normalizedDate.Length > 0
                && (!DateTime.TryParseExact(normalizedDate, "yyyyMMdd", CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out _)))
                throw new ArgumentException("PotatoJourneyReferenceDateInvalid", nameof(referenceDate));

            var route = PotatoJourneyApiRoutes.Read + "?lookbackDays="
                        + lookbackDays.ToString(CultureInfo.InvariantCulture);
            if (!string.IsNullOrWhiteSpace(cultivationStableId))
                route += "&cultivationStableId=" + Uri.EscapeDataString(cultivationStableId.Trim());
            if (normalizedDate.Length > 0)
                route += "&referenceDate=" + normalizedDate;
            return route;
        }
    }

    public sealed class PotatoJourneyReadResult
    {
        public string StateCode { get; set; } = PotatoJourneyReadStateCodes.InitialLoading;
        public PotatoJourneySnapshot? Snapshot { get; set; }
        public bool IsShowingLastSuccess { get; set; }
        public string ErrorCode { get; set; } = string.Empty;
    }

    public sealed class PotatoJourneyReadSession
    {
        private readonly PotatoJourneyQueryUseCase query;
        private PotatoJourneySnapshot? lastSuccess;

        public PotatoJourneyReadSession(PotatoJourneyQueryUseCase queryUseCase)
            => query = queryUseCase ?? throw new ArgumentNullException(nameof(queryUseCase));

        public PotatoJourneyReadResult Current { get; private set; } = new PotatoJourneyReadResult();

        public async Task<PotatoJourneyReadResult> RefreshAsync(
            string? cultivationStableId,
            DateTimeOffset observedNow,
            TimeSpan maximumAge,
            CancellationToken cancellationToken = default)
        {
            if (maximumAge <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(maximumAge));
            Current = new PotatoJourneyReadResult
            {
                StateCode = lastSuccess == null
                    ? PotatoJourneyReadStateCodes.InitialLoading
                    : PotatoJourneyReadStateCodes.Refreshing,
                Snapshot = lastSuccess,
                IsShowingLastSuccess = lastSuccess != null,
            };

            try
            {
                var snapshot = await query.ExecuteAsync(cultivationStableId, cancellationToken)
                    .ConfigureAwait(false);
                lastSuccess = snapshot;
                Current = new PotatoJourneyReadResult
                {
                    StateCode = Classify(snapshot, observedNow, maximumAge),
                    Snapshot = snapshot,
                };
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception error)
            {
                Current = new PotatoJourneyReadResult
                {
                    StateCode = PotatoJourneyReadStateCodes.Error,
                    Snapshot = lastSuccess,
                    IsShowingLastSuccess = lastSuccess != null,
                    ErrorCode = NormalizeError(error),
                };
            }

            return Current;
        }

        private static string Classify(
            PotatoJourneySnapshot snapshot,
            DateTimeOffset observedNow,
            TimeSpan maximumAge)
        {
            if (snapshot.GeneratedAt > observedNow + TimeSpan.FromMinutes(5)
                || observedNow - snapshot.GeneratedAt > maximumAge)
                return PotatoJourneyReadStateCodes.Stale;
            return snapshot.DomesticPrice.StatusCode == PotatoPriceObservationStatusCodes.Ready
                ? PotatoJourneyReadStateCodes.Ready
                : PotatoJourneyReadStateCodes.Partial;
        }

        private static string NormalizeError(Exception error)
        {
            var message = error.Message?.Trim();
            if (string.IsNullOrWhiteSpace(message)) return "PotatoJourneyReadFailed";
            var separator = message.IndexOf(':');
            return separator < 0 ? message : message.Substring(0, separator);
        }
    }

    public sealed class PotatoJourneyHubRoutePresentationModel
    {
        public bool IsVisible { get; set; }
        public string CargoStableId { get; set; } = string.Empty;
        public string HandoffStateCode { get; set; } = string.Empty;
        public string OriginWaypointKey { get; set; } = string.Empty;
        public string DestinationWaypointKey { get; set; } = string.Empty;
        public string SourceModeCode { get; set; } = string.Empty;
        public string ModeLabel { get; set; } = string.Empty;
        public string BlockReasonCode { get; set; } = string.Empty;
        public string HarvestLotStableId { get; set; } = string.Empty;
        public string PackageLotStableId { get; set; } = string.Empty;
        public int PackageCount { get; set; }
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public decimal VehicleCapacityKg { get; set; }
        public string LineageText { get; set; } = string.Empty;
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class PotatoJourneyHubRouteProjector
    {
        public PotatoJourneyHubRoutePresentationModel Project(PotatoJourneySnapshot source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            var allowedLinkage = source.LinkageStatusCode == PotatoJourneyLinkageStatusCodes.CanonicalLinked
                                 || source.LinkageStatusCode == PotatoJourneyLinkageStatusCodes.SimulationLinked;
            if (!allowedLinkage || source.CargoJourney == null)
            {
                return new PotatoJourneyHubRoutePresentationModel
                {
                    SourceModeCode = source.SourceModeCode,
                    BlockReasonCode = allowedLinkage
                        ? "PotatoJourneyCargoRelationshipMissing"
                        : "PotatoJourneyLinkageNotVerified",
                };
            }

            return new PotatoJourneyHubRoutePresentationModel
            {
                IsVisible = true,
                CargoStableId = source.CargoJourney.CargoStableId,
                HandoffStateCode = source.CargoJourney.HandoffStateCode,
                OriginWaypointKey = "farm-yard.potato-cargo",
                DestinationWaypointKey = "hub.inbound-dock",
                SourceModeCode = source.SourceModeCode,
                ModeLabel = source.SourceModeCode == PotatoJourneySourceModeCodes.SimulationFixture
                    ? "SIMULATION"
                    : string.Empty,
            };
        }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class PotatoHarvestCargoHubRouteAdapter
    {
        private readonly 감자수확CargoSimulationValidator validator;

        public PotatoHarvestCargoHubRouteAdapter(감자수확CargoSimulationValidator value)
            => validator = value ?? throw new ArgumentNullException(nameof(value));

        public PotatoJourneyHubRoutePresentationModel Project(감자수확CargoSimulationSnapshot source)
        {
            validator.Validate(source);
            if (source.Cargo == null || source.PackageLot == null)
            {
                return new PotatoJourneyHubRoutePresentationModel
                {
                    SourceModeCode = PotatoJourneySourceModeCodes.SimulationFixture,
                    ModeLabel = "SIMULATION",
                    BlockReasonCode = "PotatoHarvestCargoNotLoaded",
                };
            }

            var cargo = source.Cargo;
            var package = source.PackageLot;
            return new PotatoJourneyHubRoutePresentationModel
            {
                IsVisible = true,
                CargoStableId = cargo.StableId,
                HandoffStateCode = cargo.StateCode,
                OriginWaypointKey = "farm-yard.potato-cargo",
                DestinationWaypointKey = "hub.inbound-dock",
                SourceModeCode = PotatoJourneySourceModeCodes.SimulationFixture,
                ModeLabel = "SIMULATION",
                HarvestLotStableId = source.HarvestLot.StableId,
                PackageLotStableId = package.StableId,
                PackageCount = package.PackageCount,
                Quantity = cargo.Quantity,
                UnitCode = cargo.UnitCode,
                VehicleCapacityKg = cargo.VehicleCapacityKg,
                LineageText = source.HarvestLot.StableId + " → " + package.StableId + " → " + cargo.StableId,
            };
        }
    }
}
