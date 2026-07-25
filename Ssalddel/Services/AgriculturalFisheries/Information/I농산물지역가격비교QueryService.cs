using Ssalddel.Contracts.Common.AgriculturalFisheries;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

public interface I농산물지역가격비교QueryService
{
    Task<농산물지역가격비교선택지응답> GetOptionsAsync(
        농산물지역가격비교선택지요청 request,
        CancellationToken cancellationToken = default);

    Task<농산물지역가격비교응답> CompareAsync(
        농산물지역가격비교요청 request,
        CancellationToken cancellationToken = default);
}
