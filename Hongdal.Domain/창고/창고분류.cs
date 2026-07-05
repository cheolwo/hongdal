namespace 홍달.도메인.창고;

public static class 창고소유자유형
{
    public const string 주문자 = "주문자";
    public const string 판매자 = "판매자";
    public const string 기사 = "기사";
    public const string 운영자 = "운영자";
}

public static class 창고유형
{
    public const string 실제창고 = "실제창고";
    public const string 가상창고 = "가상창고";
    public const string 차량창고 = "차량창고";
    public const string 임시보관소 = "임시보관소";
}

public static class 출고상태
{
    public const string 예정 = "출고예정";
    public const string 준비중 = "출고준비중";
    public const string 출고완료 = "출고완료";
    public const string 취소 = "출고취소";
}

public static class 입고상태
{
    public const string 예정 = "입고예정";
    public const string 운송중 = "운송중";
    public const string 입고완료 = "입고완료";
    public const string 취소 = "입고취소";
}

public static class 재고이동유형
{
    public const string 입고 = "입고";
    public const string 출고 = "출고";
    public const string 이동 = "이동";
    public const string 예약 = "예약";
    public const string 예약해제 = "예약해제";
    public const string 조정 = "조정";
}
