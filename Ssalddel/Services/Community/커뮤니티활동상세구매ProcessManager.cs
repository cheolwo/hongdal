using System.Text.Json;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Ssalddel.ApiMetadata;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Community;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Domain.Community;
using 살뜰.Data;
using 살뜰.Services.Options;
using 살뜰.도메인.결제;
using 살뜰.도메인.공통;

namespace Ssalddel.Services.Community;

public interface I커뮤니티활동상세구매ProcessManager
{
    Task<Result<커뮤니티활동상세FakePg결제승인Response>> FakePg구매Async(
        string 상세Id,
        커뮤니티활동상세FakePg결제승인Request? request,
        CancellationToken cancellationToken);

    Task<Result<커뮤니티활동상세구매WorkflowResponse>> 구매조회Async(
        string 구매Id,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<커뮤니티활동상세구매WorkflowResponse>>> 내구매목록Async(
        CancellationToken cancellationToken);
}

[SsalddelApiWorkflow(SsalddelWorkflow.CommunityTrust)]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.CommunityActivityPaidDetail,
    SsalddelCodeLayer.Application,
    "구매 요청, FakePG 승인, 열람권 발급과 상태 이력을 하나의 구매 원장 트랜잭션으로 조율합니다.",
    ContractType = typeof(I커뮤니티활동상세구매ProcessManager),
    FlowOrder = 50,
    Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
    Boundary = "Simulation 또는 Development에서만 FakePG를 사용하며 실제 카드 승인과 판매자 정산은 수행하지 않습니다.")]
public sealed class 커뮤니티활동상세구매ProcessManager : I커뮤니티활동상세구매ProcessManager
{
    private const string ProviderName = "FakePG";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly SsalddelContext _db;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ISsalddelExecutionModePolicy _executionModePolicy;

    public 커뮤니티활동상세구매ProcessManager(
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

    public async Task<Result<커뮤니티활동상세FakePg결제승인Response>> FakePg구매Async(
        string 상세Id,
        커뮤니티활동상세FakePg결제승인Request? request,
        CancellationToken cancellationToken)
    {
        if (!_hostEnvironment.IsDevelopment() && !_executionModePolicy.IsSimulation)
        {
            return Forbidden<커뮤니티활동상세FakePg결제승인Response>(
                "FakePG 결제 승인 API는 Simulation 또는 Development 환경에서만 사용할 수 있습니다.");
        }

        if (request is null)
        {
            return BadRequest<커뮤니티활동상세FakePg결제승인Response>("request body is required");
        }

        var 구매자UserId = Normalize(_currentUserAccessor.UserId);
        if (string.IsNullOrWhiteSpace(구매자UserId))
        {
            return Unauthorized<커뮤니티활동상세FakePg결제승인Response>("인증 정보가 필요합니다.");
        }

        var detail = await _db.커뮤니티활동유료상세목록
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.상세Id == 상세Id, cancellationToken);
        if (detail is null)
        {
            return NotFound<커뮤니티활동상세FakePg결제승인Response>("구매 가능한 활동 상세를 찾을 수 없습니다.");
        }

        var purchaseDecision = 커뮤니티활동유료상세Policy.구매검증(
            detail.판매상태,
            detail.판매자UserId,
            구매자UserId,
            detail.가격금액,
            request.Amount);
        if (!purchaseDecision.허용)
        {
            return BadRequest<커뮤니티활동상세FakePg결제승인Response>(purchaseDecision.메시지);
        }

        var existingPurchase = await FindPurchaseAsync(상세Id, 구매자UserId, cancellationToken);
        if (existingPurchase is not null)
        {
            return await ExistingPurchaseResponseAsync(existingPurchase, cancellationToken);
        }

        var idempotencyKey = NormalizeOptional(request.IdempotencyKey);
        if (idempotencyKey is not null)
        {
            var keyedPurchase = await _db.커뮤니티활동상세구매목록
                .AsNoTracking()
                .Include(x => x.상태이력)
                .FirstOrDefaultAsync(x => x.멱등성Key == idempotencyKey, cancellationToken);
            if (keyedPurchase is not null)
            {
                if (!string.Equals(keyedPurchase.상세Id, 상세Id, StringComparison.Ordinal)
                    || !string.Equals(keyedPurchase.구매자UserId, 구매자UserId, StringComparison.Ordinal))
                {
                    return Conflict<커뮤니티활동상세FakePg결제승인Response>("이미 다른 구매에 사용된 멱등성 키입니다.");
                }

                return await ExistingPurchaseResponseAsync(keyedPurchase, cancellationToken);
            }
        }

        var now = DateTime.UtcNow;
        var purchase = new 커뮤니티활동상세구매
        {
            구매Id = $"community-detail-purchase-{Guid.NewGuid():N}",
            상세Id = 상세Id,
            구매자UserId = 구매자UserId,
            판매자UserId = detail.판매자UserId,
            멱등성Key = idempotencyKey,
            요청금액 = detail.가격금액,
            통화Code = detail.통화Code,
            현재상태 = 커뮤니티활동상세구매상태.요청됨,
            요청일시Utc = now
        };
        RecordState(purchase, 커뮤니티활동상세구매상태.요청됨, "BuyerConfirmed", now);

        var paymentKey = $"fake_pg_community_detail_{Guid.NewGuid():N}";
        var payment = new 결제
        {
            결제Id = $"SIM-FPG-COMMUNITY-DETAIL-{Guid.NewGuid():N}",
            의뢰Id = purchase.구매Id,
            화주Id = 구매자UserId,
            결제대상유형 = 결제공통정의.결제대상유형.커뮤니티활동상세열람,
            대상Id = 상세Id,
            PG사 = ProviderName,
            결제제공자 = 결제공통정의.결제제공자.FakePG,
            결제수단 = string.IsNullOrWhiteSpace(request.결제수단) ? ProviderName : request.결제수단.Trim(),
            결제상태 = 상태값.결제상태.결제완료,
            공통결제상태 = 결제공통정의.결제상태.승인완료,
            결제금액 = detail.가격금액,
            통화 = detail.통화Code,
            OrderId = $"ssalddel_community_detail_fake_{Guid.NewGuid():N}",
            주문명 = "살뜰 커뮤니티 활동 상세 열람권",
            PaymentKey = paymentKey,
            외부거래번호 = idempotencyKey ?? paymentKey,
            원본응답Json = JsonSerializer.Serialize(new
            {
                provider = ProviderName,
                executionMode = _executionModePolicy.Mode.ToString(),
                purchase.구매Id,
                상세Id,
                구매자UserId,
                amount = detail.가격금액,
                approvedAtUtc = now
            }, JsonOptions),
            CreatedAt = now,
            승인일시 = now
        };
        payment.Toss응답Json = payment.원본응답Json;
        purchase.결제Id = payment.결제Id;
        Transition(purchase, 커뮤니티활동상세구매상태.결제승인됨, "FakePgApproved", now);

        var entitlement = new 커뮤니티활동상세열람권
        {
            열람권Id = $"community-detail-entitlement-{Guid.NewGuid():N}",
            상세Id = 상세Id,
            구매자UserId = 구매자UserId,
            결제Id = payment.결제Id,
            상태 = 커뮤니티활동상세열람권상태.활성,
            발급일시Utc = now
        };
        purchase.열람권Id = entitlement.열람권Id;
        purchase.완료일시Utc = now;
        Transition(purchase, 커뮤니티활동상세구매상태.열람권발급됨, "EntitlementGranted", now);

        _db.커뮤니티활동상세구매목록.Add(purchase);
        _db.결제.Add(payment);
        _db.커뮤니티활동상세열람권목록.Add(entitlement);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Ok(ToPaymentResponse(payment, entitlement, purchase, false));
    }

