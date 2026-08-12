using System;
using System.Collections.Generic;
using System.Linq;
using Ssalddel.Unity.Data;
using Ssalddel.Unity.InterpretationContracts;

namespace Ssalddel.Unity.Exhibition
{
    public static class 통합전시관DataStateCodes
    {
        public const string Live = "Live";
        public const string Cached = "Cached";
        public const string Fixture = "Fixture";
        public const string Uncollected = "Uncollected";
        public const string Invalid = "Invalid";
        public const string Failed = "Failed";
    }

    public static class 통합전시관ExperienceModeCodes
    {
        public const string Research = "Research";
        public const string ReadOnly = "ReadOnly";
        public const string Simulation = "Simulation";
        public const string OperationalHandoff = "OperationalHandoff";
    }

    public static class 통합전시관CompletionStateCodes
    {
        public const string Candidate = "Candidate";
        public const string Linked = "Linked";
        public const string Verified = "Verified";
        public const string Blocked = "Blocked";
        public const string Promoted = "Promoted";
    }

    public static class 통합전시관ObjectGateStateCodes
    {
        public const string Indexed = "Indexed";
        public const string MeaningMapped = "MeaningMapped";
        public const string VisualResolved = "VisualResolved";
        public const string PlacementValidated = "PlacementValidated";
        public const string BindingValidated = "BindingValidated";
        public const string RuntimeVerified = "RuntimeVerified";
        public const string PromotedToScene = "PromotedToScene";
    }

    public static class 통합전시관ObjectEvidenceKindCodes
    {
        public const string SourceIndex = "SourceIndex";
        public const string MeaningReview = "MeaningReview";
        public const string VisualResolution = "VisualResolution";
        public const string PlacementValidation = "PlacementValidation";
        public const string BindingValidation = "BindingValidation";
        public const string ObjectPreview = "ObjectPreview";
        public const string ScenePlacement = "ScenePlacement";
    }

    public static class 통합전시관InteractionIntentCodes
    {
        public const string Observe = "Observe";
        public const string ViewLineage = "ViewLineage";
        public const string Compare = "Compare";
        public const string SimulationPreview = "SimulationPreview";
        public const string SimulationConfirm = "SimulationConfirm";
        public const string RefreshCanonical = "RefreshCanonical";
        public const string WebHandoff = "WebHandoff";
        public const string DomainCommand = "DomainCommand";
    }

    public static class 통합전시관EvidenceKindCodes
    {
        public const string Code = "Code";
        public const string FocusedTest = "FocusedTest";
        public const string Runtime = "Runtime";
        public const string Operational = "Operational";
    }

    public static class 통합전시관EvidenceStatusCodes
    {
        public const string Verified = "Verified";
        public const string Partial = "Partial";
        public const string Unverified = "Unverified";
        public const string NotApplicable = "NotApplicable";
    }

    public static class 통합전시관CheckpointAuthorityCodes
    {
        public const string SimulationFixture = "SimulationFixture";
        public const string OperationalProjection = "OperationalProjection";
    }

    public static class 통합전시관DisclosureScopeCodes
    {
        public const string OwnerPrivate = "OwnerPrivate";
        public const string PrivacySafeAggregate = "PrivacySafeAggregate";
        public const string OrdererPublic = "OrdererPublic";
        public const string MarketOperatorAuthorized = "MarketOperatorAuthorized";
        public const string RestaurantAuthorized = "RestaurantAuthorized";
        public const string DriverCandidateApproximate = "DriverCandidateApproximate";
        public const string AssignedDriverAuthorized = "AssignedDriverAuthorized";
    }

    public sealed class 통합전시관ApiModel
    {
        public string StableId { get; set; } = string.Empty;
        public string Revision { get; set; } = string.Empty;
        public DateTimeOffset GeneratedAtUtc { get; set; }
        public bool IsReadOnly { get; set; }
        public 통합전시관ExhibitApiModel[] Exhibits { get; set; } = Array.Empty<통합전시관ExhibitApiModel>();
        public 통합전시관ExhibitApiModel[] Stories { get; set; } = Array.Empty<통합전시관ExhibitApiModel>();
        public 통합전시관SeedbedObjectApiModel[] SeedbedObjects { get; set; } = Array.Empty<통합전시관SeedbedObjectApiModel>();
        public 통합전시관ScenePlacementApiModel[] ScenePlacements { get; set; } = Array.Empty<통합전시관ScenePlacementApiModel>();
    }

    public sealed class 통합전시관ExhibitApiModel
    {
        public string ExhibitStableId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string ExhibitKindCode { get; set; } = string.Empty;
        public string WorkflowKey { get; set; } = string.Empty;
        public string ProductVersionCode { get; set; } = string.Empty;
        public string PerspectiveCode { get; set; } = string.Empty;
        public string AuthorizationScopeCode { get; set; } = string.Empty;
        public string WorldStableId { get; set; } = string.Empty;
        public string ZoneStableId { get; set; } = string.Empty;
        public string[] ObjectStableIds { get; set; } = Array.Empty<string>();
        public string[] ReferencedSeedbedObjectStableIds { get; set; } = Array.Empty<string>();
        public 통합전시관CanonicalRecordRelationApiModel[] CanonicalRecordRelations { get; set; } = Array.Empty<통합전시관CanonicalRecordRelationApiModel>();
        public 통합전시관WorkflowCheckpointApiModel[] WorkflowCheckpoints { get; set; } = Array.Empty<통합전시관WorkflowCheckpointApiModel>();
        public 통합전시관SourcePlanSegmentApiModel[] SourcePlan { get; set; } = Array.Empty<통합전시관SourcePlanSegmentApiModel>();
        public string SourceRevision { get; set; } = string.Empty;
        public string ProjectionRevision { get; set; } = string.Empty;
        public DateTimeOffset? ReferenceTimeUtc { get; set; }
        public string DataStateCode { get; set; } = string.Empty;
        public string ExperienceModeCode { get; set; } = string.Empty;
        public string CompletionStateCode { get; set; } = string.Empty;
        public string[] AllowedInteractionIntentCodes { get; set; } = Array.Empty<string>();
        public string[] BlockedReasonCodes { get; set; } = Array.Empty<string>();
        public string[] VisualKeys { get; set; } = Array.Empty<string>();
        public string[] PackRoleCodes { get; set; } = Array.Empty<string>();
        public 통합전시관EvidenceApiModel[] Evidence { get; set; } = Array.Empty<통합전시관EvidenceApiModel>();
    }

    public sealed class 통합전시관SeedbedObjectApiModel
    {
        public string ObjectStableId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string SemanticRoleCode { get; set; } = string.Empty;
        public string ObjectKindCode { get; set; } = string.Empty;
        public string[] VisualVariantKeys { get; set; } = Array.Empty<string>();
        public string[] PackRoleCodes { get; set; } = Array.Empty<string>();
        public string[] CompatibleZoneRoleCodes { get; set; } = Array.Empty<string>();
        public string PlacementProfileKey { get; set; } = string.Empty;
        public string[] RequiredSocketCodes { get; set; } = Array.Empty<string>();
        public string[] DataBindingKeys { get; set; } = Array.Empty<string>();
        public string[] PresentationStateCodes { get; set; } = Array.Empty<string>();
        public string GateStateCode { get; set; } = string.Empty;
        public string[] BlockedReasonCodes { get; set; } = Array.Empty<string>();
        public 통합전시관EvidenceApiModel[] Evidence { get; set; } = Array.Empty<통합전시관EvidenceApiModel>();
    }

