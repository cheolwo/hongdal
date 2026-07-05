namespace 홍달.Services.Options
{
    public sealed class 기사이용료정책Options
    {
        public const string SectionName = "DriverUsagePolicy";

        public bool 무료배차 { get; set; } = true;
        public decimal 기본이용료 { get; set; } = 500m;
        public decimal 추가이용료 { get; set; } = 5000m;
    }
}
