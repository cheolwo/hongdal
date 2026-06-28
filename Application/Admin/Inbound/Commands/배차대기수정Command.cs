using 홍달.도메인.배차;

namespace Hongdal.Application.Admin.Inbound;

public sealed record 배차대기수정Command(
    long Id,
    string 의뢰Id,
    string 화주Id,
    string 픽업_도로명주소,
    string 픽업_상세주소,
    decimal? 픽업_위도,
    decimal? 픽업_경도,
    string 하차_도로명주소,
    string 하차_상세주소,
    decimal? 하차_위도,
    decimal? 하차_경도,
    string 상태) : IRequest<배차대기?>;
