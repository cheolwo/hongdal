namespace 홍달.Services
{
    public sealed class PushNotificationsOptions
    {
        public const string SectionName = "PushNotifications";

        public string ServerKey { get; set; } = string.Empty;
        public string DefaultTitle { get; set; } = "홍달 추천 알림";
        public string DefaultBodyPrefix { get; set; } = "추천 화물이 업데이트되었습니다.";
    }
}
