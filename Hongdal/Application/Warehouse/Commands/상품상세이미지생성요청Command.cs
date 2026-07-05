using FluentResults;
using MediatR;
using Hongdal.Application.Abstractions;

namespace Hongdal.Application.Warehouse;

public sealed record 상품상세이미지생성요청Command(
    string 요청자Id,
    long 상품Id,
    long? 주문Id,
    long? 통관절차Id,
    string 참여자Id,
    홍달.도메인.사용자.홍달역할유형 실행역할) : IRequest<Result<long>>;
