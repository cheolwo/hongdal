using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;
using 홍달.Services;

namespace Hongdal.Controllers.Driver.Food
{
    [ApiController]
    [Route("api/v1/drivers/{driverId}/monthly-settlements")]
    [Authorize(Roles = 역할명.기사)]
    public class 배달기사월정산Controller : ControllerBase
    {
        private readonly HongdalContext _db;
        private readonly I기사월정산Service _driverMonthlySettlementService;

        public 배달기사월정산Controller(HongdalContext db, I기사월정산Service driverMonthlySettlementService)
        {
            _db = db;
            _driverMonthlySettlementService = driverMonthlySettlementService;
        }

        [HttpGet("current")]
        public async Task<IActionResult> 당월조회(string driverId)
        {
            if (!현재기사확인(driverId)) return Forbid();

            var now = DateTime.UtcNow;
            var settlement = await _db.기사월정산
                .Where(x => x.기사Id == driverId && x.년도 == now.Year && x.월 == now.Month)
                .FirstOrDefaultAsync();

            if (settlement == null)
            {
                return Ok(new 배달기사월정산응답
                {
                    기사Id = driverId,
                    년도 = now.Year,
                    월 = now.Month,
                    배차건수 = 0,
                    이용료 = 0,
                    결제완료 = false
                });
            }

            return Ok(응답변환(settlement));
        }

        [HttpPost("{year:int}/{month:int}/mark-paid")]
        public async Task<IActionResult> 결제완료처리(string driverId, int year, int month)
        {
            if (!현재기사확인(driverId)) return Forbid();

            if (month < 1 || month > 12)
            {
                return BadRequest("month must be between 1 and 12");
            }

            var settlement = await _driverMonthlySettlementService.월말청구결제완료처리Async(driverId, year, month, DateTime.UtcNow);

            return Ok(new 배달기사월정산결제완료응답
            {
                기사Id = settlement.기사Id,
                년도 = settlement.년도,
                월 = settlement.월,
                배차건수 = settlement.배차건수,
                차감이용료 = settlement.이용료,
                결제완료 = settlement.결제완료,
                처리일시Utc = settlement.UpdatedAt
            });
        }

        private static 배달기사월정산응답 응답변환(홍달.도메인.기사.기사월정산 settlement)
        {
            return new 배달기사월정산응답
            {
                기사Id = settlement.기사Id,
                년도 = settlement.년도,
                월 = settlement.월,
                배차건수 = settlement.배차건수,
                이용료 = settlement.이용료,
                결제완료 = settlement.결제완료
            };
        }

        private bool 현재기사확인(string driverId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return !string.IsNullOrWhiteSpace(currentUserId)
                   && string.Equals(currentUserId, driverId, StringComparison.Ordinal);
        }
    }

    public sealed class 배달기사월정산응답
    {
        public string 기사Id { get; set; } = string.Empty;
        public int 년도 { get; set; }
        public int 월 { get; set; }
        public int 배차건수 { get; set; }
        public decimal 이용료 { get; set; }
        public bool 결제완료 { get; set; }
    }

    public sealed class 배달기사월정산결제완료응답
    {
        public string 기사Id { get; set; } = string.Empty;
        public int 년도 { get; set; }
        public int 월 { get; set; }
        public int 배차건수 { get; set; }
        public decimal 차감이용료 { get; set; }
        public bool 결제완료 { get; set; }
        public DateTime 처리일시Utc { get; set; }
    }
}
