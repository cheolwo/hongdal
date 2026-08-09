using System;
using System.Linq;
using Ssalddel.Unity.Data;
using Ssalddel.Unity.Npcs;
using Ssalddel.Unity.PresentationContracts.LearningCards;

namespace Ssalddel.Unity.UrbanMarket
{
    public sealed class UrbanMarketSupplyManagementApiModel
    {
        public long Revision { get; set; }
        public string PresentationRevision { get; set; } = string.Empty;
        public string ModeCode { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public string QuantityUnitCode { get; set; } = string.Empty;
        public UrbanMarketDemandBriefingApiModel DemandAndOrders { get; set; } =
            new UrbanMarketDemandBriefingApiModel();
        public UrbanMarketManagementPreviewApiModel ManagementPreview { get; set; } =
            new UrbanMarketManagementPreviewApiModel();
        public UrbanMarketSupplierPortfolioApiModel[] SupplyPortfolio { get; set; } =
            Array.Empty<UrbanMarketSupplierPortfolioApiModel>();
        public UrbanMarketConceptCardSourceApiModel[] SourceLineage { get; set; } =
            Array.Empty<UrbanMarketConceptCardSourceApiModel>();
    }

    public sealed class UrbanMarketDemandBriefingApiModel
    {
        public int AsOfTick { get; set; }
        public int TodayOrderCount { get; set; }
        public decimal TodayRequestedQuantity { get; set; }
        public decimal PendingOrderQuantity { get; set; }
        public decimal CurrentAvailableInventory { get; set; }
        public decimal TodayScheduledInbound { get; set; }
        public decimal ImmediatelyFulfillableQuantity { get; set; }
        public decimal InboundAfterProcessingPotentialQuantity { get; set; }
        public decimal CannotCoverQuantity { get; set; }
        public string[] ReasonCodes { get; set; } = Array.Empty<string>();
        public string LimitationText { get; set; } = string.Empty;
    }

    public sealed class UrbanMarketManagementPreviewApiModel
    {
        public decimal HardDemandQuantity { get; set; }
        public decimal FulfilledQuantity { get; set; }
        public decimal UnfulfilledQuantity { get; set; }
        public decimal PurchaseCost { get; set; }
        public decimal EndingCash { get; set; }
        public decimal OutstandingPaymentAmount { get; set; }
        public decimal WasteQuantity { get; set; }
        public decimal ReceivingWorkload { get; set; }
    }

    public sealed class UrbanMarketSupplierPortfolioApiModel
    {
        public string SupplierStableId { get; set; } = string.Empty;
        public decimal AcceptedQuantity { get; set; }
        public decimal AcceptedSupplyShareRate { get; set; }
        public decimal PurchaseCost { get; set; }
    }

    public sealed class UrbanMarketSupplyManagementPresentationModel
    {
        public long Revision { get; set; }
        public string PresentationRevision { get; set; } = string.Empty;
        public string ModeCode { get; set; } = string.Empty;
        public string ProductStableId { get; set; } = string.Empty;
        public string QuantityUnitCode { get; set; } = string.Empty;
        public UrbanMarketDemandBriefingApiModel DemandAndOrders { get; set; } = null!;
        public UrbanMarketManagementPreviewApiModel ManagementPreview { get; set; } = null!;
        public UrbanMarketSupplierPortfolioApiModel[] SupplyPortfolio { get; set; } =
            Array.Empty<UrbanMarketSupplierPortfolioApiModel>();
        public ConceptCardSourceLineageItem[] SourceLineage { get; set; } =
            Array.Empty<ConceptCardSourceLineageItem>();
    }

