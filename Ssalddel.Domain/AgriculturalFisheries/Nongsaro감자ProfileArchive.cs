namespace Ssalddel.Domain.AgriculturalFisheries;

public sealed class Nongsaro감자ProfileArchive
{
    public long Id { get; set; }
    public string StableId { get; set; } = string.Empty;
    public int Revision { get; set; }
    public string CanonicalProductStableId { get; set; } = string.Empty;
    public string WorkScheduleGroupCode { get; set; } = string.Empty;
    public string WorkScheduleContentNo { get; set; } = string.Empty;
    public string ProductRelationStatusCode { get; set; } = string.Empty;
    public string ReviewStatusCode { get; set; } = string.Empty;
    public bool ApprovedForSimulationContext { get; set; }
    public string ProfileJson { get; set; } = "{}";
    public string SourceSetHashSha256 { get; set; } = string.Empty;
    public string DisasterPreventionHashSha256 { get; set; } = string.Empty;
    public DateTime DisasterPreventionRetrievedAtUtc { get; set; }
    public DateTime RetrievedAtUtc { get; set; }
    public DateTime ArchivedAtUtc { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
}
