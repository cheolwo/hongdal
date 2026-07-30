using Ssalddel.Contracts.Common.AgriculturalFisheries;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

public interface IUsdaAms공개사업체ArchiveService
{
    Task<UsdaAms공개사업체수집응답> CollectAsync(
        UsdaAms공개사업체수집요청 request,
        CancellationToken cancellationToken = default);
}

public interface IUsdaAms공개사업체QueryService
{
    Task<UsdaAms공개사업체조회응답> SearchAsync(
        UsdaAms공개사업체조회요청 request,
        CancellationToken cancellationToken = default);
}
