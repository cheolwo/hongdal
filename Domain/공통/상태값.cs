namespace 홍달.도메인.공통
{
    public static class 상태값
    {
        public static class 기사운행상태
        {
            public const string 대기 = "대기";
            public const string 운행중 = "운행중";
        }

        public static class 배차대기상태
        {
            public const string 대기 = "대기";
            public const string 확정 = "확정";
        }

        public static class 의뢰상태
        {
            public const string 생성됨 = "생성됨";
        }

        public static class 결제상태
        {
            public const string 결제대기 = "결제대기";
            public const string 결제완료 = "결제완료";
            public const string 환불됨 = "환불됨";

            public static readonly string[] 허용값 = { 결제대기, 결제완료, 환불됨 };
        }

        public static class 배차상태
        {
            public const string 미시작 = "미시작";
            public const string 대기 = "대기";
            public const string 매칭중 = "매칭중";
            public const string 상차중 = "상차중";
            public const string 상차완료 = "상차완료";
            public const string 운송중 = "운송중";
            public const string 하차완료 = "하차완료";
            public const string 인수완료 = "인수완료";
        }
    }
}