    public sealed class 통합전시관ScenePlacementApiModel
    {
        public string PlacementStableId { get; set; } = string.Empty;
        public string SceneStableId { get; set; } = string.Empty;
        public string ZoneStableId { get; set; } = string.Empty;
        public string ObjectStableId { get; set; } = string.Empty;
        public string VisualVariantKey { get; set; } = string.Empty;
        public string PlacementProfileKey { get; set; } = string.Empty;
        public string PlacementProfileRevision { get; set; } = string.Empty;
        public string SceneAnchorKey { get; set; } = string.Empty;
        public string DataBindingKey { get; set; } = string.Empty;
        public string ValidationStatusCode { get; set; } = string.Empty;
        public 통합전시관EvidenceApiModel[] Evidence { get; set; } = Array.Empty<통합전시관EvidenceApiModel>();
    }

    public sealed class 통합전시관CanonicalRecordRelationApiModel
    {
        public string RelationStableId { get; set; } = string.Empty;
        public string SourceRecordKindCode { get; set; } = string.Empty;
        public string SourceStableId { get; set; } = string.Empty;
        public string SourceRevision { get; set; } = string.Empty;
        public string RelationCode { get; set; } = string.Empty;
        public string TargetRecordKindCode { get; set; } = string.Empty;
        public string TargetStableId { get; set; } = string.Empty;
        public string TargetRevision { get; set; } = string.Empty;
        public string ExpectedTargetRevision { get; set; } = string.Empty;
        public string VerificationStatusCode { get; set; } = string.Empty;
    }

    public sealed class 통합전시관WorkflowCheckpointApiModel
    {
        public string CheckpointStableId { get; set; } = string.Empty;
        public int Sequence { get; set; }
        public string StateMachineCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string LineageStableId { get; set; } = string.Empty;
        public string CanonicalRecordStableId { get; set; } = string.Empty;
        public string Revision { get; set; } = string.Empty;
        public string AuthorityCode { get; set; } = string.Empty;
        public string DisclosureScopeCode { get; set; } = string.Empty;
        public bool RequiresSeparateConfirmation { get; set; }
        public string BoundaryCode { get; set; } = string.Empty;
    }

    public sealed class 통합전시관SourcePlanSegmentApiModel
    {
        public string SourceKey { get; set; } = string.Empty;
        public string SourceStableId { get; set; } = string.Empty;
        public string SourceRevision { get; set; } = string.Empty;
        public string SourceModeCode { get; set; } = string.Empty;
        public DateTimeOffset? ObservedAtUtc { get; set; }
    }

    public sealed class 통합전시관EvidenceApiModel
    {
        public string EvidenceKindCode { get; set; } = string.Empty;
        public string StatusCode { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public DateTimeOffset? VerifiedAtUtc { get; set; }
        public string Note { get; set; } = string.Empty;
    }

    public sealed class 통합전시관Snapshot
    {
        public string StableId { get; set; } = string.Empty;
        public string Revision { get; set; } = string.Empty;
        public DateTimeOffset GeneratedAtUtc { get; set; }
        public 통합전시관ExhibitSnapshot[] Exhibits { get; set; } = Array.Empty<통합전시관ExhibitSnapshot>();
        public 통합전시관SeedbedObjectSnapshot[] SeedbedObjects { get; set; } = Array.Empty<통합전시관SeedbedObjectSnapshot>();
        public 통합전시관ScenePlacementSnapshot[] ScenePlacements { get; set; } = Array.Empty<통합전시관ScenePlacementSnapshot>();
    }

    public sealed class 통합전시관ExhibitSnapshot
    {
        public string ExhibitStableId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string ExhibitKindCode { get; set; } = string.Empty;
        public string WorkflowKey { get; set; } = string.Empty;
        public string ProductVersionCode { get; set; } = string.Empty;
        public string PerspectiveCode { get; set; } = string.Empty;
        public string AuthorizationScopeCode { get; set; } = string.Empty;
        public WorldStableId WorldStableId { get; set; }
        public WorldStableId ZoneStableId { get; set; }
        public WorldStableId[] ObjectStableIds { get; set; } = Array.Empty<WorldStableId>();
        public WorldStableId[] ReferencedSeedbedObjectStableIds { get; set; } = Array.Empty<WorldStableId>();
        public 통합전시관CanonicalRecordRelationSnapshot[] CanonicalRecordRelations { get; set; } = Array.Empty<통합전시관CanonicalRecordRelationSnapshot>();
        public 통합전시관WorkflowCheckpointSnapshot[] WorkflowCheckpoints { get; set; } = Array.Empty<통합전시관WorkflowCheckpointSnapshot>();
        public 통합전시관SourcePlanSegmentSnapshot[] SourcePlan { get; set; } = Array.Empty<통합전시관SourcePlanSegmentSnapshot>();
        public string SourceRevision { get; set; } = string.Empty;
        public string ProjectionRevision { get; set; } = string.Empty;
        public DateTimeOffset? ReferenceTimeUtc { get; set; }
        public string DataStateCode { get; set; } = string.Empty;
        public string ExperienceModeCode { get; set; } = string.Empty;
        public string CompletionStateCode { get; set; } = string.Empty;
        public string[] AllowedInteractionIntentCodes { get; set; } = Array.Empty<string>();
        public string[] BlockedReasonCodes { get; set; } = Array.Empty<string>();
        public string[] VisualKeys { get; set; } = Array.Empty<string>();
        public string[] PackRoleCodes { get; set; } = Array.Empty<string>();
        public 통합전시관EvidenceSnapshot[] Evidence { get; set; } = Array.Empty<통합전시관EvidenceSnapshot>();
    }

    public sealed class 통합전시관SeedbedObjectSnapshot
    {
        public WorldStableId ObjectStableId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string SemanticRoleCode { get; set; } = string.Empty;
        public string ObjectKindCode { get; set; } = string.Empty;
        public string[] VisualVariantKeys { get; set; } = Array.Empty<string>();
        public string[] PackRoleCodes { get; set; } = Array.Empty<string>();
        public string[] CompatibleZoneRoleCodes { get; set; } = Array.Empty<string>();
        public string PlacementProfileKey { get; set; } = string.Empty;
        public string[] RequiredSocketCodes { get; set; } = Array.Empty<string>();
        public string[] DataBindingKeys { get; set; } = Array.Empty<string>();
        public string[] PresentationStateCodes { get; set; } = Array.Empty<string>();
        public string GateStateCode { get; set; } = string.Empty;
        public string[] BlockedReasonCodes { get; set; } = Array.Empty<string>();
        public 통합전시관EvidenceSnapshot[] Evidence { get; set; } = Array.Empty<통합전시관EvidenceSnapshot>();
    }

    public sealed class 통합전시관ScenePlacementSnapshot
    {
        public WorldStableId PlacementStableId { get; set; }
        public WorldStableId SceneStableId { get; set; }
        public WorldStableId ZoneStableId { get; set; }
        public WorldStableId ObjectStableId { get; set; }
        public string VisualVariantKey { get; set; } = string.Empty;
        public string PlacementProfileKey { get; set; } = string.Empty;
        public string PlacementProfileRevision { get; set; } = string.Empty;
        public string SceneAnchorKey { get; set; } = string.Empty;
        public string DataBindingKey { get; set; } = string.Empty;
        public string ValidationStatusCode { get; set; } = string.Empty;
        public 통합전시관EvidenceSnapshot[] Evidence { get; set; } = Array.Empty<통합전시관EvidenceSnapshot>();
    }

