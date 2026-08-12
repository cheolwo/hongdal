namespace Ssalddel.Contracts.Common.WorldProjection;

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

public static class 통합전시관PackRoleCodes
{
    public const string Farm = "Farm";
    public const string Town = "Town";
    public const string City = "City";
    public const string Shared = "Shared";
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

public sealed class 통합전시관ManifestResponse
{
    public string StableId { get; set; } = string.Empty;
    public string Revision { get; set; } = string.Empty;
    public DateTimeOffset GeneratedAtUtc { get; set; }
    public bool IsReadOnly { get; set; } = true;
    public IReadOnlyList<통합전시관ExhibitResponse> Exhibits { get; set; } = [];
    public IReadOnlyList<통합전시관ExhibitResponse> Stories { get; set; } = [];
    public IReadOnlyList<통합전시관SeedbedObjectResponse> SeedbedObjects { get; set; } = [];
    public IReadOnlyList<통합전시관ScenePlacementResponse> ScenePlacements { get; set; } = [];
}

public sealed class 통합전시관ExhibitResponse
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
    public IReadOnlyList<string> ObjectStableIds { get; set; } = [];
    public IReadOnlyList<string> ReferencedSeedbedObjectStableIds { get; set; } = [];
    public IReadOnlyList<통합전시관CanonicalRecordRelationResponse> CanonicalRecordRelations { get; set; } = [];
    public IReadOnlyList<통합전시관WorkflowCheckpointResponse> WorkflowCheckpoints { get; set; } = [];
    public IReadOnlyList<통합전시관SourcePlanSegmentResponse> SourcePlan { get; set; } = [];
    public string SourceRevision { get; set; } = string.Empty;
    public string ProjectionRevision { get; set; } = string.Empty;
    public DateTimeOffset? ReferenceTimeUtc { get; set; }
    public string DataStateCode { get; set; } = string.Empty;
    public string ExperienceModeCode { get; set; } = string.Empty;
    public string CompletionStateCode { get; set; } = string.Empty;
    public IReadOnlyList<string> AllowedInteractionIntentCodes { get; set; } = [];
    public IReadOnlyList<string> BlockedReasonCodes { get; set; } = [];
    public IReadOnlyList<string> VisualKeys { get; set; } = [];
    public IReadOnlyList<string> PackRoleCodes { get; set; } = [];
    public IReadOnlyList<통합전시관EvidenceResponse> Evidence { get; set; } = [];
}

public sealed class 통합전시관SeedbedObjectResponse
{
    public string ObjectStableId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string SemanticRoleCode { get; set; } = string.Empty;
    public string ObjectKindCode { get; set; } = string.Empty;
    public IReadOnlyList<string> VisualVariantKeys { get; set; } = [];
    public IReadOnlyList<string> PackRoleCodes { get; set; } = [];
    public IReadOnlyList<string> CompatibleZoneRoleCodes { get; set; } = [];
    public string PlacementProfileKey { get; set; } = string.Empty;
    public IReadOnlyList<string> RequiredSocketCodes { get; set; } = [];
    public IReadOnlyList<string> DataBindingKeys { get; set; } = [];
    public IReadOnlyList<string> PresentationStateCodes { get; set; } = [];
    public string GateStateCode { get; set; } = string.Empty;
    public IReadOnlyList<string> BlockedReasonCodes { get; set; } = [];
    public IReadOnlyList<통합전시관EvidenceResponse> Evidence { get; set; } = [];
}

public sealed class 통합전시관ScenePlacementResponse
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
    public IReadOnlyList<통합전시관EvidenceResponse> Evidence { get; set; } = [];
}

public sealed class 통합전시관CanonicalRecordRelationResponse
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

public sealed class 통합전시관WorkflowCheckpointResponse
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

public sealed class 통합전시관SourcePlanSegmentResponse
{
    public string SourceKey { get; set; } = string.Empty;
    public string SourceStableId { get; set; } = string.Empty;
    public string SourceRevision { get; set; } = string.Empty;
    public string SourceModeCode { get; set; } = string.Empty;
    public DateTimeOffset? ObservedAtUtc { get; set; }
}

public sealed class 통합전시관EvidenceResponse
{
    public string EvidenceKindCode { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public DateTimeOffset? VerifiedAtUtc { get; set; }
    public string Note { get; set; } = string.Empty;
}
