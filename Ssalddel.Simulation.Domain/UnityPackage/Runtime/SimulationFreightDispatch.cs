using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Ssalddel.Simulation.Contracts;
using Ssalddel.WorkflowRules;
using Ssalddel.WorkflowRules.Contracts;

namespace Ssalddel.Simulation.Domain
{
    public sealed partial class 경영SimulationSessionAggregate
    {
        public SimulationFreightDispatchPreviewSnapshot PreviewFreightDispatch(
            SimulationFreightDispatchPreviewRequest request)
        {
            ValidateFreightDispatchPreviewRequest(request);
            lock (gate)
            {
                return CreateFreightDispatchPreview(request);
            }
        }

        public 경영SimulationSessionSnapshot ConfirmFreightDispatch(
            SimulationFreightDispatchConfirmRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            RequireStableId(request.CommandId, "SimulationCommandIdInvalid");
            if (request.ExpectedRevision < 0)
                throw new SimulationContractException("SimulationExpectedRevisionInvalid");
            RequireStableId(
                request.SelectedCarrierCandidateStableId,
                "SimulationFreightCarrierCandidateStableIdInvalid");
            ValidateFreightDispatchPreviewRequest(request.FreightDispatch);

            lock (gate)
            {
                var alreadyApplied = appliedLogisticsMovementCommands.ContainsKey(
                    request.CommandId.Trim());
                var preview = CreateFreightDispatchPreview(request.FreightDispatch);
                var selected = preview.CandidateEvaluations.SingleOrDefault(value =>
                    string.Equals(
                        value.CarrierCandidateStableId,
                        request.SelectedCarrierCandidateStableId.Trim(),
                        StringComparison.Ordinal));
                if (selected == null)
                    throw new SimulationContractException("SimulationFreightDispatchCandidateNotFound");
                if (!selected.IsEligible)
                    throw new SimulationConflictException("SimulationFreightDispatchCandidateIneligible");
                if (!alreadyApplied && request.ExpectedRevision != Revision)
                    throw new SimulationConflictException("SimulationExpectedRevisionMismatch");
                if (!alreadyApplied
                    && preview.LogisticsMovement.CommonDecisionPreview.Decision.BlockReasonCodes.Length > 0)
                    throw new SimulationConflictException("SimulationFreightDispatchMovementBlocked");

                var decision = CreateFreightDispatchDecision(preview, selected);
                var freight = new SimulationFreightTransportPreviewRequest
                {
                    Transport = new SimulationFreightTransportBindingRequest
                    {
                        TransportRequestStableId = preview.TransportRequestStableId,
                        DispatchOfferStableId = preview.DispatchOfferStableId,
                        CarrierCandidateStableId = selected.CarrierCandidateStableId,
                        VehicleStableId = selected.VehicleStableId,
                        VehicleCapacity = selected.VehicleCapacity,
                        VehicleCapacityUnitCode = selected.VehicleCapacityUnitCode,
                        DispatchDecision = decision,
                    },
                    Movement = CloneFreightDispatchMovement(request.FreightDispatch.Movement),
                };
                return ConfirmFreightTransport(new SimulationFreightTransportConfirmRequest
                {
                    CommandId = request.CommandId,
                    ExpectedRevision = request.ExpectedRevision,
                    Freight = freight,
                });
            }
        }