    public async Task<Result<커뮤니티활동상세구매WorkflowResponse>> 구매조회Async(
        string 구매Id,
        CancellationToken cancellationToken)
    {
        var 구매자UserId = Normalize(_currentUserAccessor.UserId);
        if (string.IsNullOrWhiteSpace(구매자UserId))
        {
            return Unauthorized<커뮤니티활동상세구매WorkflowResponse>("인증 정보가 필요합니다.");
        }

        var purchase = await _db.커뮤니티활동상세구매목록
            .AsNoTracking()
            .Include(x => x.상태이력)
            .FirstOrDefaultAsync(x => x.구매Id == 구매Id, cancellationToken);
        if (purchase is null)
        {
            return NotFound<커뮤니티활동상세구매WorkflowResponse>("구매 원장을 찾을 수 없습니다.");
        }

        if (!string.Equals(purchase.구매자UserId, 구매자UserId, StringComparison.Ordinal))
        {
            return Forbidden<커뮤니티활동상세구매WorkflowResponse>("구매자 본인만 구매 진행 상태를 조회할 수 있습니다.");
        }

        return Result.Ok(ToWorkflowResponse(purchase));
    }

    public async Task<Result<IReadOnlyList<커뮤니티활동상세구매WorkflowResponse>>> 내구매목록Async(
        CancellationToken cancellationToken)
    {
        var 구매자UserId = Normalize(_currentUserAccessor.UserId);
        if (string.IsNullOrWhiteSpace(구매자UserId))
        {
            return Unauthorized<IReadOnlyList<커뮤니티활동상세구매WorkflowResponse>>("인증 정보가 필요합니다.");
        }

        var purchases = await _db.커뮤니티활동상세구매목록
            .AsNoTracking()
            .Include(x => x.상태이력)
            .Where(x => x.구매자UserId == 구매자UserId)
            .OrderByDescending(x => x.요청일시Utc)
            .ToArrayAsync(cancellationToken);
        return Result.Ok<IReadOnlyList<커뮤니티활동상세구매WorkflowResponse>>(
            purchases.Select(ToWorkflowResponse).ToArray());
    }

