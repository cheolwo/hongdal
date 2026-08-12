using System.Security.Cryptography;
using System.Text;
using FluentResults;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.WorldProjection;

namespace Ssalddel.Application.WorldProjection;

public sealed record 통합전시관ProjectionInput(
    IReadOnlyList<통합전시관ExhibitResponse> Exhibits,
    DateTimeOffset GeneratedAtUtc)
{
    public IReadOnlyList<통합전시관SeedbedObjectResponse> SeedbedObjects { get; init; } = [];
    public IReadOnlyList<통합전시관ScenePlacementResponse> ScenePlacements { get; init; } = [];
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.IntegratedSeedbedExhibition,
    SsalddelCodeLayer.Application,
    "서로 다른 모판·전시 후보를 stable ID, source lineage, 권한과 독립 증거 축을 보존한 읽기 전용 manifest로 투영한다.",
    Effects = SsalddelCodeEffect.None,
    ContractType = typeof(통합전시관ManifestResponse),
    FlowOrder = 20,
    Boundary = "전시 manifest는 domain Command를 실행하지 않으며 generic Confirm과 prefab 경로 기반 업무 관계를 허용하지 않는다.")]
public sealed class 통합전시관Projector
{
    private const string ManifestStableId = "exhibition-manifest:integrated-seedbed";
    private const string GenericConfirmIntent = "ConfirmExhibit";

    private static readonly HashSet<string> DataStates =
    [
        통합전시관DataStateCodes.Live,
        통합전시관DataStateCodes.Cached,
        통합전시관DataStateCodes.Fixture,
        통합전시관DataStateCodes.Uncollected,
        통합전시관DataStateCodes.Invalid,
        통합전시관DataStateCodes.Failed,
    ];

    private static readonly HashSet<string> ExperienceModes =
    [
        통합전시관ExperienceModeCodes.Research,
        통합전시관ExperienceModeCodes.ReadOnly,
        통합전시관ExperienceModeCodes.Simulation,
        통합전시관ExperienceModeCodes.OperationalHandoff,
    ];

    private static readonly HashSet<string> CompletionStates =
    [
        통합전시관CompletionStateCodes.Candidate,
        통합전시관CompletionStateCodes.Linked,
        통합전시관CompletionStateCodes.Verified,
        통합전시관CompletionStateCodes.Blocked,
        통합전시관CompletionStateCodes.Promoted,
    ];

    private static readonly string[] RequiredEvidenceKinds =
    [
        통합전시관EvidenceKindCodes.Code,
        통합전시관EvidenceKindCodes.FocusedTest,
        통합전시관EvidenceKindCodes.Runtime,
        통합전시관EvidenceKindCodes.Operational,
    ];

    private static readonly HashSet<string> EvidenceStatuses =
    [
        통합전시관EvidenceStatusCodes.Verified,
        통합전시관EvidenceStatusCodes.Partial,
        통합전시관EvidenceStatusCodes.Unverified,
        통합전시관EvidenceStatusCodes.NotApplicable,
    ];

    private static readonly string[] ObjectEvidenceKinds =
    [
        통합전시관ObjectEvidenceKindCodes.SourceIndex,
        통합전시관ObjectEvidenceKindCodes.MeaningReview,
        통합전시관ObjectEvidenceKindCodes.VisualResolution,
        통합전시관ObjectEvidenceKindCodes.PlacementValidation,
        통합전시관ObjectEvidenceKindCodes.BindingValidation,
        통합전시관ObjectEvidenceKindCodes.ObjectPreview,
        통합전시관ObjectEvidenceKindCodes.ScenePlacement,
    ];

    private static readonly string[] ObjectGateStates =
    [
        통합전시관ObjectGateStateCodes.Indexed,
        통합전시관ObjectGateStateCodes.MeaningMapped,
        통합전시관ObjectGateStateCodes.VisualResolved,
        통합전시관ObjectGateStateCodes.PlacementValidated,
        통합전시관ObjectGateStateCodes.BindingValidated,
        통합전시관ObjectGateStateCodes.RuntimeVerified,
        통합전시관ObjectGateStateCodes.PromotedToScene,
    ];

    private static readonly HashSet<string> CheckpointAuthorities =
    [
        통합전시관CheckpointAuthorityCodes.SimulationFixture,
        통합전시관CheckpointAuthorityCodes.OperationalProjection,
    ];

    private static readonly HashSet<string> DisclosureScopes =
    [
        통합전시관DisclosureScopeCodes.OwnerPrivate,
        통합전시관DisclosureScopeCodes.PrivacySafeAggregate,
        통합전시관DisclosureScopeCodes.OrdererPublic,
        통합전시관DisclosureScopeCodes.MarketOperatorAuthorized,
        통합전시관DisclosureScopeCodes.RestaurantAuthorized,
        통합전시관DisclosureScopeCodes.DriverCandidateApproximate,
        통합전시관DisclosureScopeCodes.AssignedDriverAuthorized,
    ];

    public Result<통합전시관ManifestResponse> Project(통합전시관ProjectionInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.Exhibits is null || input.Exhibits.Count == 0)
        {
            return Result.Fail<통합전시관ManifestResponse>("IntegratedExhibitionEmpty");
        }

        var duplicate = input.Exhibits
            .GroupBy(value => value.ExhibitStableId, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            return Result.Fail<통합전시관ManifestResponse>(
                "IntegratedExhibitionDuplicate:" + duplicate.Key);
        }

        foreach (var exhibit in input.Exhibits)
        {
            var error = Validate(exhibit);
            if (error is not null)
            {
                return Result.Fail<통합전시관ManifestResponse>(error);
            }
        }

        var seedbedObjects = input.SeedbedObjects ?? [];
        var objectDuplicate = seedbedObjects
            .GroupBy(value => value.ObjectStableId, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);
        if (objectDuplicate is not null)
            return Result.Fail<통합전시관ManifestResponse>(
                "IntegratedExhibitionSeedbedObjectDuplicate:" + objectDuplicate.Key);

        foreach (var seedbedObject in seedbedObjects)
        {
            var error = ValidateSeedbedObject(seedbedObject);
            if (error is not null)
                return Result.Fail<통합전시관ManifestResponse>(error);
        }

        var objectIds = seedbedObjects.Select(value => value.ObjectStableId)
            .ToHashSet(StringComparer.Ordinal);
        foreach (var exhibit in input.Exhibits)
        {
            var missingObject = (exhibit.ReferencedSeedbedObjectStableIds ?? [])
                .FirstOrDefault(value => !objectIds.Contains(value));
            if (missingObject is not null)
                return Result.Fail<통합전시관ManifestResponse>(
                    "IntegratedExhibitionStoryObjectReferenceMissing:" + exhibit.ExhibitStableId + ":" + missingObject);
        }

        var placements = input.ScenePlacements ?? [];
        var placementDuplicate = placements
            .GroupBy(value => value.PlacementStableId, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);
        if (placementDuplicate is not null)
            return Result.Fail<통합전시관ManifestResponse>(
                "IntegratedExhibitionScenePlacementDuplicate:" + placementDuplicate.Key);
        foreach (var placement in placements)
        {
            var error = ValidateScenePlacement(placement, objectIds);
            if (error is not null)
                return Result.Fail<통합전시관ManifestResponse>(error);
        }

        foreach (var promoted in seedbedObjects.Where(value =>
                     value.GateStateCode == 통합전시관ObjectGateStateCodes.PromotedToScene))
        {
            if (!placements.Any(value => value.ObjectStableId == promoted.ObjectStableId
                                         && value.ValidationStatusCode == 통합전시관ObjectGateStateCodes.PromotedToScene))
                return Result.Fail<통합전시관ManifestResponse>(
                    "IntegratedExhibitionPromotedObjectPlacementMissing:" + promoted.ObjectStableId);
        }

        var exhibits = input.Exhibits
            .OrderBy(value => value.ExhibitStableId, StringComparer.Ordinal)
            .Select(Clone)
            .ToArray();
        var objectSnapshots = seedbedObjects.OrderBy(value => value.ObjectStableId, StringComparer.Ordinal)
            .Select(CloneSeedbedObject).ToArray();
        var placementSnapshots = placements.OrderBy(value => value.PlacementStableId, StringComparer.Ordinal)
            .Select(CloneScenePlacement).ToArray();