        private SimulationFreightDispatchPreviewSnapshot CreateFreightDispatchPreview(
            SimulationFreightDispatchPreviewRequest request)
        {
            var dispatch = request.Dispatch;
            var judgment = EvaluateFreightDispatch(dispatch, request.Movement);
            var logistics = CreateLogisticsMovementPreview(
                CloneFreightDispatchMovement(request.Movement));
            var evaluations = judgment.후보평가목록
                .Select(value => CreateFreightDispatchCandidateEvaluation(
                    value,
                    judgment.추천후보StableId,
                    null))
                .ToArray();
            var blocks = logistics.CommonDecisionPreview.Decision.BlockReasonCodes.ToList();
            if (judgment.적격후보수 == 0)
                blocks.Add("SimulationFreightDispatchCandidateUnavailable");

            return new SimulationFreightDispatchPreviewSnapshot
            {
                ObservedRevision = Revision,
                ObservedWorldTick = CurrentTick,
                TransportRequestStableId = dispatch.TransportRequestStableId.Trim(),
                DispatchOfferStableId = DispatchOfferStableId(dispatch.TransportRequestStableId),
                RecommendedCarrierCandidateStableId = judgment.추천후보StableId,
                RuleRevision = judgment.RuleRevision,
                BlockReasonCodes = blocks.Distinct(StringComparer.Ordinal)
                    .OrderBy(value => value, StringComparer.Ordinal).ToArray(),
                SourceStableIds = MergeSources(
                    MergeSources(dispatch.SourceStableIds, judgment.SourceStableIds),
                    logistics.CommonDecisionPreview.Decision.SourceStableIds),
                CandidateEvaluations = evaluations,
                LogisticsMovement = logistics,
            };
        }

        private static 화물배차후보선정판정 EvaluateFreightDispatch(
            SimulationFreightDispatchRequest dispatch,
            SimulationLogisticsMovementPreviewRequest movement)
        {
            try
            {
                return 화물배차후보선정Policy.판정(new 화물배차후보선정요청
                {
                    화물수량 = movement.Quantity,
                    화물단위코드 = movement.UnitCode.Trim(),
                    위치유효시간분 = dispatch.LocationFreshnessMinutes,
                    기본상차접근반경Km = dispatch.BasePickupRadiusKm,
                    원거리상차접근최대반경Km = dispatch.MaximumRemotePickupRadiusKm,
                    원거리상차평균속도KmH = dispatch.RemotePickupAverageSpeedKmH,
                    원거리상차도착여유분 = dispatch.RemotePickupArrivalBufferMinutes,
                    상차시간창남은분 = dispatch.PickupWindowRemainingMinutes,
                    제외후보StableId = dispatch.ExcludedCarrierCandidateStableId?.Trim(),
                    후보목록 = dispatch.Candidates.Select(candidate => new 화물배차후보입력
                    {
                        후보StableId = candidate.CarrierCandidateStableId.Trim(),
                        차량StableId = candidate.VehicleStableId.Trim(),
                        화물운송앱여부 = candidate.IsFreightApp,
                        차량활성여부 = candidate.IsVehicleActive,
                        기사운행중여부 = candidate.IsDriverOperating,
                        이전거절여부 = candidate.WasPreviouslyRejected,
                        위치경과분 = candidate.LocationAgeMinutes,
                        상차거리Km = candidate.PickupDistanceKm,
                        상차접근허용반경Km = candidate.PickupAllowedRadiusKm,
                        차량용량 = candidate.VehicleCapacity,
                        차량용량단위코드 = candidate.VehicleCapacityUnitCode.Trim(),
                        차량적합여부 = candidate.IsVehicleCompatible,
                        차량부적합사유코드목록 = candidate.VehicleBlockReasonCodes.ToArray(),
                        기사대기분 = candidate.DriverWaitingMinutes,
                        기본추천사유 = candidate.BaseReason,
                        추천점수요청 = new 화물배차추천점수요청
                        {
                            전체일정완수가능여부 = candidate.CanCompleteSchedule,
                            일정삽입가능여부 = candidate.CanInsertSchedule,
                            경로변경이점여부 = candidate.HasRouteChangeBenefit,
                            예상추가순이익 = candidate.EstimatedExtraProfit,
                            추가지연분 = candidate.AdditionalDelayMinutes,
                            경로기준거리Km = candidate.PickupDistanceKm,
                            추천유형 = candidate.RecommendationTypeCode,
                            화물민감여부 = candidate.IsCargoSensitive,
                            복귀우회증가거리Km = candidate.ReturnDetourDistanceKm,
                            복귀지기준사용여부 = candidate.UsesReturnDestination,
                        },
                    }).ToArray(),
                });
            }
            catch (ArgumentException)
            {
                throw new SimulationContractException("SimulationFreightDispatchRequestInvalid");
            }
        }

