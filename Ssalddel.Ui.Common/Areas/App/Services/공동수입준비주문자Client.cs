using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.Orderer;

namespace Ssalddel.Ui.Common.Areas.App.Services;

public interface I공동수입준비주문자Client
{
    Task<공동수입준비주문자조회응답?> 조회Async(
        string 공동수입원장Id,
        string 자동집단Id,
        CancellationToken cancellationToken = default);
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.GroupImportTradeReadiness,
    SsalddelCodeLayer.ClientAdapter,
    "주문자 화면이 공동수입 원장 식별자와 본인 원천 자동집단 식별자로 1.5 준비 자료를 읽습니다.",
    ContractType = typeof(I공동수입준비주문자Client),
    FlowOrder = 40,
    Effects = SsalddelCodeEffect.NetworkCall | SsalddelCodeEffect.PersistentRead,
    Boundary = "인증된 GET 조회만 수행하며 관리자 OS 실행·재시도, 포워더 전송, 계약, 결제, 신고와 운송 API를 호출하지 않습니다.")]
public sealed class 공동수입준비주문자Client(ISsalddelJsonApiClient client) : I공동수입준비주문자Client
{
    public Task<공동수입준비주문자조회응답?> 조회Async(
        string 공동수입원장Id,
        string 자동집단Id,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(공동수입원장Id);
        ArgumentException.ThrowIfNullOrWhiteSpace(자동집단Id);

        var path = $"api/v1/orderer/group-imports/{Escape(공동수입원장Id)}/readiness" +
                   $"?autoGroupId={Escape(자동집단Id)}";
        return client.GetAsync<공동수입준비주문자조회응답>(
            path,
            "주문자 공동수입 1.5 준비 조회",
            allowNotFound: true,
            cancellationToken);
    }

    private static string Escape(string value)
        => Uri.EscapeDataString(value.Trim());
}
