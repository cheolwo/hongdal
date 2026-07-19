using Ssalddel.Contracts.Admin.Dispatch;
using Ssalddel.Contracts.Food;
using Ssalddel.Services.Food;
using 살뜰.Services.Dispatch.Recommendation;
using 살뜰.Services.Storage.Local;

namespace 살뜰.Services.Dispatch.Coordination;

public interface IFoodDeliveryDispatchAIReviewService
{
    Task<FoodDeliveryDispatchAIReviewWorkspaceDto> GetWorkspaceAsync(CancellationToken cancellationToken = default);

    Task<FoodDeliveryDispatchAIReviewDecisionResponse> RecordDecisionAsync(
        FoodDeliveryDispatchAIReviewDecisionRequest request,
        string? adminUser,
        CancellationToken cancellationToken = default);
}

public sealed class FoodDeliveryDispatchAIReviewService : IFoodDeliveryDispatchAIReviewService
{
    private const decimal DefaultMaxDeliveryMinutesAfterReady = 42m;
    private static readonly FoodDeliveryReviewScope PrimaryReviewScope = new("bjd-sigungu:11260", "중랑구", "주요 배달권");
    private static readonly IReadOnlyList<FoodDeliveryReviewScope> AdjacentReviewScopes =
    [
        new("bjd-sigungu:11230", "동대문구", "인접 배달권"),
        new("bjd-sigungu:11215", "광진구", "인접 배달권"),
        new("bjd-sigungu:11350", "노원구", "인접 배달권"),
        new("bjd-sigungu:41310", "구리시", "인접 배달권")
    ];
    private static readonly IReadOnlyList<FoodDeliveryReviewScope> AllReviewScopes =
        [PrimaryReviewScope, ..AdjacentReviewScopes];

    private readonly ISsalddelFoodOrderStore _orderStore;
    private readonly I음식멀티배차조합Service _bundleService;
    private readonly I배달권실행공간Store _deliveryScopeStore;
    private readonly IDriverLocationStore _driverLocationStore;
    private readonly I배차AI판단사례LedgerStore _judgmentLedgerStore;

    public FoodDeliveryDispatchAIReviewService(
        ISsalddelFoodOrderStore orderStore,
        I음식멀티배차조합Service bundleService,
        I배달권실행공간Store deliveryScopeStore,
        IDriverLocationStore driverLocationStore,
        I배차AI판단사례LedgerStore judgmentLedgerStore)
    {
        _orderStore = orderStore;
        _bundleService = bundleService;
        _deliveryScopeStore = deliveryScopeStore;
        _driverLocationStore = driverLocationStore;
        _judgmentLedgerStore = judgmentLedgerStore;
    }

    public async Task<FoodDeliveryDispatchAIReviewWorkspaceDto> GetWorkspaceAsync(CancellationToken cancellationToken = default)
    {
        var rawOrders = _orderStore.GetOrders().Items
            .Where(IsReviewTarget)
            .OrderBy(x => x.조리예상완료시각Utc ?? x.CreatedAt)
            .Take(12)
            .ToList();

        var useJungnangScenario = rawOrders.Count == 0 || rawOrders.All(IsDefaultFoodOrderSample);
        var orders = useJungnangScenario
            ? CreateJungnangScenarioOrders()
            : rawOrders.Select(EnrichOrder).ToList();
        var source = useJungnangScenario ? "jungnang-scope-sample" : "actual";

        var drivers = await BuildDriversAsync(cancellationToken);
        if (drivers.Count == 0)
        {
            drivers = CreateSampleDrivers();
            source = source == "actual" ? "actual-with-sample-drivers" : source;
        }

        var bundles = BuildBundles(orders, drivers);
        var assignments = BuildAssignments(bundles);

        return new FoodDeliveryDispatchAIReviewWorkspaceDto
        {
            GeneratedAt = DateTimeOffset.UtcNow,
            Source = source,
            PrimaryDeliveryScopeKey = PrimaryReviewScope.Key,
            PrimaryDeliveryScopeName = PrimaryReviewScope.Name,
            AdjacentDeliveryScopeKeys = AdjacentReviewScopes.Select(x => x.Key).ToList(),
            AdjacentDeliveryScopeNames = AdjacentReviewScopes.Select(x => x.Name).ToList(),
            Orders = orders.Select(ToDto).ToList(),
            Drivers = drivers,
            Bundles = bundles,
            Assignments = assignments,
            Notes =
            [
                "음식점 주문은 화물 운송과 분리된 RAG 출처(admin-food-delivery-ai-review)로 저장합니다.",
                "현재 검토 기준은 중랑구를 주요 배달권으로 두고 동대문구, 광진구, 노원구, 구리시를 인접 배달권으로 둡니다.",
                "AI 판단은 1단계 고객 전달권 묶음 판단, 2단계 F드라이버 배정 판단으로 나눠 표시합니다.",
                "배차 확정 적용은 별도 실행 서비스 연결 단계에서 처리합니다."
            ]
        };
    }

