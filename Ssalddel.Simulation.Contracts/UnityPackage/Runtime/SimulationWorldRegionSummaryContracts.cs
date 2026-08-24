using System;

namespace Ssalddel.Simulation.Contracts
{

public sealed class SimulationWorld지역표현요약Response
{
    public string RegionStableId { get; set; } = string.Empty;
    public string? TileKey { get; set; }
    public string LodCode { get; set; } = string.Empty;
    public string ProfileRevision { get; set; } = string.Empty;
    public string ProfileHashSha256 { get; set; } = string.Empty;
    public string SummaryHashSha256 { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public DateTimeOffset GeneratedAtUtc { get; set; }
    public int TotalCandidateCount { get; set; }
    public int SelectedItemCount { get; set; }
    public int TotalRepresentedRecordCount { get; set; }
    public int SelectedRepresentedRecordCount { get; set; }
    public int OmittedRepresentedRecordCount { get; set; }
    public int RequestedVisualSlotCount { get; set; }
    public int AllocatedVisualSlotCount { get; set; }
    public SimulationWorld지역표현요약ItemResponse[] Items { get; set; } =
        Array.Empty<SimulationWorld지역표현요약ItemResponse>();
    public SimulationWorld지역표현요약CategoryReportResponse[] CategoryReports { get; set; } =
        Array.Empty<SimulationWorld지역표현요약CategoryReportResponse>();
    public bool PresentationOnly { get; set; } = true;
    public bool IsOperationalState { get; set; }
}

public sealed class SimulationWorld지역표현요약ItemResponse
{
    public string SummaryItemStableId { get; set; } = string.Empty;
    public string SourceObjectStableId { get; set; } = string.Empty;
    public string CategoryCode { get; set; } = string.Empty;
    public string ObjectTypeCode { get; set; } = string.Empty;
    public string SelectionReasonCode { get; set; } = string.Empty;
    public string EvidenceKindCode { get; set; } = string.Empty;
    public string VisualKey { get; set; } = string.Empty;
    public int RepresentedRecordCount { get; set; }
    public decimal? RepresentedAreaSquareMeters { get; set; }
    public int VisualSlotCount { get; set; }
    public int MinimumVisibleCount { get; set; }
    public bool HasPublicDetail { get; set; }
    public bool PresentationOnly { get; set; } = true;
}

public sealed class SimulationWorld지역표현요약CategoryReportResponse
{
    public string CategoryCode { get; set; } = string.Empty;
    public int CandidateCount { get; set; }
    public int TotalRepresentedRecordCount { get; set; }
    public int SelectedRepresentedRecordCount { get; set; }
    public int OmittedRepresentedRecordCount { get; set; }
    public decimal TotalRepresentedAreaSquareMeters { get; set; }
    public decimal SelectedRepresentedAreaSquareMeters { get; set; }
    public int AllocatedVisualSlotCount { get; set; }
}

public sealed class SimulationWorld공개객체상세Response
{
    public string ObjectStableId { get; set; } = string.Empty;
    public string PublicDisplayName { get; set; } = string.Empty;
    public string CategoryCode { get; set; } = string.Empty;
    public string EvidenceKindCode { get; set; } = string.Empty;
    public string SourceStableId { get; set; } = string.Empty;
    public string SourceRevision { get; set; } = string.Empty;
    public DateTimeOffset? ObservedAtUtc { get; set; }
    public string DisclosureNotice { get; set; } = string.Empty;
    public bool IsOperationalState { get; set; }
}
}
