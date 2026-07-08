namespace 홍달.Services.Dispatch.Queue
{
    public sealed class 배차큐정책Options
    {
        // 추천 유지 기본 시간(초)
        public int 추천유지시간초 { get; set; } = 30;

        // 최대 추천 라운드 수
        public int 최대추천라운드 { get; set; } = 5;

        // 추천 후 공개 전환까지의 기본 대기시간(초)
        public int 공개전환대기초 { get; set; } = 0;

        // 상차지 반경 기준 기사 후보 검색 범위(km)
        public decimal 기사후보검색반경Km { get; set; } = 50m;

        // geo-index에서 먼저 가져올 최대 기사 수
        public int 기사후보최대조회수 { get; set; } = 100;

        // 기사님이 원거리 상차 접근 의사를 밝힌 경우 active-index에서 추가로 검토할 최대 기사 수
        public int 원거리지원후보최대조회수 { get; set; } = 300;

        // 기사님이 허용 반경을 크게 입력하더라도 OS가 검토할 최대 상차 접근 반경(km)
        public decimal 원거리상차접근최대반경Km { get; set; } = 50m;

        // 원거리 후보의 상차 시간창 도착 가능성을 대략 판단할 평균 속도
        public decimal 원거리상차평균속도KmH { get; set; } = 45m;

        // 상차 시간창 종료 전 최소 도착 여유분
        public decimal 원거리상차도착여유분 { get; set; } = 10m;
    }
}
