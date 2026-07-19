using FluentResults;
using Ssalddel.ApiMetadata;
using Ssalddel.Contracts.Common.ContractManagement;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using 살뜰.Services.Options;

namespace Ssalddel.Services.Community;

public interface I주문원장서명저장소
{
    Task<주문원장서명기록?> 조회Async(
        string 주문원장Id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<string, 주문원장서명기록>> 목록조회Async(
        IEnumerable<string> 주문원장Ids,
        CancellationToken cancellationToken = default);

    Task<주문원장서명기록> 저장Async(
        string 주문원장Id,
        string 커뮤니티Id,
        ContractElectronicSignatureBundle 서명묶음,
        long? 기대Revision,
        string 변경자UserId,
        CancellationToken cancellationToken = default);
}

public sealed record 주문원장서명기록(
    string 주문원장Id,
    string 커뮤니티Id,
    long Revision,
    ContractElectronicSignatureBundle 서명묶음,
    string 최종변경자UserId,
    DateTimeOffset 수정시각Utc);

public sealed class Mongo주문원장서명저장소 : I주문원장서명저장소
{
    private const string CollectionName = "community_ledger_order_signatures";
    private readonly IMongoCollection<주문원장서명문서> _collection;
    private readonly SemaphoreSlim _indexLock = new(1, 1);
    private bool _indexesReady;

    public Mongo주문원장서명저장소(IMongoClient mongoClient, IOptions<MongoDbOptions> options)
    {
        if (string.IsNullOrWhiteSpace(options.Value.Database))
        {
            throw new InvalidOperationException("MongoDb:Database configuration is required.");
        }

        _collection = mongoClient
            .GetDatabase(options.Value.Database.Trim())
            .GetCollection<주문원장서명문서>(CollectionName);
    }

    public async Task<주문원장서명기록?> 조회Async(
        string 주문원장Id,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);
        var document = await _collection
            .Find(x => x.주문원장Id == 주문원장Id)
            .FirstOrDefaultAsync(cancellationToken);
        return document is null ? null : ToRecord(document);
    }

