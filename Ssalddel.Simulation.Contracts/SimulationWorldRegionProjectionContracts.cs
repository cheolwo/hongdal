using System;

namespace Ssalddel.Simulation.Contracts
{

public static class SimulationWorldRegionProjectionCodes
{
    public const string LegalRegion = "LegalRegion";
    public const string AdministrativeRegion = "AdministrativeRegion";
    public const string Ready = "Ready";
    public const string WaitingForRegionGeometry = "WaitingForRegionGeometry";
    public const string DatabaseDisabled = "DerivationDatabaseDisabled";
}

public sealed class SimulationWorldRegionProjectionResponse
{
    public string RegionStableId { get; set; } = string.Empty;
    public string RegionKindCode { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? RegionCode { get; set; }
    public string? AreaStableId { get; set; }
    public string ProjectionStatusCode { get; set; } = string.Empty;
    public string BuildStableId { get; set; } = string.Empty;
    public string BuildOutputHashSha256 { get; set; } = string.Empty;
    public string RecipeRevision { get; set; } = string.Empty;
    public string RuleRevision { get; set; } = string.Empty;
    public DateTimeOffset GeneratedAtUtc { get; set; }
    public string[] RelatedRegionStableIds { get; set; } = Array.Empty<string>();
    public string[] TileKeys { get; set; } = Array.Empty<string>();
    public SimulationWorldRegionBuildingCategorySummaryResponse[] BuildingCategories { get; set; }
        = Array.Empty<SimulationWorldRegionBuildingCategorySummaryResponse>();
    public bool PresentationOnly { get; set; } = true;
    public bool IsOperationalState { get; set; }
}

public sealed class SimulationWorldRegionBuildingCategorySummaryResponse
{
    public string CategoryCode { get; set; } = string.Empty;
    public long BuildingCount { get; set; }
    public string EvidenceKindCode { get; set; } = string.Empty;
    public string SourceAggregateRecordStableId { get; set; } = string.Empty;
}
}