        private static SimulationFreightDispatchDecisionSnapshot CreateFreightDispatchDecision(
            SimulationFreightDispatchPreviewSnapshot preview,
            SimulationFreightDispatchCandidateEvaluationSnapshot selected)
            => new SimulationFreightDispatchDecisionSnapshot
            {
                DispatchOfferStableId = preview.DispatchOfferStableId,
                TransportRequestStableId = preview.TransportRequestStableId,
                RecommendedCarrierCandidateStableId = preview.RecommendedCarrierCandidateStableId,
                SelectedCarrierCandidateStableId = selected.CarrierCandidateStableId,
                SelectedVehicleStableId = selected.VehicleStableId,
                RuleRevision = preview.RuleRevision,
                SourceStableIds = Copy(preview.SourceStableIds),
                CandidateEvaluations = preview.CandidateEvaluations.Select(value =>
                    CloneFreightDispatchCandidateEvaluation(
                        value,
                        string.Equals(value.CarrierCandidateStableId,
                            selected.CarrierCandidateStableId,
                            StringComparison.Ordinal))).ToArray(),
            };

        private static SimulationFreightDispatchCandidateEvaluationSnapshot
            CreateFreightDispatchCandidateEvaluation(
                화물배차후보평가 source,
                string? recommendedCarrierCandidateStableId,
                string? selectedCarrierCandidateStableId)
            => new SimulationFreightDispatchCandidateEvaluationSnapshot
            {
                CarrierCandidateStableId = source.후보StableId,
                VehicleStableId = source.차량StableId,
                IsEligible = source.적격여부,
                IsRecommended = string.Equals(
                    source.후보StableId,
                    recommendedCarrierCandidateStableId,
                    StringComparison.Ordinal),
                IsSelected = string.Equals(
                    source.후보StableId,
                    selectedCarrierCandidateStableId,
                    StringComparison.Ordinal),
                Rank = source.추천순위,
                PickupDistanceKm = source.상차거리Km,
                VehicleCapacity = source.차량용량,
                VehicleCapacityUnitCode = source.차량용량단위코드,
                Reason = source.추천사유,
                BlockReasonCodes = Copy(source.차단사유코드목록),
                Score = new SimulationFreightDispatchScoreBreakdownSnapshot
                {
                    ScheduleScore = source.점수내역.일정점수,
                    ProfitScore = source.점수내역.수익점수,
                    DelayScore = source.점수내역.지연점수,
                    DistanceScore = source.점수내역.거리점수,
                    RecommendationTypeScore = source.점수내역.추천유형점수,
                    CargoSensitivityScore = source.점수내역.화물민감도점수,
                    ReturnBurdenScore = source.점수내역.복귀부담점수,
                    BaseScore = source.기본추천점수,
                    DriverWaitingScore = source.기사대기보정점수,
                    TotalScore = source.총추천점수,
                },
            };

        internal static SimulationFreightDispatchDecisionSnapshot? CloneFreightDispatchDecision(
            SimulationFreightDispatchDecisionSnapshot? source)
            => source == null ? null : new SimulationFreightDispatchDecisionSnapshot
            {
                DispatchOfferStableId = source.DispatchOfferStableId,
                TransportRequestStableId = source.TransportRequestStableId,
                RecommendedCarrierCandidateStableId = source.RecommendedCarrierCandidateStableId,
                SelectedCarrierCandidateStableId = source.SelectedCarrierCandidateStableId,
                SelectedVehicleStableId = source.SelectedVehicleStableId,
                RuleRevision = source.RuleRevision,
                SourceStableIds = Copy(source.SourceStableIds),
                CandidateEvaluations = source.CandidateEvaluations
                    .Select(value => CloneFreightDispatchCandidateEvaluation(value, value.IsSelected))
                    .ToArray(),
            };

