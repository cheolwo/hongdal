using MongoDB.Driver;
using Microsoft.Extensions.Options;

namespace 홍달.Services
{
    public interface IDispatchRecommendationLogStore
    {
        Task AppendAsync(DispatchRecommendationLogEntry entry, CancellationToken cancellationToken = default);
    }

    public sealed class DispatchRecommendationLogStore : IDispatchRecommendationLogStore
    {
        private readonly IMongoCollection<DispatchRecommendationLogEntry> _collection;

        public DispatchRecommendationLogStore(IMongoClient client, IOptions<MongoDbOptions> options)
        {
            var databaseName = string.IsNullOrWhiteSpace(options.Value.Database)
                ? "hongdal_dev"
                : options.Value.Database;

            var database = client.GetDatabase(databaseName);
            _collection = database.GetCollection<DispatchRecommendationLogEntry>("dispatch_recommendation_logs");
        }

        public Task AppendAsync(DispatchRecommendationLogEntry entry, CancellationToken cancellationToken = default)
        {
            return _collection.InsertOneAsync(entry, cancellationToken: cancellationToken);
        }
    }

    public sealed record DispatchRecommendationLogEntry(
        string DriverId,
        DateTime SentAtUtc,
        int RecommendationCount,
        IReadOnlyList<string> RecommendationIds);
}