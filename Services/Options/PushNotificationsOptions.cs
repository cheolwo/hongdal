namespace 홍달.Services.Options
{
    public sealed class PushNotificationsOptions
    {
        public const string SectionName = "PushNotifications";

        public string ServerKey { get; set; } = string.Empty;
        public string DefaultTitle { get; set; } = "?띾떖 異붿쿇 ?뚮┝";
        public string DefaultBodyPrefix { get; set; } = "異붿쿇 ?붾Ъ???낅뜲?댄듃?섏뿀?듬땲??";
    }
}



