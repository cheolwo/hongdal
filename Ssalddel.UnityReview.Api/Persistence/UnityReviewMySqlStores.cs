using System.Text.Json;
using Microsoft.Extensions.Options;
using MySqlConnector;
using Ssalddel.Services.WorldProjection;
using Ssalddel.UnityReview.Api.Configuration;

namespace Ssalddel.UnityReview.Api.Persistence;

public sealed class UnityReviewMySqlSchema(IOptions<UnityReviewDatabaseOptions> options)
{
    private readonly SemaphoreSlim gate = new(1, 1);
    private bool initialized;

    public async Task EnsureInitializedAsync(CancellationToken cancellationToken = default)
    {
        if (initialized)
        {
            return;
        }

        await gate.WaitAsync(cancellationToken);
        try
        {
            if (initialized)
            {
                return;
            }

            await using var connection = await OpenConnectionAsync(cancellationToken);
            await ExecuteAsync(connection, """
                CREATE TABLE IF NOT EXISTS unity_review_ledgers (
                    review_item_stable_id VARCHAR(191) NOT NULL,
                    batch_stable_id VARCHAR(160) NOT NULL,
                    review_state_code VARCHAR(64) NOT NULL,
                    revision BIGINT NOT NULL,
                    updated_at_utc DATETIME(6) NOT NULL,
                    record_json LONGTEXT NOT NULL,
                    PRIMARY KEY (review_item_stable_id),
                    INDEX ix_unity_review_batch_state_updated
                        (batch_stable_id, review_state_code, updated_at_utc DESC)
                ) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
                """, cancellationToken);
            await ExecuteAsync(connection, """
                CREATE TABLE IF NOT EXISTS unity_review_capture_uploads (
                    capture_upload_id VARCHAR(191) NOT NULL,
                    review_item_stable_id VARCHAR(191) NOT NULL,
                    uploaded_at_utc DATETIME(6) NOT NULL,
                    record_json LONGTEXT NOT NULL,
                    PRIMARY KEY (capture_upload_id),
                    INDEX ix_unity_review_capture_item_uploaded
                        (review_item_stable_id, uploaded_at_utc DESC)
                ) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
                """, cancellationToken);
            initialized = true;
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task<MySqlConnection> OpenConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        var connectionString = options.Value.ConnectionString?.Trim();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "UnityReviewDatabase:ConnectionString configuration is required.");
        }

        var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private static async Task ExecuteAsync(
        MySqlConnection connection,
        string sql,
        CancellationToken cancellationToken)
    {
        await using var command = new MySqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}

public sealed class MySqlSynty공간조립검토원장Store(UnityReviewMySqlSchema schema)
    : ISynty공간조립검토원장Store
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Synty공간조립검토원장Record?> 조회Async(
        string reviewItemStableId,
        CancellationToken cancellationToken = default)
    {
        await schema.EnsureInitializedAsync(cancellationToken);
        await using var connection = await schema.OpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand("""
            SELECT record_json
            FROM unity_review_ledgers
            WHERE review_item_stable_id = @reviewItemStableId;
            """, connection);
        command.Parameters.AddWithValue("@reviewItemStableId", reviewItemStableId);
        var json = await command.ExecuteScalarAsync(cancellationToken) as string;
        return Deserialize<Synty공간조립검토원장Record>(json);
    }

