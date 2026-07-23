using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Orderer;

namespace Ssalddel.Ui.Common.Areas.App.Services;

/// <summary>
/// 공동구매와 무관하게 주문 원장의 조회, 하위 원장 구성과 서명을 제공하는 기본 업무 경계입니다.
/// </summary>
public interface I주문원장Service
{
    Task<주문원장역할별조회공개Dto?> 주문원장보호조회Async(
        string orderLedgerId,
        CancellationToken cancellationToken = default);

    Task<주문원장역할별조회공개Dto?> 주문원장역할조회Async(
        string orderLedgerId,
        string viewCode,
        CancellationToken cancellationToken = default);

    Task<주문원장통합공개Dto?> 하위원장연결Async(
        string orderLedgerId,
        주문하위원장연결ClientRequest request,
        CancellationToken cancellationToken = default);

    Task<주문원장통합공개Dto?> 하위원장분리Async(
        string orderLedgerId,
        string childLedgerId,
        long? expectedRevision = null,
        CancellationToken cancellationToken = default);

    Task<주문원장서명상태공개Dto?> 주문원장서명상태조회Async(
        string orderLedgerId,
        CancellationToken cancellationToken = default);

    Task<주문원장서명상태공개Dto?> 주문원장서명준비Async(
        string orderLedgerId,
        주문원장서명준비ClientRequest request,
        CancellationToken cancellationToken = default);

    Task<주문원장서명상태공개Dto?> 주문원장서명등록Async(
        string orderLedgerId,
        주문원장서명등록ClientRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 탐색에서 이어온 구매 의향을 집단화 미리보기, 비구속 저장과 철회로 닫는 최소 API 경계입니다.
/// 결제·주문·수입·운송·창고 실행은 이 경계에 포함하지 않습니다.
/// </summary>
public interface I비구속공동구매수요Service
{
    Task<공동구매자동집단배치미리보기응답?> 수요배치미리보기Async(
        공동구매자동수요등록Command request,
        CancellationToken cancellationToken = default)
        => throw new NotSupportedException("이 서비스는 비구속 수요 배치 미리보기를 지원하지 않습니다.");

    Task<공동구매자동집단사용자응답?> 비구속수요저장Async(
        공동구매자동수요등록Command request,
        CancellationToken cancellationToken = default)
        => throw new NotSupportedException("이 서비스는 비구속 수요 저장을 지원하지 않습니다.");

    Task<공동구매자동수요철회응답?> 비구속수요철회Async(
        string demandSourceKey,
        string idempotencyKey,
        string? reason = null,
        CancellationToken cancellationToken = default)
        => throw new NotSupportedException("이 서비스는 비구속 수요 철회를 지원하지 않습니다.");

    Task<공동구매자동수요철회응답?> 비구속수요철회Async(
        string demandSourceKey,
        string idempotencyKey,
        long expectedWishRevision,
        string? reason = null,
        CancellationToken cancellationToken = default)
        => 비구속수요철회Async(
            demandSourceKey,
            idempotencyKey,
            reason,
            cancellationToken);
}

/// <summary>
/// 공동구매 확정 이후 자동집단, 기본 주문원장 업무와 커머스 이행을 연결하는 API 경계입니다.
/// </summary>
public interface I공동구매실행Service : I주문원장Service, I비구속공동구매수요Service
{
    Task<IReadOnlyList<공동구매자동집단응답>> 자동집단목록조회Async(
        공동구매자동집단조회조건 condition,
        CancellationToken cancellationToken = default);

    Task<공동구매자동집단배치미리보기응답?> 자동배치미리보기Async(
        공동구매자동수요등록Command request,
        CancellationToken cancellationToken = default);

    Task<공동구매자동집단응답?> 자동수요등록Async(
        공동구매자동수요등록Command request,
        CancellationToken cancellationToken = default);

    Task<공동구매자동수요철회응답?> 자동수요철회Async(
        string demandSourceKey,
        string? reason = null,
        CancellationToken cancellationToken = default)
        => throw new NotSupportedException("이 공동구매 실행 서비스는 비구속 수요 철회를 지원하지 않습니다.");

    Task<IReadOnlyList<공동구매커머스이행계획공개Dto>> 공동구매별커머스이행조회Async(
        string groupPurchaseId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<공동구매커머스이행계획공개Dto>> 문서번호로커머스이행조회Async(
        string documentManagementNumber,
        CancellationToken cancellationToken = default);
}

public sealed class 공동구매실행Service(ISsalddelJsonApiClient client) : I공동구매실행Service
{
    private const string IdempotencyHeaderName = "Idempotency-Key";
    private const string AutoGroupsPath = "api/v1/orderer/group-purchase-auto-groups";
    private const string OrderLedgersPath = "api/v1/community/order-ledgers";
    private const string CommercePath = "api/v1/orderer/group-purchase-commerce-fulfillment-plans";

    public async Task<IReadOnlyList<공동구매자동집단응답>> 자동집단목록조회Async(
        공동구매자동집단조회조건 condition,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(condition);
        var query = new List<string>();
        AddQuery(query, "productKey", condition.상품키);
        AddQuery(query, "deliveryScopeKey", condition.배송권키);
        AddQuery(query, "currentStatus", condition.현재상태);
        var path = query.Count == 0 ? AutoGroupsPath : $"{AutoGroupsPath}?{string.Join("&", query)}";

        return await client.GetAsync<IReadOnlyList<공동구매자동집단응답>>(
                   path,
                   "공동구매 자동집단 목록 조회",
                   cancellationToken: cancellationToken)
               ?? [];
    }

    public Task<공동구매자동집단배치미리보기응답?> 자동배치미리보기Async(
        공동구매자동수요등록Command request,
        CancellationToken cancellationToken = default)
        => client.SendAsync<공동구매자동수요등록Command, 공동구매자동집단배치미리보기응답>(
            HttpMethod.Post,
            $"{AutoGroupsPath}/placement-preview",
            request,
            "공동구매 자동집단 배치 미리보기",
            cancellationToken: cancellationToken);

    public Task<공동구매자동집단배치미리보기응답?> 수요배치미리보기Async(
        공동구매자동수요등록Command request,
        CancellationToken cancellationToken = default)
        => 자동배치미리보기Async(request, cancellationToken);

    public Task<공동구매자동집단응답?> 자동수요등록Async(
        공동구매자동수요등록Command request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.수요출처키);
        request.요청멱등키 = string.IsNullOrWhiteSpace(request.요청멱등키)
            ? $"demand-save:{Guid.NewGuid():N}"
            : request.요청멱등키.Trim();

        return client.SendWithHeadersAsync<공동구매자동수요등록Command, 공동구매자동집단응답>(
            HttpMethod.Put,
            $"{AutoGroupsPath}/demands/{Segment(request.수요출처키)}",
            request,
            IdempotencyHeaders(request.요청멱등키),
            "공동구매 자동집단 비구속 수요 저장",
            cancellationToken: cancellationToken);
    }

    public Task<공동구매자동수요철회응답?> 자동수요철회Async(
        string demandSourceKey,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        var path = $"{AutoGroupsPath}/demands/{Segment(demandSourceKey)}";
        if (!string.IsNullOrWhiteSpace(reason))
        {
            path += $"?reason={Uri.EscapeDataString(reason.Trim())}";
        }

        var idempotencyKey = $"demand-withdraw:{Guid.NewGuid():N}";
        return client.SendWithHeadersAsync<공동구매자동수요철회응답>(
            HttpMethod.Delete,
            path,
            IdempotencyHeaders(idempotencyKey),
            "공동구매 자동집단 비구속 수요 철회",
            cancellationToken: cancellationToken);
    }

    public Task<공동구매자동집단사용자응답?> 비구속수요저장Async(
        공동구매자동수요등록Command request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.수요출처키);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.요청멱등키);

        return client.SendWithHeadersAsync<공동구매자동수요등록Command, 공동구매자동집단사용자응답>(
            HttpMethod.Put,
            $"{AutoGroupsPath}/demands/{Segment(request.수요출처키)}",
            request,
            IdempotencyHeaders(request.요청멱등키.Trim()),
            "공동구매 탐색 비구속 수요 저장",
            cancellationToken: cancellationToken);
    }