        return Result.Ok(new 통합전시관ManifestResponse
        {
            StableId = ManifestStableId,
            Revision = ComputeRevision(exhibits, objectSnapshots, placementSnapshots),
            GeneratedAtUtc = input.GeneratedAtUtc,
            IsReadOnly = true,
            Exhibits = exhibits,
            Stories = exhibits,
            SeedbedObjects = objectSnapshots,
            ScenePlacements = placementSnapshots,
        });
    }

    private static string? Validate(통합전시관ExhibitResponse exhibit)
    {
        if (exhibit is null || Required(
                exhibit.ExhibitStableId,
                exhibit.DisplayName,
                exhibit.ExhibitKindCode,
                exhibit.WorkflowKey,
                exhibit.ProductVersionCode,
                exhibit.PerspectiveCode,
                exhibit.AuthorizationScopeCode,
                exhibit.WorldStableId,
                exhibit.ZoneStableId,
                exhibit.SourceRevision,
                exhibit.ProjectionRevision))
        {
            return "IntegratedExhibitionRequiredFieldMissing";
        }

        if (!DataStates.Contains(exhibit.DataStateCode))
            return "IntegratedExhibitionDataStateInvalid:" + exhibit.ExhibitStableId;
        if (!ExperienceModes.Contains(exhibit.ExperienceModeCode))
            return "IntegratedExhibitionExperienceModeInvalid:" + exhibit.ExhibitStableId;
        if (!CompletionStates.Contains(exhibit.CompletionStateCode))
            return "IntegratedExhibitionCompletionStateInvalid:" + exhibit.ExhibitStableId;

        if (MissingOrDuplicate(exhibit.ObjectStableIds)
            || MissingOrDuplicate(exhibit.AllowedInteractionIntentCodes)
            || MissingOrDuplicate(exhibit.VisualKeys)
            || MissingOrDuplicate(exhibit.PackRoleCodes))
        {
            return "IntegratedExhibitionCollectionInvalid:" + exhibit.ExhibitStableId;
        }

        if (exhibit.SourcePlan is null || exhibit.SourcePlan.Count == 0
            || exhibit.SourcePlan.Any(value => value is null || Required(
                value.SourceKey,
                value.SourceStableId,
                value.SourceRevision,
                value.SourceModeCode))
            || HasDuplicate(exhibit.SourcePlan.Select(value => value.SourceStableId)))
        {
            return "IntegratedExhibitionSourcePlanInvalid:" + exhibit.ExhibitStableId;
        }

        if (exhibit.CanonicalRecordRelations is null
            || exhibit.CanonicalRecordRelations.Count == 0
            || exhibit.CanonicalRecordRelations.Any(value => value is null || Required(
                value.RelationStableId,
                value.SourceRecordKindCode,
                value.SourceStableId,
                value.SourceRevision,
                value.RelationCode,
                value.TargetRecordKindCode,
                value.TargetStableId,
                value.TargetRevision,
                value.ExpectedTargetRevision,
                value.VerificationStatusCode))
            || HasDuplicate(exhibit.CanonicalRecordRelations.Select(value => value.RelationStableId)))
        {
            return "IntegratedExhibitionRelationInvalid:" + exhibit.ExhibitStableId;
        }

        var checkpointError = ValidateCheckpoints(exhibit);
        if (checkpointError is not null)
            return checkpointError;

        if (!HasAllEvidenceAxes(exhibit.Evidence))
            return "IntegratedExhibitionEvidenceAxesInvalid:" + exhibit.ExhibitStableId;

        if (exhibit.Evidence.Any(value => !EvidenceStatuses.Contains(value.StatusCode)))
            return "IntegratedExhibitionEvidenceStatusInvalid:" + exhibit.ExhibitStableId;

        if (exhibit.Evidence.Any(value => value.Reference.Contains(":\\", StringComparison.Ordinal)))
            return "IntegratedExhibitionLocalEvidencePathForbidden:" + exhibit.ExhibitStableId;

        var intents = exhibit.AllowedInteractionIntentCodes;
        if (intents.Contains(GenericConfirmIntent, StringComparer.Ordinal))
            return "IntegratedExhibitionGenericConfirmForbidden:" + exhibit.ExhibitStableId;

        if ((exhibit.ExperienceModeCode == 통합전시관ExperienceModeCodes.Research
             || exhibit.ExperienceModeCode == 통합전시관ExperienceModeCodes.ReadOnly)
            && intents.Any(IsMutationIntent))
        {
            return "IntegratedExhibitionReadOnlyMutationForbidden:" + exhibit.ExhibitStableId;
        }

        if (intents.Contains(통합전시관InteractionIntentCodes.SimulationConfirm, StringComparer.Ordinal)
            && exhibit.ExperienceModeCode != 통합전시관ExperienceModeCodes.Simulation)
        {
            return "IntegratedExhibitionSimulationConfirmModeRequired:" + exhibit.ExhibitStableId;
        }

        if (intents.Contains(통합전시관InteractionIntentCodes.DomainCommand, StringComparer.Ordinal)
            && !intents.Contains(통합전시관InteractionIntentCodes.RefreshCanonical, StringComparer.Ordinal))
        {
            return "IntegratedExhibitionCanonicalRefreshRequired:" + exhibit.ExhibitStableId;
        }

        if (exhibit.ExperienceModeCode == 통합전시관ExperienceModeCodes.OperationalHandoff
            && !intents.Contains(통합전시관InteractionIntentCodes.WebHandoff, StringComparer.Ordinal)
            && !intents.Contains(통합전시관InteractionIntentCodes.DomainCommand, StringComparer.Ordinal))
        {
            return "IntegratedExhibitionOperationalHandoffMissing:" + exhibit.ExhibitStableId;
        }

        if (exhibit.DataStateCode == 통합전시관DataStateCodes.Live
            && exhibit.SourcePlan.Any(value => value.SourceModeCode.Contains("Fixture", StringComparison.OrdinalIgnoreCase)))
        {
            return "IntegratedExhibitionLiveFixtureContradiction:" + exhibit.ExhibitStableId;
        }

        if (exhibit.DataStateCode == 통합전시관DataStateCodes.Live
            && EvidenceStatus(exhibit, 통합전시관EvidenceKindCodes.Operational)
                != 통합전시관EvidenceStatusCodes.Verified)
        {
            return "IntegratedExhibitionLiveOperationalEvidenceRequired:" + exhibit.ExhibitStableId;
        }

        if (exhibit.DataStateCode == 통합전시관DataStateCodes.Uncollected
            && (exhibit.BlockedReasonCodes is null || exhibit.BlockedReasonCodes.Count == 0))
        {
            return "IntegratedExhibitionUncollectedReasonRequired:" + exhibit.ExhibitStableId;
        }

        if (exhibit.CompletionStateCode == 통합전시관CompletionStateCodes.Blocked
            && (exhibit.BlockedReasonCodes is null || exhibit.BlockedReasonCodes.Count == 0))
        {
            return "IntegratedExhibitionBlockedReasonRequired:" + exhibit.ExhibitStableId;
        }

        if (exhibit.CompletionStateCode == 통합전시관CompletionStateCodes.Promoted
            && RequiredEvidenceKinds.Take(3).Any(kind => EvidenceStatus(exhibit, kind)
                != 통합전시관EvidenceStatusCodes.Verified))
        {
            return "IntegratedExhibitionPromotionEvidenceRequired:" + exhibit.ExhibitStableId;
        }

        if (exhibit.ExhibitKindCode == "CargoHubWarehouseLineage")
        {
            var cargoError = ValidateCargoHubWarehouseLineage(exhibit);
            if (cargoError is not null)
                return cargoError;
        }

        if (exhibit.ExhibitKindCode == "OrdererGroupUrbanMarketLineage")
        {
            var marketError = ValidateOrdererGroupUrbanMarketLineage(exhibit);
            if (marketError is not null)
                return marketError;
        }

        if (exhibit.ExhibitKindCode == "FoodDeliveryLineage")
        {
            var foodDeliveryError = ValidateFoodDeliveryLineage(exhibit);
            if (foodDeliveryError is not null)
                return foodDeliveryError;
        }

        return null;
    }

    private static string? ValidateCheckpoints(통합전시관ExhibitResponse exhibit)
    {
        var checkpoints = exhibit.WorkflowCheckpoints ?? [];
        if (checkpoints.Any(value => value is null || value.Sequence <= 0 || Required(
                value.CheckpointStableId,
                value.StateMachineCode,
                value.StateCode,
                value.LineageStableId,
                value.CanonicalRecordStableId,
                value.Revision,
                value.AuthorityCode,
                value.DisclosureScopeCode,
                value.BoundaryCode))
            || HasDuplicate(checkpoints.Select(value => value.CheckpointStableId))
            || HasDuplicate(checkpoints.Select(value => value.Sequence.ToString())))
        {
            return "IntegratedExhibitionCheckpointInvalid:" + exhibit.ExhibitStableId;
        }

        if (checkpoints.Any(value => !CheckpointAuthorities.Contains(value.AuthorityCode)))
            return "IntegratedExhibitionCheckpointAuthorityInvalid:" + exhibit.ExhibitStableId;
        if (checkpoints.Any(value => !DisclosureScopes.Contains(value.DisclosureScopeCode)))
            return "IntegratedExhibitionDisclosureScopeInvalid:" + exhibit.ExhibitStableId;

        var ordered = checkpoints.OrderBy(value => value.Sequence).ToArray();
        if (ordered.Where((value, index) => value.Sequence != index + 1).Any())
            return "IntegratedExhibitionCheckpointSequenceGap:" + exhibit.ExhibitStableId;
        return null;
    }

    private static string? ValidateCargoHubWarehouseLineage(통합전시관ExhibitResponse exhibit)
    {
        if (exhibit.CanonicalRecordRelations.Count < 5 || exhibit.WorkflowCheckpoints.Count < 7)
            return "IntegratedExhibitionCargoLineageIncomplete:" + exhibit.ExhibitStableId;

        var lineages = exhibit.WorkflowCheckpoints
            .Select(value => value.LineageStableId)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (lineages.Length != 1 || !exhibit.ObjectStableIds.Contains(lineages[0], StringComparer.Ordinal))
            return "IntegratedExhibitionCargoLineageMismatch:" + exhibit.ExhibitStableId;

        var warehouseArrival = exhibit.WorkflowCheckpoints.SingleOrDefault(value =>
            value.StateMachineCode == "WarehouseHandoff" && value.StateCode == "ArrivedAtWarehouse");
        if (!exhibit.WorkflowCheckpoints.Any(value =>
                value.StateMachineCode == "CargoJourney" && value.StateCode == "ArrivedAtHub")
            || warehouseArrival is null
            || !warehouseArrival.RequiresSeparateConfirmation
            || !exhibit.WorkflowCheckpoints.Any(value =>
                value.StateMachineCode == "WarehouseHandoff" && value.StateCode == "ReceivingCompleted"))
        {
            return "IntegratedExhibitionCargoHandoffStatesInvalid:" + exhibit.ExhibitStableId;
        }

        var nextBySource = exhibit.CanonicalRecordRelations
            .ToDictionary(value => value.SourceStableId, StringComparer.Ordinal);
        var current = exhibit.CanonicalRecordRelations.SingleOrDefault(value =>
            value.SourceRecordKindCode == "ShipperRequestCandidate");
        if (current is null)
            return "IntegratedExhibitionCargoLineageStartMissing:" + exhibit.ExhibitStableId;
        var visited = new HashSet<string>(StringComparer.Ordinal);
        while (visited.Add(current.RelationStableId)
               && nextBySource.TryGetValue(current.TargetStableId, out var next))
            current = next;
        return visited.Count == exhibit.CanonicalRecordRelations.Count
               && current.TargetRecordKindCode == "WarehouseWorldSnapshot"
            ? null
            : "IntegratedExhibitionCargoRelationChainInvalid:" + exhibit.ExhibitStableId;
    }

    private static string? ValidateOrdererGroupUrbanMarketLineage(통합전시관ExhibitResponse exhibit)
    {
        if (exhibit.CanonicalRecordRelations.Count < 6 || exhibit.WorkflowCheckpoints.Count < 6)
            return "IntegratedExhibitionOrdererMarketLineageIncomplete:" + exhibit.ExhibitStableId;

        var checkpoints = exhibit.WorkflowCheckpoints;
        var privateIntent = checkpoints.SingleOrDefault(value =>
            value.StateMachineCode == "IndividualIntent"
            && value.DisclosureScopeCode == 통합전시관DisclosureScopeCodes.OwnerPrivate);
        var preview = checkpoints.SingleOrDefault(value =>
            value.StateMachineCode == "GroupingPreview"
            && value.DisclosureScopeCode == 통합전시관DisclosureScopeCodes.PrivacySafeAggregate);
        var publicProduct = checkpoints.SingleOrDefault(value =>
            value.StateMachineCode == "MartPublicProduct"
            && value.DisclosureScopeCode == 통합전시관DisclosureScopeCodes.OrdererPublic);
        var operatorInventory = checkpoints.SingleOrDefault(value =>
            value.StateMachineCode == "MarketInventory"
            && value.DisclosureScopeCode == 통합전시관DisclosureScopeCodes.MarketOperatorAuthorized);
        var shelfTask = checkpoints.SingleOrDefault(value =>
            value.StateMachineCode == "ShelfTask"
            && value.DisclosureScopeCode == 통합전시관DisclosureScopeCodes.MarketOperatorAuthorized);
        if (privateIntent is null || !privateIntent.RequiresSeparateConfirmation
            || preview is null || !preview.RequiresSeparateConfirmation
            || publicProduct is null || operatorInventory is null
            || shelfTask is null || !shelfTask.RequiresSeparateConfirmation)
        {
            return "IntegratedExhibitionOrdererMarketDisclosureBoundaryInvalid:" + exhibit.ExhibitStableId;
        }

        if (publicProduct.CanonicalRecordStableId == operatorInventory.CanonicalRecordStableId
            || exhibit.AllowedInteractionIntentCodes.Contains(통합전시관InteractionIntentCodes.SimulationConfirm, StringComparer.Ordinal)
            || exhibit.AllowedInteractionIntentCodes.Contains(통합전시관InteractionIntentCodes.DomainCommand, StringComparer.Ordinal))
        {
            return "IntegratedExhibitionOrdererMarketAuthorityBoundaryInvalid:" + exhibit.ExhibitStableId;
        }

        var priceComparison = exhibit.CanonicalRecordRelations.SingleOrDefault(value =>
            value.SourceRecordKindCode == "KamisObservation"
            && value.RelationCode == "ComparedWithNotUsedAsSalePrice"
            && value.TargetRecordKindCode == "MartPublicProduct");
        if (priceComparison is null
            || !exhibit.BlockedReasonCodes.Contains("SalePriceIsNotKamisObservation", StringComparer.Ordinal)
            || !exhibit.BlockedReasonCodes.Contains("PublicQuantityIsNotPhysicalInventory", StringComparer.Ordinal))
        {
            return "IntegratedExhibitionOrdererMarketPriceInventoryBoundaryInvalid:" + exhibit.ExhibitStableId;
        }

        return null;
    }

    private static string? ValidateFoodDeliveryLineage(통합전시관ExhibitResponse exhibit)
    {
        if (exhibit.CanonicalRecordRelations.Count < 7 || exhibit.WorkflowCheckpoints.Count < 8)
            return "IntegratedExhibitionFoodDeliveryLineageIncomplete:" + exhibit.ExhibitStableId;

        var lineages = exhibit.WorkflowCheckpoints.Select(value => value.LineageStableId)
            .Distinct(StringComparer.Ordinal).ToArray();
        if (lineages.Length != 1 || !exhibit.ObjectStableIds.Contains(lineages[0], StringComparer.Ordinal))
            return "IntegratedExhibitionFoodDeliveryLineageMismatch:" + exhibit.ExhibitStableId;
        if (exhibit.WorkflowCheckpoints.Any(value => value.StateMachineCode is "CargoJourney" or "WarehouseHandoff")
            || exhibit.CanonicalRecordRelations.Any(value =>
                value.SourceRecordKindCode == "Cargo" || value.TargetRecordKindCode == "Cargo"))
            return "IntegratedExhibitionFoodDeliveryFreightReuseForbidden:" + exhibit.ExhibitStableId;

        var offer = exhibit.WorkflowCheckpoints.SingleOrDefault(value => value.StateMachineCode == "DriverOffer");
        var assignment = exhibit.WorkflowCheckpoints.SingleOrDefault(value => value.StateMachineCode == "DriverAssignment");
        var delivered = exhibit.WorkflowCheckpoints.SingleOrDefault(value =>
            value.StateMachineCode == "FoodDelivery" && value.StateCode == "전달완료");
        var receipt = exhibit.WorkflowCheckpoints.SingleOrDefault(value =>
            value.StateMachineCode == "OrdererReceipt" && value.StateCode == "수령확인");
        if (offer is null || offer.DisclosureScopeCode != 통합전시관DisclosureScopeCodes.DriverCandidateApproximate
            || assignment is null || assignment.DisclosureScopeCode != 통합전시관DisclosureScopeCodes.AssignedDriverAuthorized
            || !assignment.RequiresSeparateConfirmation
            || delivered is null || !delivered.RequiresSeparateConfirmation
            || receipt is null || !receipt.RequiresSeparateConfirmation
            || delivered.CanonicalRecordStableId == receipt.CanonicalRecordStableId)
            return "IntegratedExhibitionFoodDeliveryHandoffBoundaryInvalid:" + exhibit.ExhibitStableId;

        if (!exhibit.BlockedReasonCodes.Contains("ApproximateDropoffBeforeDriverAcceptance", StringComparer.Ordinal)
            || !exhibit.BlockedReasonCodes.Contains("DeliveryCompletionIsNotReceiptConfirmation", StringComparer.Ordinal)
            || exhibit.AllowedInteractionIntentCodes.Contains(통합전시관InteractionIntentCodes.SimulationConfirm, StringComparer.Ordinal)
            || exhibit.AllowedInteractionIntentCodes.Contains(통합전시관InteractionIntentCodes.DomainCommand, StringComparer.Ordinal))
            return "IntegratedExhibitionFoodDeliveryAuthorityBoundaryInvalid:" + exhibit.ExhibitStableId;

        var nextBySource = exhibit.CanonicalRecordRelations.ToDictionary(value => value.SourceStableId, StringComparer.Ordinal);
        var current = exhibit.CanonicalRecordRelations.SingleOrDefault(value => value.SourceRecordKindCode == "FoodOrder");
        if (current is null)
            return "IntegratedExhibitionFoodDeliveryStartMissing:" + exhibit.ExhibitStableId;
        var visited = new HashSet<string>(StringComparer.Ordinal);
        while (visited.Add(current.RelationStableId)
               && nextBySource.TryGetValue(current.TargetStableId, out var next))
            current = next;
        return visited.Count == exhibit.CanonicalRecordRelations.Count
               && current.TargetRecordKindCode == "OrdererReceipt"
            ? null
            : "IntegratedExhibitionFoodDeliveryRelationChainInvalid:" + exhibit.ExhibitStableId;
    }

    private static string? ValidateSeedbedObject(통합전시관SeedbedObjectResponse value)
    {
        if (value is null || Required(
                value.ObjectStableId,
                value.DisplayName,
                value.SemanticRoleCode,
                value.ObjectKindCode,
                value.PlacementProfileKey,
                value.GateStateCode))
            return "IntegratedExhibitionSeedbedObjectRequiredFieldMissing";

        if (MissingOrDuplicate(value.VisualVariantKeys)
            || MissingOrDuplicate(value.PackRoleCodes)
            || MissingOrDuplicate(value.CompatibleZoneRoleCodes)
            || MissingOrDuplicate(value.RequiredSocketCodes)
            || MissingOrDuplicate(value.DataBindingKeys)
            || MissingOrDuplicate(value.PresentationStateCodes))
            return "IntegratedExhibitionSeedbedObjectCollectionInvalid:" + value.ObjectStableId;

        var gateIndex = Array.IndexOf(ObjectGateStates, value.GateStateCode);
        if (gateIndex < 0)
            return "IntegratedExhibitionSeedbedObjectGateInvalid:" + value.ObjectStableId;

        if (value.Evidence is null
            || value.Evidence.Count != ObjectEvidenceKinds.Length
            || value.Evidence.Any(evidence => evidence is null || Required(
                evidence.EvidenceKindCode, evidence.StatusCode, evidence.Reference))
            || ObjectEvidenceKinds.Any(kind => value.Evidence.Count(evidence =>
                evidence.EvidenceKindCode == kind) != 1)
            || value.Evidence.Any(evidence => !EvidenceStatuses.Contains(evidence.StatusCode)))
            return "IntegratedExhibitionSeedbedObjectEvidenceInvalid:" + value.ObjectStableId;

        if (ContainsUnityAssetLocator(value.PlacementProfileKey)
            || value.VisualVariantKeys.Any(ContainsUnityAssetLocator)
            || value.Evidence.Any(evidence => ContainsUnityAssetLocator(evidence.Reference)))
            return "IntegratedExhibitionSeedbedObjectUnityAssetLocatorForbidden:" + value.ObjectStableId;

        for (var index = 0; index <= gateIndex; index++)
        {
            var requiredKind = ObjectEvidenceKinds[index];
            if (value.Evidence.Single(evidence => evidence.EvidenceKindCode == requiredKind).StatusCode
                != 통합전시관EvidenceStatusCodes.Verified)
                return "IntegratedExhibitionSeedbedObjectGateEvidenceRequired:"
                       + value.ObjectStableId + ":" + requiredKind;
        }

        if (gateIndex < ObjectGateStates.Length - 1
            && (value.BlockedReasonCodes is null || value.BlockedReasonCodes.Count == 0))
            return "IntegratedExhibitionSeedbedObjectBlockedReasonRequired:" + value.ObjectStableId;

        return null;
    }

    private static string? ValidateScenePlacement(
        통합전시관ScenePlacementResponse value,
        IReadOnlySet<string> objectIds)
    {
        if (value is null || Required(
                value.PlacementStableId,
                value.SceneStableId,
                value.ZoneStableId,
                value.ObjectStableId,
                value.VisualVariantKey,
                value.PlacementProfileKey,
                value.PlacementProfileRevision,
                value.SceneAnchorKey,
                value.DataBindingKey,
                value.ValidationStatusCode))
            return "IntegratedExhibitionScenePlacementRequiredFieldMissing";
        if (!objectIds.Contains(value.ObjectStableId))
            return "IntegratedExhibitionScenePlacementObjectMissing:"
                   + value.PlacementStableId + ":" + value.ObjectStableId;
        if (ContainsUnityAssetLocator(value.VisualVariantKey)
            || ContainsUnityAssetLocator(value.PlacementProfileKey))
            return "IntegratedExhibitionScenePlacementUnityAssetLocatorForbidden:" + value.PlacementStableId;
        if (!ObjectGateStates.Contains(value.ValidationStatusCode, StringComparer.Ordinal))
            return "IntegratedExhibitionScenePlacementStatusInvalid:" + value.PlacementStableId;
        if (!HasAllEvidenceAxes(value.Evidence)
            || value.Evidence.Any(evidence => !EvidenceStatuses.Contains(evidence.StatusCode)))
            return "IntegratedExhibitionScenePlacementEvidenceInvalid:" + value.PlacementStableId;
        return null;
    }

    private static bool ContainsUnityAssetLocator(string value)
        => value.Contains("Assets/", StringComparison.OrdinalIgnoreCase)
           || value.Contains(".prefab", StringComparison.OrdinalIgnoreCase)
           || value.Contains(":\\", StringComparison.Ordinal);

    private static bool IsMutationIntent(string intent)
        => intent == 통합전시관InteractionIntentCodes.SimulationConfirm
           || intent == 통합전시관InteractionIntentCodes.DomainCommand;

    private static string EvidenceStatus(통합전시관ExhibitResponse exhibit, string kind)
        => exhibit.Evidence.Single(value => value.EvidenceKindCode == kind).StatusCode;

    private static bool HasAllEvidenceAxes(IReadOnlyList<통합전시관EvidenceResponse> evidence)
        => evidence is not null
           && evidence.Count == RequiredEvidenceKinds.Length
           && evidence.All(value => value is not null
                                    && !Required(value.EvidenceKindCode, value.StatusCode, value.Reference))
           && RequiredEvidenceKinds.All(kind => evidence.Count(value => value.EvidenceKindCode == kind) == 1);

    private static bool MissingOrDuplicate(IReadOnlyList<string> values)
        => values is null || values.Count == 0 || values.Any(string.IsNullOrWhiteSpace) || HasDuplicate(values);

    private static bool HasDuplicate(IEnumerable<string> values)
        => values.GroupBy(value => value, StringComparer.Ordinal).Any(group => group.Count() > 1);

    private static bool Required(params string[] values)
        => values.Any(string.IsNullOrWhiteSpace);

    private static 통합전시관ExhibitResponse Clone(통합전시관ExhibitResponse source)
        => new()
        {
            ExhibitStableId = source.ExhibitStableId.Trim(),
            DisplayName = source.DisplayName.Trim(),
            ExhibitKindCode = source.ExhibitKindCode.Trim(),
            WorkflowKey = source.WorkflowKey.Trim(),
            ProductVersionCode = source.ProductVersionCode.Trim(),
            PerspectiveCode = source.PerspectiveCode.Trim(),
            AuthorizationScopeCode = source.AuthorizationScopeCode.Trim(),
            WorldStableId = source.WorldStableId.Trim(),
            ZoneStableId = source.ZoneStableId.Trim(),
            ObjectStableIds = source.ObjectStableIds.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
            ReferencedSeedbedObjectStableIds = (source.ReferencedSeedbedObjectStableIds ?? [])
                .OrderBy(value => value, StringComparer.Ordinal).ToArray(),
            CanonicalRecordRelations = source.CanonicalRecordRelations
                .OrderBy(value => value.RelationStableId, StringComparer.Ordinal)
                .Select(value => new 통합전시관CanonicalRecordRelationResponse
                {
                    RelationStableId = value.RelationStableId.Trim(),
                    SourceRecordKindCode = value.SourceRecordKindCode.Trim(),
                    SourceStableId = value.SourceStableId.Trim(),
                    SourceRevision = value.SourceRevision.Trim(),
                    RelationCode = value.RelationCode.Trim(),
                    TargetRecordKindCode = value.TargetRecordKindCode.Trim(),
                    TargetStableId = value.TargetStableId.Trim(),
                    TargetRevision = value.TargetRevision.Trim(),
                    ExpectedTargetRevision = value.ExpectedTargetRevision.Trim(),
                    VerificationStatusCode = value.VerificationStatusCode.Trim(),
                }).ToArray(),
            WorkflowCheckpoints = source.WorkflowCheckpoints
                .OrderBy(value => value.Sequence)
                .Select(value => new 통합전시관WorkflowCheckpointResponse
                {
                    CheckpointStableId = value.CheckpointStableId.Trim(),
                    Sequence = value.Sequence,
                    StateMachineCode = value.StateMachineCode.Trim(),
                    StateCode = value.StateCode.Trim(),
                    LineageStableId = value.LineageStableId.Trim(),
                    CanonicalRecordStableId = value.CanonicalRecordStableId.Trim(),
                    Revision = value.Revision.Trim(),
                    AuthorityCode = value.AuthorityCode.Trim(),
                    DisclosureScopeCode = value.DisclosureScopeCode.Trim(),
                    RequiresSeparateConfirmation = value.RequiresSeparateConfirmation,
                    BoundaryCode = value.BoundaryCode.Trim(),
                }).ToArray(),
            SourcePlan = source.SourcePlan
                .OrderBy(value => value.SourceStableId, StringComparer.Ordinal)
                .Select(value => new 통합전시관SourcePlanSegmentResponse
                {
                    SourceKey = value.SourceKey.Trim(),
                    SourceStableId = value.SourceStableId.Trim(),
                    SourceRevision = value.SourceRevision.Trim(),
                    SourceModeCode = value.SourceModeCode.Trim(),
                    ObservedAtUtc = value.ObservedAtUtc,
                }).ToArray(),
            SourceRevision = source.SourceRevision.Trim(),
            ProjectionRevision = source.ProjectionRevision.Trim(),
            ReferenceTimeUtc = source.ReferenceTimeUtc,
            DataStateCode = source.DataStateCode,
            ExperienceModeCode = source.ExperienceModeCode,
            CompletionStateCode = source.CompletionStateCode,
            AllowedInteractionIntentCodes = source.AllowedInteractionIntentCodes.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
            BlockedReasonCodes = (source.BlockedReasonCodes ?? []).OrderBy(value => value, StringComparer.Ordinal).ToArray(),
            VisualKeys = source.VisualKeys.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
            PackRoleCodes = source.PackRoleCodes.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
            Evidence = source.Evidence
                .OrderBy(value => value.EvidenceKindCode, StringComparer.Ordinal)
                .Select(value => new 통합전시관EvidenceResponse
                {
                    EvidenceKindCode = value.EvidenceKindCode,
                    StatusCode = value.StatusCode,
                    Reference = value.Reference.Trim(),
                    VerifiedAtUtc = value.VerifiedAtUtc,
                    Note = value.Note?.Trim() ?? string.Empty,
                }).ToArray(),
        };

    private static 통합전시관SeedbedObjectResponse CloneSeedbedObject(
        통합전시관SeedbedObjectResponse source)
        => new()
        {
            ObjectStableId = source.ObjectStableId.Trim(),
            DisplayName = source.DisplayName.Trim(),
            SemanticRoleCode = source.SemanticRoleCode.Trim(),
            ObjectKindCode = source.ObjectKindCode.Trim(),
            VisualVariantKeys = source.VisualVariantKeys.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
            PackRoleCodes = source.PackRoleCodes.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
            CompatibleZoneRoleCodes = source.CompatibleZoneRoleCodes.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
            PlacementProfileKey = source.PlacementProfileKey.Trim(),
            RequiredSocketCodes = source.RequiredSocketCodes.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
            DataBindingKeys = source.DataBindingKeys.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
            PresentationStateCodes = source.PresentationStateCodes.OrderBy(value => value, StringComparer.Ordinal).ToArray(),
            GateStateCode = source.GateStateCode,
            BlockedReasonCodes = (source.BlockedReasonCodes ?? []).OrderBy(value => value, StringComparer.Ordinal).ToArray(),
            Evidence = source.Evidence.OrderBy(value => value.EvidenceKindCode, StringComparer.Ordinal)
                .Select(CloneEvidence).ToArray(),
        };

    private static 통합전시관ScenePlacementResponse CloneScenePlacement(
        통합전시관ScenePlacementResponse source)
        => new()
        {
            PlacementStableId = source.PlacementStableId.Trim(),
            SceneStableId = source.SceneStableId.Trim(),
            ZoneStableId = source.ZoneStableId.Trim(),
            ObjectStableId = source.ObjectStableId.Trim(),
            VisualVariantKey = source.VisualVariantKey.Trim(),
            PlacementProfileKey = source.PlacementProfileKey.Trim(),
            PlacementProfileRevision = source.PlacementProfileRevision.Trim(),
            SceneAnchorKey = source.SceneAnchorKey.Trim(),
            DataBindingKey = source.DataBindingKey.Trim(),
            ValidationStatusCode = source.ValidationStatusCode,
            Evidence = source.Evidence.OrderBy(value => value.EvidenceKindCode, StringComparer.Ordinal)
                .Select(CloneEvidence).ToArray(),
        };

    private static 통합전시관EvidenceResponse CloneEvidence(통합전시관EvidenceResponse value)
        => new()
        {
            EvidenceKindCode = value.EvidenceKindCode,
            StatusCode = value.StatusCode,
            Reference = value.Reference.Trim(),
            VerifiedAtUtc = value.VerifiedAtUtc,
            Note = value.Note?.Trim() ?? string.Empty,
        };

    private static string ComputeRevision(
        IEnumerable<통합전시관ExhibitResponse> exhibits,
        IEnumerable<통합전시관SeedbedObjectResponse> seedbedObjects,
        IEnumerable<통합전시관ScenePlacementResponse> placements)
    {
        var parts = exhibits.SelectMany(exhibit => new[]
        {
            exhibit.ExhibitStableId,
            exhibit.SourceRevision,
            exhibit.ProjectionRevision,
            exhibit.DataStateCode,
            exhibit.ExperienceModeCode,
            exhibit.CompletionStateCode,
            exhibit.ReferenceTimeUtc?.ToUniversalTime().ToString("O") ?? string.Empty,
            string.Join(",", exhibit.SourcePlan.Select(value => value.SourceStableId + "@" + value.SourceRevision)),
            string.Join(",", exhibit.CanonicalRecordRelations.Select(value => value.RelationStableId + "@" + value.ExpectedTargetRevision + "@" + value.VerificationStatusCode)),
            string.Join(",", exhibit.WorkflowCheckpoints.Select(value => value.Sequence + "@" + value.StateMachineCode + "@" + value.StateCode + "@" + value.Revision + "@" + value.DisclosureScopeCode)),
            string.Join(",", exhibit.Evidence.Select(value => value.EvidenceKindCode + "@" + value.StatusCode)),
            string.Join(",", exhibit.ReferencedSeedbedObjectStableIds),
        }).Concat(seedbedObjects.SelectMany(value => new[]
        {
            value.ObjectStableId,
            value.SemanticRoleCode,
            value.ObjectKindCode,
            value.PlacementProfileKey,
            value.GateStateCode,
            string.Join(",", value.Evidence.Select(evidence => evidence.EvidenceKindCode + "@" + evidence.StatusCode)),
        })).Concat(placements.SelectMany(value => new[]
        {
            value.PlacementStableId,
            value.SceneStableId,
            value.ZoneStableId,
            value.ObjectStableId,
            value.PlacementProfileRevision,
            value.ValidationStatusCode,
        }));
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("|", parts)));
        return "exhibition:" + Convert.ToHexString(bytes).ToLowerInvariant()[..16];
    }
}

