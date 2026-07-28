using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.Json;
using Ssalddel.Contracts.Common.Documents;

namespace SsalddelAdmin.Services;

public sealed class 문서관리메모리Service
{
    private readonly ConcurrentDictionary<long, 문서정책요약응답> _policies = new();
    private readonly ConcurrentDictionary<long, 문서조회요약응답> _documents = new();
    private readonly ConcurrentDictionary<long, 문서조회로그요약응답> _logs = new();
    private readonly ConcurrentDictionary<long, byte[]> _content = new();
    private long _policyId;
    private long _documentId;
    private long _logId;

    public 문서관리메모리Service()
    {
        SeedDefaults();
    }

    public IReadOnlyList<문서정책요약응답> GetPolicies()
    {
        return _policies.Values.OrderBy(x => x.문서코드).ToArray();
    }

    public 문서정책요약응답? UpdatePolicy(string documentCode, 문서정책수정요청 request)
    {
        var policy = _policies.Values.FirstOrDefault(x => string.Equals(x.문서코드, documentCode.Trim(), StringComparison.OrdinalIgnoreCase));
        if (policy is null)
        {
            return null;
        }

        policy.사용여부 = request.사용여부;
        policy.암호화여부 = request.암호화여부;
        policy.다운로드허용여부 = request.다운로드허용여부;
        policy.서명필요여부 = request.서명필요여부;
        policy.자동생성시점 = request.자동생성시점?.Trim() ?? string.Empty;
        policy.조회가능역할목록Json = NormalizeJson(request.조회가능역할목록Json);
        policy.보관일수 = Math.Max(0, request.보관일수);
        policy.수정가능여부 = request.수정가능여부;
        policy.감사로그여부 = request.감사로그여부;
        policy.수정일시 = DateTime.UtcNow;
        return policy;
    }