    public async Task<IReadOnlyDictionary<string, 주문원장서명기록>> 목록조회Async(
        IEnumerable<string> 주문원장Ids,
        CancellationToken cancellationToken = default)
    {
        var ids = 주문원장Ids
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (ids.Length == 0)
        {
            return new Dictionary<string, 주문원장서명기록>(StringComparer.OrdinalIgnoreCase);
        }

        await EnsureIndexesAsync(cancellationToken);
        var documents = await _collection
            .Find(Builders<주문원장서명문서>.Filter.In(x => x.주문원장Id, ids))
            .ToListAsync(cancellationToken);
        return documents
            .Select(ToRecord)
            .ToDictionary(x => x.주문원장Id, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<주문원장서명기록> 저장Async(
        string 주문원장Id,
        string 커뮤니티Id,
        ContractElectronicSignatureBundle 서명묶음,
        long? 기대Revision,
        string 변경자UserId,
        CancellationToken cancellationToken = default)
    {
        await EnsureIndexesAsync(cancellationToken);
        var existing = await _collection
            .Find(x => x.주문원장Id == 주문원장Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (기대Revision.HasValue && (existing?.Revision ?? 0) != 기대Revision.Value)
        {
            throw new InvalidOperationException("서명 정보가 다른 요청에서 먼저 변경되었습니다.");
        }

        var now = DateTime.UtcNow;
        var document = new 주문원장서명문서
        {
            주문원장Id = 주문원장Id,
            커뮤니티Id = 커뮤니티Id,
            Revision = (existing?.Revision ?? 0) + 1,
            서명묶음 = ToDocument(서명묶음),
            최종변경자UserId = 변경자UserId,
            생성시각Utc = existing?.생성시각Utc ?? now,
            수정시각Utc = now
        };

        if (existing is null)
        {
            try
            {
                await _collection.InsertOneAsync(document, cancellationToken: cancellationToken);
            }
            catch (MongoWriteException exception) when (exception.WriteError?.Category == ServerErrorCategory.DuplicateKey)
            {
                throw new InvalidOperationException("서명 정보가 다른 요청에서 먼저 생성되었습니다.", exception);
            }
        }
        else
        {
            var result = await _collection.ReplaceOneAsync(
                x => x.주문원장Id == 주문원장Id && x.Revision == existing.Revision,
                document,
                cancellationToken: cancellationToken);
            if (result.ModifiedCount != 1)
            {
                throw new InvalidOperationException("서명 정보가 다른 요청에서 먼저 변경되었습니다.");
            }
        }

        return ToRecord(document);
    }

    private async Task EnsureIndexesAsync(CancellationToken cancellationToken)
    {
        if (_indexesReady)
        {
            return;
        }

        await _indexLock.WaitAsync(cancellationToken);
        try
        {
            if (_indexesReady)
            {
                return;
            }

            await _collection.Indexes.CreateOneAsync(
                new CreateIndexModel<주문원장서명문서>(
                    Builders<주문원장서명문서>.IndexKeys
                        .Ascending(x => x.커뮤니티Id)
                        .Descending(x => x.수정시각Utc),
                    new CreateIndexOptions { Name = "ix_order_signature_community" }),
                cancellationToken: cancellationToken);
            _indexesReady = true;
        }
        finally
        {
            _indexLock.Release();
        }
    }

    private static 주문원장서명기록 ToRecord(주문원장서명문서 document)
        => new(
            document.주문원장Id,
            document.커뮤니티Id,
            document.Revision,
            ToBundle(document.서명묶음),
            document.최종변경자UserId,
            AsUtcOffset(document.수정시각Utc));

    private static 주문원장서명묶음문서 ToDocument(ContractElectronicSignatureBundle bundle)
        => new()
        {
            계약문서번호 = bundle.ContractNumber,
            문서Hash = bundle.DocumentHash,
            서명요청목록 = bundle.SignatureRequests.Select(x => new 주문원장서명요청문서
            {
                당사자Id = x.PartyId,
                역할Code = x.RoleCode,
                서명자표시명 = x.SignerDisplayName,
                필수서명자여부 = x.IsRequiredSigner,
                요청시각Utc = x.RequestedAtUtc?.UtcDateTime
            }).ToArray(),
            서명증적목록 = bundle.Evidences.Select(x => new 주문원장서명증적문서
            {
                당사자Id = x.PartyId,
                서명자표시명 = x.SignerDisplayName,
                서명방법Code = x.SignatureMethodCode,
                서명문서Hash = x.SignedDocumentHash,
                동의문Hash = x.ConsentTextHash,
                서명증적Hash = x.SignatureEvidenceHash,
                서명시각Utc = x.SignedAtUtc.UtcDateTime,
                접속IpHash = x.ClientIpHash
            }).ToArray(),
            생성시각Utc = bundle.CreatedAtUtc.UtcDateTime,
            만료시각Utc = bundle.ExpiresAtUtc?.UtcDateTime
        };

    private static ContractElectronicSignatureBundle ToBundle(주문원장서명묶음문서 document)
        => new(
            document.계약문서번호,
            document.문서Hash,
            document.서명요청목록.Select(x => new ContractSignatureRequest(
                x.당사자Id,
                x.역할Code,
                x.서명자표시명,
                x.필수서명자여부,
                x.요청시각Utc.HasValue ? AsUtcOffset(x.요청시각Utc.Value) : null)).ToArray(),
            document.서명증적목록.Select(x => new ContractSignatureEvidence(
                x.당사자Id,
                x.서명자표시명,
                x.서명방법Code,
                x.서명문서Hash,
                x.동의문Hash,
                x.서명증적Hash,
                AsUtcOffset(x.서명시각Utc),
                x.접속IpHash)).ToArray(),
            AsUtcOffset(document.생성시각Utc),
            document.만료시각Utc.HasValue ? AsUtcOffset(document.만료시각Utc.Value) : null);

    private static DateTimeOffset AsUtcOffset(DateTime value)
        => new(DateTime.SpecifyKind(value, DateTimeKind.Utc));
}

public interface I주문원장서명UseCase
{
    Task<Result<주문원장서명상태Dto>> 서명요청준비Async(
        string 주문원장Id,
        주문원장서명요청준비요청 request,
        string 현재UserId,
        CancellationToken cancellationToken = default);

    Task<Result<주문원장서명상태Dto>> 서명등록Async(
        string 주문원장Id,
        주문원장서명등록요청 request,
        string 현재UserId,
        CancellationToken cancellationToken = default);

    Task<Result<주문원장서명상태Dto>> 조회Async(
        string 주문원장Id,
        string 현재UserId,
        CancellationToken cancellationToken = default);
}

[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelUseCase("주문자 서명", Summary = "개별 주문 원장에 주문자 서명 증적을 연결하고 공동주문에서는 상태만 집계합니다.")]
[SsalddelUseCaseActor(SsalddelActor.CommunityMember)]
public sealed class 주문원장서명UseCase : I주문원장서명UseCase
{
    private const string 주문자역할Code = "Orderer";
    private readonly I커뮤니티원장저장소 _원장저장소;
    private readonly I주문원장서명저장소 _서명저장소;

    public 주문원장서명UseCase(
        I커뮤니티원장저장소 원장저장소,
        I주문원장서명저장소 서명저장소)
    {
        _원장저장소 = 원장저장소;
        _서명저장소 = 서명저장소;
    }

    public async Task<Result<주문원장서명상태Dto>> 서명요청준비Async(
        string 주문원장Id,
        주문원장서명요청준비요청 request,
        string 현재UserId,
        CancellationToken cancellationToken = default)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.계약문서번호) || string.IsNullOrWhiteSpace(request.문서Hash))
        {
            return BadRequest("계약 문서번호와 문서 Hash가 필요합니다.");
        }

        var 원장결과 = await 주문원장조회Async(주문원장Id, 현재UserId, cancellationToken);
        if (원장결과.IsFailed)
        {
            return 원장결과.ToResult<주문원장서명상태Dto>();
        }

        var now = DateTimeOffset.UtcNow;
        if (request.만료시각Utc.HasValue && request.만료시각Utc.Value <= now)
        {
            return BadRequest("서명 만료 시각은 현재 이후여야 합니다.");
        }

        var existing = await _서명저장소.조회Async(주문원장Id, cancellationToken);
        if (existing is not null && request.기대Revision is null)
        {
            if (string.Equals(existing.서명묶음.ContractNumber, request.계약문서번호.Trim(), StringComparison.OrdinalIgnoreCase)
                && string.Equals(existing.서명묶음.DocumentHash, request.문서Hash.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return Result.Ok(주문원장서명상태Factory.생성(existing, now));
            }

            return Conflict("기존 서명 요청을 교체하려면 최신 서명 Revision이 필요합니다.");
        }

        var 원장 = 원장결과.Value;
        var 표시명 = string.IsNullOrWhiteSpace(request.주문자표시명)
            ? 원장.생성자표시명
            : request.주문자표시명.Trim();
        var bundle = ContractElectronicSignaturePlanner.CreateBundle(
            request.계약문서번호.Trim(),
            request.문서Hash.Trim(),
            [new ContractSignatureRequest(현재UserId, 주문자역할Code, 표시명, true, now)],
            now,
            request.만료시각Utc);

        try
        {
            var saved = await _서명저장소.저장Async(
                원장.원장Id,
                원장.커뮤니티Id,
                bundle,
                request.기대Revision,
                현재UserId,
                cancellationToken);
            return Result.Ok(주문원장서명상태Factory.생성(saved, now));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(exception.Message);
        }
    }

    public async Task<Result<주문원장서명상태Dto>> 서명등록Async(
        string 주문원장Id,
        주문원장서명등록요청 request,
        string 현재UserId,
        CancellationToken cancellationToken = default)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.문서Hash)
            || string.IsNullOrWhiteSpace(request.동의문Hash)
            || string.IsNullOrWhiteSpace(request.서명증적Hash))
        {
            return BadRequest("문서·동의문·서명 증적 Hash가 필요합니다.");
        }

        var 원장결과 = await 주문원장조회Async(주문원장Id, 현재UserId, cancellationToken);
        if (원장결과.IsFailed)
        {
            return 원장결과.ToResult<주문원장서명상태Dto>();
        }

        var existing = await _서명저장소.조회Async(주문원장Id, cancellationToken);
        if (existing is null)
        {
            return NotFound("준비된 서명 요청을 찾을 수 없습니다.");
        }

        if (!string.Equals(existing.서명묶음.DocumentHash, request.문서Hash.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return Conflict("현재 서명 대상 문서와 요청한 문서 Hash가 다릅니다.");
        }

        var signer = existing.서명묶음.SignatureRequests.FirstOrDefault(x =>
            string.Equals(x.PartyId, 현재UserId, StringComparison.OrdinalIgnoreCase));
        if (signer is null)
        {
            return Forbidden("현재 사용자는 이 주문의 서명 요청 대상이 아닙니다.");
        }

        var now = DateTimeOffset.UtcNow;
        var signedAt = request.서명시각Utc ?? now;
        if (signedAt > now.AddMinutes(5))
        {
            return BadRequest("서명 시각을 미래로 지정할 수 없습니다.");
        }

        var methodCode = string.IsNullOrWhiteSpace(request.서명방법Code)
            ? ContractSignatureMethodCode.PlatformClickSign
            : request.서명방법Code.Trim();
        var bundle = ContractElectronicSignaturePlanner.AddEvidence(
            existing.서명묶음,
            new ContractSignatureEvidence(
                현재UserId,
                signer.SignerDisplayName,
                methodCode,
                existing.서명묶음.DocumentHash,
                request.동의문Hash.Trim(),
                request.서명증적Hash.Trim(),
                signedAt,
                Clean(request.접속IpHash)));

        try
        {
            var saved = await _서명저장소.저장Async(
                existing.주문원장Id,
                existing.커뮤니티Id,
                bundle,
                request.기대Revision ?? existing.Revision,
                현재UserId,
                cancellationToken);
            return Result.Ok(주문원장서명상태Factory.생성(saved, now));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(exception.Message);
        }
    }

    public async Task<Result<주문원장서명상태Dto>> 조회Async(
        string 주문원장Id,
        string 현재UserId,
        CancellationToken cancellationToken = default)
    {
        var 원장결과 = await 주문원장조회Async(주문원장Id, 현재UserId, cancellationToken);
        if (원장결과.IsFailed)
        {
            return 원장결과.ToResult<주문원장서명상태Dto>();
        }

        var record = await _서명저장소.조회Async(주문원장Id, cancellationToken);
        return record is null
            ? NotFound("준비된 서명 요청을 찾을 수 없습니다.")
            : Result.Ok(주문원장서명상태Factory.생성(record, DateTimeOffset.UtcNow));
    }

    private async Task<Result<커뮤니티원장Dto>> 주문원장조회Async(
        string 주문원장Id,
        string 현재UserId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(주문원장Id) || string.IsNullOrWhiteSpace(현재UserId))
        {
            return BadRequest<커뮤니티원장Dto>("주문 원장 ID와 사용자 ID가 필요합니다.");
        }

        var 원장 = await _원장저장소.원장조회Async(주문원장Id.Trim(), cancellationToken);
        if (원장 is null)
        {
            return NotFound<커뮤니티원장Dto>("주문 원장을 찾을 수 없습니다.");
        }

        if (!주문원장구성정책.주문루트인가(원장.원장템플릿Key))
        {
            return BadRequest<커뮤니티원장Dto>("서명은 공동주문 묶음이 아닌 개별 주문 원장에만 등록할 수 있습니다.");
        }

        var 접근가능 = string.Equals(원장.생성자UserId, 현재UserId, StringComparison.OrdinalIgnoreCase)
            || 원장.참여자목록.Any(x => string.Equals(x.UserId, 현재UserId, StringComparison.OrdinalIgnoreCase));
        if (!접근가능)
        {
            return Forbidden<커뮤니티원장Dto>("이 주문 원장의 주문자만 서명을 관리할 수 있습니다.");
        }

        return Result.Ok(원장);
    }

    private static string? Clean(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static Result<T> BadRequest<T>(string message)
        => Result.Fail<T>(new Error(message).WithMetadata("StatusCode", StatusCodes.Status400BadRequest));

    private static Result<주문원장서명상태Dto> BadRequest(string message)
        => BadRequest<주문원장서명상태Dto>(message);

    private static Result<T> Forbidden<T>(string message)
        => Result.Fail<T>(new Error(message).WithMetadata("StatusCode", StatusCodes.Status403Forbidden));

    private static Result<주문원장서명상태Dto> Forbidden(string message)
        => Forbidden<주문원장서명상태Dto>(message);

    private static Result<T> NotFound<T>(string message)
        => Result.Fail<T>(new Error(message).WithMetadata("StatusCode", StatusCodes.Status404NotFound));

    private static Result<주문원장서명상태Dto> NotFound(string message)
        => NotFound<주문원장서명상태Dto>(message);

    private static Result<주문원장서명상태Dto> Conflict(string message)
        => Result.Fail<주문원장서명상태Dto>(
            new Error(message).WithMetadata("StatusCode", StatusCodes.Status409Conflict));
}

