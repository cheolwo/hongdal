using FluentResults;
using Ssalddel.Services.Community;

namespace Ssalddel.Application.Community;

public sealed class 주문자주문원장조회QueryHandler
    : IRequestHandler<주문자주문원장조회Query, Result<주문원장역할별조회Dto>>
{
    private readonly I주문원장역할별조회Service _service;

    public 주문자주문원장조회QueryHandler(I주문원장역할별조회Service service) => _service = service;

    public Task<Result<주문원장역할별조회Dto>> Handle(주문자주문원장조회Query request, CancellationToken cancellationToken)
        => _service.조회Async(request.주문원장Id, request.주문자UserId, 주문원장조회역할.주문자, cancellationToken);
}

public sealed class 판매자주문원장조회QueryHandler
    : IRequestHandler<판매자주문원장조회Query, Result<주문원장역할별조회Dto>>
{
    private readonly I주문원장역할별조회Service _service;

    public 판매자주문원장조회QueryHandler(I주문원장역할별조회Service service) => _service = service;

    public Task<Result<주문원장역할별조회Dto>> Handle(판매자주문원장조회Query request, CancellationToken cancellationToken)
        => _service.조회Async(request.주문원장Id, request.판매자UserId, 주문원장조회역할.판매자, cancellationToken);
}

public sealed class 창고담당자주문원장조회QueryHandler
    : IRequestHandler<창고담당자주문원장조회Query, Result<주문원장역할별조회Dto>>
{
    private readonly I주문원장역할별조회Service _service;

    public 창고담당자주문원장조회QueryHandler(I주문원장역할별조회Service service) => _service = service;

    public Task<Result<주문원장역할별조회Dto>> Handle(창고담당자주문원장조회Query request, CancellationToken cancellationToken)
        => _service.조회Async(request.주문원장Id, request.창고담당자UserId, 주문원장조회역할.창고담당자, cancellationToken);
}

public sealed class 운송담당자주문원장조회QueryHandler
    : IRequestHandler<운송담당자주문원장조회Query, Result<주문원장역할별조회Dto>>
{
    private readonly I주문원장역할별조회Service _service;

    public 운송담당자주문원장조회QueryHandler(I주문원장역할별조회Service service) => _service = service;

    public Task<Result<주문원장역할별조회Dto>> Handle(운송담당자주문원장조회Query request, CancellationToken cancellationToken)
        => _service.조회Async(request.주문원장Id, request.운송담당자UserId, 주문원장조회역할.운송담당자, cancellationToken);
}
