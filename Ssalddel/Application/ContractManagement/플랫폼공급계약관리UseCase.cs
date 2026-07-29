using FluentResults;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Contracts.Common.ContractManagement;
using Ssalddel.Contracts.Common.Metadata;
using 살뜰.Data;
using 살뜰.도메인.공급중개;

namespace Ssalddel.Application.ContractManagement;

public interface I플랫폼공급계약관리UseCase
{
    Task<Result<플랫폼공급계약응답>> 등록Async(
        플랫폼공급계약등록요청 request,
        CancellationToken cancellationToken);

    Task<Result<플랫폼공급계약응답>> 활성화Async(
        Guid agreementId,
        플랫폼공급계약활성화요청 request,
        CancellationToken cancellationToken);

    Task<Result<개별공급발주응답>> 공급자응답기록Async(
        Guid orderId,
        개별공급발주공급자응답기록요청 request,
        CancellationToken cancellationToken);
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.PlatformSupplyBrokerage,
    SsalddelCodeLayer.Application,
    "공급자와 플랫폼의 공급조건 계약을 관리하고 공급자의 개별 발주 응답을 증거와 함께 기록합니다.",
    Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
    ContractType = typeof(플랫폼공급계약등록요청),
    FlowOrder = 50,
    Boundary = "플랫폼 운영자는 공급자 응답을 기록할 뿐 개별 발주의 판매자·매수인 또는 재판매자가 되지 않습니다.")]