    public sealed class 통합전시관CanonicalRecordRelationSnapshot
    {
        public string RelationStableId { get; set; } = string.Empty;
        public string SourceRecordKindCode { get; set; } = string.Empty;
        public string SourceStableId { get; set; } = string.Empty;
        public string SourceRevision { get; set; } = string.Empty;
        public string RelationCode { get; set; } = string.Empty;
        public string TargetRecordKindCode { get; set; } = string.Empty;
        public string TargetStableId { get; set; } = string.Empty;
        public string TargetRevision { get; set; } = string.Empty;
        public string ExpectedTargetRevision { get; set; } = string.Empty;
        public string VerificationStatusCode { get; set; } = string.Empty;
    }

    public sealed class 통합전시관WorkflowCheckpointSnapshot
    {
        public string CheckpointStableId { get; set; } = string.Empty;
        public int Sequence { get; set; }
        public string StateMachineCode { get; set; } = string.Empty;
        public string StateCode { get; set; } = string.Empty;
        public string LineageStableId { get; set; } = string.Empty;
        public string CanonicalRecordStableId { get; set; } = string.Empty;
        public string Revision { get; set; } = string.Empty;
        public string AuthorityCode { get; set; } = string.Empty;
        public string DisclosureScopeCode { get; set; } = string.Empty;
        public bool RequiresSeparateConfirmation { get; set; }
        public string BoundaryCode { get; set; } = string.Empty;
    }

    public sealed class 통합전시관SourcePlanSegmentSnapshot
    {
        public SourceStableId SourceStableId { get; set; }
        public string SourceKey { get; set; } = string.Empty;
        public string SourceRevision { get; set; } = string.Empty;
        public string SourceModeCode { get; set; } = string.Empty;
        public DateTimeOffset? ObservedAtUtc { get; set; }
    }

    public sealed class 통합전시관EvidenceSnapshot
    {
        public string EvidenceKindCode { get; set; } = string.Empty;
        public string StatusCode { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public DateTimeOffset? VerifiedAtUtc { get; set; }
        public string Note { get; set; } = string.Empty;
    }

    public sealed class 통합전시관Mapper
    {
        private static readonly string[] RequiredEvidenceKinds =
        {
            통합전시관EvidenceKindCodes.Code,
            통합전시관EvidenceKindCodes.FocusedTest,
            통합전시관EvidenceKindCodes.Runtime,
            통합전시관EvidenceKindCodes.Operational,
        };

        private static readonly HashSet<string> DataStates = new HashSet<string>(StringComparer.Ordinal)
        {
            통합전시관DataStateCodes.Live,
            통합전시관DataStateCodes.Cached,
            통합전시관DataStateCodes.Fixture,
            통합전시관DataStateCodes.Uncollected,
            통합전시관DataStateCodes.Invalid,
            통합전시관DataStateCodes.Failed,
        };

        private static readonly HashSet<string> ExperienceModes = new HashSet<string>(StringComparer.Ordinal)
        {
            통합전시관ExperienceModeCodes.Research,
            통합전시관ExperienceModeCodes.ReadOnly,
            통합전시관ExperienceModeCodes.Simulation,
            통합전시관ExperienceModeCodes.OperationalHandoff,
        };

        private static readonly HashSet<string> CompletionStates = new HashSet<string>(StringComparer.Ordinal)
        {
            통합전시관CompletionStateCodes.Candidate,
            통합전시관CompletionStateCodes.Linked,
            통합전시관CompletionStateCodes.Verified,
            통합전시관CompletionStateCodes.Blocked,
            통합전시관CompletionStateCodes.Promoted,
        };

        private static readonly HashSet<string> EvidenceStatuses = new HashSet<string>(StringComparer.Ordinal)
        {
            통합전시관EvidenceStatusCodes.Verified,
            통합전시관EvidenceStatusCodes.Partial,
            통합전시관EvidenceStatusCodes.Unverified,
            통합전시관EvidenceStatusCodes.NotApplicable,
        };

        private static readonly string[] ObjectEvidenceKinds =
        {
            통합전시관ObjectEvidenceKindCodes.SourceIndex,
            통합전시관ObjectEvidenceKindCodes.MeaningReview,
            통합전시관ObjectEvidenceKindCodes.VisualResolution,
            통합전시관ObjectEvidenceKindCodes.PlacementValidation,
            통합전시관ObjectEvidenceKindCodes.BindingValidation,
            통합전시관ObjectEvidenceKindCodes.ObjectPreview,
            통합전시관ObjectEvidenceKindCodes.ScenePlacement,
        };

        private static readonly string[] ObjectGateStates =
        {
            통합전시관ObjectGateStateCodes.Indexed,
            통합전시관ObjectGateStateCodes.MeaningMapped,
            통합전시관ObjectGateStateCodes.VisualResolved,
            통합전시관ObjectGateStateCodes.PlacementValidated,
            통합전시관ObjectGateStateCodes.BindingValidated,
            통합전시관ObjectGateStateCodes.RuntimeVerified,
            통합전시관ObjectGateStateCodes.PromotedToScene,
        };

        private static readonly HashSet<string> CheckpointAuthorities = new HashSet<string>(StringComparer.Ordinal)
        {
            통합전시관CheckpointAuthorityCodes.SimulationFixture,
            통합전시관CheckpointAuthorityCodes.OperationalProjection,
        };

        private static readonly HashSet<string> DisclosureScopes = new HashSet<string>(StringComparer.Ordinal)
        {
            통합전시관DisclosureScopeCodes.OwnerPrivate,
            통합전시관DisclosureScopeCodes.PrivacySafeAggregate,
            통합전시관DisclosureScopeCodes.OrdererPublic,
            통합전시관DisclosureScopeCodes.MarketOperatorAuthorized,
            통합전시관DisclosureScopeCodes.RestaurantAuthorized,
            통합전시관DisclosureScopeCodes.DriverCandidateApproximate,
            통합전시관DisclosureScopeCodes.AssignedDriverAuthorized,
        };

