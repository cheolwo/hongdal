using FluentResults;
using Ssalddel.Application.Abstractions;
using Ssalddel.Services.Community;

namespace Ssalddel.Application.Community;

public sealed record 주문자주문원장조회Query(
    string 주문원장Id,
    string 주문자UserId) : IQuery<Result<주문원장역할별조회Dto>>;

public sealed record 판매자주문원장조회Query(
    string 주문원장Id,
    string 판매자UserId) : IQuery<Result<주문원장역할별조회Dto>>;

public sealed record 창고담당자주문원장조회Query(
    string 주문원장Id,
    string 창고담당자UserId) : IQuery<Result<주문원장역할별조회Dto>>;

public sealed record 운송담당자주문원장조회Query(
    string 주문원장Id,
    string 운송담당자UserId) : IQuery<Result<주문원장역할별조회Dto>>;