    public async Task<FoodDeliveryDispatchAIReviewDecisionResponse> RecordDecisionAsync(
        FoodDeliveryDispatchAIReviewDecisionRequest request,
        string? adminUser,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var orderNos = request.OrderNos
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();
        if (orderNos.Count == 0)
        {
            throw new ArgumentException("판정할 음식 주문을 하나 이상 선택해야 합니다.");
        }

        if (string.IsNullOrWhiteSpace(request.DriverId))
        {
            throw new ArgumentException("판정할 배달 기사를 선택해야 합니다.");
        }

        var accepted = request.Accepted ? "승인" : "보류";
        var manual = request.ManualBundle ? "수동 묶음" : "AI 제안";
        var bundleLabel = string.IsNullOrWhiteSpace(request.BundleKey)
            ? string.Join("+", orderNos)
            : request.BundleKey.Trim();
        var note = string.IsNullOrWhiteSpace(request.AdminNote)
            ? $"{manual} {bundleLabel}을 {accepted}했습니다."
            : request.AdminNote.Trim();

        var item = await _judgmentLedgerStore.CreateAsync(
            new DispatchAIJudgmentCaseCreateRequest
            {
                Title = $"{manual} 음식배달 배차 판단: {bundleLabel}",
                RelatedOS = "음식 배달 OS",
                Keywords =
                [
                    "음식배달OS",
                    "음식점주문",
                    "F드라이버",
                    "운영자판정",
                    request.ManualBundle ? "수동묶음" : "AI제안",
                    request.Accepted ? "승인" : "보류",
                    "조리완료",
                    "픽업",
                    "고객전달",
                    "음식멀티배차",
                    "중랑구",
                    "인접배달권"
                ],
                SituationSummary = $"운영자가 음식배달 AI 검토 화면에서 중랑구 주요 배달권과 동대문구·광진구·노원구·구리시 인접 배달권 기준으로 주문 {string.Join(", ", orderNos)}와 배달 기사 {request.DriverId.Trim()}의 위치, 묶음, 조리 완료 기준 시간을 확인했습니다.",
                JudgmentSummary = note,
                UserDecision = request.DecisionType,
                BalancedDecision = $"{manual} {accepted}",
                Source = "admin-food-delivery-ai-review",
                Active = true
            },
            adminUser,
            cancellationToken);

        return new FoodDeliveryDispatchAIReviewDecisionResponse
        {
            CaseId = item.CaseId,
            Message = $"{item.CaseId} 음식배달 판단 사례를 RAG 저장소에 저장했습니다."
        };
    }

