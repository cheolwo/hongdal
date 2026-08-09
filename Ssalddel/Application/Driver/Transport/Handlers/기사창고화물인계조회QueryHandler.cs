using Ssalddel.Contracts.Common.Inbound;
using Ssalddel.Contracts.Common.Metadata;
using Ssalddel.Contracts.Common.WorldProjection;
using Ssalddel.Contracts.Driver.Transport;
using Ssalddel.Application.WorldProjection;

namespace Ssalddel.Application.Driver.Transport;

[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.WorldRolePerspective,
    SsalddelCodeLayer.Application,
    "현재 기사의 운송과 연계 입고를 운송 NPC·창고 NPC 화물 인계 workflow로 투영한다.",
    Effects = SsalddelCodeEffect.PersistentRead,
    ContractType = typeof(CargoWarehouseHandoffResponse),
    FlowOrder = 50,
    Boundary = "배정 운송과 연결된 입고만 조회하며 주소·연락처·상품 상세와 Unity 좌표를 반환하지 않는다.")]
public sealed class 기사창고화물인계조회QueryHandler
    : IRequestHandler<기사창고화물인계조회Query, CargoWarehouseHandoffResponse?>
{
    private readonly IRequestHandler<운송현재조회Query, 기사운송요약응답?> currentTransportReader;
    private readonly IRequestHandler<운송연계입고조회Query, 운송연계입고Projection?> inboundReader;

    public 기사창고화물인계조회QueryHandler(
        IRequestHandler<운송현재조회Query, 기사운송요약응답?> currentTransportReader,
        IRequestHandler<운송연계입고조회Query, 운송연계입고Projection?> inboundReader)
    {
        this.currentTransportReader = currentTransportReader;
        this.inboundReader = inboundReader;
    }

    public async Task<CargoWarehouseHandoffResponse?> Handle(
        기사창고화물인계조회Query request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.기사Id);

        var transport = await currentTransportReader.Handle(
            new 운송현재조회Query(request.기사Id), cancellationToken);
        if (transport is null)
        {
            return null;
        }

        var inbound = await inboundReader.Handle(
            new 운송연계입고조회Query(transport.운송번호), cancellationToken);
        if (inbound is null)
        {
            return null;
        }

        return CargoWarehouseHandoffProjectionBuilder.Build(
            transport.Id,
            transport.상태,
            transport.UpdatedAt,
            inbound.Id,
            inbound.상태,
            inbound.UpdatedAt,
            DateTimeOffset.UtcNow);
    }
}
