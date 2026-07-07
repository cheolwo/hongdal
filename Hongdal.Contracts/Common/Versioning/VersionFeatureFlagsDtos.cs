namespace Hongdal.Contracts.Common.Versioning;

public sealed class VersionFeatureFlagsResponse
{
    public IReadOnlyDictionary<string, bool> Flags { get; init; } = new Dictionary<string, bool>();

    public IReadOnlyList<WorkflowFlagStateDto> Workflows { get; init; } = [];

    public IReadOnlyList<WorkflowRelationDto> WorkflowRelations { get; init; } = [];
}

public sealed class WorkflowFlagStateDto
{
    public string WorkflowCode { get; init; } = string.Empty;

    public string WorkflowName { get; init; } = string.Empty;

    public string FlagKey { get; init; } = string.Empty;

    public bool IsEnabled { get; init; }

    public string BoundarySummary { get; init; } = string.Empty;

    public IReadOnlyList<WorkflowParticipantDto> Participants { get; init; } = [];

    public IReadOnlyList<WorkflowScreenDto> Screens { get; init; } = [];

    public IReadOnlyList<WorkflowUseCaseDto> UseCases { get; init; } = [];
}

public sealed class WorkflowParticipantDto
{
    public string ActorCode { get; init; } = string.Empty;

    public string ActorName { get; init; } = string.Empty;

    public bool IsPrimary { get; init; }

    public string Responsibility { get; init; } = string.Empty;
}

public sealed class WorkflowScreenDto
{
    public string ActorCode { get; init; } = string.Empty;

    public string AppCode { get; init; } = string.Empty;

    public string AppName { get; init; } = string.Empty;

    public string ScreenName { get; init; } = string.Empty;

    public string Route { get; init; } = string.Empty;

    public string Purpose { get; init; } = string.Empty;
}

public sealed class WorkflowUseCaseDto
{
    public string UseCaseCode { get; init; } = string.Empty;

    public string UseCaseName { get; init; } = string.Empty;

    public string Summary { get; init; } = string.Empty;

    public bool IsRequired { get; init; }

    public IReadOnlyList<WorkflowUseCaseActorDto> PrimaryActors { get; init; } = [];

    public IReadOnlyList<WorkflowUseCaseActorDto> SupportingActors { get; init; } = [];
}

public sealed class WorkflowUseCaseActorDto
{
    public string ActorCode { get; init; } = string.Empty;

    public string ActorName { get; init; } = string.Empty;

    public string RoleCode { get; init; } = string.Empty;

    public string RoleName { get; init; } = string.Empty;
}

public sealed class WorkflowRelationDto
{
    public string SourceWorkflowCode { get; init; } = string.Empty;

    public string SourceWorkflowName { get; init; } = string.Empty;

    public string TargetWorkflowCode { get; init; } = string.Empty;

    public string TargetWorkflowName { get; init; } = string.Empty;

    public string RelationKindCode { get; init; } = string.Empty;

    public string RelationKindName { get; init; } = string.Empty;

    public string Summary { get; init; } = string.Empty;
}
