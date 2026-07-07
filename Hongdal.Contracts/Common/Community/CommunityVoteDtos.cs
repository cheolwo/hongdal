using Hongdal.Contracts.Common.ContractManagement;
using Hongdal.Contracts.Common.Privacy;

namespace Hongdal.Contracts.Common.Community;

public sealed class CommunityVoteCreateRequest
{
    public string AppKey { get; set; } = "platform";

    public string CommunityScope { get; set; } = "platform";

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public IReadOnlyList<string> Options { get; set; } = [];

    public bool AllowMultipleSelection { get; set; }

    public bool ResolutionDocumentEnabled { get; set; }

    public bool SignatureRequired { get; set; }

    public DateTime? ClosesAtUtc { get; set; }

    public string CreatedByDisplayName { get; set; } = string.Empty;
}

public sealed class CommunityVoteCastRequest
{
    [IsmsPProtectedData(
        PersonalDataFieldKey.DisplayName,
        "커뮤니티 투표 참여자 표시",
        ProtectionNote = "투표 결과 공개에는 실명 대신 표시명 또는 해시 기반 참여자 키 사용")]
    public string VoterDisplayName { get; set; } = string.Empty;

    public string VoterKey { get; set; } = string.Empty;

    public IReadOnlyList<string> OptionIds { get; set; } = [];
}

public sealed class CommunityVoteCloseRequest
{
    public string ClosedByDisplayName { get; set; } = string.Empty;
}

public sealed class CommunityVoteResolutionDraftRequest
{
    public string DocumentTitle { get; set; } = string.Empty;

    public string ResolutionText { get; set; } = string.Empty;

    public IReadOnlyList<CommunityVoteResolutionSignerRequest> RequiredSigners { get; set; } = [];

    public bool LegalReviewRequested { get; set; } = true;
}

public sealed class CommunityVoteResolutionSignerRequest
{
    public string PartyId { get; set; } = string.Empty;

    public string RoleCode { get; set; } = "CommunityParticipant";

    [IsmsPProtectedData(
        PersonalDataFieldKey.DisplayName,
        "커뮤니티 결의문 서명자 표시",
        IsContractData = true,
        ProtectionNote = "결의문 서명 요청 목록에는 표시명과 역할 중심으로 노출")]
    public string SignerDisplayName { get; set; } = string.Empty;
}

public sealed class CommunityVoteResolutionSignRequest
{
    public string PartyId { get; set; } = string.Empty;

    public string SignerDisplayName { get; set; } = string.Empty;

    public string SignatureMethodCode { get; set; } = ContractSignatureMethodCode.PlatformClickSign;

    public string ConsentText { get; set; } = string.Empty;

    public string SignatureEvidencePayload { get; set; } = string.Empty;

    public string? ClientIpHash { get; set; }
}

public sealed class CommunityVoteResolutionReadyToSignRequest
{
    public string ReviewedByDisplayName { get; set; } = string.Empty;

    public string ReviewMemo { get; set; } = string.Empty;
}

public sealed class CommunityVoteListResponse
{
    public IReadOnlyList<CommunityVoteResponse> Items { get; set; } = [];
}

public sealed class CommunityVoteResponse
{
    public Guid Id { get; set; }

    public string AppKey { get; set; } = string.Empty;

    public string CommunityScope { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Status { get; set; } = CommunityVoteStatusCodes.Open;

    public bool AllowMultipleSelection { get; set; }

    public bool ResolutionDocumentEnabled { get; set; }

    public bool SignatureRequired { get; set; }

    public string CreatedByDisplayName { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? ClosesAtUtc { get; set; }

    public DateTime? ClosedAtUtc { get; set; }

    public int TotalVoteCount { get; set; }

    public IReadOnlyList<CommunityVoteOptionResponse> Options { get; set; } = [];

    public CommunityVoteResolutionDocumentResponse? ResolutionDocument { get; set; }
}

public sealed class CommunityVoteOptionResponse
{
    public string OptionId { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public int VoteCount { get; set; }

    public bool IsWinningOption { get; set; }
}

public sealed class CommunityVoteResolutionDocumentResponse
{
    public Guid Id { get; set; }

    public Guid VoteId { get; set; }

    public string DocumentNumber { get; set; } = string.Empty;

    public string DocumentTitle { get; set; } = string.Empty;

    public string ResolutionText { get; set; } = string.Empty;

    public string DocumentHash { get; set; } = string.Empty;

    public string Status { get; set; } = CommunityVoteResolutionStatusCodes.LegalReviewRequired;

    public string LegalEffectNotice { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public ContractElectronicSignaturePlan? SignaturePlan { get; set; }
}

public static class CommunityVoteStatusCodes
{
    public const string Open = "Open";

    public const string Closed = "Closed";

    public const string ResolutionDrafted = "ResolutionDrafted";
}

public static class CommunityVoteResolutionStatusCodes
{
    public const string Draft = "Draft";

    public const string LegalReviewRequired = "LegalReviewRequired";

    public const string ReadyToSign = "ReadyToSign";

    public const string PartiallySigned = "PartiallySigned";

    public const string Signed = "Signed";
}
