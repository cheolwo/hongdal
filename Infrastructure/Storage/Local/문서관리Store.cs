using System.Collections.Concurrent;
using 홍달.Services.Documents;

namespace 홍달.Infrastructure.Storage.Local;

public interface I문서관리Store
{
    IReadOnlyList<문서종류정책> GetPolicies();
    문서종류정책? FindPolicy(string 문서코드);
    문서종류정책 UpsertPolicy(문서종류정책 policy);

    운송문서 AddDocument(운송문서 document);
    운송문서? FindDocument(long id);
    IReadOnlyList<운송문서> ListDocuments(string? 문서코드 = null, string? 의뢰Id = null, string? 생성상태 = null);

    문서조회로그 AddLog(문서조회로그 log);
    IReadOnlyList<문서조회로그> ListLogs(long? 문서Id = null);

    void SeedPolicies(IEnumerable<문서종류정책> policies);
}

public sealed class 문서관리Store : I문서관리Store
{
    private readonly ConcurrentDictionary<long, 문서종류정책> _policies = new();
    private readonly ConcurrentDictionary<long, 운송문서> _documents = new();
    private readonly ConcurrentDictionary<long, 문서조회로그> _logs = new();
    private long _policyId;
    private long _documentId;
    private long _logId;

    public IReadOnlyList<문서종류정책> GetPolicies() => _policies.Values.OrderBy(x => x.문서코드).ToArray();

    public 문서종류정책? FindPolicy(string 문서코드)
    {
        if (string.IsNullOrWhiteSpace(문서코드)) return null;
        return _policies.Values.FirstOrDefault(x => string.Equals(x.문서코드, 문서코드.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    public 문서종류정책 UpsertPolicy(문서종류정책 policy)
    {
        if (policy.Id <= 0)
        {
            policy.Id = Interlocked.Increment(ref _policyId);
        }

        _policies[policy.Id] = policy;
        return policy;
    }

    public 운송문서 AddDocument(운송문서 document)
    {
        if (document.Id <= 0)
        {
            document.Id = Interlocked.Increment(ref _documentId);
        }

        _documents[document.Id] = document;
        return document;
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
        if (log.Id <= 0)
        {
            log.Id = Interlocked.Increment(ref _logId);
        }

        _logs[log.Id] = log;
        return log;
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

    public void SeedPolicies(IEnumerable<문서종류정책> policies)
    {
        foreach (var policy in policies)
        {
            var existing = FindPolicy(policy.문서코드);
            if (existing is null)
            {
                UpsertPolicy(policy);
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
    }
}