        public 통합전시관Snapshot Map(통합전시관ApiModel source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            StableDataId.EnsureValid(source.StableId, nameof(source.StableId));
            Require(source.Revision, "IntegratedExhibitionRevisionMissing");
            if (!source.IsReadOnly) throw new InvalidOperationException("IntegratedExhibitionReadOnlyRequired");
            var stories = source.Stories != null && source.Stories.Length > 0
                ? source.Stories
                : source.Exhibits;
            if (stories == null || stories.Length == 0)
                throw new InvalidOperationException("IntegratedExhibitionEmpty");

            if (source.Stories != null && source.Stories.Length > 0
                && source.Exhibits != null && source.Exhibits.Length > 0
                && !source.Stories.Select(value => value.ExhibitStableId).OrderBy(value => value, StringComparer.Ordinal)
                    .SequenceEqual(source.Exhibits.Select(value => value.ExhibitStableId)
                        .OrderBy(value => value, StringComparer.Ordinal), StringComparer.Ordinal))
                throw new InvalidOperationException("IntegratedExhibitionStoryCompatibilityMismatch");

            var duplicate = stories
                .GroupBy(value => value.ExhibitStableId, StringComparer.Ordinal)
                .FirstOrDefault(group => group.Count() > 1);
            if (duplicate != null)
                throw new InvalidOperationException("IntegratedExhibitionDuplicate:" + duplicate.Key);

            var seedbedObjects = MapSeedbedObjects(source.SeedbedObjects ?? Array.Empty<통합전시관SeedbedObjectApiModel>());
            var seedbedObjectIds = new HashSet<string>(
                seedbedObjects.Select(value => value.ObjectStableId.Value), StringComparer.Ordinal);
            foreach (var story in stories)
            {
                var missing = (story.ReferencedSeedbedObjectStableIds ?? Array.Empty<string>())
                    .FirstOrDefault(value => !seedbedObjectIds.Contains(value));
                if (missing != null)
                    throw new InvalidOperationException(
                        "IntegratedExhibitionStoryObjectReferenceMissing:" + story.ExhibitStableId + ":" + missing);
            }
            var placements = MapScenePlacements(
                source.ScenePlacements ?? Array.Empty<통합전시관ScenePlacementApiModel>(), seedbedObjectIds);
            foreach (var promoted in seedbedObjects.Where(value =>
                         value.GateStateCode == 통합전시관ObjectGateStateCodes.PromotedToScene))
                if (!placements.Any(value => value.ObjectStableId.Equals(promoted.ObjectStableId)
                                             && value.ValidationStatusCode == 통합전시관ObjectGateStateCodes.PromotedToScene))
                    throw new InvalidOperationException(
                        "IntegratedExhibitionPromotedObjectPlacementMissing:" + promoted.ObjectStableId.Value);

            return new 통합전시관Snapshot
            {
                StableId = source.StableId,
                Revision = source.Revision,
                GeneratedAtUtc = source.GeneratedAtUtc,
                Exhibits = stories
                    .OrderBy(value => value.ExhibitStableId, StringComparer.Ordinal)
                    .Select(MapExhibit)
                    .ToArray(),
                SeedbedObjects = seedbedObjects,
                ScenePlacements = placements,
            };
        }

        private static 통합전시관ExhibitSnapshot MapExhibit(통합전시관ExhibitApiModel source)
        {
            if (source == null) throw new InvalidOperationException("IntegratedExhibitionEntryMissing");
            StableDataId.EnsureValid(source.ExhibitStableId, nameof(source.ExhibitStableId));
            StableDataId.EnsureValid(source.WorldStableId, nameof(source.WorldStableId));
            StableDataId.EnsureValid(source.ZoneStableId, nameof(source.ZoneStableId));
            Require(source.DisplayName, "IntegratedExhibitionDisplayNameMissing");
            Require(source.ExhibitKindCode, "IntegratedExhibitionKindMissing");
            Require(source.WorkflowKey, "IntegratedExhibitionWorkflowMissing");
            Require(source.ProductVersionCode, "IntegratedExhibitionProductVersionMissing");
            Require(source.PerspectiveCode, "IntegratedExhibitionPerspectiveMissing");
            Require(source.AuthorizationScopeCode, "IntegratedExhibitionScopeMissing");
            Require(source.SourceRevision, "IntegratedExhibitionSourceRevisionMissing");
            Require(source.ProjectionRevision, "IntegratedExhibitionProjectionRevisionMissing");

            if (!DataStates.Contains(source.DataStateCode))
                throw new InvalidOperationException("IntegratedExhibitionDataStateInvalid:" + source.ExhibitStableId);
            if (!ExperienceModes.Contains(source.ExperienceModeCode))
                throw new InvalidOperationException("IntegratedExhibitionExperienceModeInvalid:" + source.ExhibitStableId);
            if (!CompletionStates.Contains(source.CompletionStateCode))
                throw new InvalidOperationException("IntegratedExhibitionCompletionStateInvalid:" + source.ExhibitStableId);

            var objects = StableIds(source.ObjectStableIds, "IntegratedExhibitionObjectIdsInvalid");
            var referencedSeedbedObjects = OptionalStableIds(source.ReferencedSeedbedObjectStableIds,
                "IntegratedExhibitionSeedbedObjectReferencesInvalid");
            var sourcePlan = MapSourcePlan(source.SourcePlan, source.ExhibitStableId);
            var relations = MapRelations(source.CanonicalRecordRelations, source.ExhibitStableId);
            var checkpoints = MapCheckpoints(source.WorkflowCheckpoints, source.ExhibitStableId);
            var evidence = MapEvidence(source.Evidence, source.ExhibitStableId);
            var intents = Values(source.AllowedInteractionIntentCodes, "IntegratedExhibitionIntentsInvalid");
            var visualKeys = Values(source.VisualKeys, "IntegratedExhibitionVisualKeysInvalid");
            var packRoles = Values(source.PackRoleCodes, "IntegratedExhibitionPackRolesInvalid");
            var blockers = OptionalValues(source.BlockedReasonCodes);

            if (intents.Contains("ConfirmExhibit", StringComparer.Ordinal))
                throw new InvalidOperationException("IntegratedExhibitionGenericConfirmForbidden:" + source.ExhibitStableId);
            if ((source.ExperienceModeCode == 통합전시관ExperienceModeCodes.Research
                 || source.ExperienceModeCode == 통합전시관ExperienceModeCodes.ReadOnly)
                && intents.Any(value => value == 통합전시관InteractionIntentCodes.SimulationConfirm
                                        || value == 통합전시관InteractionIntentCodes.DomainCommand))
                throw new InvalidOperationException("IntegratedExhibitionReadOnlyMutationForbidden:" + source.ExhibitStableId);
            if (intents.Contains(통합전시관InteractionIntentCodes.SimulationConfirm, StringComparer.Ordinal)
                && source.ExperienceModeCode != 통합전시관ExperienceModeCodes.Simulation)
                throw new InvalidOperationException("IntegratedExhibitionSimulationConfirmModeRequired:" + source.ExhibitStableId);
            if (intents.Contains(통합전시관InteractionIntentCodes.DomainCommand, StringComparer.Ordinal)
                && !intents.Contains(통합전시관InteractionIntentCodes.RefreshCanonical, StringComparer.Ordinal))
                throw new InvalidOperationException("IntegratedExhibitionCanonicalRefreshRequired:" + source.ExhibitStableId);
            if (source.ExperienceModeCode == 통합전시관ExperienceModeCodes.OperationalHandoff
                && !intents.Contains(통합전시관InteractionIntentCodes.WebHandoff, StringComparer.Ordinal)
                && !intents.Contains(통합전시관InteractionIntentCodes.DomainCommand, StringComparer.Ordinal))
                throw new InvalidOperationException("IntegratedExhibitionOperationalHandoffMissing:" + source.ExhibitStableId);
            if (source.DataStateCode == 통합전시관DataStateCodes.Live
                && sourcePlan.Any(value => value.SourceModeCode.IndexOf("Fixture", StringComparison.OrdinalIgnoreCase) >= 0))
                throw new InvalidOperationException("IntegratedExhibitionLiveFixtureContradiction:" + source.ExhibitStableId);
            if (source.DataStateCode == 통합전시관DataStateCodes.Live
                && EvidenceStatus(evidence, 통합전시관EvidenceKindCodes.Operational)
                    != 통합전시관EvidenceStatusCodes.Verified)
                throw new InvalidOperationException("IntegratedExhibitionLiveOperationalEvidenceRequired:" + source.ExhibitStableId);
            if ((source.DataStateCode == 통합전시관DataStateCodes.Uncollected
                 || source.CompletionStateCode == 통합전시관CompletionStateCodes.Blocked)
                && blockers.Length == 0)
                throw new InvalidOperationException("IntegratedExhibitionBlockedReasonRequired:" + source.ExhibitStableId);
            if (source.CompletionStateCode == 통합전시관CompletionStateCodes.Promoted
                && RequiredEvidenceKinds.Take(3).Any(kind =>
                    EvidenceStatus(evidence, kind) != 통합전시관EvidenceStatusCodes.Verified))
                throw new InvalidOperationException("IntegratedExhibitionPromotionEvidenceRequired:" + source.ExhibitStableId);

            if (source.ExhibitKindCode == "CargoHubWarehouseLineage")
                ValidateCargoHubWarehouseLineage(source, objects, relations, checkpoints);
            if (source.ExhibitKindCode == "OrdererGroupUrbanMarketLineage")
                ValidateOrdererGroupUrbanMarketLineage(source, relations, checkpoints, intents, blockers);
            if (source.ExhibitKindCode == "FoodDeliveryLineage")
                ValidateFoodDeliveryLineage(source, objects, relations, checkpoints, intents, blockers);

            return new 통합전시관ExhibitSnapshot
            {
                ExhibitStableId = source.ExhibitStableId,
                DisplayName = source.DisplayName,
                ExhibitKindCode = source.ExhibitKindCode,
                WorkflowKey = source.WorkflowKey,
                ProductVersionCode = source.ProductVersionCode,
                PerspectiveCode = source.PerspectiveCode,
                AuthorizationScopeCode = source.AuthorizationScopeCode,
                WorldStableId = new WorldStableId(source.WorldStableId),
                ZoneStableId = new WorldStableId(source.ZoneStableId),
                ObjectStableIds = objects,
                ReferencedSeedbedObjectStableIds = referencedSeedbedObjects,
                CanonicalRecordRelations = relations,
                WorkflowCheckpoints = checkpoints,
                SourcePlan = sourcePlan,
                SourceRevision = source.SourceRevision,
                ProjectionRevision = source.ProjectionRevision,
                ReferenceTimeUtc = source.ReferenceTimeUtc,
                DataStateCode = source.DataStateCode,
                ExperienceModeCode = source.ExperienceModeCode,
                CompletionStateCode = source.CompletionStateCode,
                AllowedInteractionIntentCodes = intents,
                BlockedReasonCodes = blockers,
                VisualKeys = visualKeys,
                PackRoleCodes = packRoles,
                Evidence = evidence,
            };
        }

