using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using 홍달.Data;
using 홍달.Services;
using 홍달.도메인.공통;
using 홍달.도메인.기사;

namespace Hongdal.Controllers.Driver.Cargo
{
    [ApiController]
    [Route("api/v1/drivers/{driverId}/shifts")]
    [Authorize(Roles = 역할명.기사)]
    public class 용달기사근무Controller : ControllerBase
    {
        private readonly HongdalContext _db;
        private readonly IDispatchRecommendationService _dispatchRecommendationService;
        private readonly I기사월정산Service _driverMonthlySettlementService;
        private readonly IDriverWorkQueueStore _driverWorkQueueStore;

        public 용달기사근무Controller(
            HongdalContext db,
            IDispatchRecommendationService dispatchRecommendationService,
            I기사월정산Service driverMonthlySettlementService,
            IDriverWorkQueueStore driverWorkQueueStore)
        {
            _db = db;
            _dispatchRecommendationService = dispatchRecommendationService;
            _driverMonthlySettlementService = driverMonthlySettlementService;
            _driverWorkQueueStore = driverWorkQueueStore;
        }
        /// <summary>
        /// 근무시작요청이 되면 내가 생각하기에 큐가 필요하다고 생각해.
        /// 큐에 근무시작을 한 사람들을 넣어서 관리를 하고 화주운송의뢰건에 따라서 적정한 기사를
        /// 배치하는 식으로 가는 게 바람직하다고 생각을 하거든. 
        /// 그래서 DB랑 Queue랑 이게 데이터가 일치가 되도록 해서 관리가 되도록 하는 게 바람직하다고 생각을 하고.
        /// 그렇게 해야 이게 내가 생각하기도 편하고, 자료구조를 설계를 하는데 있어서도 바람직한 면이 존재하거든.
        /// 처음에는 간단한 Queue로서 이제 관리하지만 이게 나중에는 뭐... 어느 지역 건에 있는지에 따라서 Queue를 넣고
        /// Queue도 하나만 있는 게 아닐테니까 그렇게 하고,
        /// 이제 그렇게 근무시작 Queue를 해서 Queue에 넣어진 기사님은 이제 자신이 속한 지역과 관계된 자신과 지금 그 시간에서
        /// 그 공간에서 가까운 화주운송의뢰건들에 대해 조회를 할 수 있게끔 하는 그런 식이 됐으면 좋겠다라는 게 내 생각이야. 
        /// 이 부분에 대해서는 따로 뭐... 관리가 되었으면 좋겠어. 
        /// </summary>
        /// <param name="driverId"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("start")]
        public async Task<IActionResult> 근무시작(string driverId, [FromBody] 근무시작요청 req)
        {
            if (!현재기사확인(driverId)) return Forbid();
            if (req == null) return BadRequest();
            if (string.IsNullOrWhiteSpace(req.시작모드)) return BadRequest("시작모드가 필요합니다. (startMode required)");
            if (string.IsNullOrWhiteSpace(req.시작위치)) return BadRequest("시작위치가 필요합니다. (startLocation required)");

            var driver = await _db.용달기사.FirstOrDefaultAsync(x => x.기사Id == driverId);
            if (driver == null)
            {
                return NotFound("용달기사 정보를 찾을 수 없습니다.");
            }

            var shift = new 홍달.도메인.기사.기사근무
            {
                기사Id = driverId,
                시작모드 = req.시작모드,
                시작시각 = req.시작시각 ?? DateTime.UtcNow,
                시작위치 = req.시작위치,
                복귀지 = req.복귀지,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await using var tx = await _db.Database.BeginTransactionAsync();

            try
            {
                driver.운행상태 = 상태값.기사운행상태.운행중;
                driver.UpdatedAt = DateTime.UtcNow;

                _db.기사근무.Add(shift);
                await _db.SaveChangesAsync();
                await _driverWorkQueueStore.UpsertAsync(new DriverWorkQueueEntry(
                    driverId,
                    shift.Id,
                    shift.CreatedAt,
                    shift.시작모드,
                    shift.시작위치,
                    shift.복귀지));

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }

            await _dispatchRecommendationService.SendToDriverAsync(driverId);

            return CreatedAtAction(nameof(근무조회), new { driverId = driverId, id = shift.Id }, shift);
        }

        [HttpPost("stop")]
        public async Task<IActionResult> 근무종료(string driverId)
        {
            if (!현재기사확인(driverId)) return Forbid();

            var driver = await _db.용달기사.FirstOrDefaultAsync(x => x.기사Id == driverId);
            if (driver == null)
            {
                return NotFound("용달기사 정보를 찾을 수 없습니다.");
            }

            await using var tx = await _db.Database.BeginTransactionAsync();

            try
            {
                driver.운행상태 = 상태값.기사운행상태.대기;
                driver.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                await _driverWorkQueueStore.RemoveAsync(driverId);

                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }

            return NoContent();
        }
        /// <summary>
        /// 근무예약을 한다는 거는 이게 기사입장에서, 
        /// </summary>
        /// <param name="driverId"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("reserve")]
        public async Task<IActionResult> 근무예약(string driverId, [FromBody] 근무시작요청 req)
        {
            if (!현재기사확인(driverId)) return Forbid();
            // 예약 배차는 시작과 비슷하지만, 시작시각이 미래여야 합니다.
            if (req == null) return BadRequest();
            if (string.IsNullOrWhiteSpace(req.시작모드)) return BadRequest("시작모드가 필요합니다. (startMode required)");
            if (!req.시작시각.HasValue) return BadRequest("예약 배차에는 시작시각이 필요합니다. (startedAt is required for reserve)");
            if (req.시작시각.Value <= DateTime.UtcNow) return BadRequest("예약 배차의 시작시각은 현재보다 미래여야 합니다. (startedAt must be in the future for reserve)");

            var shift = new 홍달.도메인.기사.기사근무
            {
                기사Id = driverId,
                시작모드 = req.시작모드,
                시작시각 = req.시작시각,
                시작위치 = req.시작위치 ?? string.Empty,
                복귀지 = req.복귀지,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.기사근무.Add(shift);
            await _db.SaveChangesAsync();

            await _dispatchRecommendationService.SendToDriverAsync(driverId);

            return CreatedAtAction(nameof(근무조회), new { driverId = driverId, id = shift.Id }, shift);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> 근무조회(string driverId, long id)
        {
            if (!현재기사확인(driverId)) return Forbid();

            var s = await _db.기사근무.FindAsync(id);
            if (s == null || s.기사Id != driverId) return NotFound();
            return Ok(s);
        }

        [HttpPost("~/api/v1/drivers/{driverId}/dispatches/confirm")]
        public async Task<IActionResult> 배차확정(string driverId, [FromBody] 배차확정요청 request)
        {
            if (!현재기사확인(driverId)) return Forbid();
            if (request == null) return BadRequest("request body is required");
            if (string.IsNullOrWhiteSpace(request.의뢰Id)) return BadRequest("의뢰Id is required");

            var queue = await _db.배차대기.FirstOrDefaultAsync(x => x.의뢰Id == request.의뢰Id);
            if (queue == null)
            {
                return NotFound("배차대기 건을 찾을 수 없습니다.");
            }

            if (queue.상태 == 상태값.배차대기상태.확정)
            {
                var existingSettlement = await _db.기사월정산
                    .Where(x => x.기사Id == driverId && x.년도 == DateTime.UtcNow.Year && x.월 == DateTime.UtcNow.Month)
                    .FirstOrDefaultAsync();

                return Ok(new 배차확정응답
                {
                    의뢰Id = queue.의뢰Id,
                    배차상태 = queue.상태,
                    월배차건수 = existingSettlement?.배차건수 ?? 0,
                    월이용료 = existingSettlement?.이용료 ?? 0
                });
            }

            if (!string.Equals(queue.상태, 상태값.배차대기상태.대기, StringComparison.Ordinal))
            {
                return BadRequest("확정 가능한 배차 상태가 아닙니다.");
            }

            await using var tx = await _db.Database.BeginTransactionAsync();

            queue.상태 = 상태값.배차대기상태.확정;
            queue.UpdatedAt = DateTime.UtcNow;

            var settlement = await _driverMonthlySettlementService.배차확정반영Async(driverId, DateTime.UtcNow);

            await tx.CommitAsync();

            return Ok(new 배차확정응답
            {
                의뢰Id = queue.의뢰Id,
                배차상태 = queue.상태,
                월배차건수 = settlement.배차건수,
                월이용료 = settlement.이용료
            });
        }

        private bool 현재기사확인(string driverId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return !string.IsNullOrWhiteSpace(currentUserId)
                   && string.Equals(currentUserId, driverId, StringComparison.Ordinal);
        }
    }

    public class 근무시작요청
    {
        public string 시작모드 { get; set; } = "immediate"; // 즉시(immediate) | 예약(reserved)
        public DateTime? 시작시각 { get; set; }
        public string 시작위치 { get; set; } = string.Empty;
        public string? 복귀지 { get; set; }
    }

    public sealed class 배차확정요청
    {
        public string 의뢰Id { get; set; } = string.Empty;
    }

    public sealed class 배차확정응답
    {
        public string 의뢰Id { get; set; } = string.Empty;
        public string 배차상태 { get; set; } = string.Empty;
        public int 월배차건수 { get; set; }
        public decimal 월이용료 { get; set; }
    }
}