    public Task<공동구매자동수요철회응답?> 비구속수요철회Async(
        string demandSourceKey,
        string idempotencyKey,
        string? reason = null,
        CancellationToken cancellationToken = default)
        => 비구속수요철회내부Async(
            demandSourceKey,
            idempotencyKey,
            expectedWishRevision: null,
            reason,
            cancellationToken);

    public Task<공동구매자동수요철회응답?> 비구속수요철회Async(
        string demandSourceKey,
        string idempotencyKey,
        long expectedWishRevision,
        string? reason = null,
        CancellationToken cancellationToken = default)
        => 비구속수요철회내부Async(
            demandSourceKey,
            idempotencyKey,
            expectedWishRevision,
            reason,
            cancellationToken);

    private Task<공동구매자동수요철회응답?> 비구속수요철회내부Async(
        string demandSourceKey,
        string idempotencyKey,
        long? expectedWishRevision,
        string? reason,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(demandSourceKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);
        var path = $"{AutoGroupsPath}/demands/{Segment(demandSourceKey)}";
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(reason))
        {
            query.Add($"reason={Uri.EscapeDataString(reason.Trim())}");
        }
        if (expectedWishRevision is not null)
        {
            query.Add($"expectedWishRevision={expectedWishRevision.Value}");
        }
        if (query.Count > 0)
        {
            path += $"?{string.Join("&", query)}";
        }