        private static 통합전시관SeedbedObjectSnapshot[] MapSeedbedObjects(
            통합전시관SeedbedObjectApiModel[] values)
        {
            var duplicate = values.GroupBy(value => value.ObjectStableId, StringComparer.Ordinal)
                .FirstOrDefault(group => group.Count() > 1);
            if (duplicate != null)
                throw new InvalidOperationException("IntegratedExhibitionSeedbedObjectDuplicate:" + duplicate.Key);
            return values.OrderBy(value => value.ObjectStableId, StringComparer.Ordinal)
                .Select(MapSeedbedObject).ToArray();
        }

        private static 통합전시관SeedbedObjectSnapshot MapSeedbedObject(
            통합전시관SeedbedObjectApiModel value)
        {
            if (value == null) throw new InvalidOperationException("IntegratedExhibitionSeedbedObjectRequiredFieldMissing");
            StableDataId.EnsureValid(value.ObjectStableId, nameof(value.ObjectStableId));
            Require(value.DisplayName, "IntegratedExhibitionSeedbedObjectRequiredFieldMissing");
            Require(value.SemanticRoleCode, "IntegratedExhibitionSeedbedObjectRequiredFieldMissing");
            Require(value.ObjectKindCode, "IntegratedExhibitionSeedbedObjectRequiredFieldMissing");
            Require(value.PlacementProfileKey, "IntegratedExhibitionSeedbedObjectRequiredFieldMissing");
            var gateIndex = Array.IndexOf(ObjectGateStates, value.GateStateCode);
            if (gateIndex < 0)
                throw new InvalidOperationException("IntegratedExhibitionSeedbedObjectGateInvalid:" + value.ObjectStableId);
            var evidence = MapObjectEvidence(value.Evidence, value.ObjectStableId);
            for (var index = 0; index <= gateIndex; index++)
                if (evidence.Single(item => item.EvidenceKindCode == ObjectEvidenceKinds[index]).StatusCode
                    != 통합전시관EvidenceStatusCodes.Verified)
                    throw new InvalidOperationException("IntegratedExhibitionSeedbedObjectGateEvidenceRequired:"
                        + value.ObjectStableId + ":" + ObjectEvidenceKinds[index]);
            var blockers = OptionalValues(value.BlockedReasonCodes);
            if (gateIndex < ObjectGateStates.Length - 1 && blockers.Length == 0)
                throw new InvalidOperationException("IntegratedExhibitionSeedbedObjectBlockedReasonRequired:" + value.ObjectStableId);
            var visualVariants = Values(value.VisualVariantKeys, "IntegratedExhibitionSeedbedObjectCollectionInvalid");
            if (ContainsUnityAssetLocator(value.PlacementProfileKey)
                || visualVariants.Any(ContainsUnityAssetLocator)
                || evidence.Any(item => ContainsUnityAssetLocator(item.Reference)))
                throw new InvalidOperationException("IntegratedExhibitionSeedbedObjectUnityAssetLocatorForbidden:" + value.ObjectStableId);
            return new 통합전시관SeedbedObjectSnapshot
            {
                ObjectStableId = new WorldStableId(value.ObjectStableId),
                DisplayName = value.DisplayName,
                SemanticRoleCode = value.SemanticRoleCode,
                ObjectKindCode = value.ObjectKindCode,
                VisualVariantKeys = visualVariants,
                PackRoleCodes = Values(value.PackRoleCodes, "IntegratedExhibitionSeedbedObjectCollectionInvalid"),
                CompatibleZoneRoleCodes = Values(value.CompatibleZoneRoleCodes, "IntegratedExhibitionSeedbedObjectCollectionInvalid"),
                PlacementProfileKey = value.PlacementProfileKey,
                RequiredSocketCodes = Values(value.RequiredSocketCodes, "IntegratedExhibitionSeedbedObjectCollectionInvalid"),
                DataBindingKeys = Values(value.DataBindingKeys, "IntegratedExhibitionSeedbedObjectCollectionInvalid"),
                PresentationStateCodes = Values(value.PresentationStateCodes, "IntegratedExhibitionSeedbedObjectCollectionInvalid"),
                GateStateCode = value.GateStateCode,
                BlockedReasonCodes = blockers,
                Evidence = evidence,
            };
        }

