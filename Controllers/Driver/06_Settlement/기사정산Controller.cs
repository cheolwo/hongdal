using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;
using 홍달.Services.Settlement;
using 홍달.도메인.공통;
using Hongdal.Contracts.Driver.Settlement;

namespace Hongdal.Controllers.Driver.Settlement06
{
    [ApiController]
    [Authorize(Roles = 역할명.기사)]
    [Route("api/v1/driver/settlements")]
    public sealed class 기사정산Controller : ControllerBase
    {
        private readonly HongdalContext _db;
        private readonly I기사월정산Service _settlementService;

        public 기사정산Controller(HongdalContext db, I기사월정산Service settlementService)
        {
            _db = db;
            _settlementService = settlementService;
        }

        [HttpGet("current-month")]
        public async Task<IActionResult> 현재월조회()
        {
            var driverId = 현재기사Id();
            var now = DateTime.UtcNow;
            var settlement = await _db.기사월정산.AsNoTracking().FirstOrDefaultAsync(x => x.기사Id == driverId && x.년도 == now.Year && x.월 == now.Month);
            if (settlement == null)
            {
                settlement = await _settlementService.배차확정반영Async(driverId, now);
            }

            return Ok(new 기사정산응답
            {
                DriverId = driverId,
                Year = settlement.년도,
                Month = settlement.월,
                DispatchCount = settlement.배차건수,
                UsageFee = settlement.이용료,
                IsPaid = settlement.결제완료
            });
        }

        private string 현재기사Id()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? throw new InvalidOperationException("기사 인증 정보가 없습니다.");
        }
    }

}
