using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.Orderer;

namespace Ssalddel.Ui.Common.Areas.App.Services;

public interface I공동구매내원함Client
{
    Task<공동구매내원함목록응답?> 내원함목록조회Async(
        CancellationToken cancellationToken = default);
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.GroupPurchaseDemandOperatingSystem,
    SsalddelCodeLayer.ClientAdapter,
    "주문자 화면이 로그인 사용자의 개별 원함과 본인이 참여한 자동집단 공개 요약을 조회합니다.",
    ContractType = typeof(I공동구매내원함Client),
    FlowOrder = 45,
    Effects = SsalddelCodeEffect.NetworkCall | SsalddelCodeEffect.PersistentRead,
    Boundary = "Bearer 인증 GET만 수행하며 다른 주문자의 원함이나 자동집단 내부 수요를 조회하지 않습니다.")]
public sealed class 공동구매내원함Client(ISsalddelJsonApiClient client) : I공동구매내원함Client
{
    public Task<공동구매내원함목록응답?> 내원함목록조회Async(
        CancellationToken cancellationToken = default)
        => client.GetAsync<공동구매내원함목록응답>(
            "api/v1/orderer/group-purchase-wishes/me",
            "내 공동구매 원함 목록 조회",
            cancellationToken: cancellationToken);
}
