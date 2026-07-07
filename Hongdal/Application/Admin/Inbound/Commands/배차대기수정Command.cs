using 홍달.도메인.배차;

namespace Hongdal.Application.Admin.Inbound;

public sealed record 배차대기수정Command(
    long Id,
    string 의뢰Id,
    string 화주Id,
    int? 배차업무유형,
    string? 원본의뢰유형,
    string? 원본의뢰Id,
    string 픽업_도로명주소,
    string 픽업_상세주소,
    decimal? 픽업_위도,
    decimal? 픽업_경도,
    string 하차_도로명주소,
    string 하차_상세주소,
    decimal? 하차_위도,
    decimal? 하차_경도,
    string 상태,
    string? 공동구매도착지유형코드 = null,
    bool? 공동구매기사세대배송여부 = null,
    string? 공동구매세대배송방식코드 = null,
    int? 공동구매세대배송건수 = null,
    string? 공동구매분배책임코드 = null) : IRequest<배차대기?>;
