namespace 홍달.Services.Options
{
    public sealed class PushNotificationsOptions
    {
        public const string SectionName = "PushNotifications";

        public string ProjectId { get; set; } = string.Empty;
        public string ServiceAccountJsonPath { get; set; } = string.Empty;

        // Legacy FCM endpoint key. Keep it only as a temporary fallback while moving to HTTP v1.
        public string ServerKey { get; set; } = string.Empty;

        public string DefaultTitle { get; set; } = "홍달 추천 알림";
        public string DefaultBodyPrefix { get; set; } = "추천 목록이 업데이트되었습니다.";
    }
}
