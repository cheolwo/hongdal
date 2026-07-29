using FluentResults;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Application.CommandProcessing;
using Ssalddel.Contracts.Common.ContractManagement;
using Ssalddel.Contracts.Common.Metadata;
using 살뜰.Data;
using 살뜰.도메인.공급중개;

namespace Ssalddel.Application.ContractManagement;

public interface I조직개별공급발주UseCase
{
    Task<Result<IReadOnlyList<플랫폼공급계약응답>>> 이용가능계약조회Async(
        string organizationTypeCode,
        CancellationToken cancellationToken);

    Task<Result<공급계약이용등록응답>> 공급계약이용등록Async(
        Guid agreementId,
        공급계약이용등록요청 request,
        CancellationToken cancellationToken);

    Task<Result<개별공급발주응답>> 발주등록Async(
        개별공급발주등록요청 request,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<개별공급발주응답>>> 발주목록조회Async(
        개별공급발주목록조회요청 request,
        CancellationToken cancellationToken);

    Task<Result<개별공급발주응답>> 발주철회Async(
        Guid orderId,
        개별공급발주철회요청 request,
        CancellationToken cancellationToken);
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.PlatformSupplyBrokerage,
    SsalddelCodeLayer.Application,
    "음식점과 살들마트가 공통 공급조건 계약을 이용하고 자기 명의의 개별 발주를 공급자에게 제출하도록 중개합니다.",
    Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
    ContractType = typeof(개별공급발주등록요청),
    FlowOrder = 60,
    Boundary = "조직 접근 Claim과 별도 동의를 검증하고 공급자 제출 원장만 저장하며 결제·재고·입고 상태는 변경하지 않습니다.")]
