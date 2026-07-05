namespace Hongdal.Security
{
    public sealed class JwtOptions
    {
        public const string SectionName = "Jwt";

        public string Issuer { get; set; } = "Hongdal";
        public string Audience { get; set; } = "Hongdal.Client";
        public string SecretKey { get; set; } = string.Empty;
        public int AccessTokenMinutes { get; set; } = 30;
        public int RefreshTokenDays { get; set; } = 14;
    }
}