    public async Task<IReadOnlyList<Synty공간조립검토원장Record>> 목록Async(
        string? batchStableId,
        string? reviewStateCode,
        int take,
        CancellationToken cancellationToken = default)
    {
        await schema.EnsureInitializedAsync(cancellationToken);
        await using var connection = await schema.OpenConnectionAsync(cancellationToken);
        var filters = new List<string>();
        if (!string.IsNullOrWhiteSpace(batchStableId))
        {
            filters.Add("batch_stable_id = @batchStableId");
        }
        if (!string.IsNullOrWhiteSpace(reviewStateCode))
        {
            filters.Add("review_state_code = @reviewStateCode");
        }
        var where = filters.Count == 0 ? string.Empty : "WHERE " + string.Join(" AND ", filters);
        await using var command = new MySqlCommand($"""
            SELECT record_json
            FROM unity_review_ledgers
            {where}
            ORDER BY updated_at_utc DESC
            LIMIT @take;
            """, connection);
        if (!string.IsNullOrWhiteSpace(batchStableId))
        {
            command.Parameters.AddWithValue("@batchStableId", batchStableId.Trim());
        }
        if (!string.IsNullOrWhiteSpace(reviewStateCode))
        {
            command.Parameters.AddWithValue("@reviewStateCode", reviewStateCode.Trim());
        }
        command.Parameters.AddWithValue("@take", Math.Clamp(take, 1, 100));

        var result = new List<Synty공간조립검토원장Record>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(Deserialize<Synty공간조립검토원장Record>(reader.GetString(0))
                       ?? throw new InvalidDataException("Unity 검토 원장 JSON이 손상되었습니다."));
        }
        return result;
    }

    public async Task<bool> 추가Async(
        Synty공간조립검토원장Record record,
        CancellationToken cancellationToken = default)
    {
        await schema.EnsureInitializedAsync(cancellationToken);
        await using var connection = await schema.OpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand("""
            INSERT IGNORE INTO unity_review_ledgers (
                review_item_stable_id,
                batch_stable_id,
                review_state_code,
                revision,
                updated_at_utc,
                record_json)
            VALUES (
                @reviewItemStableId,
                @batchStableId,
                @reviewStateCode,
                @revision,
                @updatedAtUtc,
                @recordJson);
            """, connection);
        AddLedgerParameters(command, record);
        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    public async Task<bool> 교체Async(
        Synty공간조립검토원장Record record,
        long expectedRevision,
        CancellationToken cancellationToken = default)
    {
        await schema.EnsureInitializedAsync(cancellationToken);
        await using var connection = await schema.OpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand("""
            UPDATE unity_review_ledgers
            SET batch_stable_id = @batchStableId,
                review_state_code = @reviewStateCode,
                revision = @revision,
                updated_at_utc = @updatedAtUtc,
                record_json = @recordJson
            WHERE review_item_stable_id = @reviewItemStableId
              AND revision = @expectedRevision;
            """, connection);
        AddLedgerParameters(command, record);
        command.Parameters.AddWithValue("@expectedRevision", expectedRevision);
        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }

    private static void AddLedgerParameters(
        MySqlCommand command,
        Synty공간조립검토원장Record record)
    {
        command.Parameters.AddWithValue("@reviewItemStableId", record.ReviewItemStableId);
        command.Parameters.AddWithValue("@batchStableId", record.BatchStableId);
        command.Parameters.AddWithValue("@reviewStateCode", record.ReviewStateCode);
        command.Parameters.AddWithValue("@revision", record.Revision);
        command.Parameters.AddWithValue("@updatedAtUtc", record.UpdatedAtUtc);
        command.Parameters.AddWithValue("@recordJson", JsonSerializer.Serialize(record, JsonOptions));
    }

    private static T? Deserialize<T>(string? json)
        => string.IsNullOrWhiteSpace(json)
            ? default
            : JsonSerializer.Deserialize<T>(json, JsonOptions);
}

public sealed class MySqlSynty공간조립검토촬영업로드Store(UnityReviewMySqlSchema schema)
    : ISynty공간조립검토촬영업로드Store
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Synty공간조립검토촬영업로드Record?> 조회Async(
        string captureUploadId,
        CancellationToken cancellationToken = default)
    {
        await schema.EnsureInitializedAsync(cancellationToken);
        await using var connection = await schema.OpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand("""
            SELECT record_json
            FROM unity_review_capture_uploads
            WHERE capture_upload_id = @captureUploadId;
            """, connection);
        command.Parameters.AddWithValue("@captureUploadId", captureUploadId);
        var json = await command.ExecuteScalarAsync(cancellationToken) as string;
        return string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<Synty공간조립검토촬영업로드Record>(json, JsonOptions);
    }

    public async Task<bool> 추가Async(
        Synty공간조립검토촬영업로드Record record,
        CancellationToken cancellationToken = default)
    {
        await schema.EnsureInitializedAsync(cancellationToken);
        await using var connection = await schema.OpenConnectionAsync(cancellationToken);
        await using var command = new MySqlCommand("""
            INSERT IGNORE INTO unity_review_capture_uploads (
                capture_upload_id,
                review_item_stable_id,
                uploaded_at_utc,
                record_json)
            VALUES (
                @captureUploadId,
                @reviewItemStableId,
                @uploadedAtUtc,
                @recordJson);
            """, connection);
        command.Parameters.AddWithValue("@captureUploadId", record.CaptureUploadId);
        command.Parameters.AddWithValue("@reviewItemStableId", record.ReviewItemStableId);
        command.Parameters.AddWithValue("@uploadedAtUtc", record.UploadedAtUtc);
        command.Parameters.AddWithValue("@recordJson", JsonSerializer.Serialize(record, JsonOptions));
        return await command.ExecuteNonQueryAsync(cancellationToken) == 1;
    }
}
