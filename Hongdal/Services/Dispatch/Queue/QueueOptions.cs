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
    }
}
