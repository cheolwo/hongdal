using System.Threading;
using System.Threading.Tasks;
using Ssalddel.Simulation.Contracts;

namespace Ssalddel.Simulation.Application
{
public interface ISimulation공유공공데이터조회Port
{
    Task<Simulation공유공공데이터조회결과> Kamis가격관측조회Async(
        string? itemName,
        int limit,
        CancellationToken cancellationToken);
}
}
