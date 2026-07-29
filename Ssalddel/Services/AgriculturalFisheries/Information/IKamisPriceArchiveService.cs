namespace Ssalddel.Services.AgriculturalFisheries.Information;

public sealed record KamisPriceArchiveResult(
    long CollectionRunId,
    int FetchedCount,
    int InsertedCount,
    int UpdatedCount,
    int ExistingCount,
    DateOnly? LatestSurveyDate);

public interface IKamisPriceArchiveService
{
    Task<KamisPriceArchiveResult> CollectDailyPricesAsync(
        DateOnly requestedDate,
        CancellationToken cancellationToken = default);

    Task<KamisPriceArchiveResult> CollectPeriodPricesAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default);

    Task<KamisPriceArchiveResult> CollectPeriodPricesForItemCodesAsync(
        DateOnly startDate,
        DateOnly endDate,
        IReadOnlyCollection<string> itemCodes,
        CancellationToken cancellationToken = default)
        => CollectPeriodPricesAsync(startDate, endDate, cancellationToken);

    Task<KamisPriceArchiveResult> CollectMonthlyPricesAsync(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default);
}
