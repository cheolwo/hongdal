using DriverApp.Models.Driver;
using Ssalddel.Contracts.Common.Drivers;

namespace DriverApp.Services;

public interface IDriverHomeMapService
{
    IReadOnlyList<DriverMapMarkerItem> BuildMarkers(IEnumerable<DriverRequestItem> requests);
}
