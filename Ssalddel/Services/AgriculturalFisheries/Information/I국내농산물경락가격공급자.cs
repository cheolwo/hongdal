using Ssalddel.Contracts.Common.AgriculturalFisheries;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

public interface I국내농산물경락가격공급자
{
    string SourceKey { get; }

    국내농산물경락가격원천응답 GetSource();

    Task<국내농산물경락가격조회응답> 조회Async(
        국내농산물경락가격조회요청 request,
        CancellationToken cancellationToken = default);
}

public interface I국내농산물경락가격조회Service
{
    IReadOnlyList<국내농산물경락가격원천응답> GetSources();

    Task<국내농산물경락가격조회응답> 조회Async(
        국내농산물경락가격조회요청 request,
        CancellationToken cancellationToken = default);
}
