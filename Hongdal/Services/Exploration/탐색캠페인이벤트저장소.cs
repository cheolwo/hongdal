using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Text.Json;

namespace 홍달.Services.Exploration;

public interface I탐색캠페인이벤트저장소
{
    Task AppendAsync(탐색캠페인이벤트 entry, CancellationToken cancellationToken = default);
}

public sealed class 탐색캠페인이벤트저장소 : I탐색캠페인이벤트저장소
{
    private readonly IMongoCollection<탐색캠페인이벤트> _collection;

    public 탐색캠페인이벤트저장소(IMongoClient client, IOptions<MongoDbOptions> options)
    {
        var databaseName = string.IsNullOrWhiteSpace(options.Value.Database)
            ? "hongdal_dev"
            : options.Value.Database;

        var database = client.GetDatabase(databaseName);
        _collection = database.GetCollection<탐색캠페인이벤트>("exploration_campaign_events");
    }

    public Task AppendAsync(탐색캠페인이벤트 entry, CancellationToken cancellationToken = default)
    {
        return _collection.InsertOneAsync(entry, cancellationToken: cancellationToken);
    }
}

public sealed record 탐색캠페인이벤트(
    long 탐색캠페인Id,
    string 이벤트유형,
    DateTime 발생일시Utc,
    string 행위자UserId,
    string 행위자역할,
    string 데이터Json)
{
    public static 탐색캠페인이벤트 Create<TPayload>(long campaignId, string eventType, string actorUserId, string actorRole, TPayload payload)
        => new(
            campaignId,
            eventType,
            DateTime.UtcNow,
            actorUserId,
            actorRole,
            JsonSerializer.Serialize(payload));
}
