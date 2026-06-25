using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using 홍달.Data;
using 홍달.Services;
using 홍달.도메인.공통;
using 홍달.도메인.화주;
using 홍달.도메인.배차;
using 홍달.도메인.운송;

namespace Hongdal.Controllers.Driver.Cargo
{
    [ApiController]
    [Route("api/v1/drivers/{driverId}/shipper-requests")]
    [Authorize(Roles = 역할명.기사)]
    public sealed class 용달기사운송의뢰Controller : ControllerBase
    {
        private readonly HongdalContext _db;
        private readonly IDispatchRecommendationService _dispatchRecommendationService;
        private readonly INationalDispatchRequestService _nationalDispatchRequestService;
        private readonly I기사월정산Service _driverMonthlySettlementService;
        private readonly IDriverRejectedRequestStore _rejectedRequestStore;
        private readonly IDriverCallScopeStore _callScopeStore;
        private readonly IDispatchAcceptanceLogStore _acceptanceLogStore;
        private readonly ILogger<용달기사운송의뢰Controller> _logger;

        public 용달기사운송의뢰Controller(
            HongdalContext db,
            IDispatchRecommendationService dispatchRecommendationService,
            INationalDispatchRequestService nationalDispatchRequestService,
            I기사월정산Service driverMonthlySettlementService,
            IDriverRejectedRequestStore rejectedRequestStore,
            IDriverCallScopeStore callScopeStore,
            IDispatchAcceptanceLogStore acceptanceLogStore,
            ILogger<용달기사운송의뢰Controller> logger)
        {
            _db = db;
            _dispatchRecommendationService = dispatchRecommendationService;
            _nationalDispatchRequestService = nationalDispatchRequestService;
            _driverMonthlySettlementService = driverMonthlySettlementService;
            _rejectedRequestStore = rejectedRequestStore;
            _callScopeStore = callScopeStore;
            _acceptanceLogStore = acceptanceLogStore;
            _logger = logger;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="driverId"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> 추천목록조회(
            string driverId,
            [FromQuery] decimal? latitude,
            [FromQuery] decimal? longitude,
            [FromQuery] decimal? radiusKm)
        {
            if (!현재기사확인(driverId)) return Forbid();

            DispatchRecommendationSearchCriteria? criteria = null;
            if (latitude.HasValue || longitude.HasValue || radiusKm.HasValue)
            {
                if (!latitude.HasValue || !longitude.HasValue || !radiusKm.HasValue)
                {
                    return BadRequest("latitude, longitude, radiusKm are required together.");
                }

                if (radiusKm.Value <= 0)
                {
                    return BadRequest("radiusKm must be greater than 0.");
                }

                criteria = new DispatchRecommendationSearchCriteria(latitude.Value, longitude.Value, radiusKm.Value);
            }

            var items = await _dispatchRecommendationService.GetRecommendationsAsync(driverId, criteria);
            return Ok(items.Select(x => new 기사용운송의뢰목록응답
            {
                의뢰Id = x.의뢰Id,
                화물종류 = x.화물종류,
                픽업지 = x.픽업지,
                하차지 = x.하차지,
                픽업거리Km = x.픽업거리Km,
                상태 = x.상태,
                배차상태 = x.배차상태
            }));
        }

        [HttpGet("search")]
        public Task<IActionResult> 추천검색(string driverId, [FromQuery] decimal latitude, [FromQuery] decimal longitude, [FromQuery] decimal radiusKm)
        {
            return 추천목록조회(driverId, latitude, longitude, radiusKm);
        }

        [HttpGet("national")]
        public async Task<IActionResult> 전국콜조회(string driverId)
        {
            if (!현재기사확인(driverId)) return Forbid();
            if (!await _callScopeStore.IsNationwideEnabledAsync(driverId))
            {
                return Forbid();
            }

            var items = await _nationalDispatchRequestService.GetNationwideRequestsAsync(driverId);
            return Ok(items.Select(x => new 기사용운송의뢰목록응답
            {
                의뢰Id = x.의뢰Id,
                화물종류 = x.화물종류,
                픽업지 = x.픽업지,
                하차지 = x.하차지,
                픽업거리Km = x.픽업거리Km,
                상태 = x.상태,
                배차상태 = x.배차상태
            }));
        }

        [HttpGet("call-scope")]
        public async Task<IActionResult> 콜범위조회(string driverId)
        {
            if (!현재기사확인(driverId)) return Forbid();

            var enabled = await _callScopeStore.IsNationwideEnabledAsync(driverId);
            return Ok(new 기사콜범위응답
            {
                DriverId = driverId,
                NationwideEnabled = enabled
            });
        }

        [HttpPut("call-scope")]
        public async Task<IActionResult> 콜범위수정(string driverId, [FromBody] 기사콜범위수정요청 request)
        {
            if (!현재기사확인(driverId)) return Forbid();
            if (request == null) return BadRequest("request body is required");

            await _callScopeStore.SetNationwideEnabledAsync(driverId, request.NationwideEnabled);
            return Ok(new 기사콜범위응답
            {
                DriverId = driverId,
                NationwideEnabled = request.NationwideEnabled
            });
        }

        [HttpGet("{requestId}")]
        public async Task<IActionResult> 의뢰상세조회(string driverId, string requestId)
        {
            if (!현재기사확인(driverId)) return Forbid();

            var entity = await _db.화주운송의뢰.AsNoTracking().FirstOrDefaultAsync(x => x.의뢰Id == requestId);
            if (entity == null)
            {
                return NotFound("의뢰를 찾을 수 없습니다.");
            }

            var queue = await _db.배차대기.AsNoTracking().FirstOrDefaultAsync(x => x.의뢰Id == requestId);

            return Ok(new 기사용운송의뢰상세응답
            {
                의뢰Id = entity.의뢰Id,
                화주Id = entity.화주Id,
                화물종류 = entity.화물종류,
                화물설명 = entity.화물설명,
                픽업지 = entity.픽업_도로명주소,
                픽업상세지 = entity.픽업_상세주소,
                픽업위도 = entity.픽업_위도,
                픽업경도 = entity.픽업_경도,
                하차지 = entity.하차_도로명주소,
                하차상세지 = entity.하차_상세주소,
                하차위도 = entity.하차_위도,
                하차경도 = entity.하차_경도,
                결제상태 = entity.결제상태,
                의뢰상태 = entity.상태,
                배차상태 = entity.배차상태,
                결제수단 = entity.결제수단,
                결제예정금액 = entity.결제예정금액,
                배차대기상태 = queue?.상태,
                생성일시 = entity.CreatedAt,
                수정일시 = entity.UpdatedAt
            });
        }

        [HttpPost("{requestId}/accept")]
        public async Task<IActionResult> 수락(string driverId, string requestId)
        {
            if (!현재기사확인(driverId)) return Forbid();

            var queue = await _db.배차대기.FirstOrDefaultAsync(x => x.의뢰Id == requestId);
            if (queue == null)
            {
                return NotFound("배차대기 건을 찾을 수 없습니다.");
            }

            if (queue.상태 == 상태값.배차대기상태.확정)
            {
                var existingSettlement = await _db.기사월정산
                    .Where(x => x.기사Id == driverId && x.년도 == DateTime.UtcNow.Year && x.월 == DateTime.UtcNow.Month)
                    .FirstOrDefaultAsync();

                return Ok(new 기사용수락응답
                {
                    의뢰Id = queue.의뢰Id,
                    배차상태 = queue.상태,
                    월배차건수 = existingSettlement?.배차건수 ?? 0,
                    월이용료 = existingSettlement?.이용료 ?? 0
                });
            }

            if (!string.Equals(queue.상태, 상태값.배차대기상태.대기, StringComparison.Ordinal))
            {
                return BadRequest("수락 가능한 배차 상태가 아닙니다.");
            }

            var request = await _db.화주운송의뢰.FirstOrDefaultAsync(x => x.의뢰Id == requestId);
            if (request == null)
            {
                return NotFound("의뢰를 찾을 수 없습니다.");
            }

            await using var tx = await _db.Database.BeginTransactionAsync();

            queue.상태 = 상태값.배차대기상태.확정;
            queue.UpdatedAt = DateTime.UtcNow;

            request.배차상태 = 상태값.배차상태.매칭중;
            request.결제상태 = 상태값.결제상태.결제완료;
            request.UpdatedAt = DateTime.UtcNow;

            _db.운송이벤트.Add(new 운송이벤트
            {
                의뢰Id = requestId,
                이벤트타입 = "driver.accepted",
                이벤트시각 = DateTime.UtcNow,
                메타데이터 = System.Text.Json.JsonSerializer.Serialize(new
                {
                    DriverId = driverId,
                    ShipperId = request.화주Id,
                    QueueStatus = queue.상태,
                    DispatchStatus = request.배차상태,
                    PaymentStatus = request.결제상태
                })
            });

            await _db.SaveChangesAsync();
            var settlement = await _driverMonthlySettlementService.배차확정반영Async(driverId, DateTime.UtcNow);

            await tx.CommitAsync();

            try
            {
                await _acceptanceLogStore.AppendAsync(new DispatchAcceptanceLogEntry(
                    DriverId: driverId,
                    ShipperId: request.화주Id,
                    RequestId: requestId,
                    AcceptedAtUtc: DateTime.UtcNow,
                    QueueStatus: queue.상태,
                    DispatchStatus: request.배차상태,
                    PaymentStatus: request.결제상태));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "MongoDB acceptance log write failed for request {RequestId} and driver {DriverId}.", requestId, driverId);
            }

            return Ok(new 기사용수락응답
            {
                의뢰Id = queue.의뢰Id,
                배차상태 = queue.상태,
                월배차건수 = settlement.배차건수,
                월이용료 = settlement.이용료
            });
        }

