using Ssalddel.Contracts.Driver.Development;

namespace Ssalddel.Services.Driver.Development;

public sealed class InMemory기사개발스냅샷Provider : I기사개발스냅샷Provider
{
    private 기사개발스냅샷응답 _snapshot;

    public InMemory기사개발스냅샷Provider()
    {
        _snapshot = CreateDefaultSnapshot(DateTime.Now);
    }

    public 기사개발스냅샷응답 GetSnapshot() => _snapshot;

    public void ReplaceSnapshot(기사개발스냅샷응답 snapshot)
    {
        _snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
    }

    private static 기사개발스냅샷응답 CreateDefaultSnapshot(DateTime now)
    {
        var snapshot = new 기사개발스냅샷응답
        {
            현재위치 = new 기사개발현재위치응답
            {
                위치명 = "서울 강서구 화곡동",
                위도 = 37.5412m,
                경도 = 126.8409m,
                갱신시각 = now.AddMinutes(-3)
            },
            추천의뢰목록 =
            [
                CreateRequest("101", "냉장식품", "혼적", "1톤", "냉동탑", "서울 양천구 목동", "경기 수원시 영통구", 37.5268m, 126.8755m, 37.2636m, 127.0286m, 88000m, 92m, "적합추천"),
                CreateRequest("102", "전자제품", "독차", "2.5톤", "윙", "서울 마포구 성산동", "경기 고양시 덕양구", 37.5638m, 126.9084m, 37.6374m, 126.8320m, 132000m, 86m, "추천"),
                CreateRequest("103", "생활용품", "혼적", "1톤", "탑", "경기 부천시 중동", "인천 연수구 송도동", 37.5034m, 126.7660m, 37.3896m, 126.6426m, 74000m, 81m, "추천"),
                CreateRequest("104", "소형 이사", "독차", "1톤", "카고", "서울 송파구 문정동", "서울 강서구 마곡동", 37.4861m, 127.1226m, 37.5668m, 126.8298m, 118000m, 78m, "긴급추천"),
                CreateRequest(
                    "GP-201",
                    "수입 냉장식품 공동주문",
                    "공동주문 세대배송",
                    "1톤",
                    "냉동탑",
                    "인천항 보세창고",
                    "서울 강서구 살뜰아파트",
                    37.4559m,
                    126.6243m,
                    37.5610m,
                    126.8370m,
                    176000m,
                    94m,
                    "공동주문추천",
                    requestTypeCode: "GroupPurchaseCargoTransport",
                    requestTypeLabel: "공동주문 운송",
                    isGroupPurchaseTransport: true,
                    includesApartmentUnitDelivery: true,
                    apartmentUnitDeliveryCount: 33,
                    apartmentUnitDeliveryScopeLabel: "상하차 + 세대 문앞 33건")
            ],
            예약목록 =
            [
                new 기사개발예약응답
                {
                    Id = 1,
                    시작시각 = DateTime.Today.AddHours(15),
                    시작모드 = "예약 운행",
                    시작위치 = "서울 양천구 목동",
                    복귀지 = "서울 강서구 화곡동",
                    상태 = "확정",
                    메모 = "예약 시간 20분 전 위치 확인"
                }
            ],
            운송목록 =
            [
                new 기사개발운송응답
                {
                    Id = 1,
                    의뢰Id = "DRV-2026-001",
                    화물종류 = "가구",
                    픽업지 = "서울 강서구 마곡동",
                    하차지 = "서울 양천구 목동",
                    픽업위도 = 37.5668m,
                    픽업경도 = 126.8298m,
                    하차위도 = 37.5268m,
                    하차경도 = 126.8755m,
                    현재단계 = "하차지 이동중",
                    예정시각 = now.AddMinutes(35),
                    운송거리Km = 12.4m,
                    예상수익 = 65000m,
                    인수증필요 = true,
                    인수증서명필수 = false,
                    결제방식 = "인수증 정산",
                    다음행동 = "하차지 도착"
                }
            ],
            알림목록 =
            [
                new 기사개발알림응답
                {
                    Id = 1,
                    종류 = "추천",
                    제목 = "연계 가능한 추천콜이 도착했습니다.",
                    내용 = "현재 하차지 이후 이어 받을 수 있는 후보입니다.",
                    발생시각 = now.AddMinutes(-5),
                    읽음 = false
                }
            ]
        };

        snapshot.근무상태 = new 기사개발근무상태응답
        {
            기사명 = "홍길동 기사님",
            운행상태 = "운행중",
            시작모드 = "일반 운행",
            시작위치 = snapshot.현재위치.위치명,
            복귀지 = "서울 양천구 목동",
            시작시각 = DateTime.Today.AddHours(8).AddMinutes(30),
            추천콜수 = snapshot.추천의뢰목록.Count,
            오늘예약수 = snapshot.예약목록.Count
        };

        snapshot.정산요약 = new 기사개발정산요약응답
        {
            년도 = DateTime.Today.Year,
            월 = DateTime.Today.Month,
            배차건수 = 12,
            이용료 = 5000m,
            월상한 = 5000m,
            결제완료 = true,
            상세항목 =
            [
                new 기사개발정산상세응답 { 항목명 = "이용료", 설명 = "배차 확정 건수 기준", 금액 = 5000m },
                new 기사개발정산상세응답 { 항목명 = "월 상한 조정", 설명 = "월 이용료 상한 적용", 금액 = 0m }
            ]
        };

        return snapshot;
    }

