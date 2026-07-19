using DriverApp.Models.Driver;
using Ssalddel.Contracts.Common.Drivers;

namespace DriverApp.Services;

public sealed class DriverHomeMapService : IDriverHomeMapService
{
    public IReadOnlyList<DriverMapMarkerItem> BuildMarkers(IEnumerable<DriverRequestItem> requests)
    {
        return requests
            .Where(HasPickupCoordinate)
            .Select(x => new DriverMapMarkerItem(
                x.의뢰Id,
                ToDouble(x.픽업_위도),
                ToDouble(x.픽업_경도),
                ToDouble(x.하차_위도),
                ToDouble(x.하차_경도),
                x.화물종류,
                x.요약설명,
                x.픽업지,
                x.하차지,
                "추천 상차지",
                "추천 하차지"))
            .ToArray();
    }

    private static bool HasPickupCoordinate(DriverRequestItem request)
    {
        return request.픽업_위도 is not null
            && request.픽업_경도 is not null
            && request.픽업_위도 != 0m
            && request.픽업_경도 != 0m;
    }

    private static double ToDouble(decimal? value)
    {
        return (double)(value ?? 0m);
    }
}
