using FluentResults;
using Hongdal.ApiMetadata;
using Hongdal.Contracts.Common.Community;

namespace Hongdal.Services.Community;

public interface I주문원장통합UseCase
{
    Task<Result<주문원장통합Dto>> 조회Async(
        string 주문원장Id,
        CancellationToken cancellationToken = default);

    Task<Result<주문원장통합Dto>> 하위원장연결Async(
        string 주문원장Id,
        주문하위원장연결요청 request,
        string 변경자,
        CancellationToken cancellationToken = default);

    Task<Result<주문원장통합Dto>> 하위원장분리Async(
        string 주문원장Id,
        string 하위원장Id,
        long? 기대Revision,
        string 변경자,
        CancellationToken cancellationToken = default);
}

[HongdalApiWorkflow(HongdalWorkflow.CommunityTrust)]
[HongdalUseCase("주문 원장 통합", Summary = "주문 원장을 루트로 두고 판매, 입출고, 배송과 운송 원장의 최신 상태를 한 번에 조합합니다.")]
[HongdalUseCaseActor(HongdalActor.CommunityMember)]
[HongdalUseCaseActor(HongdalActor.PlatformOperator, HongdalUseCaseActorRole.Supporting)]
public sealed class 주문원장통합UseCase : I주문원장통합UseCase
{
    private readonly I커뮤니티원장저장소 _원장저장소;
    private readonly I주문원장서명저장소 _서명저장소;

    public 주문원장통합UseCase(I커뮤니티원장저장소 원장저장소)
        : this(원장저장소, 빈주문원장서명저장소.Instance)
    {
    }

    public 주문원장통합UseCase(
        I커뮤니티원장저장소 원장저장소,
        I주문원장서명저장소 서명저장소)
    {
        _원장저장소 = 원장저장소;
        _서명저장소 = 서명저장소;
    }

    public async Task<Result<주문원장통합Dto>> 조회Async(
        string 주문원장Id,
        CancellationToken cancellationToken = default)
    {
        var 주문원장결과 = await 주문원장조회Async(주문원장Id, cancellationToken);
        return 주문원장결과.IsFailed
            ? 주문원장결과.ToResult<주문원장통합Dto>()
            : Result.Ok(await 통합Dto생성Async(주문원장결과.Value, cancellationToken));
    }

