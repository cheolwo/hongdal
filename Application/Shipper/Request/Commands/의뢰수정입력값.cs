namespace Hongdal.Application.Shipper.Request;

public sealed record 운송조건입력값(
    string? 운송방식,
    string? 차량종류,
    string? 서비스레벨);

public sealed record 화물정보입력값(
    string? 화물종류,
    string? 화물설명,
    int? 화물수량,
    decimal? 화물중량Kg,
    decimal? 화물부피Cbm,
    bool? 화물파손주의여부,
    string? 화물온도조건);

public sealed record 위치정보입력값(
    string? 도로명주소,
    string? 상세주소,
    decimal? 위도,
    decimal? 경도,
    string? 연락처이름,
    string? 연락처전화번호,
    DateTime? 시간창시작일시,
    DateTime? 시간창종료일시);

public sealed record 요청조건입력값(
    string? 요청사항);

public sealed record 정산조건입력값(
    string? 결제수단,
    Hongdal.Contracts.Shipper.Request.화주운송정산조건DTO? 정산조건);
