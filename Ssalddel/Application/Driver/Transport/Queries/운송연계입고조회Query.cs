namespace Ssalddel.Application.Driver.Transport;

public sealed record 운송연계입고조회Query(string 운송의뢰Id)
    : IRequest<운송연계입고Projection?>;

public sealed record 운송연계입고Projection(
    long Id,
    long 창고Id,
    string 상태,
    DateTime UpdatedAt);
