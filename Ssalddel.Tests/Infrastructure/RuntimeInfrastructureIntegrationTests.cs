using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using StackExchange.Redis;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;

namespace Ssalddel.Tests.Infrastructure;

public sealed class RuntimeInfrastructureIntegrationTests
{
    [Fact]
    public async Task MySql_마이그레이션을_적용하고_GmailOutbox테이블을_조회한다()
    {
        var connectionString = RequiredSetting("SSALDDEL_TEST_MYSQL_CONNECTION");
        if (connectionString is null)
        {
            return;
        }

        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 4, 0)),
                mysql => mysql.MigrationsAssembly("Ssalddel"))
            .Options;

        await using var context = new SsalddelContext(options, new PassThroughEncryptionService());
        await context.Database.MigrateAsync();
        Assert.True(await context.Database.CanConnectAsync());

        await context.Database.OpenConnectionAsync();
        await using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM community_post_email_notification_outbox";
        var count = await command.ExecuteScalarAsync();
        Assert.NotNull(count);
    }

    [Fact]
    public async Task MongoDb_임시문서를_왕복하고_정리한다()
    {
        var connectionString = RequiredSetting("SSALDDEL_TEST_MONGODB_CONNECTION");
        var databaseName = RequiredSetting("SSALDDEL_TEST_MONGODB_DATABASE");
        if (connectionString is null || databaseName is null)
        {
            return;
        }

        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);
        await database.RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1));

        var collection = database.GetCollection<BsonDocument>("integration_probe");
        var id = $"runtime-{Guid.NewGuid():N}";
        try
        {
            await collection.InsertOneAsync(new BsonDocument
            {
                ["_id"] = id,
                ["createdAtUtc"] = DateTime.UtcNow
            });
            var stored = await collection.Find(Builders<BsonDocument>.Filter.Eq("_id", id)).SingleAsync();
            Assert.Equal(id, stored["_id"].AsString);
        }
        finally
        {
            await collection.DeleteOneAsync(Builders<BsonDocument>.Filter.Eq("_id", id));
        }
    }

    [Fact]
    public async Task Redis_임시키를_왕복하고_정리한다()
    {
        var connectionString = RequiredSetting("SSALDDEL_TEST_REDIS_CONNECTION");
        if (connectionString is null)
        {
            return;
        }

        await using var redis = await ConnectionMultiplexer.ConnectAsync(connectionString);
        var database = redis.GetDatabase();
        var key = $"ssalddel:integration:{Guid.NewGuid():N}";
        var value = Guid.NewGuid().ToString("N");
        try
        {
            Assert.True(await database.StringSetAsync(key, value, TimeSpan.FromMinutes(2)));
            Assert.Equal(value, (string?)await database.StringGetAsync(key));
        }
        finally
        {
            await database.KeyDeleteAsync(key);
        }
    }

    private static string? RequiredSetting(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        if (!string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        Assert.False(
            string.Equals(
                Environment.GetEnvironmentVariable("SSALDDEL_INFRA_INTEGRATION_REQUIRED"),
                "true",
                StringComparison.OrdinalIgnoreCase),
            $"{name} is required when SSALDDEL_INFRA_INTEGRATION_REQUIRED=true.");
        return null;
    }

    private sealed class PassThroughEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;

        public string? Unprotect(string? value) => value;
    }
}
