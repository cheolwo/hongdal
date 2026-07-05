using Microsoft.Extensions.Options;
using Hongdal.Contracts.Driver.Home;
using 홍달.Services.Options;

namespace Hongdal.Application.Driver.Home;

public sealed class 기사홈조회QueryHandler : IRequestHandler<기사홈조회Query, 기사홈요약응답?>
{
    private readonly HongdalContext _db;
    private readonly IDriverPushTokenStore _pushTokenStore;
    private readonly IDriverCallScopeStore _callScopeStore;
    private readonly INationalDispatchRequestService _nationalDispatchRequestService;
    private readonly I배차추천Service _dispatchRecommendationService;
    private readonly 기사이용료정책Options _policy;

    public 기사홈조회QueryHandler(
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

    public async Task<기사홈요약응답?> Handle(기사홈조회Query request, CancellationToken cancellationToken)
    {
        var driver = await _db.용달기사
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.기사Id == request.기사Id, cancellationToken);

        if (driver == null)
        {
            return null;
        }

        var currentShift = await _db.기사근무
            .AsNoTracking()
            .Where(x => x.기사Id == request.기사Id)
            .OrderByDescending(x => x.UpdatedAt)
            .ThenByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var recommendationItems = await _dispatchRecommendationService.GetRecommendationsAsync(request.기사Id);
        var nationalItems = await _nationalDispatchRequestService.GetNationwideRequestsAsync(request.기사Id, cancellationToken);
        var currentMonth = DateTime.UtcNow;
        var settlement = await _db.기사월정산
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.기사Id == request.기사Id && x.년도 == currentMonth.Year && x.월 == currentMonth.Month,
                cancellationToken);

        var pushToken = await _pushTokenStore.GetAsync(request.기사Id, cancellationToken);
        var nationwideEnabled = await _callScopeStore.IsNationwideEnabledAsync(request.기사Id, cancellationToken);
        var inProgressTransportCount = await _db.배송_운송
            .AsNoTracking()
            .CountAsync(x => x.기사_운송자 == driver.기사명 && x.상태 != "인수완료", cancellationToken);

        var usageFee = settlement?.이용료 ?? 0m;
        var monthlyCap = _policy.무료배차 ? 0m : _policy.추가이용료;

        return new 기사홈요약응답
        {
            DriverId = request.기사Id,
            기사명 = driver.기사명,
            운행상태 = driver.운행상태,
            현재근무Id = currentShift?.Id,
            운행시작시각 = currentShift?.시작시각,
            추천콜수 = recommendationItems.Count,
            적합추천콜수 = recommendationItems.Count(x => x.차량적합여부),
            진행중운송수 = inProgressTransportCount,
            이번달배차건수 = settlement?.배차건수 ?? 0,
            이번달이용료 = usageFee,
            이번달이용료상한 = monthlyCap,
            남은이용료 = Math.Max(0, monthlyCap - usageFee),
            정산결제완료 = settlement?.결제완료 ?? false,
            푸시토큰등록됨 = !string.IsNullOrWhiteSpace(pushToken),
            전국콜사용가능 = nationwideEnabled || nationalItems.Count > 0
        };
    }
}
