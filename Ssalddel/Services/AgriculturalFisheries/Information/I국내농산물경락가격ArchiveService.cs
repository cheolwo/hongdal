using Ssalddel.Contracts.Common.AgriculturalFisheries;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

public sealed record 국내농산물경락가격수집Result(
    long CollectionRunId,
    int FetchedCount,
    int InsertedCount,
    int UpdatedCount,
    int ExistingCount,
    int CompletedPages,
    bool IsTruncated);

public interface I국내농산물경락가격ArchiveService
{
    Task<국내농산물경락가격수집Result> CollectAsync(
        DateOnly settlementDate,
        CancellationToken cancellationToken = default);

    Task<국내농산물경락가격조회응답> SearchAsync(
        국내농산물경락가격조회요청 request,
        CancellationToken cancellationToken = default);
}
