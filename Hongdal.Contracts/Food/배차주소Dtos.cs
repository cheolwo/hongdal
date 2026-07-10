using Hongdal.Contracts.Common.Privacy;

namespace Hongdal.Contracts.Food;

public sealed class 배차주소저장요청
{
    [IsmsPProtectedData(
        PersonalDataFieldKey.RoadAddressLevel2,
        "상차지 주소 확인",
        ProtectionNote = "상차지 기본주소는 목록/지도 단계에서 최소 범위로 표시합니다.")]
    public string 상차지우편번호 { get; set; } = string.Empty;

    [IsmsPProtectedData(
        PersonalDataFieldKey.RoadAddressLevel2,
        "상차지 주소 확인",
        ProtectionNote = "상차지 기본주소는 목록/지도 단계에서 최소 범위로 표시합니다.")]
    public string 상차지기본주소 { get; set; } = string.Empty;

    [IsmsPProtectedData(
        PersonalDataFieldKey.DetailedAddress,
        "상차지 상세 위치 확인",
        ProtectionNote = "상차지 상세주소는 실제 배차와 방문이 필요한 단계에서만 취급합니다.")]
    public string 상차지상세주소 { get; set; } = string.Empty;

    [IsmsPProtectedData(
        PersonalDataFieldKey.RoadAddressLevel2,
        "하차지 주소 확인",
        ProtectionNote = "하차지 기본주소는 목록/지도 단계에서 최소 범위로 표시합니다.")]
    public string 하차지우편번호 { get; set; } = string.Empty;

    [IsmsPProtectedData(
        PersonalDataFieldKey.RoadAddressLevel2,
        "하차지 주소 확인",
        ProtectionNote = "하차지 기본주소는 목록/지도 단계에서 최소 범위로 표시합니다.")]
    public string 하차지기본주소 { get; set; } = string.Empty;

    [IsmsPProtectedData(
        PersonalDataFieldKey.DetailedAddress,
        "하차지 상세 위치 확인",
        ProtectionNote = "하차지 상세주소는 실제 배차와 방문이 필요한 단계에서만 취급합니다.")]
    public string 하차지상세주소 { get; set; } = string.Empty;

    public string 사업자등록번호 { get; set; } = string.Empty;
}

public sealed class 배차주소저장응답
{
    public string 메시지 { get; set; } = string.Empty;

    public double? 상차지위도 { get; set; }

    public double? 상차지경도 { get; set; }

    public double? 하차지위도 { get; set; }

    public double? 하차지경도 { get; set; }
}