        return client.SendWithHeadersAsync<공동구매자동수요철회응답>(
            HttpMethod.Delete,
            path,
            IdempotencyHeaders(idempotencyKey.Trim()),
            "공동구매 탐색 비구속 수요 철회",
            cancellationToken: cancellationToken);
    }

    private static IReadOnlyDictionary<string, string> IdempotencyHeaders(string idempotencyKey)
        => new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [IdempotencyHeaderName] = idempotencyKey
        };

    public Task<주문원장역할별조회공개Dto?> 주문원장보호조회Async(
        string orderLedgerId,
        CancellationToken cancellationToken = default)
        => client.GetAsync<주문원장역할별조회공개Dto>(
            $"{OrderLedgersPath}/{Segment(orderLedgerId)}",
            "주문 원장 주문자 보호형 조회",
            cancellationToken: cancellationToken);

    public Task<주문원장역할별조회공개Dto?> 주문원장역할조회Async(
        string orderLedgerId,
        string viewCode,
        CancellationToken cancellationToken = default)
        => client.GetAsync<주문원장역할별조회공개Dto>(
            $"{OrderLedgersPath}/{Segment(orderLedgerId)}/views/{Segment(viewCode)}",
            "주문 원장 역할별 조회",
            cancellationToken: cancellationToken);

    public Task<주문원장통합공개Dto?> 하위원장연결Async(
        string orderLedgerId,
        주문하위원장연결ClientRequest request,
        CancellationToken cancellationToken = default)
        => client.SendAsync<주문하위원장연결ClientRequest, 주문원장통합공개Dto>(
            HttpMethod.Post,
            $"{OrderLedgersPath}/{Segment(orderLedgerId)}/children",
            request,
            "주문 하위 원장 연결",
            cancellationToken: cancellationToken);

    public Task<주문원장통합공개Dto?> 하위원장분리Async(
        string orderLedgerId,
        string childLedgerId,
        long? expectedRevision = null,
        CancellationToken cancellationToken = default)
    {
        var path = $"{OrderLedgersPath}/{Segment(orderLedgerId)}/children/{Segment(childLedgerId)}";
        if (expectedRevision is not null)
        {
            path += $"?{Uri.EscapeDataString("기대Revision")}={expectedRevision.Value}";
        }

        return client.SendAsync<주문원장통합공개Dto>(
            HttpMethod.Delete,
            path,
            "주문 하위 원장 분리",
            cancellationToken: cancellationToken);
    }

    public Task<주문원장서명상태공개Dto?> 주문원장서명상태조회Async(
        string orderLedgerId,
        CancellationToken cancellationToken = default)
        => client.GetAsync<주문원장서명상태공개Dto>(
            $"{OrderLedgersPath}/{Segment(orderLedgerId)}/signature",
            "주문 원장 서명 상태 조회",
            cancellationToken: cancellationToken);

    public Task<주문원장서명상태공개Dto?> 주문원장서명준비Async(
        string orderLedgerId,
        주문원장서명준비ClientRequest request,
        CancellationToken cancellationToken = default)
        => client.SendAsync<주문원장서명준비ClientRequest, 주문원장서명상태공개Dto>(
            HttpMethod.Post,
            $"{OrderLedgersPath}/{Segment(orderLedgerId)}/signature-request",
            request,
            "주문 원장 서명 요청 준비",
            cancellationToken: cancellationToken);

    public Task<주문원장서명상태공개Dto?> 주문원장서명등록Async(
        string orderLedgerId,
        주문원장서명등록ClientRequest request,
        CancellationToken cancellationToken = default)
        => client.SendAsync<주문원장서명등록ClientRequest, 주문원장서명상태공개Dto>(
            HttpMethod.Post,
            $"{OrderLedgersPath}/{Segment(orderLedgerId)}/signatures",
            request,
            "주문 원장 서명 등록",
            cancellationToken: cancellationToken);

    public async Task<IReadOnlyList<공동구매커머스이행계획공개Dto>> 공동구매별커머스이행조회Async(
        string groupPurchaseId,
        CancellationToken cancellationToken = default)
        => await client.GetAsync<IReadOnlyList<공동구매커머스이행계획공개Dto>>(
               $"{CommercePath}/by-group-purchase/{Segment(groupPurchaseId)}",
               "공동구매별 커머스 이행 조회",
               cancellationToken: cancellationToken)
           ?? [];

    public async Task<IReadOnlyList<공동구매커머스이행계획공개Dto>> 문서번호로커머스이행조회Async(
        string documentManagementNumber,
        CancellationToken cancellationToken = default)
        => await client.GetAsync<IReadOnlyList<공동구매커머스이행계획공개Dto>>(
               $"{CommercePath}/lookup?documentManagementNumber={Uri.EscapeDataString(documentManagementNumber.Trim())}",
               "문서관리번호 커머스 이행 조회",
               allowNotFound: true,
               cancellationToken: cancellationToken)
           ?? [];

    private static string Segment(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return Uri.EscapeDataString(value.Trim());
    }

    private static void AddQuery(ICollection<string> query, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            query.Add($"{name}={Uri.EscapeDataString(value.Trim())}");
        }
    }
}
