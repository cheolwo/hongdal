using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Ssalddel.Contracts.Common.Documents;
using 살뜰.Infrastructure.Storage.Local;

namespace 살뜰.Services.Documents;

public interface I문서관리Service
{
    Task<IReadOnlyList<문서정책요약응답>> GetPoliciesAsync(CancellationToken cancellationToken = default);
    Task<문서정책요약응답?> UpdatePolicyAsync(string 문서코드, 문서정책수정요청 request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<문서조회요약응답>> ListDocumentsAsync(string? 문서코드 = null, string? 의뢰Id = null, string? 생성상태 = null, CancellationToken cancellationToken = default);
    Task<문서관계그래프응답> GetRelationshipGraphAsync(string 기준StableId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<문서조회로그요약응답>> ListLogsAsync(long? 문서Id = null, CancellationToken cancellationToken = default);
    Task<문서조회요약응답?> CreateDocumentAsync(문서생성요청 request, Stream content, CancellationToken cancellationToken = default);
    Task<문서조회요약응답?> TransitionLifecycleAsync(long id, 문서생명주기변경요청 request, CancellationToken cancellationToken = default);
    Task<문서다운로드응답?> DownloadAsync(long id, CancellationToken cancellationToken = default);
    Task SeedDefaultsAsync(CancellationToken cancellationToken = default);
}

public sealed class 문서관리Service : I문서관리Service
{
    private const string ProtectorPurpose = "Ssalddel.Documents.v1";
    private readonly I문서관리Store _store;
    private readonly IDataProtector _protector;
    private readonly string _storageRoot;
    private readonly object _lifecycleGate = new();

    public 문서관리Service(I문서관리Store store, IDataProtectionProvider dataProtectionProvider, IWebHostEnvironment environment)
    {
        _store = store;
        _protector = dataProtectionProvider.CreateProtector(ProtectorPurpose);
        _storageRoot = Path.Combine(environment.ContentRootPath, "App_Data", "documents");
        Directory.CreateDirectory(_storageRoot);
    }

    public Task SeedDefaultsAsync(CancellationToken cancellationToken = default)
    {
        _store.SeedPolicies(GetDefaultPolicies());
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<문서정책요약응답>> GetPoliciesAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<문서정책요약응답> items = _store.GetPolicies().Select(ToPolicyResponse).ToArray();
        return Task.FromResult(items);
    }

    public Task<문서정책요약응답?> UpdatePolicyAsync(string 문서코드, 문서정책수정요청 request, CancellationToken cancellationToken = default)
    {
        var entity = _store.FindPolicy(문서코드);
        if (entity is null)
        {
            return Task.FromResult<문서정책요약응답?>(null);
        }

        entity.사용여부 = request.사용여부;
        entity.암호화여부 = request.암호화여부;
        entity.다운로드허용여부 = request.다운로드허용여부;
        entity.서명필요여부 = request.서명필요여부;
        entity.자동생성시점 = request.자동생성시점?.Trim() ?? string.Empty;
        entity.조회가능역할목록Json = NormalizeJsonArray(request.조회가능역할목록Json);
        entity.보관일수 = Math.Max(0, request.보관일수);
        entity.수정가능여부 = request.수정가능여부;
        entity.감사로그여부 = request.감사로그여부;
        entity.수정일시 = DateTime.UtcNow;
        _store.UpsertPolicy(entity);

        return Task.FromResult<문서정책요약응답?>(ToPolicyResponse(entity));
    }

    public Task<IReadOnlyList<문서조회요약응답>> ListDocumentsAsync(string? 문서코드 = null, string? 의뢰Id = null, string? 생성상태 = null, CancellationToken cancellationToken = default)
    {
        var items = _store.ListDocuments(문서코드, 의뢰Id, 생성상태).Select(ToDocumentResponse).ToArray();
        return Task.FromResult<IReadOnlyList<문서조회요약응답>>(items);
    }

    public Task<문서관계그래프응답> GetRelationshipGraphAsync(
        string 기준StableId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var normalizedSeed = 기준StableId?.Trim() ?? string.Empty;
        if (!문서StableId.분석(normalizedSeed, out _, out _))
        {
            throw new InvalidOperationException("종류코드:값 형식의 stable ID가 필요합니다.");
        }

        var documents = _store.ListDocuments();
        var documentRelations = documents.ToDictionary(
            document => document.Id,
            DocumentStableIds);
        var discoveredIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            normalizedSeed
        };
        var matchedDocumentIds = new HashSet<long>();

        var changed = true;
        while (changed && matchedDocumentIds.Count < 100)
        {
            changed = false;
            foreach (var document in documents)
            {
                if (matchedDocumentIds.Contains(document.Id))
                {
                    continue;
                }

                var relations = documentRelations[document.Id];
                if (!relations.Any(discoveredIds.Contains))
                {
                    continue;
                }

                matchedDocumentIds.Add(document.Id);
                changed = true;
                foreach (var relation in relations)
                {
                    discoveredIds.Add(relation);
                }

                if (matchedDocumentIds.Count >= 100)
                {
                    break;
                }
            }
        }

        var nodes = documents
            .Where(document => matchedDocumentIds.Contains(document.Id))
            .OrderBy(document => document.생성일시)
            .ThenBy(document => document.Id)
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
                연결StableId목록 = documentRelations[document.Id]
            })
            .ToArray();