        [HttpPost("{requestId}/reject")]
        public async Task<IActionResult> 거절(string driverId, string requestId)
        {
            if (!현재기사확인(driverId)) return Forbid();

            var queue = await _db.배차대기.AsNoTracking().FirstOrDefaultAsync(x => x.의뢰Id == requestId);
            if (queue == null)
            {
                return NotFound("배차대기 건을 찾을 수 없습니다.");
            }

            await _rejectedRequestStore.RejectAsync(driverId, requestId);
            return NoContent();
        }

        private bool 현재기사확인(string driverId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return !string.IsNullOrWhiteSpace(currentUserId)
                   && string.Equals(currentUserId, driverId, StringComparison.Ordinal);
        }
    }

    public sealed class 기사용운송의뢰목록응답
    {
        public string 의뢰Id { get; set; } = string.Empty;
        public string 화물종류 { get; set; } = string.Empty;
        public string 픽업지 { get; set; } = string.Empty;
        public string 하차지 { get; set; } = string.Empty;
        public decimal? 픽업거리Km { get; set; }
        public string 상태 { get; set; } = string.Empty;
        public string 배차상태 { get; set; } = string.Empty;
    }

    public sealed class 기사용운송의뢰상세응답
    {
        public string 의뢰Id { get; set; } = string.Empty;
        public string 화주Id { get; set; } = string.Empty;
        public string 화물종류 { get; set; } = string.Empty;
        public string 화물설명 { get; set; } = string.Empty;
        public string 픽업지 { get; set; } = string.Empty;
        public string 픽업상세지 { get; set; } = string.Empty;
        public decimal? 픽업위도 { get; set; }
        public decimal? 픽업경도 { get; set; }
        public string 하차지 { get; set; } = string.Empty;
        public string 하차상세지 { get; set; } = string.Empty;
        public decimal? 하차위도 { get; set; }
        public decimal? 하차경도 { get; set; }
        public string 결제상태 { get; set; } = string.Empty;
        public string 의뢰상태 { get; set; } = string.Empty;
        public string 배차상태 { get; set; } = string.Empty;
        public string 결제수단 { get; set; } = string.Empty;
        public int? 결제예정금액 { get; set; }
        public string? 배차대기상태 { get; set; }
        public DateTime 생성일시 { get; set; }
        public DateTime 수정일시 { get; set; }
    }

    public sealed class 기사콜범위수정요청
    {
        public bool NationwideEnabled { get; set; }
    }

    public sealed class 기사콜범위응답
    {
        public string DriverId { get; set; } = string.Empty;
        public bool NationwideEnabled { get; set; }
    }

    public sealed class 기사용수락응답
    {
        public string 의뢰Id { get; set; } = string.Empty;
        public string 배차상태 { get; set; } = string.Empty;
        public int 월배차건수 { get; set; }
        public decimal 월이용료 { get; set; }
    }
}
