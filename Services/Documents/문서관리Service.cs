using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using 홍달.Infrastructure.Storage.Local;

namespace 홍달.Services.Documents;

public interface I문서관리Service
{
    Task<IReadOnlyList<문서정책요약응답>> GetPoliciesAsync(CancellationToken cancellationToken = default);
    Task<문서정책요약응답?> UpdatePolicyAsync(string 문서코드, 문서정책수정요청 request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<문서조회요약응답>> ListDocumentsAsync(string? 문서코드 = null, string? 의뢰Id = null, string? 생성상태 = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<문서조회로그요약응답>> ListLogsAsync(long? 문서Id = null, CancellationToken cancellationToken = default);
    Task<문서조회요약응답?> CreateDocumentAsync(문서생성요청 request, Stream content, CancellationToken cancellationToken = default);
    Task<문서다운로드응답?> DownloadAsync(long id, CancellationToken cancellationToken = default);
    Task SeedDefaultsAsync(CancellationToken cancellationToken = default);
}

public sealed class 문서관리Service : I문서관리Service
{
    private const string ProtectorPurpose = "Hongdal.Documents.v1";
    private readonly I문서관리Store _store;
    private readonly IDataProtector _protector;
    private readonly string _storageRoot;

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

        return Task.FromResult<문서정책요약응답?>(ToPolicyResponse(entity));
    }

    public Task<IReadOnlyList<문서조회요약응답>> ListDocumentsAsync(string? 문서코드 = null, string? 의뢰Id = null, string? 생성상태 = null, CancellationToken cancellationToken = default)
    {
        var items = _store.ListDocuments(문서코드, 의뢰Id, 생성상태).Select(ToDocumentResponse).ToArray();
        return Task.FromResult<IReadOnlyList<문서조회요약응답>>(items);
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

        var document = new 운송문서
        {
            의뢰Id = request.의뢰Id.Trim(),
            배송운송Id = request.배송운송Id,
            문서코드 = policy.문서코드,
            문서명 = string.IsNullOrWhiteSpace(request.문서명) ? policy.문서명 : request.문서명.Trim(),
            파일명 = request.파일명.Trim(),
            ContentType = string.IsNullOrWhiteSpace(request.ContentType) ? "application/pdf" : request.ContentType.Trim(),
            암호화됨 = request.암호화여부 ?? policy.암호화여부,
            다운로드허용여부 = request.다운로드허용여부 ?? policy.다운로드허용여부,
            수정가능여부 = policy.수정가능여부,
            보관만료일시 = policy.보관일수 > 0 ? DateTime.UtcNow.AddDays(policy.보관일수) : null,
            생성상태 = 문서상태값.생성완료,
            생성자 = request.생성자?.Trim() ?? string.Empty,
            생성일시 = DateTime.UtcNow,
            수정일시 = DateTime.UtcNow
        };

        document = _store.AddDocument(document);
        var relativePath = Path.Combine(document.문서코드, document.Id.ToString(), SanitizeFileName(document.파일명) + ".bin");
        document.파일경로 = relativePath.Replace('\\', '/');
        document.암호화키식별자 = ProtectorPurpose;

        var targetPath = Path.Combine(_storageRoot, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(targetPath) ?? _storageRoot);

        await using var memory = new MemoryStream();
        await content.CopyToAsync(memory, cancellationToken);
        var rawBytes = memory.ToArray();
        var payload = document.암호화됨 ? _protector.Protect(rawBytes) : rawBytes;
        await File.WriteAllBytesAsync(targetPath, payload, cancellationToken);

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
            배송운송Id = entity.배송운송Id,
            문서코드 = entity.문서코드,
            문서명 = entity.문서명,
            파일명 = entity.파일명,
            생성상태 = entity.생성상태,
            암호화됨 = entity.암호화됨,
            다운로드허용여부 = entity.다운로드허용여부,
            수정가능여부 = entity.수정가능여부,
            생성일시 = entity.생성일시,
            보관만료일시 = entity.보관만료일시
        };
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
            생성일시 = entity.생성일시
        };
    }

    private static string NormalizeJsonArray(string value)
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

    private static IEnumerable<문서종류정책> GetDefaultPolicies()
    {
        var now = DateTime.UtcNow;
        var roles = "[\"화주\",\"기사\",\"서버관리자\",\"알선소\"]";

        return new[]
        {
            new 문서종류정책 { 문서코드 = "인수증", 문서명 = "인수증", 사용여부 = true, 암호화여부 = true, 다운로드허용여부 = true, 서명필요여부 = true, 자동생성시점 = "운송인수완료", 조회가능역할목록Json = roles, 보관일수 = 365 * 5, 수정가능여부 = false, 감사로그여부 = true, 생성일시 = now },
            new 문서종류정책 { 문서코드 = "운송확인서", 문서명 = "운송확인서", 사용여부 = true, 암호화여부 = true, 다운로드허용여부 = true, 서명필요여부 = false, 자동생성시점 = "운송완료", 조회가능역할목록Json = roles, 보관일수 = 365 * 5, 수정가능여부 = false, 감사로그여부 = true, 생성일시 = now },
            new 문서종류정책 { 문서코드 = "정산내역서", 문서명 = "정산내역서", 사용여부 = true, 암호화여부 = true, 다운로드허용여부 = false, 서명필요여부 = false, 자동생성시점 = "정산확정", 조회가능역할목록Json = "[\"화주\",\"서버관리자\"]", 보관일수 = 365 * 3, 수정가능여부 = false, 감사로그여부 = true, 생성일시 = now },
            new 문서종류정책 { 문서코드 = "세금계산서연결정보", 문서명 = "세금계산서 연결정보", 사용여부 = true, 암호화여부 = true, 다운로드허용여부 = false, 서명필요여부 = false, 자동생성시점 = "결제완료", 조회가능역할목록Json = "[\"서버관리자\"]", 보관일수 = 365 * 5, 수정가능여부 = true, 감사로그여부 = true, 생성일시 = now },
            new 문서종류정책 { 문서코드 = "결제영수증", 문서명 = "결제영수증", 사용여부 = true, 암호화여부 = true, 다운로드허용여부 = true, 서명필요여부 = false, 자동생성시점 = "결제완료", 조회가능역할목록Json = roles, 보관일수 = 365 * 3, 수정가능여부 = false, 감사로그여부 = true, 생성일시 = now },
            new 문서종류정책 { 문서코드 = "환불확인서", 문서명 = "환불확인서", 사용여부 = true, 암호화여부 = true, 다운로드허용여부 = true, 서명필요여부 = false, 자동생성시점 = "환불처리", 조회가능역할목록Json = roles, 보관일수 = 365 * 3, 수정가능여부 = false, 감사로그여부 = true, 생성일시 = now },
            new 문서종류정책 { 문서코드 = "배차확정서", 문서명 = "배차확정서", 사용여부 = true, 암호화여부 = false, 다운로드허용여부 = true, 서명필요여부 = false, 자동생성시점 = "배차확정", 조회가능역할목록Json = roles, 보관일수 = 365 * 3, 수정가능여부 = false, 감사로그여부 = true, 생성일시 = now },
            new 문서종류정책 { 문서코드 = "사고분쟁기록", 문서명 = "사고/분쟁기록", 사용여부 = true, 암호화여부 = true, 다운로드허용여부 = false, 서명필요여부 = false, 자동생성시점 = "사고신고", 조회가능역할목록Json = "[\"서버관리자\"]", 보관일수 = 365 * 5, 수정가능여부 = true, 감사로그여부 = true, 생성일시 = now },
        };
    }
}
