using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using 살뜰.Services.Documents;

namespace 살뜰.Infrastructure.Storage.Local;

public interface I문서관리Store
{
    IReadOnlyList<문서종류정책> GetPolicies();
    문서종류정책? FindPolicy(string 문서코드);
    문서종류정책 UpsertPolicy(문서종류정책 policy);

    운송문서 AddDocument(운송문서 document);
    운송문서 UpdateDocument(운송문서 document);
    운송문서? FindDocument(long id);
    IReadOnlyList<운송문서> ListDocuments(string? 문서코드 = null, string? 의뢰Id = null, string? 생성상태 = null);

    문서조회로그 AddLog(문서조회로그 log);
    IReadOnlyList<문서조회로그> ListLogs(long? 문서Id = null);

    문서생성Outbox항목 AddOrGetDocumentGenerationOutbox(문서생성Outbox항목 item);
    IReadOnlyList<문서생성Outbox항목> ClaimDocumentGenerationOutbox(
        int take,
        DateTime attemptedAtUtc,
        DateTime retryCutoffUtc,
        DateTime leaseCutoffUtc);
    문서생성Outbox항목 UpdateDocumentGenerationOutbox(문서생성Outbox항목 item);
    IReadOnlyList<문서생성Outbox항목> ListDocumentGenerationOutbox();

    void SeedPolicies(IEnumerable<문서종류정책> policies);
}