public sealed class 조직개별공급발주UseCase(
    SsalddelContext db,
    ICurrentUserAccessor currentUserAccessor,
    I공급조직접근Accessor organizationAccess) : I조직개별공급발주UseCase
{
    public async Task<Result<IReadOnlyList<플랫폼공급계약응답>>> 이용가능계약조회Async(
        string organizationTypeCode,
        CancellationToken cancellationToken)
    {
        var access = ResolveAccess<IReadOnlyList<플랫폼공급계약응답>>(organizationTypeCode);
        if (access.Error is not null)
        {
            return access.Error;
        }

        var now = DateTime.UtcNow;
        var agreements = await db.플랫폼공급조건계약
            .AsNoTracking()
            .Include(item => item.품목목록)
            .Where(item =>
                item.상태코드 == 플랫폼공급계약상태코드.활성
                && item.유효시작Utc <= now
                && item.유효종료Utc >= now)
            .OrderBy(item => item.공급자명)
            .ToListAsync(cancellationToken);

        IReadOnlyList<플랫폼공급계약응답> response = agreements
            .Where(item => item.품목목록.Any(product => product.조직유형허용(organizationTypeCode)))
            .Select(item => 공급중개Mapper.ToResponse(item, organizationTypeCode))
            .ToArray();
        return Result.Ok(response);
    }

    public async Task<Result<공급계약이용등록응답>> 공급계약이용등록Async(
        Guid agreementId,
        공급계약이용등록요청 request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var access = ResolveAccess<공급계약이용등록응답>(request.조직유형코드);
        if (access.Error is not null)
        {
            return access.Error;
        }

        if (agreementId == Guid.Empty || request.클라이언트요청Id == Guid.Empty)
        {
            return 공급중개Results.BadRequest<공급계약이용등록응답>(
                "공급계약과 이용등록 요청 ID를 확인해 주세요.");
        }

        if (!request.공급계약이용동의
            || !request.개별발주별도확인동의
            || !string.Equals(request.안내버전, 공급중개안내.현재버전, StringComparison.Ordinal))
        {
            return 공급중개Results.BadRequest<공급계약이용등록응답>(
                "공급계약 이용과 개별 발주 별도 확인 안내에 동의해 주세요.");
        }

        var organizationKey = access.OrganizationKey!;
        var existingByRequest = await db.공급계약이용등록.SingleOrDefaultAsync(
            item => item.조직유형코드 == request.조직유형코드
                    && item.조직참조Key == organizationKey
                    && item.클라이언트요청Id == request.클라이언트요청Id,
            cancellationToken);
        if (existingByRequest is not null)
        {
            return existingByRequest.공급계약Id == agreementId
                ? Result.Ok(공급중개Mapper.ToResponse(existingByRequest))
                : 공급중개Results.Conflict<공급계약이용등록응답>(
                    "같은 요청 ID를 다른 공급계약 이용등록에 다시 사용할 수 없습니다.");
        }

        var existingParticipation = await db.공급계약이용등록.SingleOrDefaultAsync(
            item => item.공급계약Id == agreementId
                    && item.조직유형코드 == request.조직유형코드
                    && item.조직참조Key == organizationKey,
            cancellationToken);
        if (existingParticipation is not null)
        {
            return string.Equals(
                       existingParticipation.계약문서버전,
                       request.계약문서버전.Trim(),
                       StringComparison.Ordinal)
                   && string.Equals(
                       existingParticipation.상태코드,
                       공급계약이용상태코드.이용중,
                       StringComparison.Ordinal)
                ? Result.Ok(공급중개Mapper.ToResponse(existingParticipation))
                : 공급중개Results.Conflict<공급계약이용등록응답>(
                    "기존 공급계약 이용등록의 버전 또는 상태를 확인해 주세요.");
        }

        var now = DateTime.UtcNow;
        var agreement = await db.플랫폼공급조건계약
            .Include(item => item.품목목록)
            .SingleOrDefaultAsync(item => item.Id == agreementId, cancellationToken);
        if (agreement is null)
        {
            return 공급중개Results.NotFound<공급계약이용등록응답>("공급계약을 찾을 수 없습니다.");
        }

        if (!IsUsable(agreement, now)
            || !agreement.품목목록.Any(item => item.조직유형허용(request.조직유형코드)))
        {
            return 공급중개Results.Conflict<공급계약이용등록응답>(
                "현재 조직이 이용할 수 있는 활성 공급계약이 아닙니다.");
        }

        if (!string.Equals(
                agreement.계약문서버전,
                request.계약문서버전.Trim(),
                StringComparison.Ordinal))
        {
            return 공급중개Results.Conflict<공급계약이용등록응답>(
                "현재 활성 공급계약 문서 버전을 다시 확인해 주세요.");
        }

        var participation = new 공급계약이용등록
        {
            Id = Guid.NewGuid(),
            클라이언트요청Id = request.클라이언트요청Id,
            공급계약Id = agreement.Id,
            조직유형코드 = request.조직유형코드,
            조직참조Key = organizationKey,
            운영자UserId = access.UserId!,
            계약문서버전 = agreement.계약문서버전,
            상태코드 = 공급계약이용상태코드.이용중,
            공급계약이용동의 = true,
            개별발주별도확인동의 = true,
            안내버전 = 공급중개안내.현재버전,
            등록시각Utc = now,
            수정시각Utc = now
        };
        db.공급계약이용등록.Add(participation);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Ok(공급중개Mapper.ToResponse(participation));
    }

    public async Task<Result<개별공급발주응답>> 발주등록Async(
        개별공급발주등록요청 request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userId = currentUserAccessor.UserId?.Trim();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return 공급중개Results.Unauthorized<개별공급발주응답>();
        }

        var validation = ValidateOrderRequest(request);
        if (validation is not null)
        {
            return 공급중개Results.BadRequest<개별공급발주응답>(validation);
        }

        var participation = await db.공급계약이용등록
            .Include(item => item.공급계약)
            .ThenInclude(item => item.품목목록)
            .SingleOrDefaultAsync(item => item.Id == request.공급계약이용등록Id, cancellationToken);
        if (participation is null)
        {
            return 공급중개Results.NotFound<개별공급발주응답>(
                "공급계약 이용등록을 찾을 수 없습니다.");
        }

        var organizationKey = organizationAccess.조직참조Key조회(participation.조직유형코드)?.Trim();
        if (string.IsNullOrWhiteSpace(organizationKey)
            || !string.Equals(organizationKey, participation.조직참조Key, StringComparison.Ordinal))
        {
            return 공급중개Results.Forbidden<개별공급발주응답>(
                "현재 계정은 이 공급계약 이용조직을 대신해 발주할 수 없습니다.");
        }

        var existing = await db.조직개별공급발주.SingleOrDefaultAsync(
            item => item.구매조직유형코드 == participation.조직유형코드
                    && item.구매조직참조Key == participation.조직참조Key
                    && item.클라이언트요청Id == request.클라이언트요청Id,
            cancellationToken);
        if (existing is not null)
        {
            return SameOrder(existing, request)
                ? Result.Ok(공급중개Mapper.ToResponse(existing))
                : 공급중개Results.Conflict<개별공급발주응답>(
                    "같은 요청 ID를 다른 품목이나 수량의 발주에 다시 사용할 수 없습니다.");
        }

        var now = DateTime.UtcNow;
        var agreement = participation.공급계약;
        if (!string.Equals(
                participation.상태코드,
                공급계약이용상태코드.이용중,
                StringComparison.Ordinal)
            || !IsUsable(agreement, now))
        {
            return 공급중개Results.Conflict<개별공급발주응답>(
                "현재 이용 중인 활성 공급계약에서만 개별 발주할 수 있습니다.");
        }

        if (!string.Equals(participation.계약문서버전, agreement.계약문서버전, StringComparison.Ordinal)
            || !string.Equals(
                request.계약문서버전.Trim(),
                agreement.계약문서버전,
                StringComparison.Ordinal))
        {
            return 공급중개Results.Conflict<개별공급발주응답>(
                "공급계약 문서 버전이 변경되었습니다. 계약과 이용등록을 다시 확인해 주세요.");
        }

        var item = agreement.품목목록.SingleOrDefault(product => product.Id == request.공급계약품목Id);
        if (item is null || !item.조직유형허용(participation.조직유형코드))
        {
            return 공급중개Results.NotFound<개별공급발주응답>(
                "현재 조직이 발주할 수 있는 계약 품목을 찾을 수 없습니다.");
        }

        if (request.발주수량 < item.최소발주수량
            || item.최대발주수량.HasValue && request.발주수량 > item.최대발주수량.Value)
        {
            return 공급중개Results.Conflict<개별공급발주응답>(
                "발주 수량이 계약 품목의 최소·최대 발주 범위를 벗어났습니다.");
        }

        var order = new 조직개별공급발주
        {
            Id = Guid.NewGuid(),
            클라이언트요청Id = request.클라이언트요청Id,
            공급계약이용등록Id = participation.Id,
            공급계약Id = agreement.Id,
            공급계약품목Id = item.Id,
            구매조직유형코드 = participation.조직유형코드,
            구매조직참조Key = participation.조직참조Key,
            요청자UserId = userId,
            계약번호Snapshot = agreement.계약번호,
            계약문서버전Snapshot = agreement.계약문서버전,
            공급자KeySnapshot = agreement.공급자Key,
            공급자명Snapshot = agreement.공급자명,
            품목명Snapshot = item.품목명,
            SKUSnapshot = item.SKU,
            공급단위Snapshot = item.공급단위,
            발주수량 = request.발주수량,
            계약단가Snapshot = item.계약단가,
            발주금액Snapshot = item.계약단가 * request.발주수량,
            통화코드Snapshot = agreement.통화코드,
            희망납품일Utc = request.희망납품일Utc,
            납품지참조Key = request.납품지참조Key.Trim(),
            상태코드 = 개별공급발주상태코드.공급자제출됨,
            플랫폼역할코드 = 공급중개역할코드.개별발주중개,
            플랫폼판매자여부 = false,
            플랫폼재판매자여부 = false,
            결제실행됨 = false,
            재고예약됨 = false,
            입고생성됨 = false,
            개별발주확인 = true,
            공급자판매자확인 = true,
            플랫폼중개자확인 = true,
            안내버전 = 공급중개안내.현재버전,
            제출시각Utc = now,
            수정시각Utc = now
        };
        db.조직개별공급발주.Add(order);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            db.Entry(order).State = EntityState.Detached;
            var concurrentlyCreated = await db.조직개별공급발주.SingleOrDefaultAsync(
                candidate => candidate.구매조직유형코드 == participation.조직유형코드
                             && candidate.구매조직참조Key == participation.조직참조Key
                             && candidate.클라이언트요청Id == request.클라이언트요청Id,
                cancellationToken);
            if (concurrentlyCreated is not null && SameOrder(concurrentlyCreated, request))
            {
                return Result.Ok(공급중개Mapper.ToResponse(concurrentlyCreated));
            }

            throw;
        }

        return Result.Ok(공급중개Mapper.ToResponse(order));
    }

    public async Task<Result<IReadOnlyList<개별공급발주응답>>> 발주목록조회Async(
        개별공급발주목록조회요청 request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var access = ResolveAccess<IReadOnlyList<개별공급발주응답>>(request.조직유형코드);
        if (access.Error is not null)
        {
            return access.Error;
        }

        var query = db.조직개별공급발주
            .AsNoTracking()
            .Where(item =>
                item.구매조직유형코드 == request.조직유형코드
                && item.구매조직참조Key == access.OrganizationKey);
        if (!string.IsNullOrWhiteSpace(request.상태코드))
        {
            var status = request.상태코드.Trim();
            query = query.Where(item => item.상태코드 == status);
        }

        var orders = await query
            .OrderByDescending(item => item.제출시각Utc)
            .ToArrayAsync(cancellationToken);
        IReadOnlyList<개별공급발주응답> response = orders
            .Select(공급중개Mapper.ToResponse)
            .ToArray();
        return Result.Ok(response);
    }

    public async Task<Result<개별공급발주응답>> 발주철회Async(
        Guid orderId,
        개별공급발주철회요청 request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var access = ResolveAccess<개별공급발주응답>(request.조직유형코드);
        if (access.Error is not null)
        {
            return access.Error;
        }

        var order = await db.조직개별공급발주.SingleOrDefaultAsync(
            item => item.Id == orderId
                    && item.구매조직유형코드 == request.조직유형코드
                    && item.구매조직참조Key == access.OrganizationKey,
            cancellationToken);
        if (order is null)
        {
            return 공급중개Results.NotFound<개별공급발주응답>(
                "개별 공급 발주를 찾을 수 없거나 현재 조직의 발주가 아닙니다.");
        }

        if (string.Equals(order.상태코드, 개별공급발주상태코드.철회, StringComparison.Ordinal))
        {
            return Result.Ok(공급중개Mapper.ToResponse(order));
        }

        if (!string.Equals(order.상태코드, request.기대상태코드, StringComparison.Ordinal)
            || !string.Equals(
                order.상태코드,
                개별공급발주상태코드.공급자제출됨,
                StringComparison.Ordinal))
        {
            return 공급중개Results.Conflict<개별공급발주응답>(
                "공급자가 이미 응답한 발주는 이 경로에서 철회할 수 없습니다.");
        }

        order.상태코드 = 개별공급발주상태코드.철회;
        order.수정시각Utc = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return Result.Ok(공급중개Mapper.ToResponse(order));
    }

    private AccessResolution<T> ResolveAccess<T>(string organizationTypeCode)
    {
        var userId = currentUserAccessor.UserId?.Trim();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return new(null, null, 공급중개Results.Unauthorized<T>());
        }

        if (!공급이용조직유형코드.지원됨(organizationTypeCode))
        {
            return new(
                userId,
                null,
                공급중개Results.BadRequest<T>("이용조직 유형은 음식점 또는 살들마트여야 합니다."));
        }

        var organizationKey = organizationAccess.조직참조Key조회(organizationTypeCode)?.Trim();
        return string.IsNullOrWhiteSpace(organizationKey)
            ? new(
                userId,
                null,
                공급중개Results.Forbidden<T>(
                    "현재 계정에서 요청한 음식점 또는 살들마트 접근 범위를 확인할 수 없습니다."))
            : new(userId, organizationKey, null);
    }

    private static string? ValidateOrderRequest(개별공급발주등록요청 request)
    {
        if (request.클라이언트요청Id == Guid.Empty
            || request.공급계약이용등록Id == Guid.Empty
            || request.공급계약품목Id == Guid.Empty)
        {
            return "개별 발주 요청, 공급계약 이용등록과 계약 품목 ID를 확인해 주세요.";
        }

        if (request.발주수량 <= 0)
        {
            return "발주 수량은 0보다 커야 합니다.";
        }

        if (request.희망납품일Utc <= DateTime.UtcNow)
        {
            return "희망 납품일은 현재 시각보다 이후여야 합니다.";
        }

        if (string.IsNullOrWhiteSpace(request.납품지참조Key)
            || request.납품지참조Key.Trim().Length > 200)
        {
            return "납품지는 주소 원문이 아닌 등록된 납품지 참조 Key로 입력해 주세요.";
        }

        if (!request.개별발주확인
            || !request.공급자판매자확인
            || !request.플랫폼중개자확인
            || !string.Equals(request.안내버전, 공급중개안내.현재버전, StringComparison.Ordinal))
        {
            return "매수인·판매자·플랫폼 중개 역할과 개별 발주 안내를 확인해 주세요.";
        }

        return null;
    }

    private static bool IsUsable(플랫폼공급조건계약 agreement, DateTime now)
        => string.Equals(agreement.상태코드, 플랫폼공급계약상태코드.활성, StringComparison.Ordinal)
           && agreement.유효시작Utc <= now
           && agreement.유효종료Utc >= now
           && !agreement.플랫폼판매자여부
           && !agreement.플랫폼재판매자여부
           && string.Equals(
               agreement.플랫폼역할코드,
               공급중개역할코드.개별발주중개,
               StringComparison.Ordinal);

    private static bool SameOrder(조직개별공급발주 order, 개별공급발주등록요청 request)
        => order.공급계약이용등록Id == request.공급계약이용등록Id
           && order.공급계약품목Id == request.공급계약품목Id
           && order.발주수량 == request.발주수량
           && order.희망납품일Utc == request.희망납품일Utc
           && string.Equals(
               order.납품지참조Key,
               request.납품지참조Key.Trim(),
               StringComparison.Ordinal);

    private sealed record AccessResolution<T>(
        string? UserId,
        string? OrganizationKey,
        Result<T>? Error);
}