    public sealed class UrbanMarketSupplyManagementPresentationMapper
    {
        public UrbanMarketSupplyManagementPresentationModel Map(UrbanMarketSupplyManagementApiModel source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (source.Revision < 0 || string.IsNullOrWhiteSpace(source.PresentationRevision))
                throw new InvalidOperationException("SupplyManagementPresentationRevisionInvalid");
            if (source.ModeCode != "Simulation")
                throw new InvalidOperationException("SupplyManagementOperationalFallbackForbidden");
            StableDataId.EnsureValid(source.ProductStableId, nameof(source.ProductStableId));
            if (string.IsNullOrWhiteSpace(source.QuantityUnitCode))
                throw new InvalidOperationException("SupplyManagementQuantityUnitMissing");
            if (source.DemandAndOrders == null || source.ManagementPreview == null
                || source.SupplyPortfolio == null || source.SupplyPortfolio.Length == 0)
                throw new InvalidOperationException("SupplyManagementSurfaceMissing");
            var demand = source.DemandAndOrders;
            if (demand.AsOfTick < 0 || demand.TodayOrderCount < 0
                || demand.TodayRequestedQuantity < 0m || demand.PendingOrderQuantity < 0m
                || demand.CurrentAvailableInventory < 0m || demand.TodayScheduledInbound < 0m
                || demand.ImmediatelyFulfillableQuantity < 0m
                || demand.InboundAfterProcessingPotentialQuantity < 0m
                || demand.CannotCoverQuantity < 0m
                || demand.PendingOrderQuantity != demand.ImmediatelyFulfillableQuantity
                    + demand.InboundAfterProcessingPotentialQuantity + demand.CannotCoverQuantity
                || string.IsNullOrWhiteSpace(demand.LimitationText))
                throw new InvalidOperationException("SupplyManagementDemandBriefingInvalid");
            var preview = source.ManagementPreview;
            if (preview.HardDemandQuantity < 0m || preview.FulfilledQuantity < 0m
                || preview.UnfulfilledQuantity < 0m
                || preview.HardDemandQuantity != preview.FulfilledQuantity + preview.UnfulfilledQuantity
                || preview.PurchaseCost < 0m || preview.EndingCash < 0m
                || preview.OutstandingPaymentAmount < 0m || preview.WasteQuantity < 0m
                || preview.ReceivingWorkload < 0m)
                throw new InvalidOperationException("SupplyManagementPreviewInvalid");
            if (source.SupplyPortfolio.Any(value => value == null
                    || string.IsNullOrWhiteSpace(value.SupplierStableId)
                    || value.AcceptedQuantity < 0m || value.AcceptedSupplyShareRate < 0m
                    || value.AcceptedSupplyShareRate > 1m || value.PurchaseCost < 0m)
                || source.SupplyPortfolio.Select(value => value.SupplierStableId)
                    .Distinct(StringComparer.Ordinal).Count() != source.SupplyPortfolio.Length)
                throw new InvalidOperationException("SupplyManagementPortfolioInvalid");
            return new UrbanMarketSupplyManagementPresentationModel
            {
                Revision = source.Revision,
                PresentationRevision = source.PresentationRevision,
                ModeCode = source.ModeCode,
                ProductStableId = source.ProductStableId,
                QuantityUnitCode = source.QuantityUnitCode.Trim(),
                DemandAndOrders = demand,
                ManagementPreview = preview,
                SupplyPortfolio = source.SupplyPortfolio.OrderBy(value => value.SupplierStableId,
                    StringComparer.Ordinal).ToArray(),
                SourceLineage = UrbanMarketConceptCardSourceMapper.MapRequired(
                    source.SourceLineage,
                    "SupplyManagementSourceLineageMissing"),
            };
        }
    }

    public interface IUrbanMarketSupplyManagementPresentationTarget
    {
        void ApplySupplyManagement(UrbanMarketSupplyManagementPresentationModel model);
    }

    public sealed class UrbanMarketSupplyManagementPresentationApplicator
    {
        private long appliedRevision = -1;

        public bool Apply(
            UrbanMarketSupplyManagementPresentationModel model,
            IUrbanMarketSupplyManagementPresentationTarget target)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (model.Revision < appliedRevision) return false;
            target.ApplySupplyManagement(model);
            appliedRevision = model.Revision;
            return true;
        }
    }

    public sealed class ResidentialGroupRepresentativeDialoguePresentationModel
    {
        public string InquiryStableId { get; set; } = string.Empty;
        public string TitleText { get; set; } = string.Empty;
        public string DemandText { get; set; } = string.Empty;
        public string BoundaryText { get; set; } = string.Empty;
        public string CommandEffectCode { get; set; } = RepresentativeVisitCommandEffectCodes.None;
    }

    public interface IResidentialGroupRepresentativeDialogueTarget
    {
        void ApplyRepresentativeDialogue(ResidentialGroupRepresentativeDialoguePresentationModel model);
    }

    public sealed class ResidentialGroupRepresentativeUnityCoordinator
    {
        private readonly ResidentialGroupRepresentativeVisitValidator visitValidator =
            new ResidentialGroupRepresentativeVisitValidator();
        private readonly NpcMovementInterpreter movementInterpreter = new NpcMovementInterpreter();
        private readonly NpcMovementPresenter movementPresenter = new NpcMovementPresenter();

        public void Apply(
            ResidentialGroupRepresentativeVisitSnapshot visit,
            ResidentialGroupRepresentativeDialoguePresentationModel dialogue,
            INpcMovementPresentationTarget npcTarget,
            IResidentialGroupRepresentativeDialogueTarget dialogueTarget)
        {
            visitValidator.Validate(visit);
            if (dialogue == null || string.IsNullOrWhiteSpace(dialogue.InquiryStableId)
                || dialogue.InquiryStableId != visit.InquiryStableId
                || string.IsNullOrWhiteSpace(dialogue.TitleText)
                || string.IsNullOrWhiteSpace(dialogue.DemandText)
                || string.IsNullOrWhiteSpace(dialogue.BoundaryText)
                || dialogue.CommandEffectCode != RepresentativeVisitCommandEffectCodes.None)
                throw new InvalidOperationException("RepresentativeDialogueInvalid");
            if (npcTarget == null || npcTarget.NpcStableId != visit.NpcStableId)
                throw new InvalidOperationException("RepresentativeNpcTargetMismatch");
            if (dialogueTarget == null) throw new ArgumentNullException(nameof(dialogueTarget));
            var movement = movementPresenter.Present(
                movementInterpreter.Interpret(visit.ActiveMovement()));
            npcTarget.ApplyMovementPresentation(movement);
            dialogueTarget.ApplyRepresentativeDialogue(dialogue);
        }
    }
}
