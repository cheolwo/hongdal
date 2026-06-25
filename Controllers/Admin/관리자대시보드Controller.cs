using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;
using 홍달.도메인.공통;

namespace Hongdal.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/dashboard")]
[Authorize(Policy = "서버관리자전용")]
public sealed class 관리자대시보드Controller : ControllerBase
{
    private readonly HongdalContext _db;

    public 관리자대시보드Controller(HongdalContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> 요약조회()
    {
        var todayStart = DateTime.UtcNow.Date;
        var tomorrowStart = todayStart.AddDays(1);

        var 오늘의뢰수Task = _db.화주운송의뢰.CountAsync(x => x.CreatedAt >= todayStart && x.CreatedAt < tomorrowStart);
        var 결제대기수Task = _db.화주운송의뢰.CountAsync(x => x.결제상태 == 상태값.결제상태.결제대기);
        var 결제완료수Task = _db.화주운송의뢰.CountAsync(x => x.결제상태 == 상태값.결제상태.결제완료);

        var 배차대기수Task = _db.배차대기.CountAsync(x => x.상태 == 상태값.배차대기상태.대기);
        var 배차확정수Task = _db.배차대기.CountAsync(x => x.상태 == 상태값.배차대기상태.확정);

        var 운송중수Task = _db.배송_운송.CountAsync(x => x.상태 == "운송중");
        var 완료수Task = _db.배송_운송.CountAsync(x => x.상태 == "완료");

        var 취소수Task = _db.화주운송의뢰.CountAsync(x => x.상태 == "취소");
        var 환불수Task = _db.결제.CountAsync(x => x.결제상태 == 상태값.결제상태.환불됨);

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

        return Ok(new 관리자대시보드요약응답
        {
            오늘의뢰수 = 오늘의뢰수Task.Result,
            결제대기수 = 결제대기수Task.Result,
            결제완료수 = 결제완료수Task.Result,
            배차대기수 = 배차대기수Task.Result,
            배차확정수 = 배차확정수Task.Result,
            운송중수 = 운송중수Task.Result,
            완료수 = 완료수Task.Result,
            취소환불수 = 취소수Task.Result + 환불수Task.Result
        });
    }
}

public sealed class 관리자대시보드요약응답
{
    public int 오늘의뢰수 { get; set; }
    public int 결제대기수 { get; set; }
    public int 결제완료수 { get; set; }
    public int 배차대기수 { get; set; }
    public int 배차확정수 { get; set; }
    public int 운송중수 { get; set; }
    public int 완료수 { get; set; }
    public int 취소환불수 { get; set; }
}
