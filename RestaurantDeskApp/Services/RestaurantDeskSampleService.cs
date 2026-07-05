using Hongdal.Contracts.Admin.Restaurants;
using Hongdal.Contracts.Restaurants;
using RestaurantDeskApp.Models.Restaurant;

namespace RestaurantDeskApp.Services;

public sealed class RestaurantDeskSampleService
{
    private readonly IReadOnlyList<주문알림항목> _orderAlerts;
    private readonly IReadOnlyList<음식점요약응답> _nearbyRestaurants;
    private readonly IReadOnlyList<음식점요약응답> _popularRestaurants;
    private readonly IReadOnlyList<음식점리뷰관리항목응답> _reviewModerationItems;
    private readonly 음식점리뷰운영정책응답 _policy;

    public RestaurantDeskSampleService()
    {
        _orderAlerts =
        [
            new 주문알림항목 { Id = 1, 주문번호 = "ORD-20260701-001", 고객명 = "김주문", 메뉴요약 = "제육덮밥 2, 김치찌개 1", 주문금액 = 28500m, 접수시각 = DateTime.Now.AddMinutes(-2), 미확인 = true },
            new 주문알림항목 { Id = 2, 주문번호 = "ORD-20260701-002", 고객명 = "박고객", 메뉴요약 = "돈까스 1, 우동 1", 주문금액 = 19000m, 접수시각 = DateTime.Now.AddMinutes(-9), 미확인 = true },
            new 주문알림항목 { Id = 3, 주문번호 = "ORD-20260701-003", 고객명 = "이배달", 메뉴요약 = "비빔밥 3", 주문금액 = 33000m, 접수시각 = DateTime.Now.AddMinutes(-18), 미확인 = false }
        ];

        _nearbyRestaurants =
        [
            new 음식점요약응답 { Id = 101, 상호명 = "홍달식당 강서점", 카테고리 = "한식", 주소 = "서울 강서구 화곡동", 위도 = 37.5412m, 경도 = 126.8409m, 거리Km = 0.6m, 평균평점 = 4.6m, 리뷰수 = 184, 주문가능여부 = true, 저평점주의필요 = false },
            new 음식점요약응답 { Id = 102, 상호명 = "목동반상", 카테고리 = "백반", 주소 = "서울 양천구 목동", 위도 = 37.5241m, 경도 = 126.8736m, 거리Km = 1.4m, 평균평점 = 4.4m, 리뷰수 = 132, 주문가능여부 = true, 저평점주의필요 = false },
            new 음식점요약응답 { Id = 103, 상호명 = "가양국수", 카테고리 = "분식", 주소 = "서울 강서구 가양동", 위도 = 37.5616m, 경도 = 126.8548m, 거리Km = 1.8m, 평균평점 = 4.1m, 리뷰수 = 67, 주문가능여부 = true, 저평점주의필요 = true }
        ];

        _popularRestaurants =
        [
            new 음식점요약응답 { Id = 201, 상호명 = "별미도시락", 카테고리 = "도시락", 주소 = "서울 마포구 합정동", 위도 = 37.5499m, 경도 = 126.9139m, 평균평점 = 4.9m, 리뷰수 = 540, 주문가능여부 = true, 저평점주의필요 = false },
            new 음식점요약응답 { Id = 202, 상호명 = "마포불향족발", 카테고리 = "야식", 주소 = "서울 마포구 서교동", 위도 = 37.5551m, 경도 = 126.9217m, 평균평점 = 4.8m, 리뷰수 = 421, 주문가능여부 = true, 저평점주의필요 = false },
            new 음식점요약응답 { Id = 203, 상호명 = "송정초밥", 카테고리 = "일식", 주소 = "서울 강서구 공항동", 위도 = 37.5582m, 경도 = 126.8112m, 평균평점 = 4.7m, 리뷰수 = 356, 주문가능여부 = true, 저평점주의필요 = false }
        ];

        _reviewModerationItems =
        [
            new 음식점리뷰관리항목응답 { 리뷰Id = 7001, 음식점Id = 103, 음식점명 = "가양국수", 주문자UserId = "orderer-a", 주문번호 = "FOOD-10001", 별점 = 1, 내용 = "면이 불고 사진과 다르게 나왔습니다.", 사진포함여부 = true, 같은음식점기준저평점3회연속여부 = true, 사장노출허용여부 = false, 관리자게시강제여부 = true, 현재노출여부 = true, CreatedAt = DateTime.UtcNow.AddDays(-1), 게시종료일시Utc = DateTime.UtcNow.AddDays(2), 최근조치사유 = "같은 음식점 기준 1~2점 사진 리뷰 3회 연속" },
            new 음식점리뷰관리항목응답 { 리뷰Id = 7002, 음식점Id = 301, 음식점명 = "홍달식당 강서점", 주문자UserId = "orderer-b", 주문번호 = "FOOD-10098", 별점 = 2, 내용 = "배달 지연이 반복되어 검토가 필요합니다.", 사진포함여부 = true, 같은음식점기준저평점3회연속여부 = false, 사장노출허용여부 = true, 관리자게시강제여부 = false, 현재노출여부 = true, CreatedAt = DateTime.UtcNow.AddHours(-10), 최근조치사유 = "대기" }
        ];

        _policy = new 음식점리뷰운영정책응답
        {
            Id = 1,
            기본저평점게시일수 = 3,
            허용게시일수옵션 = [3, 7],
            UpdatedAt = DateTime.UtcNow.AddHours(-3)
        };
    }

    public IReadOnlyList<주문알림항목> Get신규주문목록() => _orderAlerts;

    public IReadOnlyList<음식점요약응답> Get가까운음식점목록() => _nearbyRestaurants;

    public IReadOnlyList<음식점요약응답> Get인기음식점목록() => _popularRestaurants;

    public IReadOnlyList<음식점리뷰관리항목응답> Get리뷰운영목록() => _reviewModerationItems;

    public 음식점리뷰운영정책응답 Get운영정책() => _policy;
}