    private static 기사개발추천의뢰응답 CreateRequest(
        string requestId,
        string cargoType,
        string transportMode,
        string tonnage,
        string vehicleType,
        string pickup,
        string dropoff,
        decimal pickupLat,
        decimal pickupLng,
        decimal dropoffLat,
        decimal dropoffLng,
        decimal expectedProfit,
        decimal score,
        string status,
        string requestTypeCode = "GeneralCargoTransport",
        string requestTypeLabel = "일반 화물",
        bool isGroupPurchaseTransport = false,
        bool includesApartmentUnitDelivery = false,
        int? apartmentUnitDeliveryCount = null,
        string apartmentUnitDeliveryScopeLabel = "상하차")
    {
        var lineDistance = Math.Abs(pickupLat - dropoffLat) * 111m + Math.Abs(pickupLng - dropoffLng) * 88m;
        var drivingDistance = decimal.Round(lineDistance * 1.25m, 1);
        var pickupDistance = decimal.Round(Math.Abs(37.5412m - pickupLat) * 111m + Math.Abs(126.8409m - pickupLng) * 88m, 1);
        var returnDistance = decimal.Round(Math.Abs(37.5268m - dropoffLat) * 111m + Math.Abs(126.8755m - dropoffLng) * 88m, 1);

        return new 기사개발추천의뢰응답
        {
            의뢰Id = requestId,
            화물종류 = cargoType,
            운송방식 = transportMode,
            운송의뢰유형코드 = requestTypeCode,
            운송의뢰유형표시 = requestTypeLabel,
            당일상차필수 = true,
            당일하차필수 = status == "긴급추천",
            차량톤수 = tonnage,
            차량형태 = vehicleType,
            인수증필요 = true,
            공동주문운송여부 = isGroupPurchaseTransport,
            세대배송포함여부 = includesApartmentUnitDelivery,
            세대배송건수 = apartmentUnitDeliveryCount,
            세대배송업무표시 = apartmentUnitDeliveryScopeLabel,
            결제방식 = "하차 후 계좌",
            픽업지 = pickup,
            하차지 = dropoff,
            픽업_위도 = pickupLat,
            픽업_경도 = pickupLng,
            하차_위도 = dropoffLat,
            하차_경도 = dropoffLng,
            직선거리Km = decimal.Round(lineDistance, 1),
            픽업거리Km = pickupDistance,
            공차거리Km = pickupDistance,
            운송거리Km = drivingDistance,
            복귀예상거리Km = returnDistance,
            지금바로복귀거리Km = 8.2m,
            복귀우회증가거리Km = returnDistance - 8.2m,
            총공차거리Km = pickupDistance + returnDistance,
            주행거리Km = pickupDistance + drivingDistance,
            예상톨비 = 3200m,
            예상연료비 = 18000m,
            예상총비용 = 21200m,
            예상수익 = expectedProfit,
            추천점수 = score,
            추천사유 = isGroupPurchaseTransport
                ? "공동주문 수입 화물이며 세대배송 범위와 분류 상태를 확인해야 하는 추천입니다."
                : "현재 운송 하차지 이후 연계하기 좋은 후보입니다.",
            복귀지기준추천여부 = true,
            복귀지출처 = "오늘복귀지",
            복귀추천사유 = "오늘 복귀지와 다음 상차 동선이 크게 어긋나지 않습니다.",
            요약설명 = isGroupPurchaseTransport
                ? $"{cargoType} 운송, {pickup}에서 {dropoff}까지 · {apartmentUnitDeliveryScopeLabel}"
                : $"{cargoType} 운송, {pickup}에서 {dropoff}까지",
            상세설명 = isGroupPurchaseTransport
                ? $"{pickup} 상차 후 {dropoff} 하차와 세대배송 범위를 함께 확인합니다. 상품정보 스티커, 분류 상태, 세대배송 건수를 확인한 뒤 수락 여부를 결정합니다."
                : $"{pickup} 상차 후 {dropoff} 하차 예정입니다. 비용과 경로를 확인한 뒤 수락 여부를 결정합니다.",
            상태 = status,
            배차상태 = "배차대기"
        };
    }
}
