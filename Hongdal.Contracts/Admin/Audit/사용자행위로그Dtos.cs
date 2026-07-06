using Hongdal.Contracts.Common.Privacy;

namespace Hongdal.Contracts.Admin.Audit;

public sealed class 사용자행위로그검색요청
{
    public string? AppKey { get; set; }
    public string? UserId { get; set; }
    [IsmsPProtectedData(
        PersonalDataFieldKey.Email,
        "감사 로그 검색 조건",
        ProtectionNote = "관리자 검색 조건이라도 원본 이메일 접근은 감사 대상")]
    public string? Email { get; set; }
    [IsmsPProtectedData(
        PersonalDataFieldKey.PhoneNumber,
        "감사 로그 검색 조건",
        ProtectionNote = "전화번호 전체가 아닌 뒤 4자리 검색만 허용")]
    public string? PhoneLast4 { get; set; }
    public string? ActionType { get; set; }
    public string? ActionName { get; set; }
    public bool? IsSuccess { get; set; }
    public string? TraceId { get; set; }
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public sealed class 사용자행위로그목록응답
{
    public IReadOnlyList<사용자행위로그요약응답> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}

public sealed class 사용자행위로그요약응답
{
    public long Id { get; set; }
    public string AppKey { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    [IsmsPProtectedData(
        PersonalDataFieldKey.DisplayName,
        "감사 로그 행위자 표시",
        ProtectionNote = "목록에서는 사용자 식별에 필요한 표시명만 노출")]
    public string UserName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    [IsmsPProtectedData(
        PersonalDataFieldKey.Email,
        "감사 로그 행위자 이메일 표시",
        ProtectionNote = "응답 DTO는 마스킹된 이메일만 포함")]
    public string EmailMasked { get; set; } = string.Empty;
    [IsmsPProtectedData(
        PersonalDataFieldKey.PhoneNumber,
        "감사 로그 행위자 연락처 단서 표시",
        ProtectionNote = "전화번호 전체가 아닌 뒤 4자리만 포함")]
    public string PhoneLast4 { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string TraceId { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public DateTime OccurredAtUtc { get; set; }
}

public sealed class 사용자행위로그상세응답
{
    public long Id { get; set; }
    public string AppKey { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    [IsmsPProtectedData(
        PersonalDataFieldKey.DisplayName,
        "감사 로그 상세 행위자 표시",
        ProtectionNote = "상세 화면에서도 역할 기반 접근권한 필요")]
    public string UserName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    [IsmsPProtectedData(
        PersonalDataFieldKey.Email,
        "감사 로그 상세 이메일 표시",
        ProtectionNote = "응답 DTO는 마스킹된 이메일만 포함")]
    public string EmailMasked { get; set; } = string.Empty;
    [IsmsPProtectedData(
        PersonalDataFieldKey.PhoneNumber,
        "감사 로그 상세 연락처 단서 표시",
        ProtectionNote = "전화번호 전체가 아닌 뒤 4자리만 포함")]
    public string PhoneLast4 { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string ActionName { get; set; } = string.Empty;
    public string Route { get; set; } = string.Empty;
    public string TraceId { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string ErrorCode { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    [IsmsPProtectedData(
        PersonalDataFieldKey.IpAddress,
        "보안 감사와 작업장 접근 검증",
        DomainCode = IsmsPDomainCode.ProtectionSafeguards,
        ProtectionNote = "IP 주소는 보안 감사 목적 외 노출을 제한")]
    public string ClientIp { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; }
    public string MetadataJson { get; set; } = string.Empty;
}

public sealed class Trace행위로그묶음응답
{
    public string TraceId { get; set; } = string.Empty;
    public IReadOnlyList<사용자행위로그요약응답> Items { get; set; } = [];
}