        private static 통합전시관ScenePlacementSnapshot[] MapScenePlacements(
            통합전시관ScenePlacementApiModel[] values,
            ISet<string> objectIds)
        {
            var duplicate = values.GroupBy(value => value.PlacementStableId, StringComparer.Ordinal)
                .FirstOrDefault(group => group.Count() > 1);
            if (duplicate != null)
                throw new InvalidOperationException("IntegratedExhibitionScenePlacementDuplicate:" + duplicate.Key);
            return values.OrderBy(value => value.PlacementStableId, StringComparer.Ordinal).Select(value =>
            {
                if (value == null) throw new InvalidOperationException("IntegratedExhibitionScenePlacementRequiredFieldMissing");
                StableDataId.EnsureValid(value.PlacementStableId, nameof(value.PlacementStableId));
                StableDataId.EnsureValid(value.SceneStableId, nameof(value.SceneStableId));
                StableDataId.EnsureValid(value.ZoneStableId, nameof(value.ZoneStableId));
                StableDataId.EnsureValid(value.ObjectStableId, nameof(value.ObjectStableId));
                Require(value.VisualVariantKey, "IntegratedExhibitionScenePlacementRequiredFieldMissing");
                Require(value.PlacementProfileKey, "IntegratedExhibitionScenePlacementRequiredFieldMissing");
                Require(value.PlacementProfileRevision, "IntegratedExhibitionScenePlacementRequiredFieldMissing");
                Require(value.SceneAnchorKey, "IntegratedExhibitionScenePlacementRequiredFieldMissing");
                Require(value.DataBindingKey, "IntegratedExhibitionScenePlacementRequiredFieldMissing");
                if (!ObjectGateStates.Contains(value.ValidationStatusCode, StringComparer.Ordinal))
                    throw new InvalidOperationException("IntegratedExhibitionScenePlacementStatusInvalid:" + value.PlacementStableId);
                if (!objectIds.Contains(value.ObjectStableId))
                    throw new InvalidOperationException("IntegratedExhibitionScenePlacementObjectMissing:"
                        + value.PlacementStableId + ":" + value.ObjectStableId);
                if (ContainsUnityAssetLocator(value.VisualVariantKey) || ContainsUnityAssetLocator(value.PlacementProfileKey))
                    throw new InvalidOperationException("IntegratedExhibitionScenePlacementUnityAssetLocatorForbidden:" + value.PlacementStableId);
                return new 통합전시관ScenePlacementSnapshot
                {
                    PlacementStableId = new WorldStableId(value.PlacementStableId),
                    SceneStableId = new WorldStableId(value.SceneStableId),
                    ZoneStableId = new WorldStableId(value.ZoneStableId),
                    ObjectStableId = new WorldStableId(value.ObjectStableId),
                    VisualVariantKey = value.VisualVariantKey,
                    PlacementProfileKey = value.PlacementProfileKey,
                    PlacementProfileRevision = value.PlacementProfileRevision,
                    SceneAnchorKey = value.SceneAnchorKey,
                    DataBindingKey = value.DataBindingKey,
                    ValidationStatusCode = value.ValidationStatusCode,
                    Evidence = MapEvidence(value.Evidence, value.PlacementStableId),
                };
            }).ToArray();
        }

        private static 통합전시관SourcePlanSegmentSnapshot[] MapSourcePlan(
            통합전시관SourcePlanSegmentApiModel[] values,
            string exhibitStableId)
        {
            if (values == null || values.Length == 0)
                throw new InvalidOperationException("IntegratedExhibitionSourcePlanInvalid:" + exhibitStableId);
            var result = values.Select(value =>
            {
                if (value == null) throw new InvalidOperationException("IntegratedExhibitionSourcePlanInvalid:" + exhibitStableId);
                StableDataId.EnsureValid(value.SourceStableId, nameof(value.SourceStableId));
                Require(value.SourceKey, "IntegratedExhibitionSourceKeyMissing");
                Require(value.SourceRevision, "IntegratedExhibitionSourceRevisionMissing");
                Require(value.SourceModeCode, "IntegratedExhibitionSourceModeMissing");
                return new 통합전시관SourcePlanSegmentSnapshot
                {
                    SourceStableId = new SourceStableId(value.SourceStableId),
                    SourceKey = value.SourceKey,
                    SourceRevision = value.SourceRevision,
                    SourceModeCode = value.SourceModeCode,
                    ObservedAtUtc = value.ObservedAtUtc,
                };
            }).OrderBy(value => value.SourceStableId).ToArray();
            EnsureDistinct(result.Select(value => value.SourceStableId.Value), "IntegratedExhibitionSourcePlanDuplicate");
            return result;
        }

        private static 통합전시관CanonicalRecordRelationSnapshot[] MapRelations(
            통합전시관CanonicalRecordRelationApiModel[] values,
            string exhibitStableId)
        {
            if (values == null || values.Length == 0)
                throw new InvalidOperationException("IntegratedExhibitionRelationInvalid:" + exhibitStableId);
            var result = values.Select(value =>
            {
                if (value == null) throw new InvalidOperationException("IntegratedExhibitionRelationInvalid:" + exhibitStableId);
                StableDataId.EnsureValid(value.RelationStableId, nameof(value.RelationStableId));
                StableDataId.EnsureValid(value.SourceStableId, nameof(value.SourceStableId));
                StableDataId.EnsureValid(value.TargetStableId, nameof(value.TargetStableId));
                Require(value.SourceRecordKindCode, "IntegratedExhibitionSourceRecordKindMissing");
                Require(value.SourceRevision, "IntegratedExhibitionRelationSourceRevisionMissing");
                Require(value.RelationCode, "IntegratedExhibitionRelationCodeMissing");
                Require(value.TargetRecordKindCode, "IntegratedExhibitionTargetRecordKindMissing");
                Require(value.TargetRevision, "IntegratedExhibitionRelationTargetRevisionMissing");
                Require(value.ExpectedTargetRevision, "IntegratedExhibitionExpectedTargetRevisionMissing");
                Require(value.VerificationStatusCode, "IntegratedExhibitionRelationVerificationMissing");
                return new 통합전시관CanonicalRecordRelationSnapshot
                {
                    RelationStableId = value.RelationStableId,
                    SourceRecordKindCode = value.SourceRecordKindCode,
                    SourceStableId = value.SourceStableId,
                    SourceRevision = value.SourceRevision,
                    RelationCode = value.RelationCode,
                    TargetRecordKindCode = value.TargetRecordKindCode,
                    TargetStableId = value.TargetStableId,
                    TargetRevision = value.TargetRevision,
                    ExpectedTargetRevision = value.ExpectedTargetRevision,
                    VerificationStatusCode = value.VerificationStatusCode,
                };
            }).OrderBy(value => value.RelationStableId, StringComparer.Ordinal).ToArray();
            EnsureDistinct(result.Select(value => value.RelationStableId), "IntegratedExhibitionRelationDuplicate");
            return result;
        }

