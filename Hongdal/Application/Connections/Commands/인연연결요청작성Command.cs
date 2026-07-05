using FluentResults;
using MediatR;
using 홍달.도메인.사용자;

namespace Hongdal.Application.Connections.Commands;

public sealed record 인연연결요청작성Command(
    string 요청자참여자Id,
    홍달역할유형 요청자역할,
    string 대상자참여자Id,
    홍달역할유형 대상자역할,
    long? 감사메시지Id,
    long? 주문Id,
    long? 통관절차Id,
    string 요청목적,
    string 요청메시지) : IRequest<Result<long>>;