public sealed class 주문원장서명요청준비요청
{
    public string 계약문서번호 { get; set; } = string.Empty;
    public string 문서Hash { get; set; } = string.Empty;
    public string? 주문자표시명 { get; set; }
    public DateTimeOffset? 만료시각Utc { get; set; }
    public long? 기대Revision { get; set; }
}

public sealed class 주문원장서명등록요청
{
    public string 문서Hash { get; set; } = string.Empty;
    public string 동의문Hash { get; set; } = string.Empty;
    public string 서명증적Hash { get; set; } = string.Empty;
    public string 서명방법Code { get; set; } = ContractSignatureMethodCode.PlatformClickSign;
    public string? 접속IpHash { get; set; }
    public DateTimeOffset? 서명시각Utc { get; set; }
    public long? 기대Revision { get; set; }
}

public sealed class 주문원장서명상태Dto
{
    public string 주문원장Id { get; set; } = string.Empty;
    public long Revision { get; set; }
    public string 상태Code { get; set; } = ContractSignatureStatusCode.Draft;
    public int 필수서명자수 { get; set; }
    public int 서명완료자수 { get; set; }
    public bool 전체서명완료여부 { get; set; }
    public DateTimeOffset? 최근서명시각Utc { get; set; }
    public DateTimeOffset? 만료시각Utc { get; set; }
}

