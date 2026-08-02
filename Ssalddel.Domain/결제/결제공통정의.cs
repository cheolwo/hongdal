namespace 살뜰.도메인.결제;

public static class 결제공통정의
{
    public static class 결제대상유형
    {
        public const int 음식주문 = 10;
        public const int 용달운송의뢰 = 20;
        public const int 기사이용료 = 30;
        public const int 회원구독 = 40;
        public const int 후원 = 50;
        public const int 노드스티커팩 = 60;
        public const int 커뮤니티활동상세열람 = 70;
    }

    public static class 결제제공자
    {
        public const int TossPayments = 10;
        public const int NaverPay = 20;
        public const int KakaoPay = 30;
        public const int ManualBankTransfer = 90;
        public const int FakePG = 990;
    }

    public static class 결제상태
    {
        public const int 요청생성 = 10;
        public const int 결제창진입 = 20;
        public const int 승인대기 = 30;
        public const int 승인완료 = 40;
        public const int 실패 = 50;
        public const int 취소요청 = 60;
        public const int 취소완료 = 70;
        public const int 환불완료 = 80;
    }
}
