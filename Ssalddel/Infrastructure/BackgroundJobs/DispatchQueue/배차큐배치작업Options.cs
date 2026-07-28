namespace 살뜰.Infrastructure.BackgroundJobs.DispatchQueue
{
    public sealed class 배차큐배치작업Options
    {
        public const string SectionName = "DispatchQueueJobs";

        public int 큐스캔주기초 { get; set; } = 30;
        public int 추천만료정리주기초 { get; set; } = 30;
        public int 알림발송주기초 { get; set; } = 15;
        public int 결제승인Outbox발행주기초 { get; set; } = 15;
        public int 음식마트원장동기화주기초 { get; set; } = 30;
        public int 통관상태동기화주기초 { get; set; } = 1800;
        public int 처리배치크기 { get; set; } = 100;
    }
}
