using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Admin.Content;

namespace Ssalddel.Services.Content;

/// <summary>배포자가 기존 자산 대장에서 검토 후 내보낸 읽기 전용 사본. HTTP 요청으로 등록하지 않는다.</summary>
public sealed class 개체시각자산Options
{
    public const string Section = "EntityVisualBindings";
    public bool Enabled { get; set; }
    public bool ReviewEnabled { get; set; }
    // 서버 배포자가 지정하는 읽기 전용 로컬 경계. HTTP 입력으로 경로 루트를 바꾸지 않는다.
    public string? UnitySourceRoot { get; set; }
    public string? EvidenceRoot { get; set; }
    public List<개체시각등록자산> Entries { get; set; } = [];
}
public sealed record 개체시각등록자산(string Kind, string StateCode, string Purpose, string Representation,
    개체시각후보Dto Candidate, string? RecordStableId = null, bool AllowTypeDefault = false);
public interface I개체시각자산Catalog
{
    string Check(개체시각대상Dto target, 개체시각후보Dto? candidate, bool typeDefault = false);
}
public sealed class 개체시각자산Catalog(IOptionsMonitor<개체시각자산Options> options) : I개체시각자산Catalog
{
    public string Check(개체시각대상Dto target, 개체시각후보Dto? candidate, bool typeDefault = false)
    {
        if (candidate is null) return "CandidateMissing";
        if (string.IsNullOrWhiteSpace(candidate.VisualKey) || string.IsNullOrWhiteSpace(candidate.CatalogRevision) ||
            !Fingerprint(candidate.CatalogFingerprint) || !Fingerprint(candidate.AssetFingerprint) ||
            !Fingerprint(candidate.EvidenceFingerprint) || !EvidenceReference(candidate.EvidenceRef))
            return "CandidateEvidenceMissing";
        var entries = options.CurrentValue.Entries.Where(x => x is not null && x.Candidate is not null &&
            x.Kind == target.Kind && x.StateCode == target.StateCode && x.Purpose == target.Purpose &&
            x.Representation == target.Representation && x.Candidate.VisualKey == candidate.VisualKey).ToArray();
        if (entries.Length == 0) return "UnregisteredCandidate";
        if (entries.Length != 1) return "CatalogConflict";
        if (entries[0].Candidate != candidate) return "StaleCandidate";
        if (typeDefault && (!entries[0].AllowTypeDefault || entries[0].RecordStableId is not null)) return "TypeDefaultNotApproved";
        if (entries[0].RecordStableId is not null && entries[0].RecordStableId != target.StableId) return "RecordCandidateMismatch";
        return candidate.Fitness == "ApprovedForContext" ? "Valid" : "FitnessNotApproved";
    }
    private static bool Fingerprint(string? value) => value is { Length: 64 } && value.All(Uri.IsHexDigit);
    public static bool EvidenceReference(string? value) => !string.IsNullOrWhiteSpace(value) &&
        value.StartsWith("docs/", StringComparison.Ordinal) && !value.Contains('\\') && !value.Contains(':') &&
        !value.Split('/').Any(x => x is ".." or "." or "");
}
