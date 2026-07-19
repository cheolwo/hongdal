using Ssalddel.Contracts.Common.Warehouse;

namespace Ssalddel.Services.LogisticsProcessing.Warehouse;

public interface I피킹배치Engine
{
    Task<피킹배치계획결과> 계획Async(피킹배치계획요청 request, CancellationToken cancellationToken);
}