public sealed class 플랫폼공급계약관리UseCase(
    SsalddelContext db,
    ICurrentUserAccessor currentUserAccessor) : I플랫폼공급계약관리UseCase
{
    public async Task<Result<플랫폼공급계약응답>> 등록Async(
        플랫폼공급계약등록요청 request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var adminUserId = currentUserAccessor.UserId?.Trim();
        if (string.IsNullOrWhiteSpace(adminUserId))
        {
            return 공급중개Results.Unauthorized<플랫폼공급계약응답>();
        }

        var validation = ValidateRegistration(request);
        if (validation is not null)
        {
            return 공급중개Results.BadRequest<플랫폼공급계약응답>(validation);
        }

        var existing = await db.플랫폼공급조건계약
            .Include(item => item.품목목록)
            .SingleOrDefaultAsync(
                item => item.생성자UserId == adminUserId
                        && item.클라이언트요청Id == request.클라이언트요청Id,
                cancellationToken);
        if (existing is not null)
        {
            return SameRegistration(existing, request)
                ? Result.Ok(공급중개Mapper.ToResponse(existing))
                : 공급중개Results.Conflict<플랫폼공급계약응답>(
                    "같은 요청 ID를 다른 공급계약에 다시 사용할 수 없습니다.");
        }

        var normalizedContractNumber = request.계약번호.Trim();
        if (await db.플랫폼공급조건계약.AnyAsync(
                item => item.계약번호 == normalizedContractNumber,
                cancellationToken))
        {
            return 공급중개Results.Conflict<플랫폼공급계약응답>(
                "이미 등록된 공급계약 번호입니다.");
        }

        var now = DateTime.UtcNow;
        var agreement = new 플랫폼공급조건계약
        {
            Id = Guid.NewGuid(),
            클라이언트요청Id = request.클라이언트요청Id,
            계약번호 = normalizedContractNumber,
            공급자Key = request.공급자Key.Trim(),
            공급자명 = request.공급자명.Trim(),
            계약문서버전 = request.계약문서버전.Trim(),
            상태코드 = 플랫폼공급계약상태코드.초안,
            유효시작Utc = request.유효시작Utc,
            유효종료Utc = request.유효종료Utc,
            통화코드 = request.통화코드.Trim().ToUpperInvariant(),
            정산조건 = request.정산조건.Trim(),
            반품조건 = request.반품조건.Trim(),
            플랫폼역할코드 = 공급중개역할코드.개별발주중개,
            플랫폼판매자여부 = false,
            플랫폼재판매자여부 = false,
            생성자UserId = adminUserId,
            생성시각Utc = now,
            수정시각Utc = now,
            품목목록 = request.품목목록.Select(item => new 플랫폼공급조건계약품목
            {
                Id = Guid.NewGuid(),
                계약품목Key = item.계약품목Key.Trim(),
                SKU = item.SKU.Trim(),
                품목명 = item.품목명.Trim(),
                공급단위 = item.공급단위.Trim(),
                계약단가 = item.계약단가,
                최소발주수량 = item.최소발주수량,
                최대발주수량 = item.최대발주수량,
                원산지표시 = item.원산지표시.Trim(),
                보관조건 = item.보관조건.Trim(),
                허용조직유형Csv = string.Join(
                    ',',
                    item.허용조직유형목록
                        .Select(value => value.Trim())
                        .Distinct(StringComparer.Ordinal)
                        .OrderBy(value => value, StringComparer.Ordinal))
            }).ToList()
        };
        db.플랫폼공급조건계약.Add(agreement);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            db.Entry(agreement).State = EntityState.Detached;
            var concurrentlyCreated = await db.플랫폼공급조건계약
                .Include(item => item.품목목록)
                .SingleOrDefaultAsync(
                    item => item.생성자UserId == adminUserId
                            && item.클라이언트요청Id == request.클라이언트요청Id,
                    cancellationToken);
            if (concurrentlyCreated is not null && SameRegistration(concurrentlyCreated, request))
            {
                return Result.Ok(공급중개Mapper.ToResponse(concurrentlyCreated));
            }

            throw;
        }

        return Result.Ok(공급중개Mapper.ToResponse(agreement));
    }

    public async Task<Result<플랫폼공급계약응답>> 활성화Async(
        Guid agreementId,
        플랫폼공급계약활성화요청 request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(currentUserAccessor.UserId))
        {
            return 공급중개Results.Unauthorized<플랫폼공급계약응답>();
        }

        if (agreementId == Guid.Empty)
        {
            return 공급중개Results.BadRequest<플랫폼공급계약응답>("활성화할 공급계약 ID를 확인해 주세요.");
        }

        if (!request.공급자체결확인
            || !request.플랫폼중개전용확인
            || string.IsNullOrWhiteSpace(request.계약체결근거참조))
        {
            return 공급중개Results.BadRequest<플랫폼공급계약응답>(
                "공급자 체결 근거와 플랫폼의 중개 전용 역할을 확인해야 합니다.");
        }

        var agreement = await db.플랫폼공급조건계약
            .Include(item => item.품목목록)
            .SingleOrDefaultAsync(item => item.Id == agreementId, cancellationToken);
        if (agreement is null)
        {
            return 공급중개Results.NotFound<플랫폼공급계약응답>("공급계약을 찾을 수 없습니다.");
        }

        if (string.Equals(agreement.상태코드, 플랫폼공급계약상태코드.활성, StringComparison.Ordinal))
        {
            return Result.Ok(공급중개Mapper.ToResponse(agreement));
        }

        if (!string.Equals(agreement.상태코드, request.기대상태코드, StringComparison.Ordinal)
            || !string.Equals(agreement.상태코드, 플랫폼공급계약상태코드.초안, StringComparison.Ordinal))
        {
            return 공급중개Results.Conflict<플랫폼공급계약응답>(
                "공급계약 상태가 먼저 변경되었습니다. 최신 계약을 다시 조회해 주세요.");
        }

        if (!string.Equals(
                agreement.계약문서버전,
                request.계약문서버전.Trim(),
                StringComparison.Ordinal))
        {
            return 공급중개Results.Conflict<플랫폼공급계약응답>(
                "활성화하려는 계약 문서 버전이 현재 초안과 다릅니다.");
        }

        agreement.상태코드 = 플랫폼공급계약상태코드.활성;
        agreement.계약체결근거참조 = request.계약체결근거참조.Trim();
        agreement.플랫폼판매자여부 = false;
        agreement.플랫폼재판매자여부 = false;
        agreement.활성화시각Utc = DateTime.UtcNow;
        agreement.수정시각Utc = agreement.활성화시각Utc.Value;
        await db.SaveChangesAsync(cancellationToken);

        return Result.Ok(공급중개Mapper.ToResponse(agreement));
    }

    public async Task<Result<개별공급발주응답>> 공급자응답기록Async(
        Guid orderId,
        개별공급발주공급자응답기록요청 request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var adminUserId = currentUserAccessor.UserId?.Trim();
        if (string.IsNullOrWhiteSpace(adminUserId))
        {
            return 공급중개Results.Unauthorized<개별공급발주응답>();
        }

        if (orderId == Guid.Empty
            || !request.공급자응답확인
            || !개별공급발주상태코드.공급자응답상태(request.공급자응답상태코드)
            || string.IsNullOrWhiteSpace(request.공급자응답근거참조))
        {
            return 공급중개Results.BadRequest<개별공급발주응답>(
                "공급자 응답 상태와 확인 근거를 입력해 주세요.");
        }

        var order = await db.조직개별공급발주
            .SingleOrDefaultAsync(item => item.Id == orderId, cancellationToken);
        if (order is null)
        {
            return 공급중개Results.NotFound<개별공급발주응답>("개별 공급 발주를 찾을 수 없습니다.");
        }

        if (개별공급발주상태코드.공급자응답상태(order.상태코드))
        {
            return string.Equals(
                       order.상태코드,
                       request.공급자응답상태코드,
                       StringComparison.Ordinal)
                   && order.공급자수락수량 == request.수락수량
                ? Result.Ok(공급중개Mapper.ToResponse(order))
                : 공급중개Results.Conflict<개별공급발주응답>(
                    "이미 다른 공급자 응답이 기록되어 있습니다.");
        }

        if (!string.Equals(order.상태코드, request.기대상태코드, StringComparison.Ordinal)
            || !string.Equals(
                order.상태코드,
                개별공급발주상태코드.공급자제출됨,
                StringComparison.Ordinal))
        {
            return 공급중개Results.Conflict<개별공급발주응답>(
                "공급자에게 제출된 상태의 발주만 응답을 기록할 수 있습니다.");
        }

        var quantityValidation = ValidateSupplierQuantity(order, request);
        if (quantityValidation is not null)
        {
            return 공급중개Results.BadRequest<개별공급발주응답>(quantityValidation);
        }

        order.상태코드 = request.공급자응답상태코드;
        order.공급자수락수량 = request.수락수량;
        order.공급자응답근거참조 = request.공급자응답근거참조.Trim();
        order.공급자응답기록자UserId = adminUserId;
        order.공급자응답시각Utc = DateTime.UtcNow;
        order.수정시각Utc = order.공급자응답시각Utc.Value;
        await db.SaveChangesAsync(cancellationToken);

        return Result.Ok(공급중개Mapper.ToResponse(order));
    }

    private static string? ValidateRegistration(플랫폼공급계약등록요청 request)
    {
        if (request.클라이언트요청Id == Guid.Empty)
        {
            return "공급계약 요청 ID를 확인해 주세요.";
        }

        if (MissingOrTooLong(request.계약번호, 100)
            || MissingOrTooLong(request.공급자Key, 160)
            || MissingOrTooLong(request.공급자명, 200)
            || MissingOrTooLong(request.계약문서버전, 80))
        {
            return "계약번호, 공급자와 계약 문서 버전을 확인해 주세요.";
        }

        if (!request.플랫폼중개전용확인)
        {
            return "플랫폼이 판매자나 재판매자가 아닌 개별 발주 중개자임을 확인해야 합니다.";
        }

        if (request.유효시작Utc == default
            || request.유효종료Utc <= request.유효시작Utc)
        {
            return "공급계약 유효기간을 확인해 주세요.";
        }

        if (request.통화코드.Trim().Length != 3)
        {
            return "통화 코드는 3자리로 입력해 주세요.";
        }

        if (request.품목목록.Count == 0)
        {
            return "공급계약에는 한 개 이상의 품목이 필요합니다.";
        }

        if (request.품목목록
            .GroupBy(item => item.계약품목Key.Trim(), StringComparer.Ordinal)
            .Any(group => string.IsNullOrWhiteSpace(group.Key) || group.Count() > 1))
        {
            return "계약 품목 Key는 비어 있지 않고 계약 안에서 중복되지 않아야 합니다.";
        }

        foreach (var item in request.품목목록)
        {
            if (MissingOrTooLong(item.계약품목Key, 160)
                || MissingOrTooLong(item.SKU, 100)
                || MissingOrTooLong(item.품목명, 200)
                || MissingOrTooLong(item.공급단위, 100))
            {
                return "각 계약 품목의 Key, SKU, 품목명과 공급단위를 확인해 주세요.";
            }

            if (item.계약단가 < 0
                || item.최소발주수량 <= 0
                || item.최대발주수량 is <= 0
                || item.최대발주수량 < item.최소발주수량)
            {
                return "계약 품목의 단가와 최소·최대 발주수량을 확인해 주세요.";
            }

            if (item.허용조직유형목록.Count == 0
                || item.허용조직유형목록.Any(value => !공급이용조직유형코드.지원됨(value)))
            {
                return "계약 품목에는 음식점 또는 살들마트 이용 범위를 지정해야 합니다.";
            }
        }

        return null;
    }

    private static string? ValidateSupplierQuantity(
        조직개별공급발주 order,
        개별공급발주공급자응답기록요청 request)
    {
        if (string.Equals(
                request.공급자응답상태코드,
                개별공급발주상태코드.공급자거절,
                StringComparison.Ordinal))
        {
            return request.수락수량 == 0
                ? null
                : "거절 응답의 수락 수량은 0이어야 합니다.";
        }

        if (request.수락수량 <= 0 || request.수락수량 > order.발주수량)
        {
            return "수락 수량은 0보다 크고 발주 수량 이하여야 합니다.";
        }

        if (string.Equals(
                request.공급자응답상태코드,
                개별공급발주상태코드.공급자수락,
                StringComparison.Ordinal)
            && request.수락수량 != order.발주수량)
        {
            return "전체 수락 응답은 발주 수량과 같은 수량을 수락해야 합니다.";
        }

        if (string.Equals(
                request.공급자응답상태코드,
                개별공급발주상태코드.공급자부분수락,
                StringComparison.Ordinal)
            && request.수락수량 >= order.발주수량)
        {
            return "부분 수락 수량은 발주 수량보다 작아야 합니다.";
        }

        return null;
    }

    private static bool SameRegistration(
        플랫폼공급조건계약 agreement,
        플랫폼공급계약등록요청 request)
        => string.Equals(agreement.계약번호, request.계약번호.Trim(), StringComparison.Ordinal)
           && string.Equals(agreement.공급자Key, request.공급자Key.Trim(), StringComparison.Ordinal)
           && string.Equals(
               agreement.계약문서버전,
               request.계약문서버전.Trim(),
               StringComparison.Ordinal);

    private static bool MissingOrTooLong(string? value, int maximumLength)
        => string.IsNullOrWhiteSpace(value) || value.Trim().Length > maximumLength;
}
