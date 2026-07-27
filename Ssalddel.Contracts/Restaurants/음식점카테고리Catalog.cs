namespace Ssalddel.Contracts.Restaurants;

public sealed record 음식점카테고리정의(
    string 카테고리키,
    string 표시명,
    string 설명,
    string 대표메뉴안내,
    int 표시순서);

public static class 음식점카테고리Catalog
{
    public static IReadOnlyList<음식점카테고리정의> 전체 { get; } =
    [
        new("분식", "분식", "간편하게 즐기는 한 끼", "김밥 · 떡볶이 · 튀김", 10),
        new("치킨", "치킨", "바삭한 치킨과 사이드", "후라이드 · 양념 · 순살", 20),
        new("피자", "피자", "여럿이 함께 먹기 좋은 메뉴", "피자 · 파스타 · 사이드", 30),
        new("한식", "한식", "밥과 국, 든든한 한 상", "백반 · 찌개 · 국밥", 40),
        new("중식", "중식", "면과 밥, 요리를 한 번에", "짜장면 · 짬뽕 · 탕수육", 50),
        new("일식", "일식", "깔끔하게 즐기는 일식", "돈까스 · 우동 · 초밥", 60),
        new("버거", "버거", "빠르게 즐기는 버거 세트", "버거 · 감자튀김 · 음료", 70),
        new("카페·디저트", "카페·디저트", "식사 뒤 가볍게 즐기는 메뉴", "커피 · 빵 · 디저트", 80)
    ];

    public static 음식점카테고리정의? 찾기(string? 카테고리)
        => string.IsNullOrWhiteSpace(카테고리)
            ? null
            : 전체.FirstOrDefault(item =>
                string.Equals(item.카테고리키, 카테고리.Trim(), StringComparison.OrdinalIgnoreCase));
}
