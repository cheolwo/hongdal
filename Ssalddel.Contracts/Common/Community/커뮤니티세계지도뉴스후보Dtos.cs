using Ssalddel.Contracts.Common.Content;

namespace Ssalddel.Contracts.Common.Community;

public static class 커뮤니티세계지도뉴스Feed상태Codes
{
    public const string OfficialPublicFeedUnverified = "official-public-feed-unverified";
    public const string LicensedApiOnly = "licensed-api-only";
    public const string Discontinued = "discontinued";
}

public sealed record 커뮤니티세계지도뉴스Feed상태Dto(
    string Code,
    string Label,
    string Notice,
    string EvidenceHref,
    DateTimeOffset VerifiedAtUtc);

public sealed record 커뮤니티세계지도뉴스후보Response(
    string ObservationStableId,
    string PublisherName,
    string CountryCode,
    커뮤니티세계지도뉴스Feed상태Dto PublisherFeedStatus,
    IReadOnlyList<CommunityInformationSourceDto> RelatedOfficialSources,
    string? SelectedSourceKey,
    DateTime GeneratedAtUtc,
    IReadOnlyList<CommunityInformationCandidateDto> Items,
    IReadOnlyList<CommunityInformationSourceFailureDto> Failures,
    bool RequiresExplicitSourceSelection,
    bool CreatesPost,
    string RelationNotice,
    string BoundaryNotice);
