namespace Hongdal.Contracts.Common.Community;

public sealed class PlatformCommunityPostListResponse
{
    public IReadOnlyList<PlatformCommunityPostResponse> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public sealed class PlatformCommunityBoardListResponse
{
    public IReadOnlyList<PlatformCommunityBoardResponse> Items { get; set; } = [];
}

public sealed class PlatformCommunityBoardResponse
{
    public long Id { get; set; }
    public string AppKey { get; set; } = string.Empty;
    public string BoardKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RequestedBy { get; set; } = string.Empty;
    public string RequestReason { get; set; } = string.Empty;
    public string Status { get; set; } = PlatformCommunityBoardRequestStatuses.Pending;
    public string StatusName { get; set; } = string.Empty;
    public string? OperatorMemo { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }
    public DateTime? RejectedAtUtc { get; set; }
}

public sealed class PlatformCommunityBoardCreateRequest
{
    public string AppKey { get; set; } = "platform";
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RequestedBy { get; set; } = string.Empty;
    public string RequestReason { get; set; } = string.Empty;
}

public sealed class PlatformCommunityBoardReviewRequest
{
    public string OperatorMemo { get; set; } = string.Empty;
}

public static class PlatformCommunityBoardRequestStatuses
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
}

public sealed class PlatformCommunityPostResponse
{
    public long Id { get; set; }
    public string AppKey { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string WorkflowTag { get; set; } = string.Empty;
    public string RoleTag { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? SharedLinkUrl { get; set; }
    public string? 커뮤니티원장Id { get; set; }
    public PlatformCommunityPostLedgerContextResponse? 원장Context { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public bool IsReportBoardPost { get; set; }
    public string ReporterDisplayName { get; set; } = string.Empty;
    public string ReportedDisplayName { get; set; } = string.Empty;
    public string ViewerReportRole { get; set; } = PlatformCommunityReportViewerRoles.Observer;
    public bool IsReportSubjectMasked { get; set; }
    public bool IsOperatorPinned { get; set; }
    public DateTime? OperatorPinnedAtUtc { get; set; }
    public int RecommendationCount { get; set; }
    public int CommentCount { get; set; }
    public DateTime? LastEngagedAtUtc { get; set; }
    public bool IsTrending { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public IReadOnlyList<PlatformCommunityPostAttachmentResponse> Attachments { get; set; } = [];
    public IReadOnlyList<PlatformCommunityPostCommentResponse> RecentComments { get; set; } = [];
}

public sealed class PlatformCommunityPostCreateRequest
{
    public string AppKey { get; set; } = "platform";
    public string Category { get; set; } = "자유";
    public string WorkflowTag { get; set; } = "국내 화물 운송";
    public string RoleTag { get; set; } = "플랫폼 구성원";
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? SharedLinkUrl { get; set; }
    public string? 커뮤니티원장Id { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public bool IsReportBoardPost { get; set; }
    public string? ReporterDisplayName { get; set; }
    public string? ReportedDisplayName { get; set; }
    public string Password { get; set; } = string.Empty;
}

public sealed class PlatformCommunityPostAudioResponse
{
    public long PostId { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsReady { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string VoiceId { get; set; } = string.Empty;
    public string ModelVersion { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime UpdatedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public IReadOnlyList<PlatformCommunityPostAudioSegmentResponse> Segments { get; set; } = [];
}

public sealed class PlatformCommunityPostAudioSegmentResponse
{
    public int Sequence { get; set; }
    public int CharacterCount { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string DownloadPath { get; set; } = string.Empty;
}

public sealed class PlatformCommunityPostUpdateRequest
{
    public string Category { get; set; } = "자유";
    public string WorkflowTag { get; set; } = "국내 화물 운송";
    public string RoleTag { get; set; } = "플랫폼 구성원";
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? SharedLinkUrl { get; set; }
    public string? 커뮤니티원장Id { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public bool IsReportBoardPost { get; set; }
    public string? ReporterDisplayName { get; set; }
    public string? ReportedDisplayName { get; set; }
    public string Password { get; set; } = string.Empty;
}

public sealed class PlatformCommunityPostLedgerContextResponse
{
    public string 원장Id { get; set; } = string.Empty;
    public long Revision { get; set; }
    public string 원장템플릿Key { get; set; } = string.Empty;
    public string 원장템플릿명 { get; set; } = string.Empty;
    public string 제목 { get; set; } = string.Empty;
    public string 상태 { get; set; } = string.Empty;
    public string 현재단계 { get; set; } = string.Empty;
    public string 처리체계명 { get; set; } = string.Empty;
    public string 업무분류Code { get; set; } = string.Empty;
    public string 업무분류명 { get; set; } = string.Empty;
    public string 기능설정Key { get; set; } = string.Empty;
    public bool 기능활성화여부 { get; set; }
    public bool 상세조회가능여부 { get; set; }
    public bool 참여요청필요여부 { get; set; }
    public bool 재사용허용여부 { get; set; }
    public bool 재공유허용여부 { get; set; }
    public DiagramSnapshotDto? 다이어그램 { get; set; }
    public IReadOnlyList<PlatformCommunityLedgerBlockResponse> 블록목록 { get; set; } = [];
    public IReadOnlyList<string> 가능한행동목록 { get; set; } = [];
    public IReadOnlyList<PlatformCommunityLedgerNodeActionResponse> 노드행동목록 { get; set; } = [];
}

public sealed class PlatformCommunityLedgerBlockResponse
{
    public string 블록Id { get; set; } = string.Empty;
    public string 블록유형 { get; set; } = string.Empty;
    public string 제목 { get; set; } = string.Empty;
    public string? 상태 { get; set; }
    public IReadOnlyDictionary<string, string> 항목 { get; set; } = new Dictionary<string, string>();
}

public sealed class PlatformCommunityLedgerNodeActionResponse
{
    public string 행동Code { get; set; } = string.Empty;
    public string 블록Id { get; set; } = string.Empty;
    public string 표시명 { get; set; } = string.Empty;
    public string 설명 { get; set; } = string.Empty;
    public string ApiEndpointKey { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = "POST";
    public string 실행대상Id { get; set; } = string.Empty;
    public string 현재상태 { get; set; } = string.Empty;
    public bool 실행가능여부 { get; set; }
    public bool 확인필요여부 { get; set; } = true;
    public bool 사진필수여부 { get; set; }
    public string? 비활성사유 { get; set; }
}

public static class CommunityLedgerNodeActionCodes
{
    public const string TransportArrivePickup = "TransportArrivePickup";
    public const string TransportCompletePickup = "TransportCompletePickup";
}

public sealed class PlatformCommunityPostLedgerChoiceResponse
{
    public string 원장Id { get; set; } = string.Empty;
    public string 원장템플릿Key { get; set; } = string.Empty;
    public string 원장템플릿명 { get; set; } = string.Empty;
    public string 제목 { get; set; } = string.Empty;
    public string 상태 { get; set; } = string.Empty;
    public string 현재단계 { get; set; } = string.Empty;
    public string 업무분류명 { get; set; } = string.Empty;
    public string WorkflowTag { get; set; } = string.Empty;
    public bool 내가만든원장 { get; set; }
    public bool 내접근원장여부 { get; set; }
    public bool 커뮤니티공유여부 { get; set; }
    public bool 재사용허용여부 { get; set; }
    public bool 재공유허용여부 { get; set; }
    public string 참여역할 { get; set; } = string.Empty;
    public DateTime 수정시각Utc { get; set; }
}

public static class PlatformCommunityReportViewerRoles
{
    public const string Observer = "Observer";
    public const string Reporter = "Reporter";
    public const string Reported = "Reported";
    public const string Operator = "Operator";
}

public sealed class PlatformCommunityPostPasswordRequest
{
    public string Password { get; set; } = string.Empty;
}

public sealed class PlatformCommunityOperatorHiddenRequest
{
    public bool IsOperatorHidden { get; set; }
}

public sealed class PlatformCommunityPostOperatorPinRequest
{
    public bool IsOperatorPinned { get; set; }
}

public sealed class PlatformCommunityPostRecommendationRequest
{
    public string RecommenderKey { get; set; } = string.Empty;
}

public sealed class PlatformCommunityPostCommentCreateRequest
{
    public string Nickname { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}

public sealed class PlatformCommunityPostCommentResponse
{
    public long Id { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public int ReportCount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class PlatformCommunityPostAttachmentResponse
{
    public long Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string ObjectName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public int CommentCount { get; set; }
    public DateTime UploadedAtUtc { get; set; }
    public IReadOnlyList<PlatformCommunityPostAttachmentCommentResponse> RecentComments { get; set; } = [];
}

public sealed class PlatformCommunityPostAttachmentCommentCreateRequest
{
    public string Nickname { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}

public sealed class PlatformCommunityPostAttachmentCommentResponse
{
    public long Id { get; set; }
    public long AttachmentId { get; set; }
    public string Nickname { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public int ReportCount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