public static class 통합전시관FixtureCatalog
{
    public static 통합전시관ProjectionInput Create(DateTimeOffset generatedAtUtc)
    {
        var exhibits = new 통합전시관ExhibitResponse[]
        {
            AssetStudyLab(),
            CargoHubWarehouse(),
            FoodDelivery(),
            PotatoObservation(),
            PotatoLifecycle(),
            OrdererGroupUrbanMarket(),
        };
        exhibits.Single(value => value.ExhibitStableId == "exhibit:farm:potato-lifecycle")
            .ReferencedSeedbedObjectStableIds =
            [
                "seedbed-object:farm.greenhouse.a",
                "seedbed-object:farm.potato-row.a",
                "seedbed-object:farm.potato-plant-visual.a",
                "seedbed-object:farm.irrigation-sprinkler.a",
                "seedbed-object:farm.potato-harvest-box.a",
            ];
        exhibits.Single(value => value.ExhibitStableId == "exhibit:public-data:potato-observation")
            .ReferencedSeedbedObjectStableIds =
            [
                "seedbed-object:farm.potato-row.a",
                "seedbed-object:farm.potato-plant-visual.a",
                "seedbed-object:farm.irrigation-sprinkler.a",
                "seedbed-object:farm.potato-harvest-box.a",
            ];
        exhibits.Single(value => value.ExhibitStableId == "exhibit:logistics:cargo-hub-warehouse")
            .ReferencedSeedbedObjectStableIds =
            [
                "seedbed-object:farm.potato-harvest-box.a",
                "seedbed-object:town.hub-inbound-gate.a",
                "seedbed-object:town.delivery-truck.a",
                "seedbed-object:shared.cargo-pallet.a",
                "seedbed-object:farm.pallet-crate.a",
            ];
        exhibits.Single(value => value.ExhibitStableId == "exhibit:city:food-delivery")
            .ReferencedSeedbedObjectStableIds = ["seedbed-object:shared.food-pickup-handoff-box.a"];
        exhibits.Single(value => value.ExhibitStableId == "exhibit:town-city:orderer-group-urban-market")
            .ReferencedSeedbedObjectStableIds =
            [
                "seedbed-object:town.resident-visual.a",
                "seedbed-object:town.grouping-cart-table.a",
                "seedbed-object:city.urban-market-building.a",
                "seedbed-object:city.operator-inventory-shelf.a",
                "seedbed-object:city.market-operator-visual.a",
            ];

        var seedbedObjects = SeedbedObjects();
        var promotedHarvestBox = seedbedObjects.Single(value =>
            value.ObjectStableId == "seedbed-object:farm.potato-harvest-box.a");
        promotedHarvestBox.GateStateCode = 통합전시관ObjectGateStateCodes.PromotedToScene;
        promotedHarvestBox.BlockedReasonCodes = [];
        promotedHarvestBox.Evidence = ObjectEvidence(true);
        var promotedHubGate = seedbedObjects.Single(value =>
            value.ObjectStableId == "seedbed-object:town.hub-inbound-gate.a");
        promotedHubGate.GateStateCode = 통합전시관ObjectGateStateCodes.PromotedToScene;
        promotedHubGate.BlockedReasonCodes = [];
        promotedHubGate.Evidence = ObjectEvidence(
            true,
            "unity-change:2026-08-12-integrated-hub-scene-placement-obj6");
        var promotedDeliveryTruck = seedbedObjects.Single(value =>
            value.ObjectStableId == "seedbed-object:town.delivery-truck.a");
        promotedDeliveryTruck.GateStateCode = 통합전시관ObjectGateStateCodes.PromotedToScene;
        promotedDeliveryTruck.BlockedReasonCodes = [];
        promotedDeliveryTruck.Evidence = ObjectEvidence(
            true,
            "unity-change:2026-08-12-integrated-delivery-truck-scene-placement-obj6c",
            "unity-change:2026-08-12-integrated-logistics-object-seedbed-obj6b");
        var promotedCargoPallet = seedbedObjects.Single(value =>
            value.ObjectStableId == "seedbed-object:shared.cargo-pallet.a");
        promotedCargoPallet.GateStateCode = 통합전시관ObjectGateStateCodes.PromotedToScene;
        promotedCargoPallet.BlockedReasonCodes = [];
        promotedCargoPallet.Evidence = ObjectEvidence(
            true,
            "unity-change:2026-08-12-integrated-cargo-pallet-scene-placement-obj6d1",
            "unity-change:2026-08-12-integrated-logistics-object-seedbed-obj6b");
        var promotedFarmPalletCrate = seedbedObjects.Single(value =>
            value.ObjectStableId == "seedbed-object:farm.pallet-crate.a");
        promotedFarmPalletCrate.GateStateCode = 통합전시관ObjectGateStateCodes.PromotedToScene;
        promotedFarmPalletCrate.BlockedReasonCodes = [];
        promotedFarmPalletCrate.Evidence = ObjectEvidence(
            true,
            "unity-change:2026-08-12-integrated-farm-pallet-crate-scene-placement-obj6d2",
            "unity-change:2026-08-12-integrated-logistics-object-seedbed-obj6b");
        var promotedUrbanMarketShop = seedbedObjects.Single(value =>
            value.ObjectStableId == "seedbed-object:city.urban-market-building.a");
        promotedUrbanMarketShop.GateStateCode = 통합전시관ObjectGateStateCodes.PromotedToScene;
        promotedUrbanMarketShop.BlockedReasonCodes = [];
        promotedUrbanMarketShop.Evidence = ObjectEvidence(
            true,
            "unity-change:2026-08-12-integrated-urban-market-shop-scene-placement-obj7b",
            "unity-change:2026-08-12-integrated-orderer-market-object-seedbed-obj7a");
        var promotedGroupingCartTable = seedbedObjects.Single(value =>
            value.ObjectStableId == "seedbed-object:town.grouping-cart-table.a");
        promotedGroupingCartTable.GateStateCode = 통합전시관ObjectGateStateCodes.PromotedToScene;
        promotedGroupingCartTable.BlockedReasonCodes = [];
        promotedGroupingCartTable.Evidence = ObjectEvidence(
            true,
            "unity-change:2026-08-12-integrated-grouping-cart-table-scene-placement-obj7c",
            "unity-change:2026-08-12-integrated-orderer-market-object-seedbed-obj7a");

        return new 통합전시관ProjectionInput(exhibits, generatedAtUtc)
        {
            SeedbedObjects = seedbedObjects,
            ScenePlacements =
            [
                PotatoHarvestBoxScenePlacement(),
                HubInboundGateScenePlacement(),
                DeliveryTruckScenePlacement(),
                CargoPalletScenePlacement(),
                FarmPalletCrateScenePlacement(),
                UrbanMarketShopScenePlacement(),
                GroupingCartTableScenePlacement(),
            ],
        };
    }

