using System;
using System.Linq;
using Ssalddel.Unity.Data;

namespace Ssalddel.Unity.Farm
{
    public sealed class HarvestDispositionImpactPreviewRequestData
    {
        public string DispositionDecisionStableId { get; set; } = string.Empty;
        public long DispositionDecisionRevision { get; set; }
        public string HarvestLotStableId { get; set; } = string.Empty;
        public long HarvestLotRevision { get; set; }
        public string ProductStableId { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string UnitCode { get; set; } = string.Empty;
        public string ChoiceCode { get; set; } = string.Empty;
        public string NextWorkflowCode { get; set; } = string.Empty;
        public string ActorStableId { get; set; } = string.Empty;
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class HarvestDispositionTaskCandidateData
    {
        public string CandidateTaskStableId { get; set; } = string.Empty;
        public string TaskTypeCode { get; set; } = string.Empty;
        public string[] InputLotStableIds { get; set; } = Array.Empty<string>();
        public string[] OutputCandidateCodes { get; set; } = Array.Empty<string>();
        public string[] SourceStableIds { get; set; } = Array.Empty<string>();
    }

    public sealed class HarvestDispositionBranchEnvelope
    {
        public HarvestDispositionImpactPreviewRequestData PreviewRequest { get; set; }
            = new HarvestDispositionImpactPreviewRequestData();
        public HarvestDispositionTaskCandidateData TaskCandidate { get; set; }
            = new HarvestDispositionTaskCandidateData();
        public bool RequiresServerPreview { get; set; }
        public bool RequiresExplicitConfirmation { get; set; }
        public bool ServerMustRecalculatePolicy { get; set; }
        public bool DoesNotApplySettlementState { get; set; }
        public bool DoesNotCreateCargoOrSale { get; set; }
    }

    [Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
        Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E7,
        "플레이어 입력·화면·피드백과 플레이 경험 표현을 조율한다.",
        Boundary = "Unity 표현은 권위 상태 변경이나 실제 플레이 완료를 대신하지 않는다.")]
    public sealed class HarvestDispositionBranchAdapter
    {
        private readonly HarvestDispositionSimulationValidator validator;

        public HarvestDispositionBranchAdapter(HarvestDispositionSimulationValidator value)
            => validator = value ?? throw new ArgumentNullException(nameof(value));

        public HarvestDispositionBranchEnvelope CreatePreviewEnvelope(
            HarvestDispositionSimulationSnapshot snapshot,
            string actorStableId)
        {
            validator.Validate(snapshot);
            if (snapshot.StateCode != HarvestDispositionStateCodes.Decided
                || snapshot.Decision == null)
            {
                throw new InvalidOperationException("HarvestDispositionDecisionRequired");
            }

            if (!StableDataId.IsValid(actorStableId))
                throw new InvalidOperationException("HarvestDispositionActorStableIdInvalid");

            var decision = snapshot.Decision;
            var expectedWorkflow = HarvestDispositionWorkflowCodes.ForChoice(decision.ChoiceCode);
            if (!string.Equals(decision.NextWorkflowCode, expectedWorkflow, StringComparison.Ordinal))
                throw new InvalidOperationException("HarvestDispositionWorkflowMismatch");

            var sourceStableIds = decision.SourceStableIds
                .Concat(snapshot.HarvestLot.SourceStableIds)
                .Concat(snapshot.SourceStableIds)
                .Append(decision.StableId)
                .ToArray();
            if (sourceStableIds.Any(value => !StableDataId.IsValid(value)))
                throw new InvalidOperationException("HarvestDispositionSourceStableIdsInvalid");
            sourceStableIds = sourceStableIds
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            var request = new HarvestDispositionImpactPreviewRequestData
            {
                DispositionDecisionStableId = decision.StableId,
                DispositionDecisionRevision = decision.Revision,
                HarvestLotStableId = snapshot.HarvestLot.StableId,
                HarvestLotRevision = snapshot.HarvestLot.Revision,
                ProductStableId = snapshot.HarvestLot.CanonicalProductStableId,
                Quantity = decision.Quantity,
                UnitCode = decision.UnitCode,
                ChoiceCode = decision.ChoiceCode,
                NextWorkflowCode = decision.NextWorkflowCode,
                ActorStableId = actorStableId,
                SourceStableIds = sourceStableIds,
            };

            return new HarvestDispositionBranchEnvelope
            {
                PreviewRequest = request,
                TaskCandidate = new HarvestDispositionTaskCandidateData
                {
                    CandidateTaskStableId = "task:harvest-impact:" + decision.StableId,
                    TaskTypeCode = decision.ChoiceCode + "Work",
                    InputLotStableIds = new[] { snapshot.HarvestLot.StableId },
                    OutputCandidateCodes = new[] { decision.NextWorkflowCode },
                    SourceStableIds = sourceStableIds.ToArray(),
                },
                RequiresServerPreview = true,
                RequiresExplicitConfirmation = true,
                ServerMustRecalculatePolicy = true,
                DoesNotApplySettlementState = true,
                DoesNotCreateCargoOrSale = true,
            };
        }
    }
}