        private static SimulationFreightDispatchCandidateEvaluationSnapshot
            CloneFreightDispatchCandidateEvaluation(
                SimulationFreightDispatchCandidateEvaluationSnapshot source,
                bool isSelected)
            => new SimulationFreightDispatchCandidateEvaluationSnapshot
            {
                CarrierCandidateStableId = source.CarrierCandidateStableId,
                VehicleStableId = source.VehicleStableId,
                IsEligible = source.IsEligible,
                IsRecommended = source.IsRecommended,
                IsSelected = isSelected,
                Rank = source.Rank,
                PickupDistanceKm = source.PickupDistanceKm,
                VehicleCapacity = source.VehicleCapacity,
                VehicleCapacityUnitCode = source.VehicleCapacityUnitCode,
                Reason = source.Reason,
                BlockReasonCodes = Copy(source.BlockReasonCodes),
                Score = new SimulationFreightDispatchScoreBreakdownSnapshot
                {
                    ScheduleScore = source.Score.ScheduleScore,
                    ProfitScore = source.Score.ProfitScore,
                    DelayScore = source.Score.DelayScore,
                    DistanceScore = source.Score.DistanceScore,
                    RecommendationTypeScore = source.Score.RecommendationTypeScore,
                    CargoSensitivityScore = source.Score.CargoSensitivityScore,
                    ReturnBurdenScore = source.Score.ReturnBurdenScore,
                    BaseScore = source.Score.BaseScore,
                    DriverWaitingScore = source.Score.DriverWaitingScore,
                    TotalScore = source.Score.TotalScore,
                },
            };

        private static SimulationLogisticsMovementPreviewRequest CloneFreightDispatchMovement(
            SimulationLogisticsMovementPreviewRequest source)
            => new SimulationLogisticsMovementPreviewRequest
            {
                CargoStableId = source.CargoStableId,
                CargoRevision = source.CargoRevision,
                SourceExportCargoHandoffStableId = source.SourceExportCargoHandoffStableId,
                SourceAllocationStableId = source.SourceAllocationStableId,
                HarvestLotStableId = source.HarvestLotStableId,
                PackageLotStableId = source.PackageLotStableId,
                ProductStableId = source.ProductStableId,
                Quantity = source.Quantity,
                UnitCode = source.UnitCode,
                RouteStableId = source.RouteStableId,
                OriginFacilityStableId = source.OriginFacilityStableId,
                DestinationFacilityStableId = source.DestinationFacilityStableId,
                ActorStableId = source.ActorStableId,
                RequiredRouteTicks = source.RequiredRouteTicks,
                SourceStableIds = Copy(source.SourceStableIds),
            };

        private static void ValidateFreightDispatchPreviewRequest(
            SimulationFreightDispatchPreviewRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.Dispatch == null)
                throw new SimulationContractException("SimulationFreightDispatchMissing");
            if (request.Movement == null)
                throw new SimulationContractException("SimulationLogisticsMovementMissing");
            if (request.Movement.FreightTransport != null)
                throw new SimulationContractException("SimulationFreightDispatchBindingNotAllowed");