    private static 통합전시관SeedbedObjectResponse[] SeedbedObjects()
        =>
        [
            SeedbedObject(
                "seedbed-object:farm.potato-harvest-box.a",
                "감자 수확 상자",
                "HarvestCargoVisual",
                "CargoVisual",
                "farm.potato-harvest-box.a",
                통합전시관PackRoleCodes.Farm,
                "FarmProduction",
                "placement-profile:farm.harvest-box.a",
                ["Cargo", "Interaction", "Label", "CameraFocus"],
                ["CanonicalProductHarvestCargo", "HarvestLot" ]),
            SeedbedObject(
                "seedbed-object:town.hub-inbound-gate.a",
                "Hub 입고 Gate",
                "HubInboundHandoff",
                "Facility",
                "town.hub-inbound-gate.a",
                통합전시관PackRoleCodes.Town,
                "LogisticsHub",
                "placement-profile:town.hub-inbound-gate.a",
                ["Entry", "Exit", "Vehicle", "Cargo", "Interaction", "Label", "CameraFocus"],
                ["CargoJourney", "HubReceiving", "WarehouseHandoff"]),
            SeedbedObject(
                "seedbed-object:shared.food-pickup-handoff-box.a",
                "음식 픽업 인계 상자",
                "FoodPickupHandoff",
                "CargoVisual",
                "shared.food-pickup-handoff-box.a",
                통합전시관PackRoleCodes.Shared,
                "FoodPickup",
                "placement-profile:shared.food-pickup-handoff-box.a",
                ["Cargo", "Actor", "Interaction", "Label", "CameraFocus"],
                ["RestaurantPreparation", "DriverAssignment", "FoodPickupHandoff"]),
            SeedbedObject(
                "seedbed-object:farm.greenhouse.a",
                "농장 온실",
                "ProtectedCultivationFacility",
                "Facility",
                "farm.greenhouse.a",
                통합전시관PackRoleCodes.Farm,
                "FarmProduction",
                "placement-profile:farm.greenhouse.a",
                ["Entry", "CropBed", "Irrigation", "Interaction", "Label", "CameraFocus"],
                ["CultivationEnvironment", "FarmEnvironmentalGrowthTurn"]),
            SeedbedObject(
                "seedbed-object:farm.potato-row.a",
                "감자 밭고랑",
                "CultivationSoilVisual",
                "Surface",
                "farm.potato-row.a",
                통합전시관PackRoleCodes.Farm,
                "FarmProduction",
                "placement-profile:farm.potato-row.a",
                ["Crop", "SoilObservation", "Irrigation", "Interaction", "Label", "CameraFocus"],
                ["FarmSoilTile", "SoilObservation"]),
            SeedbedObject(
                "seedbed-object:farm.potato-plant-visual.a",
                "감자 재배체",
                "CropGrowthVisual",
                "CropVisual",
                "farm.potato-plant-visual.a",
                통합전시관PackRoleCodes.Farm,
                "FarmProduction",
                "placement-profile:farm.potato-plant-visual.a",
                ["Soil", "WeatherObservation", "Interaction", "Label", "CameraFocus"],
                ["CanonicalProductCultivation", "FarmEnvironmentalGrowthTurn"]),
            SeedbedObject(
                "seedbed-object:farm.irrigation-sprinkler.a",
                "밭 관수 스프링클러",
                "IrrigationFacilityVisual",
                "Facility",
                "farm.irrigation-sprinkler.a",
                통합전시관PackRoleCodes.Farm,
                "FarmProduction",
                "placement-profile:farm.irrigation-sprinkler.a",
                ["WaterInput", "CropTarget", "WeatherObservation", "Interaction", "Label", "CameraFocus"],
                ["FarmEnvironmentalGrowthTurn", "AgriculturalWeatherObservation"]),
            SeedbedObject(
                "seedbed-object:town.delivery-truck.a",
                "화물 배송 차량",
                "CargoJourneyVehicle",
                "Vehicle",
                "town.delivery-truck.a",
                통합전시관PackRoleCodes.Town,
                "LogisticsRoute",
                "placement-profile:town.delivery-truck.a",
                ["Driver", "Cargo", "RouteEntry", "RouteExit", "Interaction", "Label", "CameraFocus"],
                ["CargoJourney", "TransportTask", "ShipperRequestCandidate"],
                "unity-change:2026-08-12-integrated-logistics-object-seedbed-obj6b"),
            SeedbedObject(
                "seedbed-object:shared.cargo-pallet.a",
                "공용 화물 Pallet",
                "CargoStagingSurface",
                "CargoSupport",
                "shared.cargo-pallet.a",
                통합전시관PackRoleCodes.Shared,
                "WarehouseHandoff",
                "placement-profile:shared.cargo-pallet.a",
                ["Cargo", "Forklift", "Interaction", "Label", "CameraFocus"],
                ["Cargo", "HubReceiving", "WarehouseHandoff"],
                "unity-change:2026-08-12-integrated-logistics-object-seedbed-obj6b"),
            SeedbedObject(
                "seedbed-object:farm.pallet-crate.a",
                "농장 출하 Pallet Crate",
                "HarvestCargoStaging",
                "CargoSupport",
                "farm.pallet-crate.a",
                통합전시관PackRoleCodes.Farm,
                "FarmOutbound",
                "placement-profile:farm.pallet-crate.a",
                ["HarvestCargo", "Vehicle", "HubHandoff", "Interaction", "Label", "CameraFocus"],
                ["CanonicalProductHarvestCargo", "CargoJourney", "HubReceiving"],
                "unity-change:2026-08-12-integrated-logistics-object-seedbed-obj6b"),
            SeedbedObject(
                "seedbed-object:town.resident-visual.a",
                "주민 관점 Visual",
                "OrdererPerspectiveVisual",
                "ActorVisual",
                "town.resident-visual.a",
                통합전시관PackRoleCodes.Town,
                "TownOrdererPerspective",
                "placement-profile:town.resident-visual.a",
                ["Perspective", "AggregateBoundary", "Interaction", "Label", "CameraFocus"],
                ["IndividualIntent", "OwnerAuthorizedPerspective"],
                "unity-change:2026-08-12-integrated-orderer-market-object-seedbed-obj7a"),
            SeedbedObject(
                "seedbed-object:town.grouping-cart-table.a",
                "집단수요 Cart Table",
                "GroupingPreviewSurface",
                "DemandVisual",
                "town.grouping-cart-table.a",
                통합전시관PackRoleCodes.Town,
                "TownDemandAggregation",
                "placement-profile:town.grouping-cart-table.a",
                ["IntentInput", "AggregateOutput", "ConsentBoundary", "Interaction", "Label", "CameraFocus"],
                ["GroupingPreview", "OrdererGroupSummary"],
                "unity-change:2026-08-12-integrated-orderer-market-object-seedbed-obj7a"),
            SeedbedObject(
                "seedbed-object:city.urban-market-building.a",
                "도심마트 Shop",
                "UrbanMarketPublicFront",
                "Facility",
                "city.urban-market-building.a",
                통합전시관PackRoleCodes.City,
                "UrbanMarketPublic",
                "placement-profile:city.urban-market-building.a",
                ["Entry", "PublicProduct", "DemandSignal", "Interaction", "Label", "CameraFocus"],
                ["MartPublicProduct", "MarketDemandSignal"],
                "unity-change:2026-08-12-integrated-orderer-market-object-seedbed-obj7a"),
            SeedbedObject(
                "seedbed-object:city.operator-inventory-shelf.a",
                "운영자 전용 재고 Shelf",
                "MarketOperatorInventoryVisual",
                "InventorySupport",
                "city.operator-inventory-shelf.a",
                통합전시관PackRoleCodes.City,
                "UrbanMarketOperations",
                "placement-profile:city.operator-inventory-shelf.a",
                ["Inventory", "ShelfTask", "Operator", "Interaction", "Label", "CameraFocus"],
                ["MarketInventory", "ShelfTask"],
                "unity-change:2026-08-12-integrated-orderer-market-object-seedbed-obj7a"),
            SeedbedObject(
                "seedbed-object:city.market-operator-visual.a",
                "마트 운영자 Visual",
                "MarketOperatorPerspectiveVisual",
                "ActorVisual",
                "city.market-operator-visual.a",
                통합전시관PackRoleCodes.City,
                "UrbanMarketOperations",
                "placement-profile:city.market-operator-visual.a",
                ["Perspective", "Inventory", "ShelfTask", "Interaction", "Label", "CameraFocus"],
                ["MarketInventory", "ShelfTask", "MarketOperatorPerspective"],
                "unity-change:2026-08-12-integrated-orderer-market-object-seedbed-obj7a"),
        ];

