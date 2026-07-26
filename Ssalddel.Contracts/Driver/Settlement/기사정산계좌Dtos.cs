using Ssalddel.Contracts.Common.Privacy;

namespace Ssalddel.Contracts.Driver.Settlement;

public sealed class 기사정산계좌수정요청
{
    public string CountryCode { get; set; } = "KR";

    public string BankName { get; set; } = string.Empty;

    [IsmsPProtectedData(
        PersonalDataFieldKey.DisplayName,
        "기사 정산계좌 예금주 등록",
        IsContractData = true,
        ProtectionNote = "기사 본인의 정산계좌 관리 범위에서만 처리")]
    public string AccountHolderName { get; set; } = string.Empty;

    [IsmsPProtectedData(
        PersonalDataFieldKey.BankAccountNumber,
        "기사 정산계좌 등록",
        IsContractData = true,
        ProtectionNote = "요청 원문은 persistence 경계에서 암호화하고 로그에 남기지 않음")]
    public string AccountNumber { get; set; } = string.Empty;

    public bool 개인정보저장동의 { get; set; }
}

public sealed class 기사정산계좌응답
{
    public string DriverId { get; set; } = string.Empty;

    public bool HasAccount { get; set; }

    public string CountryCode { get; set; } = string.Empty;

    public string BankName { get; set; } = string.Empty;

    [IsmsPProtectedData(
        PersonalDataFieldKey.DisplayName,
        "기사 본인의 정산계좌 예금주 확인",
        IsContractData = true,
        ProtectionNote = "인증된 기사 본인 응답에서만 노출")]
    public string AccountHolderName { get; set; } = string.Empty;

    [IsmsPProtectedData(
        PersonalDataFieldKey.BankAccountNumber,
        "기사 본인의 정산계좌 표시",
        IsContractData = true,
        ProtectionNote = "응답에는 끝 4자리만 남긴 마스킹 계좌번호를 포함")]
    public string MaskedAccountNumber { get; set; } = string.Empty;

    public string VerificationStatus { get; set; } = string.Empty;

    public DateTime? UpdatedAtUtc { get; set; }
}
