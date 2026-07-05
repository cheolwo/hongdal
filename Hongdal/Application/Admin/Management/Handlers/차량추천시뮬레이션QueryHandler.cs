using Hongdal.Application.Shipper.Request;
using Hongdal.Contracts.Shipper.Request;

namespace Hongdal.Application.Admin.Management;

public sealed class 차량추천시뮬레이션QueryHandler : IRequestHandler<차량추천시뮬레이션Query, Hongdal.Contracts.Admin.Management.차량추천시뮬레이션응답>
{
    private readonly I차량추천Service _vehicleRecommendationService;

    public 차량추천시뮬레이션QueryHandler(I차량추천Service vehicleRecommendationService)
    {
        _vehicleRecommendationService = vehicleRecommendationService;
    }

    public async Task<Hongdal.Contracts.Admin.Management.차량추천시뮬레이션응답> Handle(차량추천시뮬레이션Query request, CancellationToken cancellationToken)
    {
        var response = await _vehicleRecommendationService.추천Async(new 차량추천요청
        {
            화물종류 = request.요청.화물종류,
            화물수량 = request.요청.화물수량,
            화물길이Mm = request.요청.화물길이Mm,
            화물폭Mm = request.요청.화물폭Mm,
            화물높이Mm = request.요청.화물높이Mm,
            화물중량Kg = request.요청.화물중량Kg,
            화물부피Cbm = request.요청.화물부피Cbm,
            팔레트개수 = request.요청.팔레트개수,
            화물온도조건 = request.요청.화물온도조건,
            화물파손주의여부 = request.요청.화물파손주의여부
        }, cancellationToken);

        return 차량추천관리매퍼.To응답(response);
    }
}