        private static 통합전시관WorkflowCheckpointSnapshot[] MapCheckpoints(
            통합전시관WorkflowCheckpointApiModel[] values,
            string exhibitStableId)
        {
            if (values == null) return Array.Empty<통합전시관WorkflowCheckpointSnapshot>();
            var result = values.Select(value =>
            {
                if (value == null)
                    throw new InvalidOperationException("IntegratedExhibitionCheckpointInvalid:" + exhibitStableId);
                StableDataId.EnsureValid(value.CheckpointStableId, nameof(value.CheckpointStableId));
                StableDataId.EnsureValid(value.LineageStableId, nameof(value.LineageStableId));
                StableDataId.EnsureValid(value.CanonicalRecordStableId, nameof(value.CanonicalRecordStableId));
                if (value.Sequence <= 0)
                    throw new InvalidOperationException("IntegratedExhibitionCheckpointSequenceInvalid:" + exhibitStableId);
                Require(value.StateMachineCode, "IntegratedExhibitionCheckpointStateMachineMissing");
                Require(value.StateCode, "IntegratedExhibitionCheckpointStateMissing");
                Require(value.Revision, "IntegratedExhibitionCheckpointRevisionMissing");
                Require(value.AuthorityCode, "IntegratedExhibitionCheckpointAuthorityMissing");
                Require(value.DisclosureScopeCode, "IntegratedExhibitionCheckpointDisclosureScopeMissing");
                Require(value.BoundaryCode, "IntegratedExhibitionCheckpointBoundaryMissing");
                if (!CheckpointAuthorities.Contains(value.AuthorityCode))
                    throw new InvalidOperationException("IntegratedExhibitionCheckpointAuthorityInvalid:" + exhibitStableId);
                if (!DisclosureScopes.Contains(value.DisclosureScopeCode))
                    throw new InvalidOperationException("IntegratedExhibitionDisclosureScopeInvalid:" + exhibitStableId);
                return new 통합전시관WorkflowCheckpointSnapshot
                {
                    CheckpointStableId = value.CheckpointStableId,
                    Sequence = value.Sequence,
                    StateMachineCode = value.StateMachineCode,
                    StateCode = value.StateCode,
                    LineageStableId = value.LineageStableId,
                    CanonicalRecordStableId = value.CanonicalRecordStableId,
                    Revision = value.Revision,
                    AuthorityCode = value.AuthorityCode,
                    DisclosureScopeCode = value.DisclosureScopeCode,
                    RequiresSeparateConfirmation = value.RequiresSeparateConfirmation,
                    BoundaryCode = value.BoundaryCode,
                };
            }).OrderBy(value => value.Sequence).ToArray();
            EnsureDistinct(result.Select(value => value.CheckpointStableId), "IntegratedExhibitionCheckpointDuplicate");
            EnsureDistinct(result.Select(value => value.Sequence.ToString()), "IntegratedExhibitionCheckpointSequenceDuplicate");
            if (result.Where((value, index) => value.Sequence != index + 1).Any())
                throw new InvalidOperationException("IntegratedExhibitionCheckpointSequenceGap:" + exhibitStableId);
            return result;
        }

        private static void ValidateCargoHubWarehouseLineage(
            통합전시관ExhibitApiModel source,
            WorldStableId[] objects,
            통합전시관CanonicalRecordRelationSnapshot[] relations,
            통합전시관WorkflowCheckpointSnapshot[] checkpoints)
        {
            if (relations.Length < 5 || checkpoints.Length < 7)
                throw new InvalidOperationException("IntegratedExhibitionCargoLineageIncomplete:" + source.ExhibitStableId);
            var lineageIds = checkpoints.Select(value => value.LineageStableId).Distinct(StringComparer.Ordinal).ToArray();
            if (lineageIds.Length != 1 || !objects.Any(value => value.Value == lineageIds[0]))
                throw new InvalidOperationException("IntegratedExhibitionCargoLineageMismatch:" + source.ExhibitStableId);
            if (!checkpoints.Any(value => value.StateMachineCode == "CargoJourney" && value.StateCode == "ArrivedAtHub")
                || !checkpoints.Any(value => value.StateMachineCode == "WarehouseHandoff" && value.StateCode == "ArrivedAtWarehouse")
                || !checkpoints.Any(value => value.StateMachineCode == "WarehouseHandoff" && value.StateCode == "ReceivingCompleted")
                || !checkpoints.Single(value => value.StateMachineCode == "WarehouseHandoff"
                    && value.StateCode == "ArrivedAtWarehouse").RequiresSeparateConfirmation)
                throw new InvalidOperationException("IntegratedExhibitionCargoHandoffStatesInvalid:" + source.ExhibitStableId);

            var nextBySource = relations.ToDictionary(value => value.SourceStableId, StringComparer.Ordinal);
            var current = relations.SingleOrDefault(value => value.SourceRecordKindCode == "ShipperRequestCandidate")
                ?? throw new InvalidOperationException("IntegratedExhibitionCargoLineageStartMissing:" + source.ExhibitStableId);
            var visited = new HashSet<string>(StringComparer.Ordinal);
            while (visited.Add(current.RelationStableId) && nextBySource.TryGetValue(current.TargetStableId, out var next))
                current = next;
            if (visited.Count != relations.Length || current.TargetRecordKindCode != "WarehouseWorldSnapshot")
                throw new InvalidOperationException("IntegratedExhibitionCargoRelationChainInvalid:" + source.ExhibitStableId);
        }

        private static void ValidateOrdererGroupUrbanMarketLineage(
            통합전시관ExhibitApiModel source,
            통합전시관CanonicalRecordRelationSnapshot[] relations,
            통합전시관WorkflowCheckpointSnapshot[] checkpoints,
            string[] intents,
            string[] blockers)
        {
            if (relations.Length < 6 || checkpoints.Length < 6)
                throw new InvalidOperationException("IntegratedExhibitionOrdererMarketLineageIncomplete:" + source.ExhibitStableId);

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
            if (privateIntent == null || !privateIntent.RequiresSeparateConfirmation
                || preview == null || !preview.RequiresSeparateConfirmation
                || publicProduct == null || operatorInventory == null
                || shelfTask == null || !shelfTask.RequiresSeparateConfirmation)
                throw new InvalidOperationException("IntegratedExhibitionOrdererMarketDisclosureBoundaryInvalid:" + source.ExhibitStableId);

            if (publicProduct.CanonicalRecordStableId == operatorInventory.CanonicalRecordStableId
                || intents.Contains(통합전시관InteractionIntentCodes.SimulationConfirm, StringComparer.Ordinal)
                || intents.Contains(통합전시관InteractionIntentCodes.DomainCommand, StringComparer.Ordinal))
                throw new InvalidOperationException("IntegratedExhibitionOrdererMarketAuthorityBoundaryInvalid:" + source.ExhibitStableId);

            if (!relations.Any(value => value.SourceRecordKindCode == "KamisObservation"
                                        && value.RelationCode == "ComparedWithNotUsedAsSalePrice"
                                        && value.TargetRecordKindCode == "MartPublicProduct")
                || !blockers.Contains("SalePriceIsNotKamisObservation", StringComparer.Ordinal)
                || !blockers.Contains("PublicQuantityIsNotPhysicalInventory", StringComparer.Ordinal))
                throw new InvalidOperationException("IntegratedExhibitionOrdererMarketPriceInventoryBoundaryInvalid:" + source.ExhibitStableId);
        }