    private static 통합전시관SeedbedObjectResponse SeedbedObject(
        string stableId,
        string displayName,
        string semanticRole,
        string objectKind,
        string visualVariant,
        string packRole,
        string zoneRole,
        string placementProfile,
        IReadOnlyList<string> sockets,
        IReadOnlyList<string> bindings,
        string objectPreviewReference = "unity-change:2026-08-11-integrated-object-seedbed-obj4")
        => new()
        {
            ObjectStableId = stableId,
            DisplayName = displayName,
            SemanticRoleCode = semanticRole,
            ObjectKindCode = objectKind,
            VisualVariantKeys = [visualVariant],
            PackRoleCodes = [packRole],
            CompatibleZoneRoleCodes = [zoneRole],
            PlacementProfileKey = placementProfile,
            RequiredSocketCodes = sockets,
            DataBindingKeys = bindings,
            PresentationStateCodes = ["Normal", "Selected", "Blocked", "Stale"],
            GateStateCode = 통합전시관ObjectGateStateCodes.RuntimeVerified,
            BlockedReasonCodes = ["TargetScenePlacementNotPromoted"],
            Evidence = ObjectEvidence(false, objectPreviewReference: objectPreviewReference),
        };

    private static 통합전시관EvidenceResponse[] ObjectEvidence(
        bool promotedToScene,
        string scenePlacementReference = "unity-change:2026-08-12-integrated-object-scene-placement-obj5",
        string objectPreviewReference = "unity-change:2026-08-11-integrated-object-seedbed-obj4")
        =>
        [
            ObjectEvidence(통합전시관ObjectEvidenceKindCodes.SourceIndex, 통합전시관EvidenceStatusCodes.Verified),
            ObjectEvidence(통합전시관ObjectEvidenceKindCodes.MeaningReview, 통합전시관EvidenceStatusCodes.Verified),
            ObjectEvidence(통합전시관ObjectEvidenceKindCodes.VisualResolution, 통합전시관EvidenceStatusCodes.Verified),
            ObjectEvidence(통합전시관ObjectEvidenceKindCodes.PlacementValidation, 통합전시관EvidenceStatusCodes.Verified),
            ObjectEvidence(통합전시관ObjectEvidenceKindCodes.BindingValidation, 통합전시관EvidenceStatusCodes.Verified),
            ObjectEvidence(
                통합전시관ObjectEvidenceKindCodes.ObjectPreview,
                통합전시관EvidenceStatusCodes.Verified,
                objectPreviewReference),
            ObjectEvidence(
                통합전시관ObjectEvidenceKindCodes.ScenePlacement,
                promotedToScene
                    ? 통합전시관EvidenceStatusCodes.Verified
                    : 통합전시관EvidenceStatusCodes.Unverified,
                promotedToScene
                    ? scenePlacementReference
                    : "unity-scene-placement:not-promoted"),
        ];

