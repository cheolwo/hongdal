using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Unity.Data;

namespace Ssalddel.Unity.Learning
{
    public static class 수출항만인수ApiRoutes
    {
        public static string Preview(string sessionStableId)
        {
            if (!StableDataId.IsValid(sessionStableId))
                throw new InvalidOperationException("ExportPortReceiptSessionStableIdInvalid");
            return "api/simulation/v1/sessions/" + sessionStableId
                + "/export-port-receipt-previews";
        }
    }

    public sealed class 수출항만인수DecisionApiModel
    {
        public string DecisionStableId { get; set; } = string.Empty;
        public string DecisionTypeCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public long Revision { get; set; }
        public string SessionStableId { get; set; } = string.Empty;
        public string[] TargetStableIds { get; set; } = Array.Empty<string>();
        public string[] Uncertainties { get; set; } = Array.Empty<string>();
        public string[] BlockReasonCodes { get; set; } = Array.Empty<string>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class 수출항만인수TaskPlanApiModel
    {
        public string TaskStableId { get; set; } = string.Empty;
        public string TaskTypeCode { get; set; } = string.Empty;
        public string FacilityStableId { get; set; } = string.Empty;
        public decimal AssignedCapacity { get; set; }
        public string AssignedCapacityUnitCode { get; set; } = string.Empty;
        public string[] InputLotStableIds { get; set; } = Array.Empty<string>();
        public string[] OutputCandidateCodes { get; set; } = Array.Empty<string>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class 수출항만인수CommonDecisionPreviewApiModel
    {
        public 수출항만인수DecisionApiModel Decision { get; set; }
            = new 수출항만인수DecisionApiModel();
        public 수출항만인수TaskPlanApiModel TaskPlan { get; set; }
            = new 수출항만인수TaskPlanApiModel();
    }

    /// <summary>
    /// Simulation수출항만인수PreviewSnapshot의 Unity transport projection입니다.
    /// 서버 contract assembly를 runtime에서 직접 참조하지 않고 같은 JSON field를 명시적으로 매핑합니다.
    /// </summary>
    public sealed class 수출항만인수PreviewApiModel
    {
        public string ReceiptStableId { get; set; } = string.Empty;
        public string CargoStableId { get; set; } = string.Empty;
        public string SourceExportCargoHandoffStableId { get; set; } = string.Empty;
        public string SourceAllocationStableId { get; set; } = string.Empty;
        public string HarvestLotStableId { get; set; } = string.Empty;
        public string PackageLotStableId { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string ReceivingFacilityStableId { get; set; } = string.Empty;
        public bool IsCandidateOnly { get; set; }
        public bool DoesNotCreateCustomsOperation { get; set; }
        public string[] BoundaryCodes { get; set; } = Array.Empty<string>();
        public 수출항만인수CommonDecisionPreviewApiModel CommonDecisionPreview { get; set; }
            = new 수출항만인수CommonDecisionPreviewApiModel();
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class 수출항만인수학당PreviewAdapter
    {
        private static readonly string[] RequiredBoundaryCodes =
        {
            "SimulationOnly",
            "PortStagingReceiptOnly",
            "NoExportDeclaration",
            "NoOfficialInspection",
            "NoCustomsClearance",
            "NoVesselLoading",
            "ExportReadinessRequiresSeparateDecision",
        };

        public 저녁학당업무Preview보강Input Map(
            수출항만인수PreviewApiModel source,
            long expectedDataRevision,
            플레이어내면상태Snapshot innerState,
            string focusedRuleCode)
        {
            Validate(source, expectedDataRevision, innerState, focusedRuleCode);

            var decision = source.CommonDecisionPreview.Decision;
            var task = source.CommonDecisionPreview.TaskPlan;
            var canonicalSources = new[]
            {
                source.ReceiptStableId,
                source.CargoStableId,
                source.SourceExportCargoHandoffStableId,
                source.SourceAllocationStableId,
                source.HarvestLotStableId,
                source.PackageLotStableId,
                source.ProductStableId,
                source.ReceivingFacilityStableId,
                decision.DecisionStableId,
                task.TaskStableId,
            }
                .Concat(decision.SourceStableIds)
                .Concat(task.SourceStableIds)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            var suffix = StableSuffix(source.ReceiptStableId);

            return new 저녁학당업무Preview보강Input
            {
                PreviewStableId = "export-port-receipt-preview:" + suffix,
                ExpectedDataRevision = expectedDataRevision,
                BusinessStageCode = "EXPORT-PORT-RECEIVING-1",
                ProductStableId = source.ProductStableId,
                Quantity = source.Quantity,
                UnitCode = source.UnitCode,
                CanonicalSourceStableIds = canonicalSources,
                InnerState = Clone(innerState),
                FocusedRuleCode = focusedRuleCode.Trim(),
                Unknowns = new[]
                {
                    new 업무Preview미확인사항
                    {
                        StableId = "preview-unknown:" + suffix + ".customs",
                        QuestionText = "수출신고·공식 검사·통관은 별도로 확인되었는가?",
                        ReasonText = "항만 준비시설 인수는 운영 통관이나 선적 완료가 아니다.",
                        SourceStableIds = new[]
                        {
                            source.ReceiptStableId,
                            source.CargoStableId,
                        },
                    },
                    new 업무Preview미확인사항
                    {
                        StableId = "preview-unknown:" + suffix + ".readiness",
                        QuestionText = "다음 수출 준비성 검토가 완료되었는가?",
                        ReasonText = "준비성 검토는 항만 인수 뒤 별도 Decision으로 진행한다.",
                        SourceStableIds = new[]
                        {
                            source.ReceiptStableId,
                            source.ReceivingFacilityStableId,
                        },
                    },
                },
                Milestones = new[]
                {
                    Milestone(suffix, 1, "allocation", "ExportAllocationVerified",
                        source.SourceAllocationStableId, source.HarvestLotStableId),
                    Milestone(suffix, 2, "handoff", "HandedOffInSimulation",
                        source.SourceExportCargoHandoffStableId, source.CargoStableId),
                    Milestone(suffix, 3, "arrival", "ArrivedAtDestination",
                        source.CargoStableId, source.ReceivingFacilityStableId),
                    Milestone(suffix, 4, "receipt-preview", "Previewed",
                        decision.DecisionStableId, task.TaskStableId, source.ReceiptStableId),
                },
            };
        }

        private static void Validate(
            수출항만인수PreviewApiModel source,
            long expectedDataRevision,
            플레이어내면상태Snapshot innerState,
            string focusedRuleCode)
        {
            if (source == null || expectedDataRevision <= 0 || innerState == null
                || focusedRuleCode == null
                || !StableDataId.IsValid(source.ReceiptStableId)
                || !StableDataId.IsValid(source.CargoStableId)
                || !StableDataId.IsValid(source.SourceExportCargoHandoffStableId)
                || !StableDataId.IsValid(source.SourceAllocationStableId)
                || !StableDataId.IsValid(source.HarvestLotStableId)
                || !StableDataId.IsValid(source.PackageLotStableId)
                || !StableDataId.IsValid(source.ProductStableId)
                || source.Quantity <= 0 || string.IsNullOrWhiteSpace(source.UnitCode)
                || !StableDataId.IsValid(source.ReceivingFacilityStableId)
                || !source.IsCandidateOnly || !source.DoesNotCreateCustomsOperation
                || source.BoundaryCodes == null || source.CommonDecisionPreview == null
                || source.CommonDecisionPreview.Decision == null
                || source.CommonDecisionPreview.TaskPlan == null)
                throw new InvalidOperationException("EveningExportPortPreviewApiModelInvalid");

            var boundaries = new HashSet<string>(source.BoundaryCodes, StringComparer.Ordinal);
            foreach (var required in RequiredBoundaryCodes)
            {
                if (!boundaries.Contains(required))
                    throw new InvalidOperationException(
                        "EveningExportPortPreviewBoundaryInvalid:" + required);
            }

            var decision = source.CommonDecisionPreview.Decision;
            var task = source.CommonDecisionPreview.TaskPlan;
            if (!StableDataId.IsValid(decision.DecisionStableId)
                || decision.DecisionTypeCode != "ExportPortReceiving"
                || decision.StateCode != "Previewed" || decision.Revision != 0
                || !StableDataId.IsValid(decision.SessionStableId)
                || !ValidIds(decision.TargetStableIds, true)
                || !decision.TargetStableIds.Contains(source.ReceiptStableId, StringComparer.Ordinal)
                || !decision.TargetStableIds.Contains(source.CargoStableId, StringComparer.Ordinal)
                || !decision.TargetStableIds.Contains(source.ReceivingFacilityStableId, StringComparer.Ordinal)
                || decision.Uncertainties == null || decision.BlockReasonCodes == null
                || !ValidIds(decision.SourceStableIds, true)
                || !StableDataId.IsValid(task.TaskStableId)
                || task.TaskTypeCode != "ExportPortReceiving"
                || task.FacilityStableId != source.ReceivingFacilityStableId
                || task.AssignedCapacity != source.Quantity
                || task.AssignedCapacityUnitCode != source.UnitCode
                || !ValidIds(task.InputLotStableIds, true)
                || !task.InputLotStableIds.Contains(source.CargoStableId, StringComparer.Ordinal)
                || task.OutputCandidateCodes == null
                || !task.OutputCandidateCodes.Contains(
                    "export-readiness-review-required", StringComparer.Ordinal)
                || !ValidIds(task.SourceStableIds, true))
                throw new InvalidOperationException("EveningExportPortPreviewLineageInvalid");

            if (decision.BlockReasonCodes.Length > 0)
                throw new InvalidOperationException("EveningExportPortPreviewBlocked:"
                    + string.Join(",", decision.BlockReasonCodes.OrderBy(
                        value => value, StringComparer.Ordinal)));
        }

        private static bool ValidIds(string[] values, bool requireAny)
            => values != null && (!requireAny || values.Length > 0)
                && values.All(StableDataId.IsValid)
                && values.Distinct(StringComparer.Ordinal).Count() == values.Length;

        private static 플레이어내면상태Snapshot Clone(플레이어내면상태Snapshot source)
            => new 플레이어내면상태Snapshot
            {
                알아차림 = source.알아차림,
                명료함 = source.명료함,
                양심 = source.양심,
                조화 = source.조화,
                의지 = source.의지,
                통찰 = source.통찰,
                ActiveRuleCodes = source.ActiveRuleCodes?.ToArray() ?? Array.Empty<string>(),
            };

        private static 업무PreviewMilestone Milestone(
            string suffix,
            int sequence,
            string code,
            string stateCode,
            params string[] sourceStableIds)
            => new 업무PreviewMilestone
            {
                StableId = "preview-milestone:" + suffix + "." + code,
                Sequence = sequence,
                TitleText = code,
                StateCode = stateCode,
                SourceStableIds = sourceStableIds,
            };

        private static string StableSuffix(string stableId)
            => stableId.Substring(stableId.IndexOf(':') + 1).Replace(':', '.');
    }
}
