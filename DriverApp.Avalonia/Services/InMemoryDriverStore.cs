using DriverApp.Avalonia.Models;

namespace DriverApp.Avalonia.Services;

public sealed class InMemoryDriverStore
{
    private readonly List<DriverProfileItem> _profiles = new()
    {
        new DriverProfileItem
        {
            DriverId = "DRV-001",
            DriverName = "홍길동",
            PhoneNumber = "010-1111-2222",
            VehicleName = "1톤 탑차",
            MainArea = "서울 / 경기",
            Status = "활동중"
        }
    };

    private readonly List<DriverRequestItem> _requests = new()
    {
        new DriverRequestItem
        {
            RequestId = "REQ-2026-001",
            CargoType = "가구",
            Pickup = "서울 강서구",
            Dropoff = "경기 수원시",
            RecommendedScore = 95,
            Reason = "거리와 수익성이 우수합니다."
        },
        new DriverRequestItem
        {
            RequestId = "REQ-2026-002",
            CargoType = "냉장식품",
            Pickup = "인천 연수구",
            Dropoff = "서울 송파구",
            RecommendedScore = 88,
            Reason = "즉시 배차가 가능한 구간입니다."
        }
    };

    private readonly List<DispatchPlanItem> _dispatchPlans = new();
    private readonly List<RecommendationItem> _recommendations = new();
    private readonly List<DriverWorkItem> _workHistory = new();

    public DriverProfileItem? GetProfile(string driverId) => _profiles.FirstOrDefault(x => x.DriverId == driverId);

    public IReadOnlyList<DriverRequestItem> GetRecommendedRequests() => _requests.OrderByDescending(x => x.RecommendedScore).ToList();

    public IReadOnlyList<DriverWorkItem> GetWorkHistory() => _workHistory.ToList();

    public IReadOnlyList<RecommendationItem> GetRecommendations() => _recommendations.ToList();

    public IReadOnlyList<DispatchPlanItem> GetDispatchPlans() => _dispatchPlans.ToList();

    public void SeedSession(string driverId)
    {
        if (_recommendations.Count == 0)
        {
            _recommendations.Add(new RecommendationItem
            {
                Title = "근처 추천 콜",
                Description = "현재 위치 기반으로 추천되는 의뢰입니다.",
                Priority = "상"
            });
        }

        if (_workHistory.Count == 0)
        {
            _workHistory.Add(new DriverWorkItem
            {
                WorkId = "WORK-001",
                Action = "근무 시작",
                OccurredAt = DateTime.Now.AddHours(-2),
                Note = "서울 강서구"
            });
        }
    }
}
