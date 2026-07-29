using Ssalddel.Domain.AgriculturalFisheries;

namespace Ssalddel.Services.AgriculturalFisheries.Information;

public sealed record 국제농수산가격공급결과(
    string SourceKey,
    string SourceUrl,
    IReadOnlyList<국제농수산가격관측> Observations,
    IReadOnlyList<string> SourceMessages);

public interface I국제농수산가격공급자
{
    string SourceKey { get; }

    Task<국제농수산가격공급결과> CollectAsync(
        int yearFrom,
        int yearTo,
        DateTime collectedAtUtc,
        CancellationToken cancellationToken = default);
}
