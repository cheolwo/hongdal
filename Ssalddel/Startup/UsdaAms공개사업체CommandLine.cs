using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Contracts.Common.AgriculturalFisheries;
using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Infrastructure.Persistence.AgriculturalFisheries;
using Ssalddel.Services.AgriculturalFisheries.Information;

namespace Ssalddel.Startup;

internal static class UsdaAms공개사업체CommandLine
{
    private const string ReportCommand =
        "--report-usda-ams-public-businesses";
    private const string CollectCommand =
        "--collect-usda-ams-public-businesses";
    private const string DirectoryTypePrefix = "--directory-type=";

    public static async Task<bool> TryRunAsync(
        IReadOnlyList<string> arguments,
        IServiceProvider services,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        if (HasCommand(arguments, ReportCommand))
        {
            await ReportAsync(
                services,
                logger,
                cancellationToken);
            return true;
        }

        if (!HasCommand(arguments, CollectCommand))
        {
            return false;
        }

        await CollectAsync(
            arguments,
            services,
            logger,
            cancellationToken);
        return true;
    }

    private static async Task ReportAsync(
        IServiceProvider services,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider
            .GetRequiredService<AgriculturalFisheriesDbContext>();
        var latestRun = await db.UsdaAmsPublicBusinessCollectionRuns
            .AsNoTracking()
            .OrderByDescending(item => item.Id)
            .FirstOrDefaultAsync(cancellationToken);
        var directoryCounts = await db.UsdaAmsPublicBusinessProfiles
            .AsNoTracking()
            .GroupBy(item => item.DirectoryTypeCode)
            .Select(group => new
            {
                DirectoryType = group.Key,
                Total = group.LongCount(),
                Current = group.LongCount(item => item.IsCurrentlyListed)
            })
            .OrderBy(item => item.DirectoryType)
            .ToArrayAsync(cancellationToken);
        var report = new
        {
            LatestRun = latestRun is null
                ? null
                : new
                {
                    latestRun.Id,
                    latestRun.StatusCode,
                    latestRun.CompletedDirectoryCount,
                    latestRun.FetchedCount,
                    latestRun.InsertedCount,
                    latestRun.UpdatedCount,
                    latestRun.UnchangedCount,
                    latestRun.NoLongerListedCount,
                    latestRun.RejectedCount,
                    latestRun.CompletedAtUtc
                },
            ProfileCount = await db.UsdaAmsPublicBusinessProfiles
                .LongCountAsync(cancellationToken),
            CurrentProfileCount = await db.UsdaAmsPublicBusinessProfiles
                .LongCountAsync(
                    item => item.IsCurrentlyListed,
                    cancellationToken),
            ProductLinkCount = await db.UsdaAmsPublicBusinessProducts
                .LongCountAsync(cancellationToken),
            CityStateProfileCount = await db.UsdaAmsPublicBusinessProfiles
                .LongCountAsync(
                    item =>
                        item.LocationPrecisionCode
                        == UsdaAms공개사업체위치정밀도Codes.도시주,
                    cancellationToken),
            DirectoryCounts = directoryCounts
        };
        logger.LogInformation(
            "USDA AMS 공개 사업체 DB 현황: {Report}",
            JsonSerializer.Serialize(report));
    }

    private static async Task CollectAsync(
        IReadOnlyList<string> arguments,
        IServiceProvider services,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var directoryTypes = arguments
            .Where(argument => argument.StartsWith(
                DirectoryTypePrefix,
                StringComparison.OrdinalIgnoreCase))
            .Select(argument => argument[DirectoryTypePrefix.Length..])
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToArray();

        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider
            .GetRequiredService<AgriculturalFisheriesDbContext>();
        await db.Database.MigrateAsync(cancellationToken);
        var service = scope.ServiceProvider
            .GetRequiredService<IUsdaAms공개사업체ArchiveService>();
        var result = await service.CollectAsync(
            new UsdaAms공개사업체수집요청
            {
                DirectoryTypes = directoryTypes
            },
            cancellationToken);
        logger.LogInformation(
            "USDA AMS 공개 사업체 DB 저장 완료. RunId={RunId}, Directories={Directories}, Fetched={Fetched}, Inserted={Inserted}, Updated={Updated}, Unchanged={Unchanged}, NoLongerListed={NoLongerListed}, Rejected={Rejected}",
            result.CollectionRunId,
            string.Join(',', result.DirectoryTypes),
            result.FetchedCount,
            result.InsertedCount,
            result.UpdatedCount,
            result.UnchangedCount,
            result.NoLongerListedCount,
            result.RejectedCount);
    }

    private static bool HasCommand(
        IEnumerable<string> arguments,
        string command)
        => arguments.Any(argument => string.Equals(
            argument,
            command,
            StringComparison.OrdinalIgnoreCase));
}
