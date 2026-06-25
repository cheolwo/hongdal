namespace 홍달.Services
{
    public sealed class RedisOptions
    {
        public const string SectionName = "Redis";

        public string ConnectionString { get; set; } = string.Empty;
    }
}