namespace 홍달.Services
{
    public sealed class 기사이용료정책Options
    {
        public const string SectionName = "DriverUsagePolicy";

        public bool 오픈베타무료 { get; set; } = true;
        public decimal 건당이용료 { get; set; } = 500m;
        public decimal 월상한 { get; set; } = 5000m;
    }
}