    public async Task<Result<주문원장통합Dto>> 하위원장연결Async(
        string 주문원장Id,
        주문하위원장연결요청 request,
        string 변경자,
        CancellationToken cancellationToken = default)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.하위원장Id))
        {
            return BadRequest("연결할 하위 원장 ID가 필요합니다.");
        }

        var 주문원장결과 = await 주문원장조회Async(주문원장Id, cancellationToken);
        if (주문원장결과.IsFailed)
        {
            return 주문원장결과.ToResult<주문원장통합Dto>();
        }

        var 주문원장 = 주문원장결과.Value;
        if (request.기대Revision.HasValue && request.기대Revision.Value != 주문원장.Revision)
        {
            return Conflict("주문 원장이 다른 요청에서 먼저 변경되었습니다. 최신 원장을 다시 조회한 뒤 재시도해야 합니다.");
        }

        var 하위원장 = await _원장저장소.원장조회Async(request.하위원장Id.Trim(), cancellationToken);
        if (하위원장 is null)
        {
            return NotFound("연결할 하위 원장을 찾을 수 없습니다.");
        }

        try
        {
            주문원장구성정책.연결검증(주문원장, 하위원장, request.역할);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(exception.Message);
        }

        var 포함원장목록 = 주문원장.포함원장목록.ToList();
        var 기존Index = 포함원장목록.FindIndex(x =>
            string.Equals(x.원장Id, 하위원장.원장Id, StringComparison.OrdinalIgnoreCase));
        var 표시순서 = request.표시순서
            ?? (기존Index >= 0
                ? 포함원장목록[기존Index].표시순서
                : 포함원장목록.Select(x => x.표시순서).DefaultIfEmpty(-1).Max() + 1);
        var 참조 = new 커뮤니티포함원장참조Dto
        {
            원장Id = 하위원장.원장Id,
            원장템플릿Key = 하위원장.원장템플릿Key,
            역할 = request.역할.Trim(),
            필수여부 = request.필수여부,
            표시순서 = 표시순서
        };

        if (기존Index >= 0)
        {
            포함원장목록[기존Index] = 참조;
        }
        else
        {
            포함원장목록.Add(참조);
        }

        var 저장결과 = await 주문원장저장Async(주문원장, 포함원장목록, 변경자, cancellationToken);
        if (저장결과.IsFailed)
        {
            return 저장결과.ToResult<주문원장통합Dto>();
        }

        return Result.Ok(await 통합Dto생성Async(저장결과.Value, cancellationToken));
    }

    public async Task<Result<주문원장통합Dto>> 하위원장분리Async(
        string 주문원장Id,
        string 하위원장Id,
        long? 기대Revision,
        string 변경자,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(하위원장Id))
        {
            return BadRequest("분리할 하위 원장 ID가 필요합니다.");
        }

        var 주문원장결과 = await 주문원장조회Async(주문원장Id, cancellationToken);
        if (주문원장결과.IsFailed)
        {
            return 주문원장결과.ToResult<주문원장통합Dto>();
        }

        var 주문원장 = 주문원장결과.Value;
        if (기대Revision.HasValue && 기대Revision.Value != 주문원장.Revision)
        {
            return Conflict("주문 원장이 다른 요청에서 먼저 변경되었습니다. 최신 원장을 다시 조회한 뒤 재시도해야 합니다.");
        }

        var 포함원장목록 = 주문원장.포함원장목록
            .Where(x => !string.Equals(x.원장Id, 하위원장Id.Trim(), StringComparison.OrdinalIgnoreCase))
            .ToArray();
        if (포함원장목록.Length == 주문원장.포함원장목록.Count)
        {
            return NotFound("주문 원장에 연결된 하위 원장을 찾을 수 없습니다.");
        }

        var 저장결과 = await 주문원장저장Async(주문원장, 포함원장목록, 변경자, cancellationToken);
        if (저장결과.IsFailed)
        {
            return 저장결과.ToResult<주문원장통합Dto>();
        }

        return Result.Ok(await 통합Dto생성Async(저장결과.Value, cancellationToken));
    }

    private async Task<Result<커뮤니티원장Dto>> 주문원장조회Async(
        string 주문원장Id,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(주문원장Id))
        {
            return BadRequest<커뮤니티원장Dto>("주문 원장 ID가 필요합니다.");
        }

        var 주문원장 = await _원장저장소.원장조회Async(주문원장Id.Trim(), cancellationToken);
        if (주문원장 is null)
        {
            return NotFound<커뮤니티원장Dto>("주문 원장을 찾을 수 없습니다.");
        }

        return 주문원장구성정책.통합대상인가(주문원장.원장템플릿Key)
            ? Result.Ok(주문원장)
            : BadRequest<커뮤니티원장Dto>("통합 조회 대상은 주문 원장이어야 합니다.");
    }

    private async Task<Result<커뮤니티원장Dto>> 주문원장저장Async(
        커뮤니티원장Dto 주문원장,
        IReadOnlyList<커뮤니티포함원장참조Dto> 포함원장목록,
        string 변경자,
        CancellationToken cancellationToken)
    {
        try
        {
            var 저장된원장 = await _원장저장소.원장저장Async(
                new 커뮤니티원장저장요청
                {
                    원장Id = 주문원장.원장Id,
                    기대Revision = 주문원장.Revision,
                    커뮤니티Id = 주문원장.커뮤니티Id,
                    원장템플릿Key = 주문원장.원장템플릿Key,
                    제목 = 주문원장.제목,
                    원함 = 주문원장.원함,
                    상태 = 주문원장.상태,
                    현재단계Key = 주문원장.현재단계Key,
                    대상OsCode = 주문원장.대상OsCode,
                    대상OsName = 주문원장.대상OsName,
                    생성자UserId = 주문원장.생성자UserId,
                    생성자표시명 = 주문원장.생성자표시명,
                    블록목록 = 주문원장.블록목록,
                    참여자목록 = 주문원장.참여자목록,
                    포함원장목록 = 포함원장목록,
                    다이어그램스냅샷 = 주문원장.다이어그램스냅샷,
                    외부참조 = 주문원장.외부참조,
                    확장속성 = 주문원장.확장속성
                },
                string.IsNullOrWhiteSpace(변경자) ? "system" : 변경자.Trim(),
                cancellationToken);
            return Result.Ok(저장된원장);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict<커뮤니티원장Dto>(exception.Message);
        }
    }

    private async Task<주문원장통합Dto> 통합Dto생성Async(
        커뮤니티원장Dto 주문원장,
        CancellationToken cancellationToken)
    {
        var 참조목록 = 주문원장.포함원장목록.OrderBy(x => x.표시순서).ToArray();
        var 조회작업목록 = 참조목록
            .Select(x => _원장저장소.원장조회Async(x.원장Id, cancellationToken))
            .ToArray();
        var 하위원장목록 = 조회작업목록.Length == 0
            ? []
            : await Task.WhenAll(조회작업목록);
        var 서명조회대상Ids = 주문원장구성정책.주문루트인가(주문원장.원장템플릿Key)
            ? [주문원장.원장Id]
            : 참조목록
                .Where(x => string.Equals(x.역할, 주문원장포함역할.개별주문, StringComparison.OrdinalIgnoreCase))
                .Select(x => x.원장Id)
                .ToArray();
        var 서명기록목록 = await _서명저장소.목록조회Async(서명조회대상Ids, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var 포함항목목록 = 참조목록
            .Select((참조, index) => new 주문포함원장Dto
            {
                원장Id = 참조.원장Id,
                원장템플릿Key = 참조.원장템플릿Key,
                역할 = 참조.역할,
                필수여부 = 참조.필수여부,
                표시순서 = 참조.표시순서,
                원장 = 하위원장목록[index],
                조회상태 = 하위원장목록[index] is null ? "원장누락" : "정상",
                주문자서명상태 = 서명기록목록.TryGetValue(참조.원장Id, out var 서명기록)
                    ? 주문원장서명상태Factory.생성(서명기록, now)
                    : null
            })
            .ToArray();
        var 개별주문항목목록 = 포함항목목록
            .Where(x => string.Equals(x.역할, 주문원장포함역할.개별주문, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        var 서명완료주문수 = 개별주문항목목록.Count(x => x.주문자서명상태?.전체서명완료여부 == true);

        return new 주문원장통합Dto
        {
            주문원장 = 주문원장,
            주문자서명상태 = 서명기록목록.TryGetValue(주문원장.원장Id, out var 주문서명기록)
                ? 주문원장서명상태Factory.생성(주문서명기록, now)
                : null,
            포함원장목록 = 포함항목목록,
            전체하위원장수 = 포함항목목록.Length,
            완료하위원장수 = 포함항목목록.Count(x => x.원장?.상태 == 커뮤니티원장상태.완료),
            필수하위원장완료여부 = 포함항목목록
                .Where(x => x.필수여부)
                .All(x => x.원장?.상태 == 커뮤니티원장상태.완료),
            서명대상주문수 = 개별주문항목목록.Length,
            서명완료주문수 = 서명완료주문수,
            미서명주문Ids = 개별주문항목목록
                .Where(x => x.주문자서명상태?.전체서명완료여부 != true)
                .Select(x => x.원장Id)
                .ToArray(),
            전체주문서명완료여부 = 개별주문항목목록.Length > 0
                && 서명완료주문수 == 개별주문항목목록.Length
        };
    }

    private static Result<T> BadRequest<T>(string message)
        => Result.Fail<T>(new Error(message).WithMetadata("StatusCode", StatusCodes.Status400BadRequest));

    private static Result<주문원장통합Dto> BadRequest(string message)
        => BadRequest<주문원장통합Dto>(message);

    private static Result<T> NotFound<T>(string message)
        => Result.Fail<T>(new Error(message).WithMetadata("StatusCode", StatusCodes.Status404NotFound));

    private static Result<주문원장통합Dto> NotFound(string message)
        => NotFound<주문원장통합Dto>(message);

    private static Result<T> Conflict<T>(string message)
        => Result.Fail<T>(new Error(message).WithMetadata("StatusCode", StatusCodes.Status409Conflict));

    private static Result<주문원장통합Dto> Conflict(string message)
        => Conflict<주문원장통합Dto>(message);
}

public sealed class 주문하위원장연결요청
{
    public string 하위원장Id { get; set; } = string.Empty;
    public string 역할 { get; set; } = string.Empty;
    public bool 필수여부 { get; set; }
    public int? 표시순서 { get; set; }
    public long? 기대Revision { get; set; }
}

public sealed class 주문원장통합Dto
{
    public 커뮤니티원장Dto 주문원장 { get; set; } = new();
    public 주문원장서명상태Dto? 주문자서명상태 { get; set; }
    public IReadOnlyList<주문포함원장Dto> 포함원장목록 { get; set; } = [];
    public int 전체하위원장수 { get; set; }
    public int 완료하위원장수 { get; set; }
    public bool 필수하위원장완료여부 { get; set; }
    public int 서명대상주문수 { get; set; }
    public int 서명완료주문수 { get; set; }
    public IReadOnlyList<string> 미서명주문Ids { get; set; } = [];
    public bool 전체주문서명완료여부 { get; set; }
}

public sealed class 주문포함원장Dto
{
    public string 원장Id { get; set; } = string.Empty;
    public string 원장템플릿Key { get; set; } = string.Empty;
    public string 역할 { get; set; } = string.Empty;
    public bool 필수여부 { get; set; }
    public int 표시순서 { get; set; }
    public string 조회상태 { get; set; } = "정상";
    public 커뮤니티원장Dto? 원장 { get; set; }
    public 주문원장서명상태Dto? 주문자서명상태 { get; set; }
}
