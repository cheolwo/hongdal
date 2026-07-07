using Microsoft.EntityFrameworkCore;
using 홍달.Data;
using 홍달.도메인.공통;
using 홍달.도메인.기사;
using 홍달.도메인.배차;
using 홍달.도메인.운송;
using 홍달.도메인.화물;
using 홍달.도메인.화주;
using 홍달.Services.Storage.Local;

namespace Hongdal.Services.Development;

public static class HongdalV1DevelopmentDataSeeder
{
    private const string DriverUserName = "driver1@hongdal.local";
    private const string ShipperUserName = "shipper1";

    private static readonly string[] SampleRequestIds =
    [
        "V1-DEV-REQ-001",
        "V1-DEV-REQ-002",
        "V1-DEV-REQ-003"
    ];

    public static async Task SeedAsync(IServiceProvider services, ILogger logger, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HongdalContext>();
        var locationStore = scope.ServiceProvider.GetRequiredService<IDriverLocationStore>();

        var driverUser = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserName == DriverUserName, cancellationToken);
        var shipperUser = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserName == ShipperUserName, cancellationToken);

        if (driverUser is null || shipperUser is null)
        {
            logger.LogWarning(
                "Hongdal 1.0 development sample data was skipped because seed users are missing. DriverUser={DriverUser} ShipperUser={ShipperUser}",
                DriverUserName,
                ShipperUserName);
            return;
        }

        var now = DateTime.UtcNow;
        await EnsureDriverAsync(db, driverUser.Id, now, cancellationToken);
        await EnsureDriverShiftAsync(db, driverUser.Id, now, cancellationToken);
        EnsureDriverLocation(locationStore, driverUser.Id, now);

        var existingRequestIds = await db.화주운송의뢰
            .AsNoTracking()
            .Where(x => SampleRequestIds.Contains(x.의뢰Id))
            .Select(x => x.의뢰Id)
            .ToListAsync(cancellationToken);
        var existingRequestIdSet = existingRequestIds.ToHashSet(StringComparer.Ordinal);

        var scenarios = CreateScenarios(shipperUser.Id, driverUser.Id, now);
        foreach (var scenario in scenarios)
        {
            if (!existingRequestIdSet.Contains(scenario.Request.의뢰Id))
            {
                db.화주운송의뢰.Add(scenario.Request);
                db.화물요구조건.Add(scenario.CargoRequirement);
                db.배차대기.Add(scenario.Queue);
            }
        }

        await EnsureTransportAsync(db, driverUser.Id, now, cancellationToken);
        await EnsureSettlementAsync(db, driverUser.Id, now, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task EnsureDriverAsync(HongdalContext db, string driverId, DateTime now, CancellationToken cancellationToken)
    {
        var driver = await db.용달기사.FirstOrDefaultAsync(x => x.기사Id == driverId, cancellationToken);
        if (driver is null)
        {
            db.용달기사.Add(new 용달기사
            {
                NotionPageId = Guid.NewGuid().ToString("N"),
                기사명 = "개발용 기사",
                기사Id = driverId,
                상태 = "활동중",
                연락처 = "010-1000-1000",
                차량 = "1톤 카고",
                운행상태 = 상태값.기사운행상태.운행중,
                주_활동지역 = "서울 강서/양천",
                기본복귀지주소 = "서울 양천구 목동",
                기본복귀지위도 = 37.5268m,
                기본복귀지경도 = 126.8755m,
                집주소를복귀지로사용허용 = true,
                메모 = "Hongdal 1.0 개발 검증용 기사",
                등록일 = now,
                CreatedAt = now,
                UpdatedAt = now
            });
            return;
        }

        driver.차량 = "1톤 카고";
        driver.운행상태 = 상태값.기사운행상태.운행중;
        driver.주_활동지역 = "서울 강서/양천";
        driver.기본복귀지주소 = "서울 양천구 목동";
        driver.기본복귀지위도 = 37.5268m;
        driver.기본복귀지경도 = 126.8755m;
        driver.집주소를복귀지로사용허용 = true;
        driver.UpdatedAt = now;
    }

    private static async Task EnsureDriverShiftAsync(HongdalContext db, string driverId, DateTime now, CancellationToken cancellationToken)
    {
        var todayStart = now.Date;
        var tomorrowStart = todayStart.AddDays(1);
        var hasTodayShift = await db.기사근무.AnyAsync(
            x => x.기사Id == driverId
                 && x.시작시각.HasValue
                 && x.시작시각.Value >= todayStart
                 && x.시작시각.Value < tomorrowStart,
            cancellationToken);
        if (hasTodayShift)
        {
            return;
        }

        db.기사근무.Add(new 기사근무
        {
            기사Id = driverId,
            시작모드 = "일반 운행",
            시작시각 = now.Date.AddHours(8).AddMinutes(30),
            시작위치 = "서울 강서구 화곡동",
            복귀지 = "서울 양천구 목동",
            오늘의복귀지주소 = "서울 양천구 목동",
            오늘의복귀지위도 = 37.5268m,
            오늘의복귀지경도 = 126.8755m,
            복귀지출처 = "오늘복귀지",
            복귀지입력일시 = now.Date.AddHours(8).AddMinutes(20),
            CreatedAt = now,
            UpdatedAt = now
        });
    }

    private static void EnsureDriverLocation(IDriverLocationStore locationStore, string driverId, DateTime now)
    {
        locationStore.Upsert(new DriverLocationSnapshot(
            driverId,
            37.5412m,
            126.8409m,
            25m,
            상태값.기사운행상태.운행중,
            now.AddMinutes(-3),
            now));
    }

    private static IReadOnlyList<V1DispatchScenario> CreateScenarios(string shipperId, string driverId, DateTime now)
    {
        return
        [
            CreateScenario(
                requestId: "V1-DEV-REQ-001",
                shipperId,
                driverId,
                cargoType: "냉장 식품 박스",
                cargoDescription: "냉장 보관 박스 12개, 비맞음 금지",
                transportType: "혼적",
                vehicleType: "1톤 냉장탑",
                paymentMethod: "카드",
                settlementTime: "선결제",
                evidenceMethod: "인수증",
                pickupAddress: "서울 양천구 목동",
                pickupDetail: "A 물류센터 2번 도크",
                pickupLat: 37.5268m,
                pickupLng: 126.8755m,
                dropoffAddress: "경기 수원시 영통구",
                dropoffDetail: "매장 후문 입고장",
                dropoffLat: 37.2636m,
                dropoffLng: 127.0286m,
                fare: 88000,
                pickupStart: now.AddMinutes(40),
                pickupEnd: now.AddHours(2),
                dropoffStart: now.AddHours(3),
                dropoffEnd: now.AddHours(5),
                queueStage: 상태값.배차큐단계.배차추천,
                exposureState: 상태값.배차노출상태.추천중,
                currentRecommendedDriverId: driverId,
                recommendationExpiresAt: now.AddMinutes(30),
                cargoWeightKg: 180,
                냉장필요: true,
                혼적허용: true),
            CreateScenario(
                requestId: "V1-DEV-REQ-002",
                shipperId,
                driverId,
                cargoType: "전자제품",
                cargoDescription: "파손주의 전자제품 박스 8개",
                transportType: "독차",
                vehicleType: "1톤 탑차",
                paymentMethod: "후불",
                settlementTime: "후불",
                evidenceMethod: "사진증빙",
                pickupAddress: "서울 마포구 성산동",
                pickupDetail: "상가 지하 하역장",
                pickupLat: 37.5638m,
                pickupLng: 126.9084m,
                dropoffAddress: "경기 고양시 덕양구",
                dropoffDetail: "고객사 물류 사무실",
                dropoffLat: 37.6374m,
                dropoffLng: 126.8320m,
                fare: 132000,
                pickupStart: now.AddHours(2),
                pickupEnd: now.AddHours(4),
                dropoffStart: now.AddHours(5),
                dropoffEnd: now.AddHours(7),
                queueStage: 상태값.배차큐단계.배차추천,
                exposureState: 상태값.배차노출상태.추천중,
                currentRecommendedDriverId: driverId,
                recommendationExpiresAt: now.AddMinutes(45),
                cargoWeightKg: 240,
                독차필수: true,
                비맞으면안됨: true),
            CreateScenario(
                requestId: "V1-DEV-REQ-003",
                shipperId,
                driverId,
                cargoType: "생활용품",
                cargoDescription: "생활용품 혼적 가능 박스 20개",
                transportType: "혼적",
                vehicleType: "1톤 카고",
                paymentMethod: "현장지급",
                settlementTime: "하차후정산",
                evidenceMethod: "없음",
                pickupAddress: "경기 부천시 중동",
                pickupDetail: "1층 출고장",
                pickupLat: 37.5034m,
                pickupLng: 126.7660m,
                dropoffAddress: "인천 연수구 송도동",
                dropoffDetail: "아파트 관리사무소 앞",
                dropoffLat: 37.3896m,
                dropoffLng: 126.6426m,
                fare: 74000,
                pickupStart: now.AddHours(4),
                pickupEnd: now.AddHours(6),
                dropoffStart: now.AddHours(7),
                dropoffEnd: now.AddHours(9),
                queueStage: 상태값.배차큐단계.공개배차,
                exposureState: 상태값.배차노출상태.공개중,
                currentRecommendedDriverId: null,
                recommendationExpiresAt: null,
                cargoWeightKg: 320,
                혼적허용: true)
        ];
    }

    private static V1DispatchScenario CreateScenario(
        string requestId,
        string shipperId,
        string driverId,
        string cargoType,
        string cargoDescription,
        string transportType,
        string vehicleType,
        string paymentMethod,
        string settlementTime,
        string evidenceMethod,
        string pickupAddress,
        string pickupDetail,
        decimal pickupLat,
        decimal pickupLng,
        string dropoffAddress,
        string dropoffDetail,
        decimal dropoffLat,
        decimal dropoffLng,
        int fare,
        DateTime pickupStart,
        DateTime pickupEnd,
        DateTime? dropoffStart,
        DateTime? dropoffEnd,
        int queueStage,
        int exposureState,
        string? currentRecommendedDriverId,
        DateTime? recommendationExpiresAt,
        int cargoWeightKg,
        bool 비맞으면안됨 = false,
        bool 냉장필요 = false,
        bool 냉동필요 = false,
        bool 혼적허용 = false,
        bool 독차필수 = false)
    {
        var now = DateTime.UtcNow;
        var request = new 화주운송의뢰
        {
            의뢰Id = requestId,
            화주Id = shipperId,
            주문자UserId = shipperId,
            화물종류 = cargoType,
            화물설명 = cargoDescription,
            화물수량 = 1,
            화물중량Kg = cargoWeightKg,
            화물파손주의여부 = 비맞으면안됨,
            화물온도조건 = 냉동필요 ? "냉동" : 냉장필요 ? "냉장" : "상온",
            운송방식 = transportType,
            차량종류 = vehicleType,
            결제수단 = paymentMethod,
            정산시점 = settlementTime,
            증빙방식 = evidenceMethod,
            수납주체 = paymentMethod == "현장지급" ? "기사" : "플랫폼",
            정산상태 = 상태값.결제상태.결제완료,
            세금계산서필요 = paymentMethod == "후불",
            현금영수증필요 = paymentMethod == "현장지급",
            결제예정금액 = fare,
            픽업_도로명주소 = pickupAddress,
            픽업_상세주소 = pickupDetail,
            픽업_위도 = pickupLat,
            픽업_경도 = pickupLng,
            픽업_연락처_이름 = "개발 상차 담당자",
            픽업_연락처_전화번호 = "010-1111-1000",
            픽업_시간창_시작일시 = pickupStart,
            픽업_시간창_종료일시 = pickupEnd,
            하차_도로명주소 = dropoffAddress,
            하차_상세주소 = dropoffDetail,
            하차_위도 = dropoffLat,
            하차_경도 = dropoffLng,
            하차_연락처_이름 = "개발 수령 담당자",
            하차_연락처_전화번호 = "010-2222-2000",
            하차_시간창_시작일시 = dropoffStart,
            하차_시간창_종료일시 = dropoffEnd,
            서비스레벨 = "V1 개발 검증",
            요청사항 = "상하차 도착 전 연락 후 진행",
            최종운임 = fare,
            클라이언트요청Id = $"dev-{requestId}",
            상태 = 상태값.의뢰상태.생성됨,
            결제상태 = 상태값.결제상태.결제완료,
            배차상태 = 상태값.배차상태.대기,
            CreatedAt = now,
            UpdatedAt = now
        };

        var queue = new 배차대기
        {
            의뢰Id = requestId,
            화주Id = shipperId,
            배차업무유형 = 상태값.배차업무유형.용달운송,
            원본의뢰유형 = "CargoTransport",
            원본의뢰Id = requestId,
            픽업_도로명주소 = pickupAddress,
            픽업_상세주소 = pickupDetail,
            픽업_위도 = pickupLat,
            픽업_경도 = pickupLng,
            하차_도로명주소 = dropoffAddress,
            하차_상세주소 = dropoffDetail,
            하차_위도 = dropoffLat,
            하차_경도 = dropoffLng,
            상태 = 상태값.배차대기상태.대기,
            배차큐단계 = queueStage,
            배차노출상태 = exposureState,
            현재추천대상기사Id = currentRecommendedDriverId,
            추천시작시각 = currentRecommendedDriverId is null ? null : now.AddMinutes(-5),
            추천만료시각 = recommendationExpiresAt,
            추천라운드 = currentRecommendedDriverId is null ? 0 : 1,
            계획배차시도횟수 = 1,
            공개전환시각 = queueStage == 상태값.배차큐단계.공개배차 ? now.AddMinutes(-10) : null,
            CreatedAt = now,
            UpdatedAt = now
        };

        var requirement = new 화물요구조건
        {
            의뢰Id = requestId,
            화물길이Mm = 600,
            화물폭Mm = 420,
            화물높이Mm = 360,
            화물무게Kg = cargoWeightKg,
            팔레트개수 = cargoWeightKg >= 300 ? 1 : null,
            비맞으면안됨 = 비맞으면안됨,
            냉장필요 = 냉장필요,
            냉동필요 = 냉동필요,
            혼적허용 = 혼적허용,
            독차필수 = 독차필수,
            주의사항 = cargoDescription,
            CreatedAt = now,
            UpdatedAt = now
        };

        return new V1DispatchScenario(request, queue, requirement);
    }

    private static async Task EnsureTransportAsync(HongdalContext db, string driverId, DateTime now, CancellationToken cancellationToken)
    {
        var transportNumbers = new[] { "V1-DEV-TRN-001", "V1-DEV-TRN-002" };
        var existing = await db.배송_운송
            .AsNoTracking()
            .Where(x => transportNumbers.Contains(x.운송번호))
            .Select(x => x.운송번호)
            .ToListAsync(cancellationToken);
        var existingSet = existing.ToHashSet(StringComparer.Ordinal);

        if (!existingSet.Contains("V1-DEV-TRN-001"))
        {
            db.배송_운송.Add(new 배송_운송
            {
                운송번호 = "V1-DEV-TRN-001",
                상태 = "상차지도착",
                기사_운송자 = driverId,
                출발지 = "서울 강서구 마곡동",
                도착지 = "서울 양천구 목동",
                운임 = 65000m,
                첨부_json = "[]",
                메모 = "상차완료 -> 하차지도착 -> 인수완료 상태 전이 검증용",
                CreatedAt = now.AddHours(-1),
                UpdatedAt = now.AddMinutes(-20)
            });
        }

        if (!existingSet.Contains("V1-DEV-TRN-002"))
        {
            db.배송_운송.Add(new 배송_운송
            {
                운송번호 = "V1-DEV-TRN-002",
                상태 = "인수완료",
                출발_픽업 = now.AddDays(-2).AddHours(1),
                도착 = now.AddDays(-2).AddHours(4),
                기사_운송자 = driverId,
                출발지 = "서울 송파구 문정동",
                도착지 = "경기 성남시 분당구",
                운임 = 92000m,
                첨부_json = "[]",
                메모 = "완료 운송 및 인수증/정산 조회 검증용",
                CreatedAt = now.AddDays(-2),
                UpdatedAt = now.AddDays(-2).AddHours(4)
            });
        }
    }

    private static async Task EnsureSettlementAsync(HongdalContext db, string driverId, DateTime now, CancellationToken cancellationToken)
    {
        var currentYear = now.Year;
        var currentMonth = now.Month;
        var previous = now.AddMonths(-1);

        var hasCurrent = await db.기사월정산.AnyAsync(
            x => x.기사Id == driverId && x.년도 == currentYear && x.월 == currentMonth,
            cancellationToken);
        if (!hasCurrent)
        {
            db.기사월정산.Add(new 기사월정산
            {
                기사Id = driverId,
                년도 = currentYear,
                월 = currentMonth,
                배차건수 = 12,
                이용료 = 5000m,
                결제완료 = true,
                CreatedAt = now,
                UpdatedAt = now
            });
        }

        var hasPrevious = await db.기사월정산.AnyAsync(
            x => x.기사Id == driverId && x.년도 == previous.Year && x.월 == previous.Month,
            cancellationToken);
        if (!hasPrevious)
        {
            db.기사월정산.Add(new 기사월정산
            {
                기사Id = driverId,
                년도 = previous.Year,
                월 = previous.Month,
                배차건수 = 18,
                이용료 = 5000m,
                결제완료 = true,
                CreatedAt = now.AddMonths(-1),
                UpdatedAt = now.AddMonths(-1)
            });
        }
    }

    private sealed record V1DispatchScenario(
        화주운송의뢰 Request,
        배차대기 Queue,
        화물요구조건 CargoRequirement);
}