    private List<FoodDeliveryDispatchAIReviewBundleDto> BuildBundles(
        IReadOnlyList<FoodDeliveryOrderReviewItem> orders,
        IReadOnlyList<FoodDeliveryDispatchAIReviewDriverDto> drivers)
    {
        var jobs = orders.Select(ToJob).ToArray();
        if (jobs.Length == 0)
        {
            return [];
        }

        var candidates = _bundleService.조합생성(
            new 멀티배차조합요청(
                jobs,
                최대묶음크기: 2,
                최대조합수: 24,
                좌표근사총거리상한Km: 6m,
                같은배달권멀티허용: true,
                인접배달권멀티허용: true,
                비인접배달권멀티허용: false));

        var suggestedKey = candidates
            .Where(x => x.조합가능여부)
            .OrderByDescending(x => x.배차묶음유형 == "멀티배차")
            .ThenByDescending(x => x.조합점수)
            .Select(x => x.조합키)
            .FirstOrDefault();

        return candidates
            .Take(24)
            .Select(candidate => ToBundleDto(candidate, drivers, string.Equals(candidate.조합키, suggestedKey, StringComparison.Ordinal)))
            .ToList();
    }

    private static List<FoodDeliveryDispatchAIReviewAssignmentDto> BuildAssignments(
        IReadOnlyList<FoodDeliveryDispatchAIReviewBundleDto> bundles)
    {
        var selected = bundles.FirstOrDefault(x => x.IsAISuggested && !string.IsNullOrWhiteSpace(x.SuggestedDriverId));
        if (selected is null)
        {
            return [];
        }

        return selected.OrderNos
            .Select((orderNo, index) => new FoodDeliveryDispatchAIReviewAssignmentDto
            {
                OrderNo = orderNo,
                DriverId = selected.SuggestedDriverId!,
                Order = index + 1,
                Score = selected.Score,
                Reason = $"{selected.BundleType} 후보를 {selected.SuggestedDriverId} 기사에게 추천합니다.",
                Badges = selected.Badges.ToList()
            })
            .ToList();
    }

    private async Task<List<FoodDeliveryDispatchAIReviewDriverDto>> BuildDriversAsync(CancellationToken cancellationToken)
    {
        var spaces = await _deliveryScopeStore.SnapshotAsync(cancellationToken);
        var drivers = new List<FoodDeliveryDispatchAIReviewDriverDto>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var item in spaces.SelectMany(x => x.운행중기사Ids.Select(driverId => (Space: x, DriverId: driverId))))
        {
            if (!seen.Add(item.DriverId))
            {
                continue;
            }

            DriverLocationSnapshot? location = null;
            if (_driverLocationStore.TryGetLatest(item.DriverId, out var snapshot))
            {
                location = snapshot;
            }

            var point = location is null ? null : new 배차경로좌표(location.Latitude, location.Longitude);
            var scope = ResolveScopeByKey(item.Space.배달권키)
                        ?? (point is null
                            ? new FoodDeliveryReviewScope(item.Space.배달권키, item.Space.배달권키, "기타 배달권")
                            : ResolveFoodDeliveryScope(null, point));
            drivers.Add(new FoodDeliveryDispatchAIReviewDriverDto
            {
                DriverId = item.DriverId,
                DriverName = item.DriverId,
                DrivingStatus = location?.DrivingStatus ?? "운행중",
                Latitude = location?.Latitude,
                Longitude = location?.Longitude,
                DeliveryScopeKey = scope.Key,
                DeliveryScopeName = scope.Name,
                DeliveryScopeRole = scope.Role,
                LastLocationReceivedAtUtc = location?.ReceivedAtUtc
            });
        }