    public IReadOnlyList<문서조회요약응답> GetDocuments(string? documentCode = null, string? requestId = null, string? status = null)
    {
        IEnumerable<문서조회요약응답> query = _documents.Values;
        if (!string.IsNullOrWhiteSpace(documentCode))
        {
            query = query.Where(x => string.Equals(x.문서코드, documentCode.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(requestId))
        {
            query = query.Where(x => string.Equals(x.의뢰Id, requestId.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => string.Equals(x.생성상태, status.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        return query.OrderByDescending(x => x.생성일시).ToArray();
    }

    public IReadOnlyList<문서조회로그요약응답> GetLogs(long? documentId = null)
    {
        IEnumerable<문서조회로그요약응답> query = _logs.Values;
        if (documentId.HasValue)
        {
            query = query.Where(x => x.문서Id == documentId.Value);
        }

        return query.OrderByDescending(x => x.생성일시).ToArray();
    }

    public 문서관계그래프응답 GetRelationshipGraph(string stableId)
    {
        var normalizedSeed = stableId?.Trim() ?? string.Empty;
        if (!문서StableId.분석(normalizedSeed, out _, out _))
        {
            throw new InvalidOperationException("종류코드:값 형식의 stable ID가 필요합니다.");
        }

        var discovered = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { normalizedSeed };
        var matched = new HashSet<long>();
        var changed = true;
        while (changed && matched.Count < 100)
        {
            changed = false;
            foreach (var document in _documents.Values)
            {
                if (matched.Contains(document.Id)
                    || !document.관련StableId목록.Any(discovered.Contains))
                {
                    continue;
                }

                matched.Add(document.Id);
                changed = true;
                foreach (var relation in document.관련StableId목록)
                {
                    discovered.Add(relation);
                }
            }
        }

        return new 문서관계그래프응답
        {
            기준StableId = normalizedSeed,
            발견StableId목록 = discovered
                .OrderBy(문서StableId.흐름순서)
                .ThenBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            문서목록 = _documents.Values
                .Where(document => matched.Contains(document.Id))
                .OrderBy(document => document.생성일시)
                .Select(document => new 문서관계그래프노드응답
                {
                    문서Id = document.Id,
                    문서코드 = document.문서코드,
                    문서명 = document.문서명,
                    문서분류코드 = document.문서분류코드,
                    생명주기상태코드 = document.생명주기상태코드,
                    원천원장Id = document.원천원장Id,
                    원천원장종류코드 = document.원천원장종류코드,
                    원천원장Revision = document.원천원장Revision,
                    내용Sha256 = document.내용Sha256,
                    생성일시 = document.생성일시,
                    연결StableId목록 = document.관련StableId목록
                })
                .ToArray()
        };
    }

    public 문서조회요약응답? TransitionLifecycle(
        long documentId,
        문서생명주기변경요청 request)
    {
        if (!_documents.TryGetValue(documentId, out var document))
        {
            return null;
        }

        var targetStatus = request.대상상태코드?.Trim() ?? string.Empty;
        if (!문서생명주기Planner.전이가능한가(document.생명주기상태코드, targetStatus))
        {
            throw new InvalidOperationException(
                $"문서 생명주기를 {document.생명주기상태코드}에서 {targetStatus}(으)로 변경할 수 없습니다.");
        }

        if (targetStatus == 문서생명주기상태코드.대체됨)
        {
            if (!request.대체문서Id.HasValue
                || request.대체문서Id.Value == documentId
                || !_documents.TryGetValue(request.대체문서Id.Value, out var replacement))
            {
                throw new InvalidOperationException("유효한 대체문서Id가 필요합니다.");
            }

            document.대체문서Id = replacement.Id;
            replacement.이전문서Id = document.Id;
        }

        var previous = document.생명주기상태코드;
        document.생명주기상태코드 = targetStatus;
        document.수정가능여부 = document.수정가능여부
                           && !문서생명주기Planner.불변스냅샷인가(targetStatus);
        AddLog(
            documentId,
            $"생명주기:{previous}>{targetStatus}",
            "admin",
            JsonSerializer.Serialize(new
            {
                previousStatus = previous,
                targetStatus,
                replacementDocumentId = request.대체문서Id,
                reason = request.변경사유?.Trim() ?? string.Empty
            }));
        return document;
    }

    public async Task<문서조회요약응답> UploadDocumentAsync(Stream fileStream, string fileName, string contentType, string documentCode, string documentName, string requestId, long? transportId = null, bool? encrypt = null, bool? allowDownload = null, string? createdBy = null, CancellationToken cancellationToken = default)
    {
        var policy = _policies.Values.FirstOrDefault(x => string.Equals(x.문서코드, documentCode.Trim(), StringComparison.OrdinalIgnoreCase));
        if (policy is null)
        {
            policy = _policies.Values.First();
        }

        using var memory = new MemoryStream();
        await fileStream.CopyToAsync(memory, cancellationToken);
        var bytes = memory.ToArray();

        var id = Interlocked.Increment(ref _documentId);
        var document = new 문서조회요약응답
        {
            Id = id,
            의뢰Id = requestId.Trim(),
            운송원장Id = transportId,
            문서코드 = policy.문서코드,
            문서명 = string.IsNullOrWhiteSpace(documentName) ? policy.문서명 : documentName.Trim(),
            파일명 = fileName,
            생성상태 = "생성완료",
            문서분류코드 = 문서분류Resolver.Resolve(policy.문서코드),
            생명주기상태코드 = 문서생명주기상태코드.초안,
            원천원장Id = requestId.Trim(),
            원천원장종류코드 = transportId.HasValue ? "TransportExecution" : "ManualReference",
            템플릿버전 = "1.0",
            생성모드코드 = 문서생성모드코드.수동업로드,
            발급주체코드 = 문서발급주체코드.플랫폼운영자,
            관련StableId목록 = transportId.HasValue
                ?
                [
                    문서StableId.만들기(문서StableId종류코드.운송의뢰, requestId),
                    문서StableId.만들기(문서StableId종류코드.운송실행, transportId.Value)
                ]
                :
                [
                    문서StableId.만들기(문서StableId종류코드.운송의뢰, requestId)
                ],
            내용Sha256 = Convert.ToHexString(SHA256.HashData(bytes)),
            암호화됨 = encrypt ?? policy.암호화여부,
            다운로드허용여부 = allowDownload ?? policy.다운로드허용여부,
            수정가능여부 = policy.수정가능여부,
            생성일시 = DateTime.UtcNow,
            보관만료일시 = policy.보관일수 > 0 ? DateTime.UtcNow.AddDays(policy.보관일수) : null
        };

        _documents[id] = document;
        _content[id] = bytes;
        AddLog(id, "생성", createdBy);
        return document;
    }

    public byte[]? GetContent(long id)
    {
        return _content.TryGetValue(id, out var bytes) ? bytes : null;
    }

    public void AddLog(
        long documentId,
        string action,
        string? userName = null,
        string? metadataJson = null)
    {
        var id = Interlocked.Increment(ref _logId);
        _logs[id] = new 문서조회로그요약응답
        {
            Id = id,
            문서Id = documentId,
            행위 = action,
            사용자Id = string.Empty,
            사용자명 = userName ?? string.Empty,
            역할명 = string.Empty,
            ClientIp = string.Empty,
            UserAgent = string.Empty,
            MetadataJson = metadataJson ?? string.Empty,
            생성일시 = DateTime.UtcNow
        };
    }

    public void SeedDefaults()
    {
        var now = DateTime.UtcNow;
        SeedPolicy(new 문서정책요약응답 { 문서코드 = "인수증", 문서명 = "인수증", 사용여부 = true, 암호화여부 = true, 다운로드허용여부 = true, 서명필요여부 = true, 자동생성시점 = "운송인수완료", 조회가능역할목록Json = "[\"화주\",\"기사\",\"서버관리자\"]", 보관일수 = 365 * 5, 수정가능여부 = false, 감사로그여부 = true, 생성일시 = now });
        SeedPolicy(new 문서정책요약응답 { 문서코드 = "운송확인서", 문서명 = "운송확인서", 사용여부 = true, 암호화여부 = true, 다운로드허용여부 = true, 서명필요여부 = false, 자동생성시점 = "운송완료", 조회가능역할목록Json = "[\"화주\",\"기사\",\"서버관리자\"]", 보관일수 = 365 * 5, 수정가능여부 = false, 감사로그여부 = true, 생성일시 = now });
        SeedPolicy(new 문서정책요약응답 { 문서코드 = "정산내역서", 문서명 = "정산내역서", 사용여부 = true, 암호화여부 = true, 다운로드허용여부 = false, 서명필요여부 = false, 자동생성시점 = "정산확정", 조회가능역할목록Json = "[\"화주\",\"서버관리자\"]", 보관일수 = 365 * 3, 수정가능여부 = false, 감사로그여부 = true, 생성일시 = now });
        SeedPolicy(new 문서정책요약응답 { 문서코드 = "세금계산서연결정보", 문서명 = "세금계산서 연결정보", 사용여부 = true, 암호화여부 = true, 다운로드허용여부 = false, 서명필요여부 = false, 자동생성시점 = "결제완료", 조회가능역할목록Json = "[\"서버관리자\"]", 보관일수 = 365 * 5, 수정가능여부 = true, 감사로그여부 = true, 생성일시 = now });
        SeedPolicy(new 문서정책요약응답 { 문서코드 = "결제영수증", 문서명 = "결제영수증", 사용여부 = true, 암호화여부 = true, 다운로드허용여부 = true, 서명필요여부 = false, 자동생성시점 = "결제완료", 조회가능역할목록Json = "[\"화주\",\"기사\",\"서버관리자\"]", 보관일수 = 365 * 3, 수정가능여부 = false, 감사로그여부 = true, 생성일시 = now });
        SeedPolicy(new 문서정책요약응답 { 문서코드 = "환불확인서", 문서명 = "환불확인서", 사용여부 = true, 암호화여부 = true, 다운로드허용여부 = true, 서명필요여부 = false, 자동생성시점 = "환불처리", 조회가능역할목록Json = "[\"화주\",\"서버관리자\"]", 보관일수 = 365 * 3, 수정가능여부 = false, 감사로그여부 = true, 생성일시 = now });
        SeedPolicy(new 문서정책요약응답 { 문서코드 = "배차확정서", 문서명 = "배차확정서", 사용여부 = true, 암호화여부 = false, 다운로드허용여부 = true, 서명필요여부 = false, 자동생성시점 = "배차확정", 조회가능역할목록Json = "[\"화주\",\"기사\",\"서버관리자\"]", 보관일수 = 365 * 3, 수정가능여부 = false, 감사로그여부 = true, 생성일시 = now });
        SeedPolicy(new 문서정책요약응답 { 문서코드 = "주문확인서", 문서명 = "주문 확인서", 사용여부 = true, 암호화여부 = true, 다운로드허용여부 = true, 서명필요여부 = false, 자동생성시점 = "주문결제완료", 조회가능역할목록Json = "[\"주문자\",\"판매자\",\"서버관리자\"]", 보관일수 = 365 * 5, 수정가능여부 = false, 감사로그여부 = true, 생성일시 = now });
        SeedPolicy(new 문서정책요약응답 { 문서코드 = "수령확인서", 문서명 = "수령 확인서", 사용여부 = true, 암호화여부 = true, 다운로드허용여부 = true, 서명필요여부 = false, 자동생성시점 = "주문자입고확인", 조회가능역할목록Json = "[\"주문자\",\"판매자\",\"창고관리자\",\"서버관리자\"]", 보관일수 = 365 * 5, 수정가능여부 = false, 감사로그여부 = true, 생성일시 = now });
        SeedPolicy(new 문서정책요약응답 { 문서코드 = "피킹완료표", 문서명 = "피킹 완료표", 사용여부 = true, 암호화여부 = true, 다운로드허용여부 = true, 서명필요여부 = false, 자동생성시점 = "창고피킹완료", 조회가능역할목록Json = "[\"창고관리자\",\"판매자\",\"화주\",\"서버관리자\"]", 보관일수 = 365 * 3, 수정가능여부 = false, 감사로그여부 = true, 생성일시 = now });
        SeedPolicy(new 문서정책요약응답 { 문서코드 = "포장완료표", 문서명 = "포장 완료표", 사용여부 = true, 암호화여부 = true, 다운로드허용여부 = true, 서명필요여부 = false, 자동생성시점 = "창고포장완료", 조회가능역할목록Json = "[\"창고관리자\",\"판매자\",\"화주\",\"서버관리자\"]", 보관일수 = 365 * 3, 수정가능여부 = false, 감사로그여부 = true, 생성일시 = now });
        SeedPolicy(new 문서정책요약응답 { 문서코드 = "출고예정목록", 문서명 = "출고 예정 목록", 사용여부 = true, 암호화여부 = true, 다운로드허용여부 = true, 서명필요여부 = false, 자동생성시점 = "창고출고인계준비완료", 조회가능역할목록Json = "[\"창고관리자\",\"판매자\",\"화주\",\"서버관리자\"]", 보관일수 = 365 * 3, 수정가능여부 = false, 감사로그여부 = true, 생성일시 = now });
        SeedPolicy(new 문서정책요약응답 { 문서코드 = "출고인계확인서", 문서명 = "출고 인계 확인서", 사용여부 = true, 암호화여부 = true, 다운로드허용여부 = true, 서명필요여부 = false, 자동생성시점 = "창고출고운송인계완료", 조회가능역할목록Json = "[\"창고관리자\",\"판매자\",\"화주\",\"기사\",\"서버관리자\"]", 보관일수 = 365 * 5, 수정가능여부 = false, 감사로그여부 = true, 생성일시 = now });
        SeedPolicy(new 문서정책요약응답 { 문서코드 = "사고분쟁기록", 문서명 = "사고/분쟁기록", 사용여부 = true, 암호화여부 = true, 다운로드허용여부 = false, 서명필요여부 = false, 자동생성시점 = "사고신고", 조회가능역할목록Json = "[\"서버관리자\"]", 보관일수 = 365 * 5, 수정가능여부 = true, 감사로그여부 = true, 생성일시 = now });

        if (_documents.IsEmpty)
        {
            var sampleContent = JsonSerializer.SerializeToUtf8Bytes(new { title = "살뜰 인수증 샘플", requestId = "REQ-SAMPLE-001" });
            var orderStableId = 문서StableId.만들기(문서StableId종류코드.주문참조, "ORDER-SAMPLE-001");
            var inboundStableId = 문서StableId.만들기(문서StableId종류코드.입고요청, 21);
            var inventoryStableId = 문서StableId.만들기(문서StableId종류코드.입고상품, 31);
            var outboundStableId = 문서StableId.만들기(문서StableId종류코드.출고예정, 41);
            var transportRequestStableId = 문서StableId.만들기(문서StableId종류코드.운송의뢰, "REQ-SAMPLE-001");
            var transportExecutionStableId = 문서StableId.만들기(문서StableId종류코드.운송실행, 101);

            AddSampleDocument(
                "출고예정목록",
                "출고 예정 목록",
                "출고예정목록-41.txt",
                "41",
                "WarehouseOutboundPlan",
                문서분류코드.업무작업지,
                문서생명주기상태코드.확인완료,
                now.AddMinutes(-20),
                [orderStableId, inboundStableId, inventoryStableId, outboundStableId]);
            AddSampleDocument(
                "출고인계확인서",
                "출고 인계 확인서",
                "출고인계확인서-41.txt",
                "41",
                "WarehouseOutboundPlan",
                문서분류코드.수행증빙,
                문서생명주기상태코드.발행완료,
                now.AddMinutes(-10),
                [orderStableId, inboundStableId, inventoryStableId, outboundStableId, transportRequestStableId]);

            var sampleId = Interlocked.Increment(ref _documentId);
            _documents[sampleId] = new 문서조회요약응답
            {
                Id = sampleId,
                의뢰Id = "REQ-SAMPLE-001",
                운송원장Id = 101,
                문서코드 = "인수증",
                문서명 = "인수증",
                파일명 = "인수증-REQ-SAMPLE-001.pdf",
                생성상태 = "생성완료",
                문서분류코드 = 문서분류코드.수행증빙,
                생명주기상태코드 = 문서생명주기상태코드.발행완료,
                원천원장Id = "101",
                원천원장종류코드 = "TransportExecution",
                원천원장Revision = 7,
                원천문서종류코드 = "DELIVERY_RECEIPT",
                템플릿버전 = "1.0",
                생성모드코드 = 문서생성모드코드.업무이벤트자동생성,
                발급주체코드 = 문서발급주체코드.플랫폼,
                관련StableId목록 = [transportRequestStableId, transportExecutionStableId],
                내용Sha256 = Convert.ToHexString(SHA256.HashData(sampleContent)),
                암호화됨 = true,
                다운로드허용여부 = true,
                수정가능여부 = false,
                생성일시 = now,
                보관만료일시 = now.AddYears(5)
            };

            _content[sampleId] = sampleContent;
            AddLog(sampleId, "생성", "sample");
        }
    }

    private void AddSampleDocument(
        string documentCode,
        string documentName,
        string fileName,
        string sourceId,
        string sourceType,
        string classification,
        string lifecycleStatus,
        DateTime createdAt,
        IReadOnlyList<string> stableIds)
    {
        var content = JsonSerializer.SerializeToUtf8Bytes(new { documentName, sourceId });
        var id = Interlocked.Increment(ref _documentId);
        _documents[id] = new 문서조회요약응답
        {
            Id = id,
            의뢰Id = sourceId,
            문서코드 = documentCode,
            문서명 = documentName,
            파일명 = fileName,
            생성상태 = "생성완료",
            문서분류코드 = classification,
            생명주기상태코드 = lifecycleStatus,
            원천원장Id = sourceId,
            원천원장종류코드 = sourceType,
            템플릿버전 = "1.0",
            생성모드코드 = 문서생성모드코드.업무이벤트자동생성,
            발급주체코드 = 문서발급주체코드.플랫폼,
            관련StableId목록 = stableIds,
            내용Sha256 = Convert.ToHexString(SHA256.HashData(content)),
            암호화됨 = true,
            다운로드허용여부 = true,
            수정가능여부 = false,
            생성일시 = createdAt,
            보관만료일시 = createdAt.AddYears(5)
        };
        _content[id] = content;
        AddLog(id, "생성", "sample");
    }

    private void SeedPolicy(문서정책요약응답 policy)
    {
        policy.Id = policy.Id <= 0 ? Interlocked.Increment(ref _policyId) : policy.Id;
        _policies[policy.Id] = policy;
    }

    private static string NormalizeJson(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "[]";
        }

        try
        {
            using var doc = JsonDocument.Parse(value);
            return doc.RootElement.ValueKind == JsonValueKind.Array ? value : "[]";
        }
        catch
        {
            return "[]";
        }
    }
}
