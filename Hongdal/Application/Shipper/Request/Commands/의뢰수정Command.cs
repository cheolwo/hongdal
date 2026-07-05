using FluentResults;

namespace Hongdal.Application.Shipper.Request;

public sealed record 의뢰수정Command(
    string RequestId,
    운송조건입력값 운송조건,
    화물정보입력값 화물정보,
    위치정보입력값 픽업지,
    위치정보입력값 하차지,
    요청조건입력값 요청조건,
    정산조건입력값? 정산조건) : IRequest<Result<Hongdal.Contracts.Shipper.Request.화주운송의뢰응답>>;
