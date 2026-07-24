using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.Orderer;

namespace Ssalddel.Ui.Common.Areas.App.Services;

public interface I무역확장원장Client
{
    Task<무역확장원장응답?> 개별수입생성Async(
        string 주문원장Id,
        개별수입원장생성요청 request,
        CancellationToken cancellationToken = default);

    Task<무역확장원장응답?> 개별수출생성Async(
        string 주문원장Id,
        개별수출원장생성요청 request,
        CancellationToken cancellationToken = default);

    Task<무역확장원장응답?> 공동수출생성Async(
        공동수출원장생성요청 request,
        CancellationToken cancellationToken = default);

    Task<무역확장원장응답?> 조회Async(
        string 원장종류,
        string 원장Id,
        CancellationToken cancellationToken = default);
}

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.TradeLedgerExtensions,
    SsalddelCodeLayer.ClientAdapter,
    "주문자 앱에서 세 무역 확장 원장 API를 인증·멱등 헤더와 함께 호출합니다.",
    Effects = SsalddelCodeEffect.NetworkCall | SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
    ContractType = typeof(I무역확장원장Client),
    FlowOrder = 40,
    Boundary = "Simulation 원장 생성·조회만 호출하며 계약·결제·신고 제출·포워더 전송 API는 호출하지 않습니다.")]
public sealed class 무역확장원장Client(ISsalddelJsonApiClient client) : I무역확장원장Client
{
    public Task<무역확장원장응답?> 개별수입생성Async(
        string 주문원장Id,
        개별수입원장생성요청 request,
        CancellationToken cancellationToken = default)
        => SendAsync(
            $"api/v1/orderer/order-ledgers/{Escape(주문원장Id)}/individual-import-ledger",
            request,
            request.요청멱등키,
            "개별수입 원장 생성",
            cancellationToken);

    public Task<무역확장원장응답?> 개별수출생성Async(
        string 주문원장Id,
        개별수출원장생성요청 request,
        CancellationToken cancellationToken = default)
        => SendAsync(
            $"api/v1/orderer/order-ledgers/{Escape(주문원장Id)}/individual-export-ledger",
            request,
            request.요청멱등키,
            "개별수출 원장 생성",
            cancellationToken);

    public Task<무역확장원장응답?> 공동수출생성Async(
        공동수출원장생성요청 request,
        CancellationToken cancellationToken = default)
        => SendAsync(
            "api/v1/orderer/group-export-ledgers",
            request,
            request.요청멱등키,
            "공동수출 원장 생성",
            cancellationToken);

    public Task<무역확장원장응답?> 조회Async(
        string 원장종류,
        string 원장Id,
        CancellationToken cancellationToken = default)
    {
        var root = 원장종류 switch
        {
            "individual-import" => "individual-import-ledgers",
            "individual-export" => "individual-export-ledgers",
            "group-export" => "group-export-ledgers",
            _ => throw new ArgumentOutOfRangeException(nameof(원장종류))
        };
        return client.GetAsync<무역확장원장응답>(
            $"api/v1/orderer/{root}/{Escape(원장Id)}",
            "무역 확장 원장 조회",
            allowNotFound: true,
            cancellationToken);
    }

    private Task<무역확장원장응답?> SendAsync<TRequest>(
        string path,
        TRequest request,
        string idempotencyKey,
        string operationName,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);
        return client.SendWithHeadersAsync<TRequest, 무역확장원장응답>(
            HttpMethod.Post,
            path,
            request,
            new Dictionary<string, string>
            {
                ["Idempotency-Key"] = idempotencyKey.Trim()
            },
            operationName,
            cancellationToken: cancellationToken);
    }

    private static string Escape(string value)
        => Uri.EscapeDataString(value.Trim());
}