        private static void ValidateFoodDeliveryLineage(
            통합전시관ExhibitApiModel source,
            WorldStableId[] objects,
            통합전시관CanonicalRecordRelationSnapshot[] relations,
            통합전시관WorkflowCheckpointSnapshot[] checkpoints,
            string[] intents,
            string[] blockers)
        {
            if (relations.Length < 7 || checkpoints.Length < 8)
                throw new InvalidOperationException("IntegratedExhibitionFoodDeliveryLineageIncomplete:" + source.ExhibitStableId);

            var lineageIds = checkpoints.Select(value => value.LineageStableId).Distinct(StringComparer.Ordinal).ToArray();
            if (lineageIds.Length != 1 || !objects.Any(value => value.Value == lineageIds[0]))
                throw new InvalidOperationException("IntegratedExhibitionFoodDeliveryLineageMismatch:" + source.ExhibitStableId);
            if (checkpoints.Any(value => value.StateMachineCode == "CargoJourney"
                                         || value.StateMachineCode == "WarehouseHandoff")
                || relations.Any(value => value.SourceRecordKindCode == "Cargo"
                                          || value.TargetRecordKindCode == "Cargo"))
                throw new InvalidOperationException("IntegratedExhibitionFoodDeliveryFreightReuseForbidden:" + source.ExhibitStableId);

            var offer = checkpoints.SingleOrDefault(value => value.StateMachineCode == "DriverOffer");
            var assignment = checkpoints.SingleOrDefault(value => value.StateMachineCode == "DriverAssignment");
            var delivered = checkpoints.SingleOrDefault(value =>
                value.StateMachineCode == "FoodDelivery" && value.StateCode == "전달완료");
            var receipt = checkpoints.SingleOrDefault(value =>
                value.StateMachineCode == "OrdererReceipt" && value.StateCode == "수령확인");
            if (offer == null
                || offer.DisclosureScopeCode != 통합전시관DisclosureScopeCodes.DriverCandidateApproximate
                || assignment == null
                || assignment.DisclosureScopeCode != 통합전시관DisclosureScopeCodes.AssignedDriverAuthorized
                || !assignment.RequiresSeparateConfirmation
                || delivered == null || !delivered.RequiresSeparateConfirmation
                || receipt == null || !receipt.RequiresSeparateConfirmation
                || delivered.CanonicalRecordStableId == receipt.CanonicalRecordStableId)
                throw new InvalidOperationException("IntegratedExhibitionFoodDeliveryHandoffBoundaryInvalid:" + source.ExhibitStableId);

            if (!blockers.Contains("ApproximateDropoffBeforeDriverAcceptance", StringComparer.Ordinal)
                || !blockers.Contains("DeliveryCompletionIsNotReceiptConfirmation", StringComparer.Ordinal)
                || intents.Contains(통합전시관InteractionIntentCodes.SimulationConfirm, StringComparer.Ordinal)
                || intents.Contains(통합전시관InteractionIntentCodes.DomainCommand, StringComparer.Ordinal))
                throw new InvalidOperationException("IntegratedExhibitionFoodDeliveryAuthorityBoundaryInvalid:" + source.ExhibitStableId);

            var nextBySource = relations.ToDictionary(value => value.SourceStableId, StringComparer.Ordinal);
            var current = relations.SingleOrDefault(value => value.SourceRecordKindCode == "FoodOrder")
                ?? throw new InvalidOperationException("IntegratedExhibitionFoodDeliveryStartMissing:" + source.ExhibitStableId);
            var visited = new HashSet<string>(StringComparer.Ordinal);
            while (visited.Add(current.RelationStableId)
                   && nextBySource.TryGetValue(current.TargetStableId, out var next))
                current = next;
            if (visited.Count != relations.Length || current.TargetRecordKindCode != "OrdererReceipt")
                throw new InvalidOperationException("IntegratedExhibitionFoodDeliveryRelationChainInvalid:" + source.ExhibitStableId);
        }

        private static 통합전시관EvidenceSnapshot[] MapEvidence(
            통합전시관EvidenceApiModel[] values,
            string exhibitStableId)
        {
            if (values == null || values.Length != RequiredEvidenceKinds.Length
                || RequiredEvidenceKinds.Any(kind => values.Count(value => value != null && value.EvidenceKindCode == kind) != 1))
                throw new InvalidOperationException("IntegratedExhibitionEvidenceAxesInvalid:" + exhibitStableId);

            return values.Select(value =>
            {
                Require(value.StatusCode, "IntegratedExhibitionEvidenceStatusMissing");
                Require(value.Reference, "IntegratedExhibitionEvidenceReferenceMissing");
                if (!EvidenceStatuses.Contains(value.StatusCode))
                    throw new InvalidOperationException("IntegratedExhibitionEvidenceStatusInvalid:" + exhibitStableId);
                if (value.Reference.Contains(":\\"))
                    throw new InvalidOperationException("IntegratedExhibitionLocalEvidencePathForbidden:" + exhibitStableId);
                return new 통합전시관EvidenceSnapshot
                {
                    EvidenceKindCode = value.EvidenceKindCode,
                    StatusCode = value.StatusCode,
                    Reference = value.Reference,
                    VerifiedAtUtc = value.VerifiedAtUtc,
                    Note = value.Note ?? string.Empty,
                };
            }).OrderBy(value => value.EvidenceKindCode, StringComparer.Ordinal).ToArray();
        }

        private static 통합전시관EvidenceSnapshot[] MapObjectEvidence(
            통합전시관EvidenceApiModel[] values,
            string objectStableId)
        {
            if (values == null || values.Length != ObjectEvidenceKinds.Length
                || ObjectEvidenceKinds.Any(kind => values.Count(value =>
                    value != null && value.EvidenceKindCode == kind) != 1))
                throw new InvalidOperationException("IntegratedExhibitionSeedbedObjectEvidenceInvalid:" + objectStableId);
            return values.Select(value =>
            {
                Require(value.StatusCode, "IntegratedExhibitionSeedbedObjectEvidenceInvalid:" + objectStableId);
                Require(value.Reference, "IntegratedExhibitionSeedbedObjectEvidenceInvalid:" + objectStableId);
                if (!EvidenceStatuses.Contains(value.StatusCode))
                    throw new InvalidOperationException("IntegratedExhibitionSeedbedObjectEvidenceInvalid:" + objectStableId);
                return new 통합전시관EvidenceSnapshot
                {
                    EvidenceKindCode = value.EvidenceKindCode,
                    StatusCode = value.StatusCode,
                    Reference = value.Reference,
                    VerifiedAtUtc = value.VerifiedAtUtc,
                    Note = value.Note ?? string.Empty,
                };
            }).OrderBy(value => value.EvidenceKindCode, StringComparer.Ordinal).ToArray();
        }

        private static string EvidenceStatus(
            IEnumerable<통합전시관EvidenceSnapshot> evidence,
            string kind)
            => evidence.Single(value => value.EvidenceKindCode == kind).StatusCode;

        private static WorldStableId[] StableIds(string[] values, string error)
        {
            var normalized = Values(values, error);
            foreach (var value in normalized) StableDataId.EnsureValid(value, nameof(values));
            return normalized.Select(value => new WorldStableId(value)).OrderBy(value => value).ToArray();
        }

        private static WorldStableId[] OptionalStableIds(string[]? values, string error)
        {
            var normalized = OptionalValues(values);
            foreach (var value in normalized) StableDataId.EnsureValid(value, nameof(values));
            EnsureDistinct(normalized, error);
            return normalized.Select(value => new WorldStableId(value)).OrderBy(value => value).ToArray();
        }

        private static bool ContainsUnityAssetLocator(string value)
            => value.IndexOf("Assets/", StringComparison.OrdinalIgnoreCase) >= 0
               || value.IndexOf(".prefab", StringComparison.OrdinalIgnoreCase) >= 0
               || value.IndexOf(":\\", StringComparison.Ordinal) >= 0;

        private static string[] Values(string[] values, string error)
        {
            if (values == null || values.Length == 0 || values.Any(string.IsNullOrWhiteSpace))
                throw new InvalidOperationException(error);
            var result = values.Select(value => value.Trim()).OrderBy(value => value, StringComparer.Ordinal).ToArray();
            EnsureDistinct(result, error);
            return result;
        }

        private static string[] OptionalValues(string[]? values)
        {
            var result = (values ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
            EnsureDistinct(result, "IntegratedExhibitionOptionalValuesDuplicate");
            return result;
        }

        private static void EnsureDistinct(IEnumerable<string> values, string error)
        {
            if (values.GroupBy(value => value, StringComparer.Ordinal).Any(group => group.Count() > 1))
                throw new InvalidOperationException(error);
        }

        private static void Require(string value, string error)
        {
            if (string.IsNullOrWhiteSpace(value)) throw new InvalidOperationException(error);
        }
    }
}
