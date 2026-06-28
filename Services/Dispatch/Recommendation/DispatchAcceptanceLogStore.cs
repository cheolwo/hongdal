using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace 홍달.Services.Dispatch.Recommendation
{
    public interface IDispatchAcceptanceLogStore
    {
        Task AppendAsync(DispatchAcceptanceLogEntry entry, CancellationToken cancellationToken = default);
    }

    public sealed class DispatchAcceptanceLogStore : IDispatchAcceptanceLogStore
    {
        private readonly IMongoCollection<DispatchAcceptanceLogEntry> _collection;

        public DispatchAcceptanceLogStore(IMongoClient client, IOptions<MongoDbOptions> options)
        {
            var databaseName = string.IsNullOrWhiteSpace(options.Value.Database)
                ? "hongdal_dev"
                : options.Value.Database;

            var database = client.GetDatabase(databaseName);
            _collection = database.GetCollection<DispatchAcceptanceLogEntry>("dispatch_acceptance_logs");
        }

        public Task AppendAsync(DispatchAcceptanceLogEntry entry, CancellationToken cancellationToken = default)
        {
            return _collection.InsertOneAsync(entry, cancellationToken: cancellationToken);
        }
    }

    public sealed record DispatchAcceptanceLogEntry(
        string DriverId,
        string ShipperId,
        string RequestId,
        DateTime AcceptedAtUtc,
        string QueueStatus,
        string DispatchStatus,
        string PaymentStatus);
}