        return drivers;
    }

    private static FoodDeliveryDispatchAIReviewBundleDto ToBundleDto(
        멀티배차조합후보 candidate,
        IReadOnlyList<FoodDeliveryDispatchAIReviewDriverDto> drivers,
        bool isSuggested)
    {
        var suggestedDriverId = isSuggested ? ResolveNearestDriver(candidate, drivers) : null;
        return new FoodDeliveryDispatchAIReviewBundleDto
        {
            BundleKey = candidate.조합키,
            BundleType = candidate.배차묶음유형,
            OrderNos = candidate.의뢰Ids.ToList(),
            BundleSize = candidate.의뢰Ids.Count,
            IsBundleAvailable = candidate.조합가능여부,
            IsAISuggested = isSuggested,
            SuggestedDriverId = suggestedDriverId,
            Score = candidate.조합점수,
            PickupDistanceKm = candidate.상차지간거리Km,
            DropoffDistanceKm = candidate.하차지간거리Km,
            ExpectedRouteDistanceKm = candidate.묶음내예상거리Km,
            Badges = candidate.배지.ToList(),
            Warnings = candidate.경고.ToList(),
            ExclusionReasons = candidate.제외사유?.ToList() ?? [],
            Reason = BuildBundleReason(candidate, suggestedDriverId),
            BundleDecisionSummary = BuildBundleDecisionSummary(candidate),
            DriverAssignmentDecisionSummary = BuildDriverAssignmentDecisionSummary(candidate, drivers, suggestedDriverId)
        };
    }

    private static string BuildBundleReason(멀티배차조합후보 candidate, string? suggestedDriverId)
    {
        if (!candidate.조합가능여부)
        {
            return candidate.제외사유 is { Count: > 0 }
                ? $"음식배달 묶음 조건을 만족하지 않습니다. {string.Join(" ", candidate.제외사유)}"
                : "음식배달 묶음 조건을 만족하지 않습니다.";
        }

        var driverText = string.IsNullOrWhiteSpace(suggestedDriverId)
            ? "배달 기사 후보 미정"
            : $"배달 기사 {suggestedDriverId} 후보";
        var distanceText = candidate.묶음내예상거리Km.HasValue
            ? $"예상 묶음 거리 {candidate.묶음내예상거리Km:0.##}km"
            : "예상 묶음 거리 미정";
        return $"{candidate.배차묶음유형} 후보입니다. {distanceText}, 점수 {candidate.조합점수:0.##}, {driverText}.";
    }

    private static string BuildBundleDecisionSummary(멀티배차조합후보 candidate)
    {
        var customerScopes = candidate.작업목록
            .Select(x => ResolveScopeDisplay(x.하차배달권키))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var scopeText = customerScopes.Length == 0 ? "고객 전달권 미정" : string.Join(", ", customerScopes);
        var distanceText = candidate.묶음내예상거리Km.HasValue
            ? $"{candidate.묶음내예상거리Km:0.##}km"
            : "미정";

        return candidate.조합가능여부
            ? $"1단계 AI: 고객 전달권 {scopeText}을 중랑구 주요/인접 배달권 기준으로 검토했고, 묶음 예상거리 {distanceText} 조건에서 {candidate.배차묶음유형} 후보로 판단했습니다."
            : $"1단계 AI: 고객 전달권 {scopeText}은 현재 중랑구 주요/인접 배달권 묶음 조건에 맞지 않아 제외 후보로 판단했습니다.";
    }

    private static string BuildDriverAssignmentDecisionSummary(
        멀티배차조합후보 candidate,
        IReadOnlyList<FoodDeliveryDispatchAIReviewDriverDto> drivers,
        string? suggestedDriverId)
    {
        if (string.IsNullOrWhiteSpace(suggestedDriverId))
        {
            return "2단계 AI: 아직 추천할 F드라이버를 고르지 못했습니다.";
        }

        var driver = drivers.FirstOrDefault(x => string.Equals(x.DriverId, suggestedDriverId, StringComparison.Ordinal));
        var driverName = driver?.DriverName ?? suggestedDriverId;
        var driverScope = string.IsNullOrWhiteSpace(driver?.DeliveryScopeName)
            ? "배달권 미정"
            : $"{driver.DeliveryScopeName}({driver.DeliveryScopeRole})";

        var firstPickup = candidate.작업목록.FirstOrDefault()?.픽업주소 ?? "픽업지 미정";
        return $"2단계 AI: {driverName}의 현재 권역 {driverScope}과 첫 픽업지 {firstPickup} 접근성을 기준으로 이 기사에게 배정하는 판단을 만들었습니다.";
    }

    private static string? ResolveNearestDriver(
        멀티배차조합후보 candidate,
        IReadOnlyList<FoodDeliveryDispatchAIReviewDriverDto> drivers)
    {
        var pickup = candidate.작업목록.FirstOrDefault()?.픽업좌표;
        if (pickup is null)
        {
            return drivers.FirstOrDefault()?.DriverId;
        }

        return drivers
            .Where(x => x.Latitude.HasValue && x.Longitude.HasValue)
            .Select(x => new
            {
                x.DriverId,
                Distance = CalculateDistanceKm(pickup, new 배차경로좌표(x.Latitude!.Value, x.Longitude!.Value))
            })
            .OrderBy(x => x.Distance)
            .Select(x => x.DriverId)
            .FirstOrDefault()
            ?? drivers.FirstOrDefault()?.DriverId;
    }

    private static 픽업하차경로작업 ToJob(FoodDeliveryOrderReviewItem item)
        => new(
            item.OrderNo,
            item.RestaurantAddress,
            CreatePoint(item.RestaurantLatitude, item.RestaurantLongitude),
            item.PickupReadyAtUtc?.AddMinutes(10),
            item.CustomerAddress,
            CreatePoint(item.CustomerLatitude, item.CustomerLongitude),
            item.PickupReadyAtUtc?.AddMinutes((double)item.MaxDeliveryMinutesAfterReady),
            item.PickupReadyAtUtc,
            item.MaxDeliveryMinutesAfterReady,
            item.PickupScopeKey,
            item.DropoffScopeKey);

    private static FoodDeliveryDispatchAIReviewOrderDto ToDto(FoodDeliveryOrderReviewItem item)
        => new()
        {
            OrderNo = item.OrderNo,
            RestaurantId = item.RestaurantId,
            RestaurantName = item.RestaurantName,
            MenuSummary = item.MenuSummary,
            OrderAmount = item.OrderAmount,
            OrderStatus = item.OrderStatus,
            DispatchStatus = item.DispatchStatus,
            RestaurantAddress = item.RestaurantAddress,
            RestaurantLatitude = item.RestaurantLatitude,
            RestaurantLongitude = item.RestaurantLongitude,
            CustomerAddress = item.CustomerAddress,
            CustomerLatitude = item.CustomerLatitude,
            CustomerLongitude = item.CustomerLongitude,
            PickupReadyAtUtc = item.PickupReadyAtUtc,
            MaxDeliveryMinutesAfterReady = item.MaxDeliveryMinutesAfterReady,
            PickupScopeKey = item.PickupScopeKey,
            PickupScopeName = item.PickupScopeName,
            PickupScopeRole = item.PickupScopeRole,
            DropoffScopeKey = item.DropoffScopeKey,
            DropoffScopeName = item.DropoffScopeName,
            DropoffScopeRole = item.DropoffScopeRole
        };

    private static FoodDeliveryOrderReviewItem EnrichOrder(음식주문응답 order)
    {
        var restaurant = ResolveRestaurant(order);
        var customer = ResolveCustomer(order, restaurant.Latitude, restaurant.Longitude);
        var pickupScope = ResolveFoodDeliveryScope(
            restaurant.Address,
            new 배차경로좌표(restaurant.Latitude, restaurant.Longitude));
        var dropoffScope = ResolveFoodDeliveryScope(
            customer.Address,
            new 배차경로좌표(customer.Latitude, customer.Longitude));

        return new FoodDeliveryOrderReviewItem(
            order.주문번호,
            order.음식점Id,
            restaurant.Name,
            FoodOrderSampleData.BuildMenuSummary(order.상품목록),
            order.총주문금액,
            order.상태,
            order.배차상태,
            restaurant.Address,
            restaurant.Latitude,
            restaurant.Longitude,
            customer.Address,
            customer.Latitude,
            customer.Longitude,
            order.조리예상완료시각Utc ?? order.음식점수락시각Utc?.AddMinutes(20) ?? order.CreatedAt.AddMinutes(20),
            DefaultMaxDeliveryMinutesAfterReady,
            pickupScope.Key,
            pickupScope.Name,
            pickupScope.Role,
            dropoffScope.Key,
            dropoffScope.Name,
            dropoffScope.Role);
    }

    private static (string Name, string Address, decimal Latitude, decimal Longitude) ResolveRestaurant(음식주문응답 order)
    {
        var sample = order.음식점Id switch
        {
            101 => ("살뜰분식 면목점", "서울특별시 중랑구 면목로 352", 37.5888m, 127.0874m),
            102 => ("살뜰도시락 상봉점", "서울특별시 중랑구 망우로 353", 37.5969m, 127.0851m),
            _ => ("살뜰음식점 중랑점", "서울특별시 중랑구 봉화산로 179", 37.6066m, 127.0927m)
        };

        return (
            string.IsNullOrWhiteSpace(order.음식점명) ? sample.Item1 : order.음식점명.Trim(),
            string.IsNullOrWhiteSpace(order.음식점주소) ? sample.Item2 : order.음식점주소.Trim(),
            order.음식점위도 ?? sample.Item3,
            order.음식점경도 ?? sample.Item4);
    }

    private static (string Address, decimal Latitude, decimal Longitude) ResolveCustomer(
        음식주문응답 order,
        decimal restaurantLatitude,
        decimal restaurantLongitude)
    {
        var address = string.IsNullOrWhiteSpace(order.수령인정보.주소)
            ? "고객 주소 미정"
            : order.수령인정보.주소.Trim();

        if (IsDefaultFoodOrderSample(order))
        {
            return order.주문번호 switch
            {
                "FOOD-20260701-001" => ("서울특별시 중랑구 면목동 15-1", 37.5867m, 127.0885m),
                "FOOD-20260701-002" => ("서울특별시 동대문구 장안동 335-1", 37.5684m, 127.0717m),
                "FOOD-20260701-003" => ("서울특별시 광진구 중곡동 130-1", 37.5586m, 127.0817m),
                _ => (address, restaurantLatitude, restaurantLongitude)
            };
        }

        if (address.Contains("중랑", StringComparison.Ordinal)
            || address.Contains("면목", StringComparison.Ordinal)
            || address.Contains("상봉", StringComparison.Ordinal)
            || address.Contains("망우", StringComparison.Ordinal)
            || address.Contains("신내", StringComparison.Ordinal))
        {
            return (address, 37.5928m, 127.0879m);
        }

        if (address.Contains("동대문", StringComparison.Ordinal)
            || address.Contains("장안", StringComparison.Ordinal))
        {
            return (address, 37.5684m, 127.0717m);
        }

        if (address.Contains("광진", StringComparison.Ordinal)
            || address.Contains("중곡", StringComparison.Ordinal))
        {
            return (address, 37.5586m, 127.0817m);
        }

        if (address.Contains("노원", StringComparison.Ordinal)
            || address.Contains("공릉", StringComparison.Ordinal))
        {
            return (address, 37.6246m, 127.0730m);
        }

        if (address.Contains("구리", StringComparison.Ordinal))
        {
            return (address, 37.5943m, 127.1296m);
        }

        var offset = StableOffset(order.주문번호);
        return (address, restaurantLatitude + offset.Latitude, restaurantLongitude + offset.Longitude);
    }

    private static (decimal Latitude, decimal Longitude) StableOffset(string value)
    {
        var sum = value.Sum(ch => ch);
        return ((sum % 7 + 1) * 0.002m, (sum % 5 + 1) * 0.002m);
    }

    private static List<FoodDeliveryOrderReviewItem> CreateJungnangScenarioOrders()
    {
        var now = DateTime.UtcNow;
        return
        [
            CreateScenarioOrder(
                "FOOD-JN-001",
                101,
                "살뜰분식 면목점",
                "제육덮밥 2",
                19000m,
                "서울특별시 중랑구 면목로 352",
                37.5888m,
                127.0874m,
                "서울특별시 중랑구 면목동 15-1",
                37.5867m,
                127.0885m,
                now.AddMinutes(14)),
            CreateScenarioOrder(
                "FOOD-JN-002",
                101,
                "살뜰분식 면목점",
                "비빔밥 1, 만두 1",
                16500m,
                "서울특별시 중랑구 면목로 352",
                37.5888m,
                127.0874m,
                "서울특별시 동대문구 장안동 335-1",
                37.5684m,
                127.0717m,
                now.AddMinutes(18)),
            CreateScenarioOrder(
                "FOOD-JN-003",
                102,
                "살뜰도시락 상봉점",
                "돈까스 1, 우동 1",
                19000m,
                "서울특별시 중랑구 망우로 353",
                37.5969m,
                127.0851m,
                "서울특별시 광진구 중곡동 130-1",
                37.5586m,
                127.0817m,
                now.AddMinutes(21)),
            CreateScenarioOrder(
                "FOOD-JN-004",
                103,
                "살뜰치킨 신내점",
                "후라이드치킨 1",
                21000m,
                "서울특별시 중랑구 신내로 128",
                37.6088m,
                127.0961m,
                "서울특별시 노원구 공릉동 375-1",
                37.6246m,
                127.0730m,
                now.AddMinutes(24)),
            CreateScenarioOrder(
                "FOOD-JN-005",
                104,
                "살뜰국밥 망우점",
                "순대국 2",
                22000m,
                "서울특별시 중랑구 망우로 470",
                37.5996m,
                127.1012m,
                "경기도 구리시 교문동 230-1",
                37.5943m,
                127.1296m,
                now.AddMinutes(28))
        ];
    }

    private static FoodDeliveryOrderReviewItem CreateScenarioOrder(
        string orderNo,
        long restaurantId,
        string restaurantName,
        string menuSummary,
        decimal orderAmount,
        string restaurantAddress,
        decimal restaurantLatitude,
        decimal restaurantLongitude,
        string customerAddress,
        decimal customerLatitude,
        decimal customerLongitude,
        DateTime pickupReadyAtUtc)
    {
        var pickupScope = ResolveFoodDeliveryScope(
            restaurantAddress,
            new 배차경로좌표(restaurantLatitude, restaurantLongitude));
        var dropoffScope = ResolveFoodDeliveryScope(
            customerAddress,
            new 배차경로좌표(customerLatitude, customerLongitude));

        return new FoodDeliveryOrderReviewItem(
            orderNo,
            restaurantId,
            restaurantName,
            menuSummary,
            orderAmount,
            음식주문상태코드.픽업대기,
            음식주문배차상태코드.배차대기,
            restaurantAddress,
            restaurantLatitude,
            restaurantLongitude,
            customerAddress,
            customerLatitude,
            customerLongitude,
            pickupReadyAtUtc,
            DefaultMaxDeliveryMinutesAfterReady,
            pickupScope.Key,
            pickupScope.Name,
            pickupScope.Role,
            dropoffScope.Key,
            dropoffScope.Name,
            dropoffScope.Role);
    }

    private static List<FoodDeliveryDispatchAIReviewDriverDto> CreateSampleDrivers()
    {
        var now = DateTime.UtcNow;
        return
        [
            new()
            {
                DriverId = "f-driver-sample-a",
                DriverName = "F드라이버 A",
                DrivingStatus = "운행중",
                Latitude = 37.5907m,
                Longitude = 127.0869m,
                DeliveryScopeKey = PrimaryReviewScope.Key,
                DeliveryScopeName = PrimaryReviewScope.Name,
                DeliveryScopeRole = PrimaryReviewScope.Role,
                LastLocationReceivedAtUtc = now.AddMinutes(-2)
            },
            new()
            {
                DriverId = "f-driver-sample-b",
                DriverName = "F드라이버 B",
                DrivingStatus = "운행중",
                Latitude = 37.5712m,
                Longitude = 127.0732m,
                DeliveryScopeKey = "bjd-sigungu:11230",
                DeliveryScopeName = "동대문구",
                DeliveryScopeRole = "인접 배달권",
                CurrentAcceptedDeliveryCount = 1,
                LastLocationReceivedAtUtc = now.AddMinutes(-5)
            },
            new()
            {
                DriverId = "f-driver-sample-c",
                DriverName = "F드라이버 C",
                DrivingStatus = "운행중",
                Latitude = 37.5949m,
                Longitude = 127.1267m,
                DeliveryScopeKey = "bjd-sigungu:41310",
                DeliveryScopeName = "구리시",
                DeliveryScopeRole = "인접 배달권",
                LastLocationReceivedAtUtc = now.AddMinutes(-7)
            }
        ];
    }

    private static bool IsReviewTarget(음식주문응답 order)
    {
        var status = 음식주문상태코드.Normalize(order.상태);
        return status is not 음식주문상태코드.전달완료 and not 음식주문상태코드.취소;
    }

    private static bool IsDefaultFoodOrderSample(음식주문응답 order)
        => order.주문번호.StartsWith("FOOD-20260701-", StringComparison.Ordinal);

    private static FoodDeliveryReviewScope ResolveFoodDeliveryScope(string? address, 배차경로좌표? point)
    {
        if (!string.IsNullOrWhiteSpace(address))
        {
            var normalized = address.Trim();
            if (normalized.Contains("중랑", StringComparison.Ordinal)
                || normalized.Contains("면목", StringComparison.Ordinal)
                || normalized.Contains("상봉", StringComparison.Ordinal)
                || normalized.Contains("망우", StringComparison.Ordinal)
                || normalized.Contains("신내", StringComparison.Ordinal))
            {
                return PrimaryReviewScope;
            }

            var adjacent = AdjacentReviewScopes.FirstOrDefault(x => normalized.Contains(x.Name, StringComparison.Ordinal));
            if (adjacent is not null)
            {
                return adjacent;
            }
        }

        var catalogScope = 국내화물배달권정책.판정(point, address);
        return ResolveScopeByKey(catalogScope.배달권키)
               ?? new FoodDeliveryReviewScope(catalogScope.배달권키, catalogScope.배달권명, "기타 배달권");
    }

    private static FoodDeliveryReviewScope? ResolveScopeByKey(string? scopeKey)
        => string.IsNullOrWhiteSpace(scopeKey)
            ? null
            : AllReviewScopes.FirstOrDefault(x => string.Equals(x.Key, scopeKey.Trim(), StringComparison.Ordinal));

    private static string ResolveScopeDisplay(string? scopeKey)
    {
        var scope = ResolveScopeByKey(scopeKey);
        return scope is null
            ? string.IsNullOrWhiteSpace(scopeKey) ? "미정" : scopeKey.Trim()
            : $"{scope.Name}({scope.Role})";
    }

    private static 배차경로좌표? CreatePoint(decimal? latitude, decimal? longitude)
        => latitude.HasValue && longitude.HasValue
            ? new 배차경로좌표(latitude.Value, longitude.Value)
            : null;

    private static decimal CalculateDistanceKm(배차경로좌표 source, 배차경로좌표 target)
    {
        const double earthRadiusKm = 6371d;
        var sourceLat = ToRadians((double)source.Latitude);
        var targetLat = ToRadians((double)target.Latitude);
        var deltaLat = ToRadians((double)(target.Latitude - source.Latitude));
        var deltaLng = ToRadians((double)(target.Longitude - source.Longitude));

        var a = Math.Sin(deltaLat / 2d) * Math.Sin(deltaLat / 2d)
                + Math.Cos(sourceLat) * Math.Cos(targetLat)
                * Math.Sin(deltaLng / 2d) * Math.Sin(deltaLng / 2d);
        var c = 2d * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1d - a));
        return Math.Round((decimal)(earthRadiusKm * c), 3);
    }

    private static double ToRadians(double degrees)
        => degrees * Math.PI / 180d;

    private sealed record FoodDeliveryOrderReviewItem(
        string OrderNo,
        long RestaurantId,
        string RestaurantName,
        string MenuSummary,
        decimal OrderAmount,
        string OrderStatus,
        string DispatchStatus,
        string RestaurantAddress,
        decimal RestaurantLatitude,
        decimal RestaurantLongitude,
        string CustomerAddress,
        decimal CustomerLatitude,
        decimal CustomerLongitude,
        DateTime? PickupReadyAtUtc,
        decimal MaxDeliveryMinutesAfterReady,
        string PickupScopeKey,
        string PickupScopeName,
        string PickupScopeRole,
        string DropoffScopeKey,
        string DropoffScopeName,
        string DropoffScopeRole);

    private sealed record FoodDeliveryReviewScope(string Key, string Name, string Role);
}
