using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Hongdal.Contracts.Driver.Home;
using 홍달.Data;
using 홍달.Services.Options;
using 홍달.Services.Storage.Local;
using 홍달.Services.Dispatch.Request;
using 홍달.도메인.공통;

namespace Hongdal.Controllers.Driver.Profile00
{
    [ApiController]
    [Authorize(Roles = 역할명.기사)]
    [Route("api/v1/driver/home")]
    public sealed class 기사홈Controller : ControllerBase
    {
        private readonly HongdalContext _db;
        private readonly IDriverPushTokenStore _pushTokenStore;
        private readonly IDriverCallScopeStore _callScopeStore;
        private readonly INationalDispatchRequestService _nationalDispatchRequestService;
        private readonly I배차추천Service _dispatchRecommendationService;
        private readonly 기사이용료정책Options _policy;

        public 기사홈Controller(
            HongdalContext db,
            IDriverPushTokenStore pushTokenStore,
            IDriverCallScopeStore callScopeStore,
            INationalDispatchRequestService nationalDispatchRequestService,
            I배차추천Service dispatchRecommendationService,
            IOptions<기사이용료정책Options> policy)
        {
            _db = db;
            _pushTokenStore = pushTokenStore;
            _callScopeStore = callScopeStore;
            _nationalDispatchRequestService = nationalDispatchRequestService;
            _dispatchRecommendationService = dispatchRecommendationService;
            _policy = policy.Value;
        }

        [HttpGet]
        public async Task<IActionResult> 조회()
        {
            var driverId = 현재기사Id();
            var driver = await _db.용달기사.AsNoTracking().FirstOrDefaultAsync(x => x.기사Id == driverId);
            if (driver == null)
            {
                return NotFound("용달기사 정보를 찾을 수 없습니다.");
            }

            var currentShift = await _db.기사근무.AsNoTracking()
                .Where(x => x.기사Id == driverId)
                .OrderByDescending(x => x.UpdatedAt)
                .ThenByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            var recommendationItems = await _dispatchRecommendationService.GetRecommendationsAsync(driverId);
            var nationalItems = await _nationalDispatchRequestService.GetNationwideRequestsAsync(driverId);
            var currentMonth = DateTime.UtcNow;
            var settlement = await _db.기사월정산.AsNoTracking()
                .FirstOrDefaultAsync(x => x.기사Id == driverId && x.년도 == currentMonth.Year && x.월 == currentMonth.Month);

            var pushToken = await _pushTokenStore.GetAsync(driverId);
            var nationwideEnabled = await _callScopeStore.IsNationwideEnabledAsync(driverId);

            var usageFee = settlement?.이용료 ?? 0m;
            var monthlyCap = _policy.무료배차 ? 0m : _policy.추가이용료;

            return Ok(new 기사홈요약응답
            {
                DriverId = driverId,
                기사명 = driver.기사명,
                운행상태 = driver.운행상태,
                현재근무Id = currentShift?.Id,
                운행시작시각 = currentShift?.시작시각,
                추천콜수 = recommendationItems.Count,
                적합추천콜수 = recommendationItems.Count(x => x.차량적합여부),
                진행중운송수 = await _db.배송_운송.AsNoTracking().CountAsync(x => x.기사_운송자 == driver.기사명 && x.상태 != "인수완료"),
                이번달배차건수 = settlement?.배차건수 ?? 0,
                이번달이용료 = usageFee,
                이번달이용료상한 = monthlyCap,
                남은이용료 = Math.Max(0, monthlyCap - usageFee),
                정산결제완료 = settlement?.결제완료 ?? false,
                푸시토큰등록됨 = !string.IsNullOrWhiteSpace(pushToken),
                전국콜사용가능 = nationwideEnabled || nationalItems.Count > 0
            });
        }

        private string 현재기사Id()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? throw new InvalidOperationException("기사 인증 정보가 없습니다.");
        }
    }
}
