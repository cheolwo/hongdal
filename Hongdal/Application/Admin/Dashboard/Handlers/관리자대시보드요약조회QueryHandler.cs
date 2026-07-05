using Hongdal.Contracts.Admin.Dashboard;

namespace Hongdal.Application.Admin.Dashboard;

public sealed class 관리자대시보드요약조회QueryHandler : IRequestHandler<관리자대시보드요약조회Query, 관리자대시보드요약응답>
{
    private readonly HongdalContext _db;

    public 관리자대시보드요약조회QueryHandler(HongdalContext db)
    {
        _db = db;
    }

    public async Task<관리자대시보드요약응답> Handle(관리자대시보드요약조회Query request, CancellationToken cancellationToken)
    {
        var todayStart = DateTime.UtcNow.Date;
        var tomorrowStart = todayStart.AddDays(1);

        var 오늘의뢰수Task = _db.화주운송의뢰.CountAsync(x => x.CreatedAt >= todayStart && x.CreatedAt < tomorrowStart, cancellationToken);
        var 결제대기수Task = _db.화주운송의뢰.CountAsync(x => x.결제상태 == 상태값.결제상태.결제대기, cancellationToken);
        var 결제완료수Task = _db.화주운송의뢰.CountAsync(x => x.결제상태 == 상태값.결제상태.결제완료, cancellationToken);

        var 배차대기수Task = _db.배차대기.CountAsync(x => x.상태 == 상태값.배차대기상태.대기, cancellationToken);
        var 배차확정수Task = _db.배차대기.CountAsync(x => x.상태 == 상태값.배차대기상태.확정, cancellationToken);

        var 운송중수Task = _db.배송_운송.CountAsync(x => x.상태 == "운송중", cancellationToken);
        var 완료수Task = _db.배송_운송.CountAsync(x => x.상태 == "완료", cancellationToken);

        var 취소수Task = _db.화주운송의뢰.CountAsync(x => x.상태 == "취소", cancellationToken);
        var 환불수Task = _db.결제.CountAsync(x => x.결제상태 == 상태값.결제상태.환불됨, cancellationToken);

        await Task.WhenAll(
            오늘의뢰수Task,
            결제대기수Task,
            결제완료수Task,
            배차대기수Task,
            배차확정수Task,
            운송중수Task,
            완료수Task,
            취소수Task,
            환불수Task);

        return new 관리자대시보드요약응답
        {
            오늘의뢰수 = 오늘의뢰수Task.Result,
            결제대기수 = 결제대기수Task.Result,
            결제완료수 = 결제완료수Task.Result,
            배차대기수 = 배차대기수Task.Result,
            배차확정수 = 배차확정수Task.Result,
            운송중수 = 운송중수Task.Result,
            완료수 = 완료수Task.Result,
            취소환불수 = 취소수Task.Result + 환불수Task.Result
        };
    }
}
