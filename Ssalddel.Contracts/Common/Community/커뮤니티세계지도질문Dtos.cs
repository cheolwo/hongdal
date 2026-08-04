namespace Ssalddel.Contracts.Common.Community;

public sealed class 커뮤니티세계지도질문초안Request
{
    public string DatasetCode { get; set; } = CommunityPageRoutes.WorldMapDayWorkDataset;

    public string? QuestionFocus { get; set; }
}

public sealed class 커뮤니티세계지도EvidenceReferenceDto
{
    public string ObservationStableId { get; set; } = string.Empty;

    public string DatasetCode { get; set; } = string.Empty;

    public string SnapshotRevision { get; set; } = string.Empty;

    public string? SourceVersion { get; set; }

    public string LayerCode { get; set; } = string.Empty;

    public string CountryCode { get; set; } = string.Empty;

    public string CountryName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string SourceName { get; set; } = string.Empty;

    public string? SourceDatasetKey { get; set; }

    public string? SourceHref { get; set; }

    public string DetailHref { get; set; } = string.Empty;

    public string MapHref { get; set; } = string.Empty;

    public DateTimeOffset? EvidenceAsOfUtc { get; set; }

    public DateTimeOffset? SourceUpdatedAtUtc { get; set; }

    public DateTimeOffset? CollectedAtUtc { get; set; }

    public string? UpdateCycle { get; set; }

    public string? LocationPrecisionCode { get; set; }

    public string? BoundaryNotice { get; set; }
}

public sealed class 커뮤니티세계지도질문초안Response
{
    public 커뮤니티세계지도EvidenceReferenceDto Evidence { get; set; } = new();

    public PlatformCommunityPostCreateRequest SuggestedPost { get; set; } = new();

    public bool RequiresUserConfirmation { get; set; } = true;

    public bool CreatesPost { get; set; }

    public bool CreatesProvisionalLedger { get; set; }

    public string BoundaryNotice { get; set; } = string.Empty;
}

public sealed class 커뮤니티세계지도질문게시Request
{
    public string DatasetCode { get; set; } = CommunityPageRoutes.WorldMapDayWorkDataset;

    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public string? OriginalLanguageCode { get; set; }

    public bool IsInterestGatheringEnabled { get; set; } = true;

    public string Nickname { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public bool IsAuthorDisplayCountryPublic { get; set; }

    public string? AuthorDisplayCountryCode { get; set; }

    public string? AuthorDisplayCountryName { get; set; }

    public bool ConfirmSourceReference { get; set; }
}

public sealed class 커뮤니티세계지도질문게시Response
{
    public PlatformCommunityPostResponse Post { get; set; } = new();

    public string PostHref { get; set; } = string.Empty;

    public string OpportunitiesHref { get; set; } = string.Empty;

    public bool ProvisionalLedgerCreated { get; set; }

    public string NextActionNotice { get; set; } = string.Empty;
}