public sealed class 문서관리Store : I문서관리Store
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly ConcurrentDictionary<long, 문서종류정책> _policies = new();
    private readonly ConcurrentDictionary<long, 운송문서> _documents = new();
    private readonly ConcurrentDictionary<long, 문서조회로그> _logs = new();
    private readonly ConcurrentDictionary<long, 문서생성Outbox항목> _generationOutbox = new();
    private readonly object _mutationGate = new();
    private readonly string? _snapshotPath;
    private readonly ILogger<문서관리Store>? _logger;
    private long _policyId;
    private long _documentId;
    private long _logId;
    private long _generationOutboxId;

    public 문서관리Store()
    {
    }

    public 문서관리Store(
        IWebHostEnvironment environment,
        ILogger<문서관리Store> logger)
    {
        ArgumentNullException.ThrowIfNull(environment);
        _logger = logger;
        _snapshotPath = Path.Combine(
            environment.ContentRootPath,
            "App_Data",
            "documents",
            "metadata.json");
        LoadSnapshot();
    }

    public IReadOnlyList<문서종류정책> GetPolicies() => _policies.Values.OrderBy(x => x.문서코드).ToArray();

    public 문서종류정책? FindPolicy(string 문서코드)
    {
        if (string.IsNullOrWhiteSpace(문서코드)) return null;
        return _policies.Values.FirstOrDefault(x => string.Equals(x.문서코드, 문서코드.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public 문서종류정책 UpsertPolicy(문서종류정책 policy)
    {
        lock (_mutationGate)
        {
            if (policy.Id <= 0)
            {
                policy.Id = ++_policyId;
            }

            _policies[policy.Id] = policy;
            PersistSnapshotLocked();
            return policy;
        }
    }

    public 운송문서 AddDocument(운송문서 document)
    {
        lock (_mutationGate)
        {
            if (document.Id <= 0)
            {
                document.Id = ++_documentId;
            }

            _documents[document.Id] = document;
            PersistSnapshotLocked();
            return document;
        }
    }

    public 운송문서 UpdateDocument(운송문서 document)
    {
        if (document.Id <= 0)
        {
            throw new InvalidOperationException("저장된 문서만 갱신할 수 있습니다.");
        }

        lock (_mutationGate)
        {
            _documents[document.Id] = document;
            PersistSnapshotLocked();
            return document;
        }
    }

    public 운송문서? FindDocument(long id)
    {
        return _documents.TryGetValue(id, out var item) ? item : null;
    }

    public IReadOnlyList<운송문서> ListDocuments(string? 문서코드 = null, string? 의뢰Id = null, string? 생성상태 = null)
    {
        IEnumerable<운송문서> query = _documents.Values;
        if (!string.IsNullOrWhiteSpace(문서코드)) query = query.Where(x => string.Equals(x.문서코드, 문서코드.Trim(), StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(의뢰Id)) query = query.Where(x => string.Equals(x.의뢰Id, 의뢰Id.Trim(), StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(생성상태)) query = query.Where(x => string.Equals(x.생성상태, 생성상태.Trim(), StringComparison.OrdinalIgnoreCase));

        return query.OrderByDescending(x => x.생성일시).ToArray();
    }

    public 문서조회로그 AddLog(문서조회로그 log)
    {
        lock (_mutationGate)
        {
            if (log.Id <= 0)
            {
                log.Id = ++_logId;
            }

            _logs[log.Id] = log;
            PersistSnapshotLocked();
            return log;
        }
    }

    public IReadOnlyList<문서조회로그> ListLogs(long? 문서Id = null)
    {
        IEnumerable<문서조회로그> query = _logs.Values;
        if (문서Id.HasValue)
        {
            query = query.Where(x => x.문서Id == 문서Id.Value);
        }

        return query.OrderByDescending(x => x.생성일시).ToArray();
    }

    public 문서생성Outbox항목 AddOrGetDocumentGenerationOutbox(문서생성Outbox항목 item)
    {
        if (string.IsNullOrWhiteSpace(item.중복방지Key))
        {
            throw new InvalidOperationException("문서 생성 Outbox 중복방지Key가 필요합니다.");
        }

        lock (_mutationGate)
        {
            var existing = _generationOutbox.Values.FirstOrDefault(candidate =>
                string.Equals(candidate.중복방지Key, item.중복방지Key, StringComparison.Ordinal));
            if (existing is not null)
            {
                return existing;
            }

            item.Id = ++_generationOutboxId;
            _generationOutbox[item.Id] = item;
            PersistSnapshotLocked();
            return item;
        }
    }

    public IReadOnlyList<문서생성Outbox항목> ClaimDocumentGenerationOutbox(
        int take,
        DateTime attemptedAtUtc,
        DateTime retryCutoffUtc,
        DateTime leaseCutoffUtc)
    {
        lock (_mutationGate)
        {
            var items = _generationOutbox.Values
                .Where(item =>
                    (item.처리상태 == 문서생성Outbox상태값.대기
                     && (item.시도횟수 == 0 || item.수정일시Utc <= retryCutoffUtc))
                    || (item.처리상태 == 문서생성Outbox상태값.처리중
                        && item.수정일시Utc <= leaseCutoffUtc))
                .OrderBy(item => item.생성일시Utc)
                .Take(Math.Clamp(take, 1, 500))
                .ToArray();

            if (items.Length == 0)
            {
                return items;
            }

            foreach (var item in items)
            {
                item.처리상태 = 문서생성Outbox상태값.처리중;
                item.시도횟수 += 1;
                item.마지막시도일시Utc = attemptedAtUtc;
                item.수정일시Utc = attemptedAtUtc;
            }

            PersistSnapshotLocked();
            return items;
        }
    }

    public 문서생성Outbox항목 UpdateDocumentGenerationOutbox(문서생성Outbox항목 item)
    {
        if (item.Id <= 0)
        {
            throw new InvalidOperationException("저장된 문서 생성 Outbox만 갱신할 수 있습니다.");
        }

        lock (_mutationGate)
        {
            _generationOutbox[item.Id] = item;
            PersistSnapshotLocked();
            return item;
        }
    }

    public IReadOnlyList<문서생성Outbox항목> ListDocumentGenerationOutbox()
        => _generationOutbox.Values
            .OrderByDescending(item => item.생성일시Utc)
            .ToArray();

    public void SeedPolicies(IEnumerable<문서종류정책> policies)
    {
        lock (_mutationGate)
        {
            foreach (var policy in policies)
            {
                var existing = FindPolicy(policy.문서코드);
                if (existing is null)
                {
                    policy.Id = ++_policyId;
                    _policies[policy.Id] = policy;
                    continue;
                }

                existing.문서명 = policy.문서명;
                existing.사용여부 = policy.사용여부;
                existing.암호화여부 = policy.암호화여부;
                existing.다운로드허용여부 = policy.다운로드허용여부;
                existing.서명필요여부 = policy.서명필요여부;
                existing.자동생성시점 = policy.자동생성시점;
                existing.조회가능역할목록Json = policy.조회가능역할목록Json;
                existing.보관일수 = policy.보관일수;
                existing.수정가능여부 = policy.수정가능여부;
                existing.감사로그여부 = policy.감사로그여부;
                existing.수정일시 = DateTime.UtcNow;
            }

            PersistSnapshotLocked();
        }
    }

    private void LoadSnapshot()
    {
        if (_snapshotPath is null || !File.Exists(_snapshotPath))
        {
            return;
        }

        try
        {
            var snapshot = JsonSerializer.Deserialize<문서관리Snapshot>(
                File.ReadAllText(_snapshotPath),
                JsonOptions) ?? throw new InvalidOperationException("문서 관리 메타데이터가 비어 있습니다.");

            foreach (var policy in snapshot.Policies)
            {
                _policies[policy.Id] = policy;
            }

            foreach (var document in snapshot.Documents)
            {
                _documents[document.Id] = document;
            }

            foreach (var log in snapshot.Logs)
            {
                _logs[log.Id] = log;
            }

            foreach (var item in snapshot.GenerationOutbox)
            {
                _generationOutbox[item.Id] = item;
            }

            _policyId = Math.Max(snapshot.PolicyId, _policies.Keys.DefaultIfEmpty().Max());
            _documentId = Math.Max(snapshot.DocumentId, _documents.Keys.DefaultIfEmpty().Max());
            _logId = Math.Max(snapshot.LogId, _logs.Keys.DefaultIfEmpty().Max());
            _generationOutboxId = Math.Max(
                snapshot.GenerationOutboxId,
                _generationOutbox.Keys.DefaultIfEmpty().Max());

            _logger?.LogInformation(
                "문서 관리 메타데이터 복구 완료. Policies={PolicyCount} Documents={DocumentCount} Outbox={OutboxCount}",
                _policies.Count,
                _documents.Count,
                _generationOutbox.Count);
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException(
                $"문서 관리 메타데이터를 복구할 수 없습니다: {_snapshotPath}",
                exception);
        }
    }

    private void PersistSnapshotLocked()
    {
        if (_snapshotPath is null)
        {
            return;
        }

        var directory = Path.GetDirectoryName(_snapshotPath)
            ?? throw new InvalidOperationException("문서 관리 메타데이터 경로가 올바르지 않습니다.");
        Directory.CreateDirectory(directory);

        var snapshot = new 문서관리Snapshot
        {
            PolicyId = _policyId,
            DocumentId = _documentId,
            LogId = _logId,
            GenerationOutboxId = _generationOutboxId,
            Policies = _policies.Values.OrderBy(item => item.Id).ToArray(),
            Documents = _documents.Values.OrderBy(item => item.Id).ToArray(),
            Logs = _logs.Values.OrderBy(item => item.Id).ToArray(),
            GenerationOutbox = _generationOutbox.Values.OrderBy(item => item.Id).ToArray()
        };
        var temporaryPath = _snapshotPath + ".tmp";

        try
        {
            File.WriteAllText(temporaryPath, JsonSerializer.Serialize(snapshot, JsonOptions));
            File.Move(temporaryPath, _snapshotPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    private sealed class 문서관리Snapshot
    {
        public long PolicyId { get; set; }
        public long DocumentId { get; set; }
        public long LogId { get; set; }
        public long GenerationOutboxId { get; set; }
        public IReadOnlyList<문서종류정책> Policies { get; set; } = [];
        public IReadOnlyList<운송문서> Documents { get; set; } = [];
        public IReadOnlyList<문서조회로그> Logs { get; set; } = [];
        public IReadOnlyList<문서생성Outbox항목> GenerationOutbox { get; set; } = [];
    }
}