            ValidateLogisticsMovementRequest(request.Movement);
            var dispatch = request.Dispatch;
            RequireStableId(
                dispatch.TransportRequestStableId,
                "SimulationFreightTransportStableIdInvalid");
            if (!string.IsNullOrWhiteSpace(dispatch.ExcludedCarrierCandidateStableId))
                RequireStableId(dispatch.ExcludedCarrierCandidateStableId,
                    "SimulationFreightCarrierCandidateStableIdInvalid");
            ValidateIds(dispatch.SourceStableIds, true,
                "SimulationFreightDispatchSourceStableIdsInvalid");
            if (dispatch.Candidates == null || dispatch.Candidates.Length == 0)
                throw new SimulationContractException("SimulationFreightDispatchCandidatesMissing");
            foreach (var candidate in dispatch.Candidates)
            {
                if (candidate == null)
                    throw new SimulationContractException("SimulationFreightDispatchCandidateInvalid");
                RequireStableId(candidate.CarrierCandidateStableId,
                    "SimulationFreightCarrierCandidateStableIdInvalid");
                RequireStableId(candidate.VehicleStableId,
                    "SimulationFreightVehicleStableIdInvalid");
                RequireText(candidate.VehicleCapacityUnitCode,
                    "SimulationFreightVehicleCapacityUnitCodeMissing");
                if (candidate.VehicleBlockReasonCodes == null
                    || candidate.DriverWaitingMinutes < 0m)
                    throw new SimulationContractException("SimulationFreightDispatchCandidateInvalid");
                ValidateIds(candidate.VehicleBlockReasonCodes, false,
                    "SimulationFreightVehicleBlockReasonCodesInvalid");
            }

            EvaluateFreightDispatch(dispatch, request.Movement);
        }

        private static string DispatchOfferStableId(string transportRequestStableId)
            => "dispatch-offer:" + transportRequestStableId.Trim();

        internal static string BuildFreightDispatchDecisionPayloadKey(
            SimulationFreightDispatchDecisionSnapshot? decision)
        {
            if (decision == null) return string.Empty;
            var values = new List<string>
            {
                decision.DispatchOfferStableId.Trim(),
                decision.TransportRequestStableId.Trim(),
                decision.RecommendedCarrierCandidateStableId?.Trim() ?? string.Empty,
                decision.SelectedCarrierCandidateStableId?.Trim() ?? string.Empty,
                decision.SelectedVehicleStableId?.Trim() ?? string.Empty,
                decision.RuleRevision.Trim(),
                string.Join("\u001d", decision.SourceStableIds.Select(value => value.Trim())
                    .OrderBy(value => value, StringComparer.Ordinal)),
            };
            foreach (var candidate in decision.CandidateEvaluations)
            {
                values.Add(string.Join("\u001c", new[]
                {
                    candidate.CarrierCandidateStableId.Trim(),
                    candidate.VehicleStableId.Trim(),
                    candidate.IsEligible.ToString(),
                    candidate.IsRecommended.ToString(),
                    candidate.IsSelected.ToString(),
                    candidate.Rank.ToString(CultureInfo.InvariantCulture),
                    candidate.PickupDistanceKm?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                    candidate.VehicleCapacity.ToString(CultureInfo.InvariantCulture),
                    candidate.VehicleCapacityUnitCode.Trim(),
                    candidate.Reason,
                    string.Join("\u001b", candidate.BlockReasonCodes.OrderBy(value => value,
                        StringComparer.Ordinal)),
                    candidate.Score.ScheduleScore.ToString(CultureInfo.InvariantCulture),
                    candidate.Score.ProfitScore.ToString(CultureInfo.InvariantCulture),
                    candidate.Score.DelayScore.ToString(CultureInfo.InvariantCulture),
                    candidate.Score.DistanceScore.ToString(CultureInfo.InvariantCulture),
                    candidate.Score.RecommendationTypeScore.ToString(CultureInfo.InvariantCulture),
                    candidate.Score.CargoSensitivityScore.ToString(CultureInfo.InvariantCulture),
                    candidate.Score.ReturnBurdenScore.ToString(CultureInfo.InvariantCulture),
                    candidate.Score.BaseScore.ToString(CultureInfo.InvariantCulture),
                    candidate.Score.DriverWaitingScore.ToString(CultureInfo.InvariantCulture),
                    candidate.Score.TotalScore.ToString(CultureInfo.InvariantCulture),
                }));
            }
            return string.Join("\u001e", values);
        }
    }
}