    private Task<커뮤니티활동상세구매?> FindPurchaseAsync(
        string 상세Id,
        string 구매자UserId,
        CancellationToken cancellationToken)
        => _db.커뮤니티활동상세구매목록
            .AsNoTracking()
            .Include(x => x.상태이력)
            .FirstOrDefaultAsync(
                x => x.상세Id == 상세Id && x.구매자UserId == 구매자UserId,
                cancellationToken);

    private async Task<Result<커뮤니티활동상세FakePg결제승인Response>> ExistingPurchaseResponseAsync(
        커뮤니티활동상세구매 purchase,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(purchase.현재상태, 커뮤니티활동상세구매상태.열람권발급됨, StringComparison.Ordinal)
            || string.IsNullOrWhiteSpace(purchase.결제Id)
            || string.IsNullOrWhiteSpace(purchase.열람권Id))
        {
            return Conflict<커뮤니티활동상세FakePg결제승인Response>("기존 구매가 완료되지 않아 새 결제를 만들 수 없습니다.");
        }

        var payment = await _db.결제.AsNoTracking()
            .FirstAsync(x => x.결제Id == purchase.결제Id, cancellationToken);
        var entitlement = await _db.커뮤니티활동상세열람권목록.AsNoTracking()
            .FirstAsync(x => x.열람권Id == purchase.열람권Id, cancellationToken);
        return Result.Ok(ToPaymentResponse(payment, entitlement, purchase, true));
    }

    private static void Transition(
        커뮤니티활동상세구매 purchase,
        string nextState,
        string reasonCode,
        DateTime recordedAtUtc)
    {
        if (!커뮤니티활동유료상세Policy.상태전이가능한가(purchase.현재상태, nextState))
        {
            throw new InvalidOperationException($"허용되지 않은 구매 상태 전이입니다: {purchase.현재상태} -> {nextState}");
        }

        purchase.현재상태 = nextState;
        RecordState(purchase, nextState, reasonCode, recordedAtUtc);
    }

    private static void RecordState(
        커뮤니티활동상세구매 purchase,
        string state,
        string reasonCode,
        DateTime recordedAtUtc)
        => purchase.상태이력.Add(new 커뮤니티활동상세구매상태이력
        {
            구매Id = purchase.구매Id,
            순서 = purchase.상태이력.Count + 1,
            상태 = state,
            사유Code = reasonCode,
            기록일시Utc = recordedAtUtc
        });

    private static 커뮤니티활동상세FakePg결제승인Response ToPaymentResponse(
        결제 payment,
        커뮤니티활동상세열람권 entitlement,
        커뮤니티활동상세구매 purchase,
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
            승인일시Utc = payment.승인일시 ?? payment.CreatedAt,
            이미완료됨 = alreadyCompleted,
            열람권 = ToEntitlementResponse(entitlement),
            구매Workflow = ToWorkflowResponse(purchase)
        };

    private static 커뮤니티활동상세열람권Response ToEntitlementResponse(커뮤니티활동상세열람권 entity)
        => new()
        {
            열람권Id = entity.열람권Id,
            상세Id = entity.상세Id,
            구매자UserId = entity.구매자UserId,
            결제Id = entity.결제Id,
            상태 = entity.상태,
            발급일시Utc = entity.발급일시Utc
        };

    private static 커뮤니티활동상세구매WorkflowResponse ToWorkflowResponse(커뮤니티활동상세구매 purchase)
        => new()
        {
            구매Id = purchase.구매Id,
            상세Id = purchase.상세Id,
            구매자UserId = purchase.구매자UserId,
            요청금액 = purchase.요청금액,
            통화Code = purchase.통화Code,
            현재상태 = purchase.현재상태,
            결제Id = purchase.결제Id,
            열람권Id = purchase.열람권Id,
            요청일시Utc = purchase.요청일시Utc,
            완료일시Utc = purchase.완료일시Utc,
            상태이력 = purchase.상태이력
                .OrderBy(x => x.순서)
                .Select(x => new 커뮤니티활동상세구매상태이력Response
                {
                    순서 = x.순서,
                    상태 = x.상태,
                    사유Code = x.사유Code,
                    기록일시Utc = x.기록일시Utc
                })
                .ToArray()
        };

    private static string Normalize(string? value) => value?.Trim() ?? string.Empty;
    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static Result<T> BadRequest<T>(string message) => Fail<T>(message, StatusCodes.Status400BadRequest);
    private static Result<T> Unauthorized<T>(string message) => Fail<T>(message, StatusCodes.Status401Unauthorized);
    private static Result<T> Forbidden<T>(string message) => Fail<T>(message, StatusCodes.Status403Forbidden);
    private static Result<T> NotFound<T>(string message) => Fail<T>(message, StatusCodes.Status404NotFound);
    private static Result<T> Conflict<T>(string message) => Fail<T>(message, StatusCodes.Status409Conflict);
    private static Result<T> Fail<T>(string message, int statusCode)
        => Result.Fail<T>(new Error(message).WithMetadata("StatusCode", statusCode));
}
