using DriverApp.Models.Driver;
using DriverApp.Models.Driver.Map;

namespace DriverApp.Services;

public interface IDriverHomeMapService
{
    IReadOnlyList<DriverMapMarkerItem> BuildMarkers(IEnumerable<DriverRequestItem> requests);
}