    private static 통합전시관ScenePlacementResponse PotatoHarvestBoxScenePlacement()
        => new()
        {
            PlacementStableId = "scene-placement:simulation-world-shell.farm.potato-harvest-box.a",
            SceneStableId = "scene:simulation-world-shell",
            ZoneStableId = "district:farm",
            ObjectStableId = "seedbed-object:farm.potato-harvest-box.a",
            VisualVariantKey = "farm.potato-harvest-box.a",
            PlacementProfileKey = "placement-profile:farm.harvest-box.a",
            PlacementProfileRevision = "r1",
            SceneAnchorKey = "farm.harvest-lot.potato-001",
            DataBindingKey = "HarvestLot:harvest-lot:potato-001",
            ValidationStatusCode = 통합전시관ObjectGateStateCodes.PromotedToScene,
            Evidence = Evidence(
                통합전시관EvidenceStatusCodes.Verified,
                통합전시관EvidenceStatusCodes.Verified,
                통합전시관EvidenceStatusCodes.Verified,
                통합전시관EvidenceStatusCodes.NotApplicable,
                "unity-change:2026-08-12-integrated-object-scene-placement-obj5"),
        };

    private static 통합전시관ScenePlacementResponse HubInboundGateScenePlacement()
        => new()
        {
            PlacementStableId = "scene-placement:simulation-world-shell.logistics.hub-inbound-gate.a",
            SceneStableId = "scene:simulation-world-shell",
            ZoneStableId = "district:logistics",
            ObjectStableId = "seedbed-object:town.hub-inbound-gate.a",
            VisualVariantKey = "town.hub-inbound-gate.a",
            PlacementProfileKey = "placement-profile:town.hub-inbound-gate.a",
            PlacementProfileRevision = "r1",
            SceneAnchorKey = "logistics.hub.inbound-gate",
            DataBindingKey = "HubReceiving:hub-receiving:sim.potato",
            ValidationStatusCode = 통합전시관ObjectGateStateCodes.PromotedToScene,
            Evidence = Evidence(
                통합전시관EvidenceStatusCodes.Verified,
                통합전시관EvidenceStatusCodes.Verified,
                통합전시관EvidenceStatusCodes.Verified,
                통합전시관EvidenceStatusCodes.NotApplicable,
                "unity-change:2026-08-12-integrated-hub-scene-placement-obj6"),
        };

    private static 통합전시관ScenePlacementResponse DeliveryTruckScenePlacement()
        => new()
        {
            PlacementStableId = "scene-placement:simulation-world-shell.logistics.delivery-truck.a",
            SceneStableId = "scene:simulation-world-shell",
            ZoneStableId = "district:logistics",
            ObjectStableId = "seedbed-object:town.delivery-truck.a",
            VisualVariantKey = "town.delivery-truck.a",
            PlacementProfileKey = "placement-profile:town.delivery-truck.a",
            PlacementProfileRevision = "r1",
            SceneAnchorKey = "logistics.cargo-journey.delivery-truck",
            DataBindingKey = "CargoJourney:cargo-journey:sim.potato.farm-hub",
            ValidationStatusCode = 통합전시관ObjectGateStateCodes.PromotedToScene,
            Evidence = Evidence(
                통합전시관EvidenceStatusCodes.Verified,
                통합전시관EvidenceStatusCodes.Verified,
                통합전시관EvidenceStatusCodes.Verified,
                통합전시관EvidenceStatusCodes.NotApplicable,
                "unity-change:2026-08-12-integrated-delivery-truck-scene-placement-obj6c"),
        };

    private static 통합전시관ScenePlacementResponse CargoPalletScenePlacement()
        => new()
        {
            PlacementStableId = "scene-placement:simulation-world-shell.logistics.cargo-pallet.a",
            SceneStableId = "scene:simulation-world-shell",
            ZoneStableId = "district:logistics",
            ObjectStableId = "seedbed-object:shared.cargo-pallet.a",
            VisualVariantKey = "shared.cargo-pallet.a",
            PlacementProfileKey = "placement-profile:shared.cargo-pallet.a",
            PlacementProfileRevision = "r1",
            SceneAnchorKey = "logistics.warehouse-handoff.cargo-pallet",
            DataBindingKey = "WarehouseHandoff:cargo-handoff:sim.potato.20260407.r3.inbound-91",
            ValidationStatusCode = 통합전시관ObjectGateStateCodes.PromotedToScene,
            Evidence = Evidence(
                통합전시관EvidenceStatusCodes.Verified,
                통합전시관EvidenceStatusCodes.Verified,
                통합전시관EvidenceStatusCodes.Verified,
                통합전시관EvidenceStatusCodes.NotApplicable,
                "unity-change:2026-08-12-integrated-cargo-pallet-scene-placement-obj6d1"),
        };

    private static 통합전시관ScenePlacementResponse FarmPalletCrateScenePlacement()
        => new()
        {
            PlacementStableId = "scene-placement:simulation-world-shell.farm.pallet-crate.a",
            SceneStableId = "scene:simulation-world-shell",
            ZoneStableId = "district:farm",
            ObjectStableId = "seedbed-object:farm.pallet-crate.a",
            VisualVariantKey = "farm.pallet-crate.a",
            PlacementProfileKey = "placement-profile:farm.pallet-crate.a",
            PlacementProfileRevision = "r1",
            SceneAnchorKey = "farm.outbound.pallet-crate",
            DataBindingKey = "CanonicalProductHarvestCargo:cargo:sim.potato.20260407.r3",
            ValidationStatusCode = 통합전시관ObjectGateStateCodes.PromotedToScene,
            Evidence = Evidence(
                통합전시관EvidenceStatusCodes.Verified,
                통합전시관EvidenceStatusCodes.Verified,
                통합전시관EvidenceStatusCodes.Verified,
                통합전시관EvidenceStatusCodes.NotApplicable,
                "unity-change:2026-08-12-integrated-farm-pallet-crate-scene-placement-obj6d2"),
        };

    private static 통합전시관ScenePlacementResponse UrbanMarketShopScenePlacement()
        => new()
        {
            PlacementStableId = "scene-placement:simulation-world-shell.market.urban-market-shop.a",
            SceneStableId = "scene:simulation-world-shell",
            ZoneStableId = "district:market",
            ObjectStableId = "seedbed-object:city.urban-market-building.a",
            VisualVariantKey = "city.urban-market-building.a",
            PlacementProfileKey = "placement-profile:city.urban-market-building.a",
            PlacementProfileRevision = "r1",
            SceneAnchorKey = "market.public-products.shop",
            DataBindingKey = "MartPublicProduct:mart-product:sim.potato.public",
            ValidationStatusCode = 통합전시관ObjectGateStateCodes.PromotedToScene,
            Evidence = Evidence(
                통합전시관EvidenceStatusCodes.Verified,
                통합전시관EvidenceStatusCodes.Verified,
                통합전시관EvidenceStatusCodes.Verified,
                통합전시관EvidenceStatusCodes.NotApplicable,
                "unity-change:2026-08-12-integrated-urban-market-shop-scene-placement-obj7b"),
        };

    private static 통합전시관ScenePlacementResponse GroupingCartTableScenePlacement()
        => new()
        {
            PlacementStableId = "scene-placement:simulation-world-shell.town.grouping-cart-table.a",
            SceneStableId = "scene:simulation-world-shell",
            ZoneStableId = "district:town",
            ObjectStableId = "seedbed-object:town.grouping-cart-table.a",
            VisualVariantKey = "town.grouping-cart-table.a",
            PlacementProfileKey = "placement-profile:town.grouping-cart-table.a",
            PlacementProfileRevision = "r1",
            SceneAnchorKey = "town.orderer-group.grouping-cart-table",
            DataBindingKey = "GroupingPreview:grouping-preview:sim.potato.town",
            ValidationStatusCode = 통합전시관ObjectGateStateCodes.PromotedToScene,
            Evidence = Evidence(
                통합전시관EvidenceStatusCodes.Verified,
                통합전시관EvidenceStatusCodes.Verified,
                통합전시관EvidenceStatusCodes.Verified,
                통합전시관EvidenceStatusCodes.NotApplicable,
                "unity-change:2026-08-12-integrated-grouping-cart-table-scene-placement-obj7c"),
        };

    private static 통합전시관EvidenceResponse ObjectEvidence(
        string kind,
        string status,
        string reference = "unity-object-catalog:obj-1")
        => new()
        {
            EvidenceKindCode = kind,
            StatusCode = status,
            Reference = reference,
            Note = "Object Gate 독립 증거 축",
        };

    private static 통합전시관ExhibitResponse AssetStudyLab()
        => Exhibit(
            "exhibit:asset-lab:synty",
            "신티 에셋 연구소",
            "AssetSeedbed",
            "CommunityTrust",
            "0.0",
            "Researcher",
            "Public",
            "zone:exhibition:seedbed",
            "world-object:asset-study-lab",
            "asset-index:synty-farm-town-city",
            "synty-prefab-index:2026-08-10:1535",
            "AssetInventory",
            통합전시관DataStateCodes.Fixture,
            통합전시관ExperienceModeCodes.Research,
            통합전시관CompletionStateCodes.Verified,
            [통합전시관InteractionIntentCodes.Observe, 통합전시관InteractionIntentCodes.ViewLineage, 통합전시관InteractionIntentCodes.Compare],
            [],
            ["exhibition.asset-study.sample"],
            [통합전시관PackRoleCodes.Farm, 통합전시관PackRoleCodes.Town, 통합전시관PackRoleCodes.City],
            "AssetGuid",
            "asset-guid:synty-index",
            "IndexedAs",
            "Exhibit",
            "exhibit:asset-lab:synty",
            "Verified",
            Evidence(
                통합전시관EvidenceStatusCodes.Verified,
                통합전시관EvidenceStatusCodes.Verified,
                통합전시관EvidenceStatusCodes.Verified,
                통합전시관EvidenceStatusCodes.NotApplicable,
                "unity-change:2026-08-10-asset-study-town-city"));

    private static 통합전시관ExhibitResponse PotatoObservation()
        => Exhibit(
            "exhibit:public-data:potato-observation",
            "감자 현실 관측",
            "PublicObservation",
            "CommunityTrust",
            "0.0",
            "PublicObserver",
            "Public",
            "zone:exhibition:public-data-hall",
            "world-object:potato-observation-table",
            "public-data:kamis-potato-observation",
            "kamis-potato-observation:uncollected:r1",
            "PublicObservation",
            통합전시관DataStateCodes.Uncollected,
            통합전시관ExperienceModeCodes.ReadOnly,
            통합전시관CompletionStateCodes.Blocked,
            [통합전시관InteractionIntentCodes.Observe, 통합전시관InteractionIntentCodes.ViewLineage, 통합전시관InteractionIntentCodes.Compare],
            ["ActualObservationNotCollected"],
            ["public-data.observation.potato"],
            [통합전시관PackRoleCodes.Farm, 통합전시관PackRoleCodes.Shared],
            "Product",
            "product:potato",
            "ObservedBy",
            "PublicObservation",
            "public-observation:kamis:potato",
            "Unverified",
            Evidence(
                통합전시관EvidenceStatusCodes.Partial,
                통합전시관EvidenceStatusCodes.Verified,
                통합전시관EvidenceStatusCodes.Verified,
                통합전시관EvidenceStatusCodes.Unverified,
                "change:2026-08-11-unity-asset-soil-seedbed"));

    private static 통합전시관ExhibitResponse PotatoLifecycle()
        => Exhibit(
            "exhibit:farm:potato-lifecycle",
            "감자 재배·수확 체험",
            "SimulationLifecycle",
            "SimulationWorld",
            "3.5-dev",
            "Producer",
            "Personal",
            "zone:exhibition:farm",
            "world-object:potato-field-6x6",
            "simulation:potato-cultivation",
            "potato-cultivation-fixture:r1",
            "SimulationFixture",
            통합전시관DataStateCodes.Fixture,
            통합전시관ExperienceModeCodes.Simulation,
            통합전시관CompletionStateCodes.Verified,
            [
                통합전시관InteractionIntentCodes.Observe,
                통합전시관InteractionIntentCodes.ViewLineage,
                통합전시관InteractionIntentCodes.SimulationPreview,
                통합전시관InteractionIntentCodes.SimulationConfirm,
                통합전시관InteractionIntentCodes.RefreshCanonical,
            ],
            ["OperationalCultivationNotConnected"],
            ["farm.plot.potato-6x6", "farm.harvest-lot.potato"],
            [통합전시관PackRoleCodes.Farm],
            "Product",
            "product:potato",
            "CultivatedAs",
            "CultivationCycle",
            "cultivation:potato:fixture",
            "SimulationLinked",
            Evidence(
                통합전시관EvidenceStatusCodes.Verified,
                통합전시관EvidenceStatusCodes.Verified,
                통합전시관EvidenceStatusCodes.Verified,
                통합전시관EvidenceStatusCodes.Unverified,
                "change:2026-08-10-potato-cultivation-lifecycle"));