        return Task.FromResult(new 문서관계그래프응답
        {
            기준StableId = normalizedSeed,
            발견StableId목록 = discoveredIds
                .OrderBy(문서StableId.흐름순서)
                .ThenBy(stableId => stableId, StringComparer.OrdinalIgnoreCase)
                .ToArray(),
            문서목록 = nodes
        });
    }

    public Task<IReadOnlyList<문서조회로그요약응답>> ListLogsAsync(long? 문서Id = null, CancellationToken cancellationToken = default)
    {
        var items = _store.ListLogs(문서Id).Select(ToLogResponse).ToArray();
        return Task.FromResult<IReadOnlyList<문서조회로그요약응답>>(items);
    }

    public async Task<문서조회요약응답?> CreateDocumentAsync(문서생성요청 request, Stream content, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.의뢰Id)) throw new InvalidOperationException("의뢰Id is required.");
        if (string.IsNullOrWhiteSpace(request.문서코드)) throw new InvalidOperationException("문서코드 is required.");
        if (string.IsNullOrWhiteSpace(request.파일명)) throw new InvalidOperationException("파일명 is required.");

        var policy = _store.FindPolicy(request.문서코드.Trim()) ?? throw new InvalidOperationException("문서종류정책을 찾을 수 없습니다.");
        if (!policy.사용여부)
        {
            throw new InvalidOperationException("비활성화된 문서 종류입니다.");
        }

        var lifecycleStatus = string.IsNullOrWhiteSpace(request.생명주기상태코드)
            ? 문서생명주기상태코드.초안
            : request.생명주기상태코드.Trim();
        if (!문서생명주기상태코드.지원목록.Contains(lifecycleStatus))
        {
            throw new InvalidOperationException($"지원하지 않는 문서 생명주기 상태입니다: {lifecycleStatus}");
        }

        var classification = string.IsNullOrWhiteSpace(request.문서분류코드)
            ? 문서분류Resolver.Resolve(policy.문서코드, request.원천문서종류코드)
            : request.문서분류코드.Trim();
        if (!문서분류코드.지원목록.Contains(classification))
        {
            throw new InvalidOperationException($"지원하지 않는 문서 분류입니다: {classification}");
        }

        await using var memory = new MemoryStream();
        await content.CopyToAsync(memory, cancellationToken);
        var rawBytes = memory.ToArray();
        var contentHash = Convert.ToHexString(SHA256.HashData(rawBytes));
        var sourceLedgerId = string.IsNullOrWhiteSpace(request.원천원장Id)
            ? request.의뢰Id.Trim()
            : request.원천원장Id.Trim();
        var templateVersion = string.IsNullOrWhiteSpace(request.템플릿버전)
            ? "1.0"
            : request.템플릿버전.Trim();
        var sourceDocumentKind = request.원천문서종류코드?.Trim() ?? string.Empty;

        var existingSnapshot = _store.ListDocuments(policy.문서코드, request.의뢰Id.Trim(), 문서상태값.생성완료)
            .FirstOrDefault(candidate =>
                string.Equals(candidate.원천원장Id, sourceLedgerId, StringComparison.Ordinal)
                && candidate.원천원장Revision == request.원천원장Revision
                && string.Equals(candidate.원천문서종류코드, sourceDocumentKind, StringComparison.Ordinal)
                && string.Equals(candidate.템플릿버전, templateVersion, StringComparison.Ordinal)
                && string.Equals(candidate.내용Sha256, contentHash, StringComparison.Ordinal));
        if (existingSnapshot is not null)
        {
            if (HasStoredPayload(existingSnapshot))
            {
                return ToDocumentResponse(existingSnapshot);
            }

            existingSnapshot.생성상태 = 문서상태값.실패;
            existingSnapshot.수정일시 = DateTime.UtcNow;
            _store.UpdateDocument(existingSnapshot);
        }

        var document = new 운송문서
        {
            의뢰Id = request.의뢰Id.Trim(),
            운송원장Id = request.운송원장Id,
            문서코드 = policy.문서코드,
            문서명 = string.IsNullOrWhiteSpace(request.문서명) ? policy.문서명 : request.문서명.Trim(),
            파일명 = request.파일명.Trim(),
            ContentType = string.IsNullOrWhiteSpace(request.ContentType) ? "application/pdf" : request.ContentType.Trim(),
            암호화됨 = request.암호화여부 ?? policy.암호화여부,
            다운로드허용여부 = request.다운로드허용여부 ?? policy.다운로드허용여부,
            수정가능여부 = policy.수정가능여부 && !문서생명주기Planner.불변스냅샷인가(lifecycleStatus),
            보관만료일시 = policy.보관일수 > 0 ? DateTime.UtcNow.AddDays(policy.보관일수) : null,
            생성상태 = 문서상태값.생성완료,
            문서분류코드 = classification,
            생명주기상태코드 = lifecycleStatus,
            원천원장Id = sourceLedgerId,
            원천원장종류코드 = request.원천원장종류코드?.Trim() ?? string.Empty,
            원천원장Revision = request.원천원장Revision,
            원천문서종류코드 = sourceDocumentKind,
            템플릿버전 = templateVersion,
            생성모드코드 = string.IsNullOrWhiteSpace(request.생성모드코드) ? 문서생성모드코드.수동업로드 : request.생성모드코드.Trim(),
            발급주체코드 = string.IsNullOrWhiteSpace(request.발급주체코드) ? 문서발급주체코드.업무담당자 : request.발급주체코드.Trim(),
            외부발급원본대체가능여부 = request.외부발급원본대체가능여부 ?? false,
            구조화스냅샷Json = NormalizeJsonValue(request.구조화스냅샷Json),
            관련StableId목록Json = NormalizeJsonArray(request.관련StableId목록Json),
            내용Sha256 = contentHash,
            생성자 = request.생성자?.Trim() ?? string.Empty,
            생성일시 = DateTime.UtcNow,
            수정일시 = DateTime.UtcNow
        };

        var relativePath = Path.Combine(
            document.문서코드,
            Guid.NewGuid().ToString("N"),
            SanitizeFileName(document.파일명) + ".bin");
        document.파일경로 = relativePath.Replace('\\', '/');
        document.암호화키식별자 = ProtectorPurpose;

        var targetPath = Path.Combine(_storageRoot, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(targetPath) ?? _storageRoot);

        var payload = document.암호화됨 ? _protector.Protect(rawBytes) : rawBytes;
        var temporaryPath = targetPath + $".{Guid.NewGuid():N}.tmp";
        try
        {
            await File.WriteAllBytesAsync(temporaryPath, payload, cancellationToken);
            File.Move(temporaryPath, targetPath, overwrite: false);
            document = _store.AddDocument(document);
        }
        catch
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }

            if (document.Id <= 0 && File.Exists(targetPath))
            {
                File.Delete(targetPath);
            }

            throw;
        }

        _store.AddLog(new 문서조회로그
        {
            문서Id = document.Id,
            행위 = "생성",
            사용자Id = string.Empty,
            사용자명 = request.생성자 ?? string.Empty,
            역할명 = string.Empty,
            ClientIp = string.Empty,
            UserAgent = string.Empty,
            생성일시 = DateTime.UtcNow
        });

        return ToDocumentResponse(document);
    }

    public Task<문서조회요약응답?> TransitionLifecycleAsync(
        long id,
        문서생명주기변경요청 request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (request is null)
        {
            throw new InvalidOperationException("문서 생명주기 변경 요청이 필요합니다.");
        }

        var targetStatus = request.대상상태코드?.Trim() ?? string.Empty;
        if (!문서생명주기상태코드.지원목록.Contains(targetStatus))
        {
            throw new InvalidOperationException($"지원하지 않는 문서 생명주기 상태입니다: {targetStatus}");
        }

        운송문서? document;
        string previousStatus;
        lock (_lifecycleGate)
        {
            document = _store.FindDocument(id);
            if (document is null)
            {
                return Task.FromResult<문서조회요약응답?>(null);
            }

            previousStatus = document.생명주기상태코드;
            if (!문서생명주기Planner.전이가능한가(previousStatus, targetStatus))
            {
                throw new InvalidOperationException(
                    $"문서 생명주기를 {previousStatus}에서 {targetStatus}(으)로 변경할 수 없습니다.");
            }

            if (targetStatus == 문서생명주기상태코드.대체됨)
            {
                if (!request.대체문서Id.HasValue || request.대체문서Id.Value == id)
                {
                    throw new InvalidOperationException("대체됨 상태에는 현재 문서와 다른 대체문서Id가 필요합니다.");
                }

                var replacement = _store.FindDocument(request.대체문서Id.Value);
                if (replacement is null)
                {
                    throw new InvalidOperationException("대체 문서를 찾을 수 없습니다.");
                }

                document.대체문서Id = replacement.Id;
                replacement.이전문서Id = document.Id;
                replacement.수정일시 = DateTime.UtcNow;
                _store.UpdateDocument(replacement);
            }

            document.생명주기상태코드 = targetStatus;
            document.수정가능여부 = !문서생명주기Planner.불변스냅샷인가(targetStatus)
                                  && document.수정가능여부;
            document.수정일시 = DateTime.UtcNow;
            _store.UpdateDocument(document);
        }

        _store.AddLog(new 문서조회로그
        {
            문서Id = id,
            행위 = $"생명주기:{previousStatus}>{targetStatus}",
            사용자Id = string.Empty,
            사용자명 = request.변경자?.Trim() ?? string.Empty,
            역할명 = string.Empty,
            ClientIp = string.Empty,
            UserAgent = string.Empty,
            MetadataJson = JsonSerializer.Serialize(new
            {
                previousStatus,
                targetStatus,
                replacementDocumentId = request.대체문서Id,
                reason = request.변경사유?.Trim() ?? string.Empty
            }),
            생성일시 = DateTime.UtcNow
        });

        return Task.FromResult<문서조회요약응답?>(ToDocumentResponse(document));
    }

    public async Task<문서다운로드응답?> DownloadAsync(long id, CancellationToken cancellationToken = default)
    {
        var document = _store.FindDocument(id);
        if (document is null)
        {
            return null;
        }

        var targetPath = Path.Combine(_storageRoot, document.파일경로.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(targetPath))
        {
            return null;
        }

        var bytes = await File.ReadAllBytesAsync(targetPath, cancellationToken);
        var payload = document.암호화됨 ? _protector.Unprotect(bytes) : bytes;
        if (!string.IsNullOrWhiteSpace(document.내용Sha256))
        {
            var actualHash = Convert.ToHexString(SHA256.HashData(payload));
            if (!string.Equals(actualHash, document.내용Sha256, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("문서 내용 무결성 검증에 실패했습니다.");
            }
        }

        _store.AddLog(new 문서조회로그
        {
            문서Id = document.Id,
            행위 = 문서상태값.다운로드,
            사용자Id = string.Empty,
            사용자명 = string.Empty,
            역할명 = string.Empty,
            ClientIp = string.Empty,
            UserAgent = string.Empty,
            생성일시 = DateTime.UtcNow
        });

        return new 문서다운로드응답
        {
            Id = document.Id,
            파일명 = document.파일명,
            ContentType = document.ContentType,
            내용 = payload
        };
    }

    private static 문서정책요약응답 ToPolicyResponse(문서종류정책 entity)
    {
        return new 문서정책요약응답
        {
            Id = entity.Id,
            문서코드 = entity.문서코드,
            문서명 = entity.문서명,
            사용여부 = entity.사용여부,
            암호화여부 = entity.암호화여부,
            다운로드허용여부 = entity.다운로드허용여부,
            서명필요여부 = entity.서명필요여부,
            자동생성시점 = entity.자동생성시점,
            조회가능역할목록Json = entity.조회가능역할목록Json,
            보관일수 = entity.보관일수,
            수정가능여부 = entity.수정가능여부,
            감사로그여부 = entity.감사로그여부,
            생성일시 = entity.생성일시,
            수정일시 = entity.수정일시
        };
    }

    private static 문서조회요약응답 ToDocumentResponse(운송문서 entity)
    {
        return new 문서조회요약응답
        {
            Id = entity.Id,
            의뢰Id = entity.의뢰Id,
            운송원장Id = entity.운송원장Id,
            문서코드 = entity.문서코드,
            문서명 = entity.문서명,
            파일명 = entity.파일명,
            생성상태 = entity.생성상태,
            문서분류코드 = entity.문서분류코드,
            생명주기상태코드 = entity.생명주기상태코드,
            원천원장Id = entity.원천원장Id,
            원천원장종류코드 = entity.원천원장종류코드,
            원천원장Revision = entity.원천원장Revision,
            원천문서종류코드 = entity.원천문서종류코드,
            템플릿버전 = entity.템플릿버전,
            생성모드코드 = entity.생성모드코드,
            발급주체코드 = entity.발급주체코드,
            외부발급원본대체가능여부 = entity.외부발급원본대체가능여부,
            관련StableId목록 = DocumentStableIds(entity),
            내용Sha256 = entity.내용Sha256,
            이전문서Id = entity.이전문서Id,
            대체문서Id = entity.대체문서Id,
            암호화됨 = entity.암호화됨,
            다운로드허용여부 = entity.다운로드허용여부,
            수정가능여부 = entity.수정가능여부,
            생성일시 = entity.생성일시,
            보관만료일시 = entity.보관만료일시
        };
    }

    private static IReadOnlyList<string> DocumentStableIds(운송문서 document)
    {
        var values = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var stableId in ParseStableIds(document.관련StableId목록Json))
        {
            values.Add(stableId);
        }

        if (!string.IsNullOrWhiteSpace(document.원천원장종류코드)
            && !string.IsNullOrWhiteSpace(document.원천원장Id))
        {
            values.Add(문서StableId.만들기(document.원천원장종류코드, document.원천원장Id));
        }

        if (document.운송원장Id.HasValue)
        {
            values.Add(문서StableId.만들기(문서StableId종류코드.운송실행, document.운송원장Id.Value));
        }

        return values
            .OrderBy(문서StableId.흐름순서)
            .ThenBy(value => value, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IEnumerable<string> ParseStableIds(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            yield break;
        }

        JsonDocument? document = null;
        try
        {
            document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                yield break;
            }

            foreach (var element in document.RootElement.EnumerateArray())
            {
                if (element.ValueKind == JsonValueKind.String
                    && 문서StableId.분석(element.GetString(), out var kind, out var value))
                {
                    yield return 문서StableId.만들기(kind, value);
                }
            }
        }
        finally
        {
            document?.Dispose();
        }
    }

    private static 문서조회로그요약응답 ToLogResponse(문서조회로그 entity)
    {
        return new 문서조회로그요약응답
        {
            Id = entity.Id,
            문서Id = entity.문서Id,
            행위 = entity.행위,
            사용자Id = entity.사용자Id,
            사용자명 = entity.사용자명,
            역할명 = entity.역할명,
            ClientIp = entity.ClientIp,
            UserAgent = entity.UserAgent,
            MetadataJson = entity.MetadataJson,
            생성일시 = entity.생성일시
        };
    }

    private static string NormalizeJsonArray(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "[]";
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

    private static string NormalizeJsonValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return "{}";
        try
        {
            using var document = JsonDocument.Parse(value);
            return document.RootElement.ValueKind is JsonValueKind.Object or JsonValueKind.Array
                ? value
                : "{}";
        }
        catch
        {
            return "{}";
        }
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var buffer = new char[fileName.Length];
        var index = 0;
        foreach (var ch in fileName)
        {
            buffer[index++] = invalidChars.Contains(ch) ? '_' : ch;
        }

        return new string(buffer, 0, index).Trim();
    }

    private bool HasStoredPayload(운송문서 document)
    {
        if (string.IsNullOrWhiteSpace(document.파일경로))
        {
            return false;
        }

        var targetPath = Path.Combine(
            _storageRoot,
            document.파일경로.Replace('/', Path.DirectorySeparatorChar));
        return File.Exists(targetPath);
    }

    private static IEnumerable<문서종류정책> GetDefaultPolicies()
    {
        var now = DateTime.UtcNow;
        var roles = "[\"화주\",\"기사\",\"서버관리자\",\"알선소\"]";

        return new[]
        {
            new 문서종류정책 { 문서코드 = "인수증", 문서명 = "인수증", 사용여부 = true, 암호화여부 = true, 다운로드허용여부 = true, 서명필요여부 = true, 자동생성시점 = "운송인수완료", 조회가능역할목록Json = roles, 보관일수 = 365 * 5, 수정가능여부 = false, 감사로그여부 = true, 생성일시 = now },
            new 문서종류정책 { 문서코드 = "상차인수확인서", 문서명 = "상차 인수 확인서", 사용여부 = true, 암호화여부 = true, 다운로드허용여부 = true, 서명필요여부 = false, 자동생성시점 = "운송상차완료", 조회가능역할목록Json = roles, 보관일수 = 365 * 5, 수정가능여부 = false, 감사로그여부 = true, 생성일시 = now },
            new 문서종류정책 { 문서코드 = "운송확인서", 문서명 = "운송확인서", 사용여부 = true, 암호화여부 = true, 다운로드허용여부 = true, 서명필요여부 = false, 자동생성시점 = "운송완료", 조회가능역할목록Json = roles, 보관일수 = 365 * 5, 수정가능여부 = false, 감사로그여부 = true, 생성일시 = now },
            new 문서종류정책 { 문서코드 = "정산내역서", 문서명 = "정산내역서", 사용여부 = true, 암호화여부 = true, 다운로드허용여부 = false, 서명필요여부 = false, 자동생성시점 = "정산확정", 조회가능역할목록Json = "[\"화주\",\"서버관리자\"]", 보관일수 = 365 * 3, 수정가능여부 = false, 감사로그여부 = true, 생성일시 = now },
            new 문서종류정책 { 문서코드 = "세금계산서연결정보", 문서명 = "세금계산서 연결정보", 사용여부 = true, 암호화여부 = true, 다운로드허용여부 = false, 서명필요여부 = false, 자동생성시점 = "결제완료", 조회가능역할목록Json = "[\"서버관리자\"]", 보관일수 = 365 * 5, 수정가능여부 = true, 감사로그여부 = true, 생성일시 = now },
            new 문서종류정책 { 문서코드 = "결제영수증", 문서명 = "결제영수증", 사용여부 = true, 암호화여부 = true, 다운로드허용여부 = true, 서명필요여부 = false, 자동생성시점 = "결제완료", 조회가능역할목록Json = roles, 보관일수 = 365 * 3, 수정가능여부 = false, 감사로그여부 = true, 생성일시 = now },
            new 문서종류정책 { 문서코드 = "환불확인서", 문서명 = "환불확인서", 사용여부 = true, 암호화여부 = true, 다운로드허용여부 = true, 서명필요여부 = false, 자동생성시점 = "환불처리", 조회가능역할목록Json = roles, 보관일수 = 365 * 3, 수정가능여부 = false, 감사로그여부 = true, 생성일시 = now },
            new 문서종류정책 { 문서코드 = "배차확정서", 문서명 = "배차확정서", 사용여부 = true, 암호화여부 = false, 다운로드허용여부 = true, 서명필요여부 = false, 자동생성시점 = "배차확정", 조회가능역할목록Json = roles, 보관일수 = 365 * 3, 수정가능여부 = false, 감사로그여부 = true, 생성일시 = now },
            new 문서종류정책 { 문서코드 = "주문확인서", 문서명 = "주문 확인서", 사용여부 = true, 암호화여부 = true, 다운로드허용여부 = true, 서명필요여부 = false, 자동생성시점 = "주문결제완료", 조회가능역할목록Json = "[\"주문자\",\"판매자\",\"서버관리자\"]", 보관일수 = 365 * 5, 수정가능여부 = false, 감사로그여부 = true, 생성일시 = now },
            new 문서종류정책 { 문서코드 = "수령확인서", 문서명 = "수령 확인서", 사용여부 = true, 암호화여부 = true, 다운로드허용여부 = true, 서명필요여부 = false, 자동생성시점 = "주문자입고확인", 조회가능역할목록Json = "[\"주문자\",\"판매자\",\"창고관리자\",\"서버관리자\"]", 보관일수 = 365 * 5, 수정가능여부 = false, 감사로그여부 = true, 생성일시 = now },
            new 문서종류정책 { 문서코드 = "피킹완료표", 문서명 = "피킹 완료표", 사용여부 = true, 암호화여부 = true, 다운로드허용여부 = true, 서명필요여부 = false, 자동생성시점 = "창고피킹완료", 조회가능역할목록Json = "[\"창고관리자\",\"판매자\",\"화주\",\"서버관리자\"]", 보관일수 = 365 * 3, 수정가능여부 = false, 감사로그여부 = true, 생성일시 = now },
            new 문서종류정책 { 문서코드 = "포장완료표", 문서명 = "포장 완료표", 사용여부 = true, 암호화여부 = true, 다운로드허용여부 = true, 서명필요여부 = false, 자동생성시점 = "창고포장완료", 조회가능역할목록Json = "[\"창고관리자\",\"판매자\",\"화주\",\"서버관리자\"]", 보관일수 = 365 * 3, 수정가능여부 = false, 감사로그여부 = true, 생성일시 = now },
            new 문서종류정책 { 문서코드 = "출고예정목록", 문서명 = "출고 예정 목록", 사용여부 = true, 암호화여부 = true, 다운로드허용여부 = true, 서명필요여부 = false, 자동생성시점 = "창고출고인계준비완료", 조회가능역할목록Json = "[\"창고관리자\",\"판매자\",\"화주\",\"서버관리자\"]", 보관일수 = 365 * 3, 수정가능여부 = false, 감사로그여부 = true, 생성일시 = now },
            new 문서종류정책 { 문서코드 = "출고인계확인서", 문서명 = "출고 인계 확인서", 사용여부 = true, 암호화여부 = true, 다운로드허용여부 = true, 서명필요여부 = false, 자동생성시점 = "창고출고운송인계완료", 조회가능역할목록Json = "[\"창고관리자\",\"판매자\",\"화주\",\"기사\",\"서버관리자\"]", 보관일수 = 365 * 5, 수정가능여부 = false, 감사로그여부 = true, 생성일시 = now },
            new 문서종류정책 { 문서코드 = "사고분쟁기록", 문서명 = "사고/분쟁기록", 사용여부 = true, 암호화여부 = true, 다운로드허용여부 = false, 서명필요여부 = false, 자동생성시점 = "사고신고", 조회가능역할목록Json = "[\"서버관리자\"]", 보관일수 = 365 * 5, 수정가능여부 = true, 감사로그여부 = true, 생성일시 = now },
            new 문서종류정책 { 문서코드 = 원장관행문서정책코드.검토초안, 문서명 = "원장 관행 문서 초안", 사용여부 = true, 암호화여부 = true, 다운로드허용여부 = true, 서명필요여부 = false, 자동생성시점 = "사용자검토후수동보관", 조회가능역할목록Json = "[\"주문자\",\"화주\",\"관세사\",\"서버관리자\"]", 보관일수 = 365 * 3, 수정가능여부 = false, 감사로그여부 = true, 생성일시 = now },
        };
    }
}
