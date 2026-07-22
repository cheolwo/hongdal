using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Ui.Common.Areas.App.Services;

namespace Ssalddel.Ui.Common.Areas.BackOffice.Services;

public interface I공동수입준비관리Client
{
    Task<IReadOnlyList<공동구매자동집단요약응답>> 작업대목록조회Async(
        CancellationToken cancellationToken = default);

    Task<공동구매수요모집Os상태응답?> 운영상태조회Async(
        string 자동집단Id,
        CancellationToken cancellationToken = default);

    Task<공동구매수요모집인계승인응답> 인계승인Async(
        string 자동집단Id,
        공동구매수요모집인계승인요청 요청,
        CancellationToken cancellationToken = default);

    Task<공동수입준비원장응답?> 준비원장조회Async(
        string 자동집단Id,
        CancellationToken cancellationToken = default);

    Task<공동수입준비원장응답> 미리보기Async(
        string 자동집단Id,
        공동수입준비원장저장요청 요청,
        CancellationToken cancellationToken = default);

    Task<공동수입준비원장응답> 저장Async(
        string 자동집단Id,
        공동수입준비원장저장요청 요청,
        CancellationToken cancellationToken = default);
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.GroupImportTradeReadiness,
    SsalddelCodeLayer.ClientAdapter,
    "두 관리자 앱에서 1.0 인계 상태와 1.5 공급·가격·무역 준비 원장을 같은 API 계약으로 조회하고 저장합니다.",
    ContractType = typeof(I공동수입준비관리Client),
    FlowOrder = 40,
    Effects = SsalddelCodeEffect.NetworkCall | SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
    Boundary = "서버 관리자 토큰과 멱등 키만 전달하며 계약, 결제, 신고, 운송 또는 창고 실행 API를 호출하지 않습니다.")]
public sealed class 공동수입준비관리Client(ISsalddelJsonApiClient client) : I공동수입준비관리Client
{
    private const string 자동집단BasePath = "api/v1/orderer/group-purchase-auto-groups";
    private const string 수요OsAdminBasePath = "api/v1/admin/orderer/group-purchase-demand-os";

    public async Task<IReadOnlyList<공동구매자동집단요약응답>> 작업대목록조회Async(
        CancellationToken cancellationToken = default)
    {
        var 확정검토Task = client.GetAsync<IReadOnlyList<공동구매자동집단요약응답>>(
            $"{자동집단BasePath}?currentStatus={Uri.EscapeDataString(공동구매자동집단상태코드.확정대기)}",
            "1.5 인계 검토 집단 조회",
            allowNotFound: false,
            cancellationToken);
        var 인계후보Task = client.GetAsync<IReadOnlyList<공동구매자동집단요약응답>>(
            $"{자동집단BasePath}?currentStatus={Uri.EscapeDataString(공동구매자동집단상태코드.확정)}",
            "1.5 인계 완료 후보 조회",
            allowNotFound: false,
            cancellationToken);

        await Task.WhenAll(확정검토Task, 인계후보Task);
        return (확정검토Task.Result ?? [])
            .Concat(인계후보Task.Result ?? [])
            .GroupBy(item => item.자동집단Id, StringComparer.Ordinal)
            .Select(group => group.OrderByDescending(item => item.수정시각Utc).First())
            .OrderBy(item => item.현재상태 == 공동구매자동집단상태코드.확정대기 ? 0 : 1)
            .ThenByDescending(item => item.수정시각Utc)
            .ToArray();
    }

    public Task<공동구매수요모집Os상태응답?> 운영상태조회Async(
        string 자동집단Id,
        CancellationToken cancellationToken = default)
        => client.GetAsync<공동구매수요모집Os상태응답>(
            $"{수요OsAdminBasePath}/groups/{Escape(자동집단Id)}/operating-status",
            "공동구매 모집 OS 상태 조회",
            allowNotFound: true,
            cancellationToken);

    public async Task<공동구매수요모집인계승인응답> 인계승인Async(
        string 자동집단Id,
        공동구매수요모집인계승인요청 요청,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(요청);
        var result = await client.SendWithHeadersAsync<공동구매수요모집인계승인요청, 공동구매수요모집인계승인응답>(
            HttpMethod.Post,
            $"{수요OsAdminBasePath}/groups/{Escape(자동집단Id)}/handoff-approval",
            요청,
            IdempotencyHeader(요청.요청멱등키),
            "1.5 준비 인계 승인",
            cancellationToken: cancellationToken);
        return result ?? throw new InvalidOperationException("1.5 준비 인계 승인 응답이 비어 있습니다.");
    }

    public Task<공동수입준비원장응답?> 준비원장조회Async(
        string 자동집단Id,
        CancellationToken cancellationToken = default)
        => client.GetAsync<공동수입준비원장응답>(
            ReadinessPath(자동집단Id),
            "1.5 준비 원장 조회",
            allowNotFound: true,
            cancellationToken);

    public async Task<공동수입준비원장응답> 미리보기Async(
        string 자동집단Id,
        공동수입준비원장저장요청 요청,
        CancellationToken cancellationToken = default)
    {
        var result = await client.SendAsync<공동수입준비원장저장요청, 공동수입준비원장응답>(
            HttpMethod.Post,
            $"{ReadinessPath(자동집단Id)}/preview",
            요청,
            "1.5 준비 원장 미리보기",
            cancellationToken: cancellationToken);
        return result ?? throw new InvalidOperationException("1.5 준비 원장 미리보기 응답이 비어 있습니다.");
    }

    public async Task<공동수입준비원장응답> 저장Async(
        string 자동집단Id,
        공동수입준비원장저장요청 요청,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(요청);
        var result = await client.SendWithHeadersAsync<공동수입준비원장저장요청, 공동수입준비원장응답>(
            HttpMethod.Put,
            ReadinessPath(자동집단Id),
            요청,
            IdempotencyHeader(요청.요청멱등키),
            "1.5 준비 원장 저장",
            cancellationToken: cancellationToken);
        return result ?? throw new InvalidOperationException("1.5 준비 원장 저장 응답이 비어 있습니다.");
    }

    private static IReadOnlyDictionary<string, string> IdempotencyHeader(string key)
        => new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Idempotency-Key"] = key
        };

    private static string ReadinessPath(string 자동집단Id)
        => $"{수요OsAdminBasePath}/groups/{Escape(자동집단Id)}/trade-readiness";

    private static string Escape(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return Uri.EscapeDataString(value.Trim());
    }
}