    private static 통합전시관ExhibitResponse CargoHubWarehouse()
    {
        const string cargo = "cargo:sim.potato.20260407.r3";
        const string request = "shipper-request-candidate:sim.potato.farm-hub.r1";
        const string journey = "cargo-journey:sim.potato.farm-hub";
        const string receiving = "hub-receiving:sim.potato";
        const string handoff = "cargo-handoff:sim.potato.20260407.r3.inbound-91";
        const string warehouse = "warehouse-zone:7";

        var exhibit = Exhibit(
            "exhibit:logistics:cargo-hub-warehouse",
            "화물·Hub·창고 계보",
            "CargoHubWarehouseLineage",
            "WarehouseFulfillment",
            "3.5-dev",
            "ShipperWarehouse",
            "RoleScopedFixture",
            "zone:exhibition:cargo-hub-warehouse",
            cargo,
            "simulation:potato-cargo-hub-warehouse",
            "potato-cargo-hub-warehouse-fixture:r1",
            "SimulationFixture",
            통합전시관DataStateCodes.Fixture,
            통합전시관ExperienceModeCodes.Simulation,
            통합전시관CompletionStateCodes.Linked,
            [
                통합전시관InteractionIntentCodes.Observe,
                통합전시관InteractionIntentCodes.ViewLineage,
                통합전시관InteractionIntentCodes.SimulationPreview,
                통합전시관InteractionIntentCodes.RefreshCanonical,
            ],
            ["OperationalCargoSnapshotNotLoaded", "WarehouseReceivingCommandNotExposedInExhibition"],
            ["logistics.cargo-truck", "logistics.hub-inbound", "warehouse.inbound-dock"],
            [통합전시관PackRoleCodes.Farm, 통합전시관PackRoleCodes.Town,
                통합전시관PackRoleCodes.City, 통합전시관PackRoleCodes.Shared],
            "ShipperRequestCandidate",
            request,
            "RequestsTransportOf",
            "Cargo",
            cargo,
            "SimulationLinked",
            Evidence(
                통합전시관EvidenceStatusCodes.Verified,
                통합전시관EvidenceStatusCodes.Verified,
                통합전시관EvidenceStatusCodes.Verified,
                통합전시관EvidenceStatusCodes.Partial,
                "unity-change:2026-08-11-integrated-exhibition-exh3"));

        exhibit.ObjectStableIds = [request, cargo, journey, receiving, handoff, warehouse];
        exhibit.SourcePlan =
        [
            Source("simulation:potato-cargo-journey", "potato-cargo-journey-fixture:r1", "SimulationFixture"),
            Source("projection:cargo-warehouse-handoff", "cargo-warehouse-handoff-contract:r1", "OperationalContract"),
            Source("projection:warehouse-world-snapshot", "warehouse-world-snapshot-contract:r1", "AuthorizedOperationalContract"),
        ];
        exhibit.CanonicalRecordRelations =
        [
            Relation("request-cargo", "ShipperRequestCandidate", request, "1", "RequestsTransportOf", "Cargo", cargo, "3"),
            Relation("cargo-journey", "Cargo", cargo, "3", "MovedBy", "CargoJourney", journey, "1"),
            Relation("journey-receiving", "CargoJourney", journey, "4", "ArrivesForInspectionAt", "HubReceiving", receiving, "1"),
            Relation("receiving-handoff", "HubReceiving", receiving, "1", "HandsOffThrough", "WarehouseHandoff", handoff, "2"),
            Relation("handoff-warehouse", "WarehouseHandoff", handoff, "2", "ProjectedInto", "WarehouseWorldSnapshot", warehouse, "warehouse-revision-1"),
        ];
        exhibit.WorkflowCheckpoints =
        [
            Checkpoint(1, "ShipperRequestCandidate", "Candidate", cargo, request, "1", false, "ShipperRequestDoesNotCreateCargo", 통합전시관DisclosureScopeCodes.MarketOperatorAuthorized),
            Checkpoint(2, "CargoJourney", "Loaded", cargo, journey, "1", true, "DispatchConfirmRequired", 통합전시관DisclosureScopeCodes.MarketOperatorAuthorized),
            Checkpoint(3, "CargoJourney", "InTransit", cargo, journey, "2", false, "RouteTickOnly", 통합전시관DisclosureScopeCodes.MarketOperatorAuthorized),
            Checkpoint(4, "CargoJourney", "ArrivedAtHub", cargo, journey, "4", false, "ArrivalIsNotReceiving", 통합전시관DisclosureScopeCodes.MarketOperatorAuthorized),
            Checkpoint(5, "HubReceiving", "Inspection", cargo, receiving, "2", true, "InspectionConfirmRequired", 통합전시관DisclosureScopeCodes.MarketOperatorAuthorized),
            Checkpoint(6, "WarehouseHandoff", "ArrivedAtWarehouse", cargo, handoff, "2", true, "WarehouseArrivalIsNotReceiving", 통합전시관DisclosureScopeCodes.MarketOperatorAuthorized),
            Checkpoint(7, "WarehouseHandoff", "ReceivingCompleted", cargo, warehouse, "warehouse-revision-1", false, "ReceivingCommandRequired", 통합전시관DisclosureScopeCodes.MarketOperatorAuthorized),
        ];
        return exhibit;
    }

    private static 통합전시관ExhibitResponse OrdererGroupUrbanMarket()
    {
        const string lineage = "demand-lineage:sim.potato.town-city";
        const string intent = "individual-intent:sim.potato.owner-private";
        const string preview = "grouping-preview:sim.potato.town";
        const string group = "orderer-group-summary:sim.potato.town";
        const string demand = "market-demand-signal:sim.potato.city";
        const string publicProduct = "mart-product:sim.potato.public";
        const string inventory = "market-inventory:sim.potato.operator";
        const string shelfTask = "market-task:sim.potato.shelf";
        const string kamis = "public-observation:kamis:potato";

        var exhibit = Exhibit(
            "exhibit:town-city:orderer-group-urban-market",
            "주문자 집단·도심마트 경계",
            "OrdererGroupUrbanMarketLineage",
            "GroupPurchaseDemand",
            "3.5-dev",
            "OrdererMarketOperator",
            "PrivacyPartitionedFixture",
            "zone:exhibition:town-city-market",
            lineage,
            "simulation:orderer-group-urban-market",
            "orderer-group-urban-market-fixture:r1",
            "SimulationFixture",
            통합전시관DataStateCodes.Fixture,
            통합전시관ExperienceModeCodes.Simulation,
            통합전시관CompletionStateCodes.Linked,
            [통합전시관InteractionIntentCodes.Observe, 통합전시관InteractionIntentCodes.ViewLineage,
                통합전시관InteractionIntentCodes.Compare, 통합전시관InteractionIntentCodes.SimulationPreview],
            ["ExplicitParticipationConsentNotExecuted", "OperationalMarketSnapshotNotLoaded",
                "SalePriceIsNotKamisObservation", "PublicQuantityIsNotPhysicalInventory"],
            ["town.orderer-group.aggregate", "city.market.public-product", "city.market.operator-inventory"],
            [통합전시관PackRoleCodes.Town, 통합전시관PackRoleCodes.City, 통합전시관PackRoleCodes.Shared],
            "IndividualIntent", intent, "AggregatedPrivatelyAs", "GroupingPreview", preview,
            "SimulationLinked",
            Evidence(통합전시관EvidenceStatusCodes.Verified, 통합전시관EvidenceStatusCodes.Verified,
                통합전시관EvidenceStatusCodes.Verified, 통합전시관EvidenceStatusCodes.Partial,
                "unity-change:2026-08-11-integrated-exhibition-exh4"));

        exhibit.ObjectStableIds = [lineage, preview, group, demand, publicProduct, inventory, shelfTask, kamis];
        exhibit.SourcePlan =
        [
            Source("simulation:individual-intent-grouping", "orderer-grouping-v2-buyer-context", "SimulationFixture"),
            Source("projection:orderer-group-public-summary", "orderer-group-public-contract:r1", "PrivacySafeAggregateContract"),
            Source("projection:urban-market-public-products", "urban-market-public-products.v1", "OrdererPublicContract"),
            Source("projection:urban-market-operations", "urban-market-operations.v1", "AuthorizedOperationalContract"),
            Source("public-data:kamis-potato-observation", "kamis-potato-observation:uncollected:r1", "PublicObservation"),
        ];
        exhibit.CanonicalRecordRelations =
        [
            TownCityRelation("intent-preview", "IndividualIntent", intent, "1", "AggregatedPrivatelyAs", "GroupingPreview", preview, "preview-r1"),
            TownCityRelation("preview-group", "GroupingPreview", preview, "preview-r1", "RequiresConsentBefore", "OrdererGroupSummary", group, "group-r1"),
            TownCityRelation("group-demand", "OrdererGroupSummary", group, "group-r1", "ProjectedAs", "MarketDemandSignal", demand, "demand-r1"),
            TownCityRelation("demand-public-product", "MarketDemandSignal", demand, "demand-r1", "PresentedAlongside", "MartPublicProduct", publicProduct, "public-product-r1"),
            TownCityRelation("public-product-inventory", "MartPublicProduct", publicProduct, "public-product-r1", "DoesNotReveal", "MarketOperationalInventory", inventory, "inventory-r1"),
            TownCityRelation("kamis-public-product", "KamisObservation", kamis, "uncollected-r1", "ComparedWithNotUsedAsSalePrice", "MartPublicProduct", publicProduct, "public-product-r1"),
        ];
        exhibit.WorkflowCheckpoints =
        [
            TownCityCheckpoint(1, "IndividualIntent", "Withdrawable", lineage, intent, "1", true, "ParticipationConsentNotGranted", 통합전시관DisclosureScopeCodes.OwnerPrivate),
            TownCityCheckpoint(2, "GroupingPreview", "Candidate", lineage, preview, "preview-r1", true, "PreviewDoesNotEnroll", 통합전시관DisclosureScopeCodes.PrivacySafeAggregate),
            TownCityCheckpoint(3, "OrdererGroupSummary", "Recruiting", lineage, group, "group-r1", true, "ExplicitParticipationRequired", 통합전시관DisclosureScopeCodes.PrivacySafeAggregate),
            TownCityCheckpoint(4, "MartPublicProduct", "PublishedProjection", lineage, publicProduct, "public-product-r1", false, "SalePriceIsNotKamisObservation", 통합전시관DisclosureScopeCodes.OrdererPublic),
            TownCityCheckpoint(5, "MarketInventory", "AuthorizedProjection", lineage, inventory, "inventory-r1", false, "PublicQuantityIsNotPhysicalInventory", 통합전시관DisclosureScopeCodes.MarketOperatorAuthorized),
            TownCityCheckpoint(6, "ShelfTask", "Candidate", lineage, shelfTask, "task-r1", true, "OperationalCommandNotExposed", 통합전시관DisclosureScopeCodes.MarketOperatorAuthorized),
        ];
        return exhibit;
    }

