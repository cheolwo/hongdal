namespace 홍달.Services
{
    public sealed class MongoDbOptions
    {
        public const string SectionName = "MongoDb";

        public string ConnectionString { get; set; } = string.Empty;
        public string Database { get; set; } = string.Empty;
    }
}