public static class 주문원장서명상태Factory
{
    public static 주문원장서명상태Dto 생성(주문원장서명기록 record, DateTimeOffset nowUtc)
    {
        var plan = ContractElectronicSignaturePlanner.Plan(record.서명묶음, nowUtc);
        return new 주문원장서명상태Dto
        {
            주문원장Id = record.주문원장Id,
            Revision = record.Revision,
            상태Code = plan.StatusCode,
            필수서명자수 = plan.RequiredSignerCount,
            서명완료자수 = plan.SignedRequiredSignerCount,
            전체서명완료여부 = plan.IsFullySigned,
            최근서명시각Utc = record.서명묶음.Evidences.OrderByDescending(x => x.SignedAtUtc).FirstOrDefault()?.SignedAtUtc,
            만료시각Utc = record.서명묶음.ExpiresAtUtc
        };
    }
}

internal sealed class 빈주문원장서명저장소 : I주문원장서명저장소
{
    public static 빈주문원장서명저장소 Instance { get; } = new();

    public Task<주문원장서명기록?> 조회Async(string 주문원장Id, CancellationToken cancellationToken = default)
        => Task.FromResult<주문원장서명기록?>(null);

    public Task<IReadOnlyDictionary<string, 주문원장서명기록>> 목록조회Async(
        IEnumerable<string> 주문원장Ids,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyDictionary<string, 주문원장서명기록>>(
            new Dictionary<string, 주문원장서명기록>(StringComparer.OrdinalIgnoreCase));

    public Task<주문원장서명기록> 저장Async(
        string 주문원장Id,
        string 커뮤니티Id,
        ContractElectronicSignatureBundle 서명묶음,
        long? 기대Revision,
        string 변경자UserId,
        CancellationToken cancellationToken = default)
        => throw new NotSupportedException();
}

public sealed class 주문원장서명문서
{
    [BsonId]
    public string 주문원장Id { get; set; } = string.Empty;
    public string 커뮤니티Id { get; set; } = string.Empty;
    public long Revision { get; set; }
    public 주문원장서명묶음문서 서명묶음 { get; set; } = new();
    public string 최종변경자UserId { get; set; } = string.Empty;
    public DateTime 생성시각Utc { get; set; }
    public DateTime 수정시각Utc { get; set; }
}

public sealed class 주문원장서명묶음문서
{
    public string 계약문서번호 { get; set; } = string.Empty;
    public string 문서Hash { get; set; } = string.Empty;
    public IReadOnlyList<주문원장서명요청문서> 서명요청목록 { get; set; } = [];
    public IReadOnlyList<주문원장서명증적문서> 서명증적목록 { get; set; } = [];
    public DateTime 생성시각Utc { get; set; }
    public DateTime? 만료시각Utc { get; set; }
}

public sealed class 주문원장서명요청문서
{
    public string 당사자Id { get; set; } = string.Empty;
    public string 역할Code { get; set; } = string.Empty;
    public string 서명자표시명 { get; set; } = string.Empty;
    public bool 필수서명자여부 { get; set; }
    public DateTime? 요청시각Utc { get; set; }
}

public sealed class 주문원장서명증적문서
{
    public string 당사자Id { get; set; } = string.Empty;
    public string 서명자표시명 { get; set; } = string.Empty;
    public string 서명방법Code { get; set; } = string.Empty;
    public string 서명문서Hash { get; set; } = string.Empty;
    public string 동의문Hash { get; set; } = string.Empty;
    public string 서명증적Hash { get; set; } = string.Empty;
    public DateTime 서명시각Utc { get; set; }
    public string? 접속IpHash { get; set; }
}