    private static 통합전시관ExhibitResponse FoodDelivery()
    {
        const string order = "food-order:sim.city-meal.001";
        const string preparation = "restaurant-preparation:sim.city-meal.001";
        const string dispatch = "food-dispatch:sim.city-meal.001";
        const string offer = "food-driver-offer:sim.city-meal.001";
        const string assignment = "food-driver-assignment:sim.city-meal.001";
        const string pickup = "food-pickup-handoff:sim.city-meal.001";
        const string delivery = "food-delivery-handoff:sim.city-meal.001";
        const string receipt = "food-orderer-receipt:sim.city-meal.001";

        var exhibit = Exhibit(
            "exhibit:city:food-delivery", "음식점·기사·주문자 인계", "FoodDeliveryLineage",
            "FoodDelivery", "3.0-dev", "FoodOrderParticipants", "ParticipantPartitionedFixture",
            "zone:exhibition:city-food-delivery", order,
            "simulation:food-delivery", "food-delivery-fixture:r1", "SimulationFixture",
            통합전시관DataStateCodes.Fixture, 통합전시관ExperienceModeCodes.Simulation,
            통합전시관CompletionStateCodes.Linked,
            [통합전시관InteractionIntentCodes.Observe, 통합전시관InteractionIntentCodes.ViewLineage,
                통합전시관InteractionIntentCodes.SimulationPreview],
            ["ApproximateDropoffBeforeDriverAcceptance", "DeliveryCompletionIsNotReceiptConfirmation",
                "OperationalFoodDeliverySnapshotNotLoaded", "FoodDeliveryCommandNotExposedInExhibition"],
            ["city.restaurant.preparation", "city.food-driver.route", "city.orderer.receipt"],
            [통합전시관PackRoleCodes.City, 통합전시관PackRoleCodes.Town, 통합전시관PackRoleCodes.Shared],
            "FoodOrder", order, "PreparedBy", "RestaurantPreparation", preparation,
            "SimulationLinked",
            Evidence(통합전시관EvidenceStatusCodes.Verified, 통합전시관EvidenceStatusCodes.Verified,
                통합전시관EvidenceStatusCodes.Verified, 통합전시관EvidenceStatusCodes.Partial,
                "unity-change:2026-08-11-integrated-exhibition-exh5"));
        exhibit.ObjectStableIds = [order, preparation, dispatch, offer, assignment, pickup, delivery, receipt];
        exhibit.SourcePlan =
        [
            Source("simulation:food-delivery", "simulation-food-delivery-contract:r1", "SimulationFixture"),
            Source("projection:food-order", "food-order-contract:r1", "ParticipantOperationalContract"),
            Source("projection:food-driver-workspace", "food-driver-workspace-contract:r1", "DriverCandidateApproximateContract"),
            Source("projection:orderer-food-delivery-progress", "orderer-food-delivery-progress:r1", "OwnerAuthorizedContract"),
        ];
        exhibit.CanonicalRecordRelations =
        [
            FoodRelation("order-preparation", "FoodOrder", order, "1", "PreparedBy", "RestaurantPreparation", preparation, "1"),
            FoodRelation("preparation-dispatch", "RestaurantPreparation", preparation, "2", "RequestsDeliveryThrough", "FoodDispatchQueue", dispatch, "1"),
            FoodRelation("dispatch-offer", "FoodDispatchQueue", dispatch, "1", "RecommendedAs", "DriverOffer", offer, "1"),
            FoodRelation("offer-assignment", "DriverOffer", offer, "1", "RequiresDriverAcceptanceFor", "DriverAssignment", assignment, "1"),
            FoodRelation("assignment-pickup", "DriverAssignment", assignment, "1", "AuthorizesPickupOf", "FoodPickupHandoff", pickup, "1"),
            FoodRelation("pickup-delivery", "FoodPickupHandoff", pickup, "1", "DeliveredThrough", "FoodDeliveryHandoff", delivery, "1"),
            FoodRelation("delivery-receipt", "FoodDeliveryHandoff", delivery, "1", "RequiresSeparateReceiptConfirmation", "OrdererReceipt", receipt, "1"),
        ];
        exhibit.WorkflowCheckpoints =
        [
            FoodCheckpoint(1, "FoodOrder", "주문대기", order, order, "1", true, "OrderConfirmRequired", 통합전시관DisclosureScopeCodes.OwnerPrivate),
            FoodCheckpoint(2, "RestaurantPreparation", "조리중", order, preparation, "1", true, "RestaurantAcceptanceRequired", 통합전시관DisclosureScopeCodes.RestaurantAuthorized),
            FoodCheckpoint(3, "RestaurantPreparation", "픽업대기", order, preparation, "2", true, "RestaurantPickupReadyRequired", 통합전시관DisclosureScopeCodes.RestaurantAuthorized),
            FoodCheckpoint(4, "DriverOffer", "추천중", order, offer, "1", false, "ApproximateDropoffBeforeDriverAcceptance", 통합전시관DisclosureScopeCodes.DriverCandidateApproximate),
            FoodCheckpoint(5, "DriverAssignment", "기사배정", order, assignment, "1", true, "DriverSelfAcceptanceRequired", 통합전시관DisclosureScopeCodes.AssignedDriverAuthorized),
            FoodCheckpoint(6, "FoodDelivery", "픽업완료", order, pickup, "1", true, "AssignedDriverPickupRequired", 통합전시관DisclosureScopeCodes.AssignedDriverAuthorized),
            FoodCheckpoint(7, "FoodDelivery", "전달완료", order, delivery, "1", true, "DeliveryCompletionIsNotReceiptConfirmation", 통합전시관DisclosureScopeCodes.AssignedDriverAuthorized),
            FoodCheckpoint(8, "OrdererReceipt", "수령확인", order, receipt, "1", true, "OrdererReceiptConfirmationRequired", 통합전시관DisclosureScopeCodes.OwnerPrivate),
        ];
        return exhibit;
    }

    private static 통합전시관ExhibitResponse Exhibit(
        string stableId,
        string displayName,
        string kind,
        string workflow,
        string version,
        string perspective,
        string scope,
        string zone,
        string objectStableId,
        string sourceStableId,
        string sourceRevision,
        string sourceMode,
        string dataState,
        string experienceMode,
        string completionState,
        IReadOnlyList<string> intents,
        IReadOnlyList<string> blockers,
        IReadOnlyList<string> visualKeys,
        IReadOnlyList<string> packRoles,
        string sourceRecordKind,
        string sourceRecordStableId,
        string relationCode,
        string targetRecordKind,
        string targetRecordStableId,
        string relationVerification,
        IReadOnlyList<통합전시관EvidenceResponse> evidence)
        => new()
        {
            ExhibitStableId = stableId,
            DisplayName = displayName,
            ExhibitKindCode = kind,
            WorkflowKey = workflow,
            ProductVersionCode = version,
            PerspectiveCode = perspective,
            AuthorizationScopeCode = scope,
            WorldStableId = "world:integrated-seedbed-exhibition:fixture",
            ZoneStableId = zone,
            ObjectStableIds = [objectStableId],
            CanonicalRecordRelations =
            [
                new 통합전시관CanonicalRecordRelationResponse
                {
                    RelationStableId = "relation:" + stableId,
                    SourceRecordKindCode = sourceRecordKind,
                    SourceStableId = sourceRecordStableId,
                    SourceRevision = sourceRevision,
                    RelationCode = relationCode,
                    TargetRecordKindCode = targetRecordKind,
                    TargetStableId = targetRecordStableId,
                    TargetRevision = "exhibit-contract:r1",
                    ExpectedTargetRevision = "exhibit-contract:r1",
                    VerificationStatusCode = relationVerification,
                },
            ],
            WorkflowCheckpoints = [],
            SourcePlan =
            [
                new 통합전시관SourcePlanSegmentResponse
                {
                    SourceKey = sourceStableId,
                    SourceStableId = sourceStableId,
                    SourceRevision = sourceRevision,
                    SourceModeCode = sourceMode,
                },
            ],
            SourceRevision = sourceRevision,
            ProjectionRevision = "integrated-exhibition-projector:r1",
            DataStateCode = dataState,
            ExperienceModeCode = experienceMode,
            CompletionStateCode = completionState,
            AllowedInteractionIntentCodes = intents,
            BlockedReasonCodes = blockers,
            VisualKeys = visualKeys,
            PackRoleCodes = packRoles,
            Evidence = evidence,
        };

    private static 통합전시관SourcePlanSegmentResponse Source(
        string stableId,
        string revision,
        string mode)
        => new()
        {
            SourceKey = stableId,
            SourceStableId = stableId,
            SourceRevision = revision,
            SourceModeCode = mode,
        };

    private static 통합전시관CanonicalRecordRelationResponse Relation(
        string key,
        string sourceKind,
        string sourceStableId,
        string sourceRevision,
        string relationCode,
        string targetKind,
        string targetStableId,
        string targetRevision)
        => new()
        {
            RelationStableId = "relation:exhibit-logistics:" + key,
            SourceRecordKindCode = sourceKind,
            SourceStableId = sourceStableId,
            SourceRevision = sourceRevision,
            RelationCode = relationCode,
            TargetRecordKindCode = targetKind,
            TargetStableId = targetStableId,
            TargetRevision = targetRevision,
            ExpectedTargetRevision = targetRevision,
            VerificationStatusCode = "SimulationLinked",
        };

    private static 통합전시관CanonicalRecordRelationResponse TownCityRelation(
        string key, string sourceKind, string sourceStableId, string sourceRevision,
        string relationCode, string targetKind, string targetStableId, string targetRevision)
    {
        var value = Relation(key, sourceKind, sourceStableId, sourceRevision,
            relationCode, targetKind, targetStableId, targetRevision);
        value.RelationStableId = "relation:exhibit-town-city:" + key;
        return value;
    }

    private static 통합전시관CanonicalRecordRelationResponse FoodRelation(
        string key, string sourceKind, string sourceStableId, string sourceRevision,
        string relationCode, string targetKind, string targetStableId, string targetRevision)
    {
        var value = Relation(key, sourceKind, sourceStableId, sourceRevision,
            relationCode, targetKind, targetStableId, targetRevision);
        value.RelationStableId = "relation:exhibit-food-delivery:" + key;
        return value;
    }

    private static 통합전시관WorkflowCheckpointResponse Checkpoint(
        int sequence,
        string machine,
        string state,
        string lineage,
        string canonical,
        string revision,
        bool requiresConfirmation,
        string boundary,
        string disclosureScope)
        => new()
        {
            CheckpointStableId = "checkpoint:exhibit-logistics:" + sequence,
            Sequence = sequence,
            StateMachineCode = machine,
            StateCode = state,
            LineageStableId = lineage,
            CanonicalRecordStableId = canonical,
            Revision = revision,
            AuthorityCode = 통합전시관CheckpointAuthorityCodes.SimulationFixture,
            DisclosureScopeCode = disclosureScope,
            RequiresSeparateConfirmation = requiresConfirmation,
            BoundaryCode = boundary,
        };

    private static 통합전시관WorkflowCheckpointResponse TownCityCheckpoint(
        int sequence, string machine, string state, string lineage, string canonical,
        string revision, bool requiresConfirmation, string boundary, string disclosureScope)
    {
        var value = Checkpoint(sequence, machine, state, lineage, canonical, revision,
            requiresConfirmation, boundary, disclosureScope);
        value.CheckpointStableId = "checkpoint:exhibit-town-city:" + sequence;
        return value;
    }

    private static 통합전시관WorkflowCheckpointResponse FoodCheckpoint(
        int sequence, string machine, string state, string lineage, string canonical,
        string revision, bool requiresConfirmation, string boundary, string disclosureScope)
    {
        var value = Checkpoint(sequence, machine, state, lineage, canonical, revision,
            requiresConfirmation, boundary, disclosureScope);
        value.CheckpointStableId = "checkpoint:exhibit-food-delivery:" + sequence;
        return value;
    }

    private static IReadOnlyList<통합전시관EvidenceResponse> Evidence(
        string code,
        string test,
        string runtime,
        string operational,
        string runtimeReference)
        =>
        [
            EvidenceItem(통합전시관EvidenceKindCodes.Code, code, "repo:integrated-exhibition"),
            EvidenceItem(통합전시관EvidenceKindCodes.FocusedTest, test, "validation:focused"),
            EvidenceItem(통합전시관EvidenceKindCodes.Runtime, runtime, runtimeReference),
            EvidenceItem(통합전시관EvidenceKindCodes.Operational, operational, "operation:not-asserted"),
        ];

    private static 통합전시관EvidenceResponse EvidenceItem(
        string kind,
        string status,
        string reference)
        => new()
        {
            EvidenceKindCode = kind,
            StatusCode = status,
            Reference = reference,
            Note = "EXH-0 현황 대장의 독립 증거 축",
        };
}
