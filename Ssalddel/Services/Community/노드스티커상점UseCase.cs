using System.Text.Json;
using FluentResults;
using Ssalddel.ApiMetadata;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Contracts.Common.Community;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using 살뜰.Data;
using 살뜰.도메인.결제;
using 살뜰.도메인.공통;
using 살뜰.Services.Options;

namespace Ssalddel.Services.Community;

public interface I노드스티커상점UseCase
{
    Task<Result<IReadOnlyList<노드스티커상점상품Response>>> 상품목록Async(CancellationToken cancellationToken);
    Task<Result<노드스티커상점상품Response>> 상품상세Async(string 상품Key, CancellationToken cancellationToken);
    Task<Result<노드스티커보유권동기화Response>> 내보유권목록Async(CancellationToken cancellationToken);
    Task<Result<노드스티커FakePg결제승인Response>> 페이크결제승인Async(
        노드스티커FakePg결제승인Request? request,
        CancellationToken cancellationToken);
}

[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelUseCase("노드 스티커 상점", Summary = "커뮤니티 다이어그램 노드에 적용할 수 있는 창작자 스티커 팩을 조회하고 개발용 FakePG로 구매 흐름을 검증합니다.")]
[SsalddelUseCaseActor(SsalddelActor.CommunityMember)]
[SsalddelUseCaseActor(SsalddelActor.PlatformOperator, SsalddelUseCaseActorRole.Supporting)]
public sealed class 노드스티커상점UseCase : I노드스티커상점UseCase
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private const string ProviderName = "FakePG";

    private readonly SsalddelContext _db;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ISsalddelExecutionModePolicy _executionModePolicy;

    public 노드스티커상점UseCase(
        SsalddelContext db,
        ICurrentUserAccessor currentUserAccessor,
        IHostEnvironment hostEnvironment,
        ISsalddelExecutionModePolicy executionModePolicy)
    {
        _db = db;
        _currentUserAccessor = currentUserAccessor;
        _hostEnvironment = hostEnvironment;
        _executionModePolicy = executionModePolicy;
    }

    public Task<Result<IReadOnlyList<노드스티커상점상품Response>>> 상품목록Async(CancellationToken cancellationToken)
    {
        IReadOnlyList<노드스티커상점상품Response> items = 상점상품목록();
        return Task.FromResult(Result.Ok(items));
    }

    public Task<Result<노드스티커상점상품Response>> 상품상세Async(string 상품Key, CancellationToken cancellationToken)
    {
        var 상품 = 상품찾기(상품Key, null);
        return Task.FromResult(상품 is null
            ? Result.Fail<노드스티커상점상품Response>("노드 스티커 상점 상품을 찾을 수 없습니다.")
            : Result.Ok(상품));
    }

    public async Task<Result<노드스티커보유권동기화Response>> 내보유권목록Async(
        CancellationToken cancellationToken)
    {
        var 사용자UserId = Normalize(_currentUserAccessor.UserId);
        if (string.IsNullOrWhiteSpace(사용자UserId))
        {
            return Result.Fail<노드스티커보유권동기화Response>("인증 정보가 필요합니다.");
        }

        var payments = await _db.결제
            .AsNoTracking()
            .Where(x => x.화주Id == 사용자UserId
                        && x.결제대상유형 == 결제공통정의.결제대상유형.노드스티커팩
                        && x.공통결제상태 == 결제공통정의.결제상태.승인완료)
            .OrderByDescending(x => x.승인일시 ?? x.CreatedAt)
            .ToListAsync(cancellationToken);

        var 보유권목록 = payments
            .GroupBy(x => x.대상Id, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .Select(payment =>
            {
                var 상품 = 상품찾기(null, payment.대상Id);
                return 상품 is null ? null : To보유권(payment, 상품, 사용자UserId);
            })
            .Where(x => x is not null)
            .Cast<노드스티커보유권Response>()
            .OrderBy(x => x.팩Key, StringComparer.Ordinal)
            .ToArray();

        return Result.Ok(new 노드스티커보유권동기화Response
        {
            사용자UserId = 사용자UserId,
            서버기준시각Utc = DateTime.UtcNow,
            보유권목록 = 보유권목록
        });
    }

    public async Task<Result<노드스티커FakePg결제승인Response>> 페이크결제승인Async(
        노드스티커FakePg결제승인Request? request,
        CancellationToken cancellationToken)
    {
        if (!_hostEnvironment.IsDevelopment() && !_executionModePolicy.IsSimulation)
        {
            return Result.Fail<노드스티커FakePg결제승인Response>("FakePG 결제 승인 API는 Simulation 또는 Development 환경에서만 사용할 수 있습니다.");
        }

        if (request is null)
        {
            return Result.Fail<노드스티커FakePg결제승인Response>("request body is required");
        }

        var 구매자UserId = Normalize(_currentUserAccessor.UserId);
        if (string.IsNullOrWhiteSpace(구매자UserId))
        {
            return Result.Fail<노드스티커FakePg결제승인Response>("인증 정보가 필요합니다.");
        }

        var 상품 = 상품찾기(request.상품Key, request.팩Key);
        if (상품 is null)
        {
            return Result.Fail<노드스티커FakePg결제승인Response>("노드 스티커 상점 상품을 찾을 수 없습니다.");
        }

        if (!노드스티커상점정책.상점노출가능한가(상품))
        {
            return Result.Fail<노드스티커FakePg결제승인Response>("현재 구매할 수 없는 노드 스티커 상품입니다.");
        }

        if (!노드스티커상점정책.구매필요한가(상품))
        {
            return Result.Fail<노드스티커FakePg결제승인Response>("무료 노드 스티커 팩은 결제 없이 사용할 수 있습니다.");
        }

        var 상품금액 = ToIntAmount(상품.거래정책.가격금액);
        if (상품금액 <= 0)
        {
            return Result.Fail<노드스티커FakePg결제승인Response>("노드 스티커 상품 금액이 유효하지 않습니다.");
        }

        var requestedAmount = request.Amount > 0 ? request.Amount : 상품금액;
        if (requestedAmount != 상품금액)
        {
            return Result.Fail<노드스티커FakePg결제승인Response>("결제금액이 노드 스티커 상품 금액과 다릅니다.");
        }

        var idempotencyKey = Normalize(request.IdempotencyKey);
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var existingPayment = await _db.결제
                .AsNoTracking()
                .Where(x => x.결제대상유형 == 결제공통정의.결제대상유형.노드스티커팩
                            && x.대상Id == 상품.팩Key
                            && x.PG사 == ProviderName
                            && x.외부거래번호 == idempotencyKey)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingPayment is not null)
            {
                return Result.Ok(ToResponse(existingPayment, 상품, 구매자UserId, alreadyCompleted: true));
            }
        }

        var now = DateTime.UtcNow;
        var paymentKey = $"fake_pg_node_sticker_{Guid.NewGuid():N}";
        var responseJson = BuildFakeResponseJson(
            상품,
            구매자UserId,
            requestedAmount,
            paymentKey,
            idempotencyKey,
            request.메모,
            now,
            _executionModePolicy.Mode);
        var payment = new 결제
        {
            결제Id = $"SIM-FPG-STICKER-{Guid.NewGuid():N}",
            의뢰Id = 상품.팩Key,
            화주Id = 구매자UserId,
            결제대상유형 = 결제공통정의.결제대상유형.노드스티커팩,
            대상Id = 상품.팩Key,
            PG사 = ProviderName,
            결제제공자 = 결제공통정의.결제제공자.FakePG,
            결제수단 = string.IsNullOrWhiteSpace(request.결제수단) ? ProviderName : request.결제수단.Trim(),
            결제상태 = 상태값.결제상태.결제완료,
            공통결제상태 = 결제공통정의.결제상태.승인완료,
            결제금액 = requestedAmount,
            통화 = 상품.거래정책.통화Code,
            OrderId = $"ssalddel_node_sticker_fake_{Guid.NewGuid():N}",
            주문명 = $"살뜰 노드 스티커 {상품.제목}",
            PaymentKey = paymentKey,
            외부거래번호 = string.IsNullOrWhiteSpace(idempotencyKey) ? paymentKey : idempotencyKey,
            Toss응답Json = responseJson,
            원본응답Json = responseJson,
            CreatedAt = now,
            승인일시 = now
        };

        await _db.결제.AddAsync(payment, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Ok(ToResponse(payment, 상품, 구매자UserId, alreadyCompleted: false));
    }

    private static IReadOnlyList<노드스티커상점상품Response> 상점상품목록()
        => 노드스티커Catalog.기본팩목록
            .Select(To상점상품)
            .Where(노드스티커상점정책.상점노출가능한가)
            .OrderBy(상품 => 노드스티커상점정책.구매필요한가(상품) ? 1 : 0)
            .ThenBy(상품 => 상품.제목, StringComparer.Ordinal)
            .ToArray();

    private static 노드스티커상점상품Response? 상품찾기(string? 상품Key, string? 팩Key)
    {
        var normalized상품Key = Normalize(상품Key);
        var normalized팩Key = Normalize(팩Key);
        return 상점상품목록()
            .FirstOrDefault(상품 =>
                (!string.IsNullOrWhiteSpace(normalized상품Key) &&
                 string.Equals(상품.상품Key, normalized상품Key, StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrWhiteSpace(normalized팩Key) &&
                 string.Equals(상품.팩Key, normalized팩Key, StringComparison.OrdinalIgnoreCase)));
    }

    private static 노드스티커상점상품Response To상점상품(노드스티커팩Response 팩)
        => new()
        {
            상품Key = $"store-{팩.팩Key}",
            팩Key = 팩.팩Key,
            제목 = 팩.제목,
            창작자표시명 = 팩.창작자표시명,
            요약 = 팩.요약,
            검수상태 = 팩.검수상태,
            판매상태 = 노드스티커검수상태.승인.Equals(팩.검수상태, StringComparison.OrdinalIgnoreCase)
                ? 노드스티커판매상태.판매중
                : 노드스티커판매상태.비공개,
            거래정책 = 팩.거래정책,
            이미지목록 = 팩.이미지목록
        };

    private static 노드스티커FakePg결제승인Response ToResponse(
        결제 payment,
        노드스티커상점상품Response 상품,
        string 구매자UserId,
        bool alreadyCompleted)
        => new()
        {
            결제Id = payment.결제Id,
            결제대상유형 = payment.결제대상유형,
            결제제공자 = payment.결제제공자,
            OrderId = payment.OrderId,
            PaymentKey = payment.PaymentKey ?? string.Empty,
            Amount = payment.결제금액,
            통화Code = payment.통화,
            결제상태 = payment.결제상태,
            결제응답 = payment.원본응답Json ?? payment.Toss응답Json ?? string.Empty,
            승인일시Utc = payment.승인일시 ?? payment.CreatedAt,
            이미완료됨 = alreadyCompleted,
            상품 = 상품,
            구매 = new()
            {
                구매Id = $"purchase-{payment.결제Id}",
                구매자UserId = string.IsNullOrWhiteSpace(payment.화주Id) ? 구매자UserId : payment.화주Id,
                상품Key = 상품.상품Key,
                팩Key = 상품.팩Key,
                구매상태 = 노드스티커구매상태.완료,
                결제금액 = payment.결제금액,
                통화Code = payment.통화
            },
            보유권 = To보유권(payment, 상품, 구매자UserId)
        };

    private static 노드스티커보유권Response To보유권(
        결제 payment,
        노드스티커상점상품Response 상품,
        string 사용자UserId)
        => new()
        {
            보유권Id = $"entitlement-{payment.결제Id}",
            사용자UserId = string.IsNullOrWhiteSpace(payment.화주Id) ? 사용자UserId : payment.화주Id,
            팩Key = 상품.팩Key,
            이미지Keys = 상품.이미지목록.Select(이미지 => 이미지.이미지Key).ToArray(),
            보유권출처 = 노드스티커보유권출처.구매
        };

    private static int ToIntAmount(decimal amount)
        => decimal.ToInt32(decimal.Round(amount, 0, MidpointRounding.AwayFromZero));

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string BuildFakeResponseJson(
        노드스티커상점상품Response 상품,
        string 구매자UserId,
        int amount,
        string paymentKey,
        string? idempotencyKey,
        string? memo,
        DateTime approvedAtUtc,
        SsalddelExecutionMode executionMode)
    {
        return JsonSerializer.Serialize(new
        {
            provider = ProviderName,
            mode = executionMode.ToString(),
            targetType = "NodeStickerPack",
            productKey = 상품.상품Key,
            packKey = 상품.팩Key,
            buyerUserId = 구매자UserId,
            amount,
            currency = 상품.거래정책.통화Code,
            paymentKey,
            idempotencyKey,
            memo,
            approvedAtUtc
        }, JsonOptions);
    }
}
