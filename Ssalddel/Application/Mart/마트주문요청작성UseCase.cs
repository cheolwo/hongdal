using FluentResults;
using Microsoft.EntityFrameworkCore;
using Ssalddel.ApiMetadata;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Contracts.Mart;
using 살뜰.Data;
using 살뜰.도메인.마트;

namespace Ssalddel.Application.Mart;

public interface I마트주문요청작성UseCase
{
    Task<Result<마트주문요청응답>> 등록Async(
        마트주문요청등록요청 request,
        CancellationToken cancellationToken);
}

[SsalddelApiWorkflow(SsalddelWorkflow.SsalddelMart)]
[SsalddelUseCase(
    "마트 주문 요청 작성",
    Summary = "공개 상품 한 건의 비구속 주문 의향을 서버 검증과 사용자별 요청 ID 멱등성으로 저장하며 재고·결제·출고 원장은 변경하지 않습니다.")]
[SsalddelUseCaseActor(SsalddelActor.Orderer)]
public sealed class 마트주문요청작성UseCase(
    SsalddelContext db,
    ICurrentUserAccessor currentUserAccessor) : I마트주문요청작성UseCase
{
    public async Task<Result<마트주문요청응답>> 등록Async(
        마트주문요청등록요청 request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userId = currentUserAccessor.UserId?.Trim();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return 마트주문요청Results.Unauthorized<마트주문요청응답>();
        }

        if (request.클라이언트요청Id == Guid.Empty)
        {
            return 마트주문요청Results.BadRequest<마트주문요청응답>("마트 주문 요청 ID를 확인해 주세요.");
        }

        if (request.공개상품Id <= 0)
        {
            return 마트주문요청Results.BadRequest<마트주문요청응답>("주문 요청할 공개 상품 ID를 확인해 주세요.");
        }

        if (request.수량 is < 1 or > 100)
        {
            return 마트주문요청Results.BadRequest<마트주문요청응답>("주문 요청 수량은 1개 이상 100개 이하로 입력해 주세요.");
        }

        var existing = await FindByClientRequestAsync(userId, request.클라이언트요청Id, cancellationToken);
        if (existing is not null)
        {
            return existing.공개상품Id == request.공개상품Id && existing.수량 == request.수량
                ? Result.Ok(마트주문요청Mapper.ToResponse(existing))
                : 마트주문요청Results.Conflict<마트주문요청응답>(
                    "같은 요청 ID를 다른 상품이나 수량에 다시 사용할 수 없습니다.");
        }

        if (!마트주문요청안내.유효한확인(request))
        {
            return 마트주문요청Results.BadRequest<마트주문요청응답>(
                "현재 마트 주문 요청 안내를 확인하고 비구속 저장에 동의해 주세요.");
        }

        var product = await db.마트공개상품
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == request.공개상품Id && item.공개여부, cancellationToken);
        if (product is null)
        {
            return 마트주문요청Results.ProductNotFound<마트주문요청응답>();
        }

        if (!product.판매허용여부 || product.판매가능수량 <= 0)
        {
            return 마트주문요청Results.Conflict<마트주문요청응답>(
                "현재 판매 가능한 상품이 아닙니다. 공개 상품 상태를 다시 확인해 주세요.");
        }

        if (request.수량 > product.판매가능수량)
        {
            return 마트주문요청Results.Conflict<마트주문요청응답>(
                $"현재 표시 가능한 수량은 {product.판매가능수량:N0}개입니다. 수량을 줄여 다시 요청해 주세요.");
        }

        var now = DateTime.UtcNow;
        var orderRequest = new 마트주문요청
        {
            Id = Guid.NewGuid(),
            요청자UserId = userId,
            클라이언트요청Id = request.클라이언트요청Id,
            공개상품Id = product.Id,
            상품명Snapshot = product.상품명.Trim(),
            판매단위Snapshot = product.판매단위.Trim(),
            단가Snapshot = product.판매가,
            수량 = request.수량,
            합계Snapshot = product.판매가 * request.수량,
            통화 = "KRW",
            제출시판매가능수량 = product.판매가능수량,
            재고기준시각Utc = product.재고기준시각Utc,
            상태코드 = 마트주문요청상태코드.제출됨,
            비구속주문요청확인 = true,
            안내버전 = 마트주문요청안내.현재버전,
            CreatedAtUtc = now
        };
        db.마트주문요청.Add(orderRequest);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            db.Entry(orderRequest).State = EntityState.Detached;
            var concurrentlyCreated = await FindByClientRequestAsync(
                userId,
                request.클라이언트요청Id,
                cancellationToken);
            if (concurrentlyCreated is null)
            {
                throw;
            }

            return concurrentlyCreated.공개상품Id == request.공개상품Id
                   && concurrentlyCreated.수량 == request.수량
                ? Result.Ok(마트주문요청Mapper.ToResponse(concurrentlyCreated))
                : 마트주문요청Results.Conflict<마트주문요청응답>(
                    "같은 요청 ID를 다른 상품이나 수량에 다시 사용할 수 없습니다.");
        }

        return Result.Ok(마트주문요청Mapper.ToResponse(orderRequest));
    }

    private Task<마트주문요청?> FindByClientRequestAsync(
        string userId,
        Guid clientRequestId,
        CancellationToken cancellationToken)
        => db.마트주문요청
            .AsNoTracking()
            .SingleOrDefaultAsync(item =>
                item.요청자UserId == userId
                && item.클라이언트요청Id == clientRequestId,
                cancellationToken);
}
