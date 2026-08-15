using System.Security.Cryptography;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Ssalddel.Domain.PublicData;
using Ssalddel.Infrastructure.Persistence.PublicData;
using 살뜰.Infrastructure;
using 살뜰.Services.External.PublicData;
using 살뜰.Services.External.PublicData.Korea;

namespace Ssalddel.Startup;

internal static class 대한민국공간공공데이터CommandLine
{
    internal const string 법정동수집Command = "--collect-korea-legal-dong-codes";
    internal const string 행정동관할수집Command = "--collect-korea-administrative-jurisdictions";
    internal const string 공개사업장가져오기Command = "--import-localdata-businesses";
    internal const string VWorld건물가져오기Command = "--import-vworld-building-master";
    internal const string 평창군공간원본등록Command = "--register-pyeongchang-spatial-sources";

    public static async Task<bool> TryRunAsync(
        IReadOnlyList<string> arguments,
        IServiceProvider services,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(logger);

        if (HasCommand(arguments, 법정동수집Command))
        {
            await CollectLegalDongAsync(arguments, services, logger, cancellationToken);
            return true;
        }

        if (HasCommand(arguments, 행정동관할수집Command))
        {
            await CollectAdministrativeJurisdictionsAsync(
                arguments,
                services,
                logger,
                cancellationToken);
            return true;
        }

        if (HasCommand(arguments, VWorld건물가져오기Command))
        {
            await ImportVWorldBuildingsAsync(arguments, services, logger, cancellationToken);
            return true;
        }

        if (HasCommand(arguments, 평창군공간원본등록Command))
        {
            await RegisterPyeongchangSpatialSourcesAsync(
                arguments, services, logger, cancellationToken);
            return true;
        }

        if (!HasCommand(arguments, 공개사업장가져오기Command))
            return false;

        await ImportLicensedBusinessesAsync(arguments, services, logger, cancellationToken);
        return true;
    }

    private static async Task RegisterPyeongchangSpatialSourcesAsync(
        IReadOnlyList<string> arguments,
        IServiceProvider services,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var sources = new[]
        {
            SpatialSource(arguments, "--vworld-dem-file=", "vworld", "ngii-dem-90m-korea", "2023-07-26", "2023-07-26", "application/zip", DateTimeOffset.Parse("2023-07-26T00:00:00Z")),
            SpatialSource(arguments, "--legal-boundary-file=", "vworld", "legal-boundary-emd-lio-sig", "2026-07-01", "2026-07-01", "application/zip", DateTimeOffset.Parse("2026-07-01T00:00:00Z")),
            SpatialSource(arguments, "--worldcover-file=", "esa-worldcover", "pyeongchang-land-cover-2021-v200-epsg5186", "2021-v200", "pyeongchang-2021-v200", "image/tiff", DateTimeOffset.Parse("2021-12-31T00:00:00Z")),
            SpatialSource(arguments, "--copernicus-dem-file=", "copernicus-dem", "pyeongchang-glo30-epsg5186", "GLO-30", "pyeongchang-glo30", "image/tiff", null),
            SpatialSource(arguments, "--landcover-stat-file=", "mcee-environmental-spatial-information", "detailed-land-cover-area-by-region", "2013-2024", "2024-12-31", "text/csv", DateTimeOffset.Parse("2024-12-31T00:00:00Z")),
            SpatialSource(arguments, "--tile-manifest-file=", "ssalddel-spatial-pipeline", "pyeongchang-spatial-tile-manifest", "v1", "worldcover-2021-v200", "application/json", DateTimeOffset.Parse("2021-12-31T00:00:00Z")),
        };

        await using var scope = services.CreateAsyncScope();
        var ingestionDb = scope.ServiceProvider.GetRequiredService<PublicDataIngestionDbContext>();
        await ingestionDb.Database.MigrateAsync(cancellationToken);
        var registration = scope.ServiceProvider.GetRequiredService<평창군공공공간원본등록Service>();
        var insertedSources = 0;
        var existingSources = 0;
        long landCoverSnapshotId = 0;
        string? landCoverPath = null;
        foreach (var source in sources)
        {
            var result = await registration.RegisterFileAsync(
                source.FilePath,
                new 공공공간원본등록Request(
                    source.SourceId,
                    source.DatasetId,
                    source.SourceVersion,
                    source.DataRevision,
                    source.EvidenceAsOfUtc,
                    source.ContentType,
                    source.PrivateStorageLocation),
                cancellationToken);
            if (result.Inserted) insertedSources++; else existingSources++;
            if (source.DatasetId == "detailed-land-cover-area-by-region")
            {
                landCoverSnapshotId = result.RawSnapshotId;
                landCoverPath = source.FilePath;
            }
        }

        var normalized = await registration.NormalizeLandCoverStatisticsAsync(
            landCoverPath!,
            landCoverSnapshotId,
            "2024-12-31",
            cancellationToken);
        logger.LogInformation(
            "평창군 공간 원본 등록 완료. AddedSources={AddedSources}, ExistingSources={ExistingSources}, LandCoverRows={Rows}, AddedMetrics={AddedMetrics}, ExistingMetrics={ExistingMetrics}",
            insertedSources,
            existingSources,
            normalized.ParsedRowCount,
            normalized.InsertedRecordCount,
            normalized.ExistingRecordCount);
    }

    private static 공간원본Argument SpatialSource(
        IReadOnlyList<string> arguments,
        string option,
        string sourceId,
        string datasetId,
        string sourceVersion,
        string dataRevision,
        string contentType,
        DateTimeOffset? evidenceAsOfUtc)
    {
        var filePath = GetOption(arguments, option);
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            throw new FileNotFoundException(
                $"{평창군공간원본등록Command}에는 존재하는 {option} 경로가 필요합니다.",
                filePath);
        var fullPath = Path.GetFullPath(filePath);
        return new 공간원본Argument(
            fullPath,
            sourceId,
            datasetId,
            sourceVersion,
            dataRevision,
            contentType,
            evidenceAsOfUtc,
            Path.GetRelativePath(Directory.GetCurrentDirectory(), fullPath));
    }

    private static async Task ImportVWorldBuildingsAsync(
        IReadOnlyList<string> arguments,
        IServiceProvider services,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var filePath = GetOption(arguments, "--file=");
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            throw new FileNotFoundException(
                $"{VWorld건물가져오기Command}에는 존재하는 --file 경로가 필요합니다.",
                filePath);
        var sourceRevision = GetOption(arguments, "--source-revision=");
        if (string.IsNullOrWhiteSpace(sourceRevision))
            throw new ArgumentException(
                $"{VWorld건물가져오기Command}에는 --source-revision이 필요합니다.");
        var sourceVintage = GetOption(arguments, "--source-vintage=") ?? sourceRevision;
        var observedAt = DateTimeOffset.TryParse(
            GetOption(arguments, "--observed-at="),
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal,
            out var parsedObservedAt)
                ? parsedObservedAt.ToUniversalTime()
                : File.GetLastWriteTimeUtc(filePath);
        var privateStorageLocation = GetOption(arguments, "--private-storage-location=")
            ?? Path.GetRelativePath(Directory.GetCurrentDirectory(), Path.GetFullPath(filePath));

        await using var scope = services.CreateAsyncScope();
        var ingestionDb = scope.ServiceProvider.GetRequiredService<PublicDataIngestionDbContext>();
        await ingestionDb.Database.MigrateAsync(cancellationToken);
        if (HasCommand(arguments, "--replace-existing"))
        {
            var existingBuildingIds = await ingestionDb.BuildingRegisterTitles
                .Where(item => item.SourceRevision == sourceRevision)
                .Select(item => item.Id)
                .ToListAsync(cancellationToken);
            if (existingBuildingIds.Count > 0)
            {
                await ingestionDb.BuildingRegisterTitles
                    .Where(item => item.SourceRevision == sourceRevision)
                    .ExecuteDeleteAsync(cancellationToken);
                logger.LogWarning(
                    "VWorld 건물통합정보 불완전 실행본 교체. Revision={Revision}, RemovedBuildings={Count}",
                    sourceRevision,
                    existingBuildingIds.Count);
            }
        }
        await using var sourceStream = File.OpenRead(filePath);
        var imported = await scope.ServiceProvider
            .GetRequiredService<VWorld건물통합정보ImportService>()
            .ImportZipAsync(
                sourceStream,
                Path.GetFileName(filePath),
                new VWorld건물통합정보ImportRequest(
                    sourceRevision,
                    sourceVintage,
                    observedAt,
                    privateStorageLocation),
                cancellationToken);
        var classified = await scope.ServiceProvider
            .GetRequiredService<건축물주용도분류원장Service>()
            .ClassifyAndAggregateAsync(sourceRevision, sourceVintage, cancellationToken);
        var massing = await scope.ServiceProvider
            .GetRequiredService<건축물형태구성원장Service>()
            .형태와시각계획생성Async(sourceRevision, cancellationToken);

        logger.LogInformation(
            "VWorld 건물통합정보 적재 완료. Parsed={Parsed}, Inserted={Inserted}, Existing={Existing}, Rejected={Rejected}, Snapshot={Snapshot}, Hash={Hash}, Categories={Categories}, MassingProfiles={Massing}, VisualPlans={VisualPlans}",
            imported.ParsedCount,
            imported.InsertedCount,
            imported.ExistingCount,
            imported.RejectedCount,
            imported.RawSnapshotId,
            imported.SourceHashSha256,
            classified.InsertedAssignmentCount,
            massing.추가형태Profile수,
            massing.추가시각계획수);
    }

    internal static 공개사업장가져오기Arguments ParseImportArguments(
        IReadOnlyList<string> arguments)
    {
        var filePath = GetOption(arguments, "--file=");
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            throw new FileNotFoundException(
                $"{공개사업장가져오기Command}에는 존재하는 --file 경로가 필요합니다.",
                filePath);

        var sourceRevision = GetOption(arguments, "--source-revision=");
        if (string.IsNullOrWhiteSpace(sourceRevision))
            throw new ArgumentException(
                $"{공개사업장가져오기Command}에는 --source-revision이 필요합니다.");

        return new 공개사업장가져오기Arguments(
            Path.GetFullPath(filePath),
            sourceRevision,
            GetOption(arguments, "--encoding=") ?? "utf-8",
            GetOption(arguments, "--building-source-revision="),
            GetOption(arguments, "--source-id=") ?? 지방행정인허가사업장ImportService.SourceId,
            GetOption(arguments, "--dataset-id=") ?? 지방행정인허가사업장ImportService.DatasetId,
            GetOption(arguments, "--default-open-service-id="),
            GetOption(arguments, "--default-open-service-name="),
            ParseDateTimeOffsetOption(arguments, "--observed-at="));
    }

    private static async Task CollectLegalDongAsync(
        IReadOnlyList<string> arguments,
        IServiceProvider services,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        var ingestionDb = scope.ServiceProvider.GetRequiredService<PublicDataIngestionDbContext>();
        await ingestionDb.Database.MigrateAsync(cancellationToken);
        var result = await CollectAsync(
            scope.ServiceProvider,
            대한민국법정동CodeDataset.SourceId,
            대한민국법정동CodeDataset.DatasetId,
            "korea-legal-dong",
            TimeSpan.FromMinutes(2),
            HasCommand(arguments, "--force-reprocess"),
            cancellationToken);

        LogCollectionResult(logger, "대한민국 법정동코드", result);
        if (!string.Equals(result.StatusCode, 외부데이터수집StatusCodes.Success, StringComparison.Ordinal)
            || !HasCommand(arguments, "--promote-regions"))
            return;

        var geographyDb = scope.ServiceProvider.GetRequiredService<SsalddelContext>();
        await geographyDb.Database.MigrateAsync(cancellationToken);
        var promoted = await scope.ServiceProvider
            .GetRequiredService<대한민국법정동행정구역원장승격Service>()
            .승격Async(cancellationToken);
        logger.LogInformation(
            "대한민국 법정동 행정구역 원장 승격 완료. Active={Active}, Added={Added}, Updated={Updated}, CodeAssignments={Assignments}, MissingParents={MissingParents}, Revision={Revision}",
            promoted.현행정규화Record수,
            promoted.행정구역추가수,
            promoted.행정구역갱신수,
            promoted.CodeAssignment추가수,
            promoted.상위구역미확인수,
            promoted.DataRevision);
    }

    private static async Task CollectAdministrativeJurisdictionsAsync(
        IReadOnlyList<string> arguments,
        IServiceProvider services,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        await using var scope = services.CreateAsyncScope();
        var ingestionDb = scope.ServiceProvider.GetRequiredService<PublicDataIngestionDbContext>();
        await ingestionDb.Database.MigrateAsync(cancellationToken);
        var result = await CollectAsync(
            scope.ServiceProvider,
            대한민국행정동관할CodeDataset.SourceId,
            대한민국행정동관할CodeDataset.DatasetId,
            "korea-administrative-jurisdictions",
            TimeSpan.FromMinutes(3),
            HasCommand(arguments, "--force-reprocess"),
            cancellationToken);
        LogCollectionResult(logger, "대한민국 행정기관·관할 법정동", result);
    }

    private static async Task<ExternalDataIngestionResult> CollectAsync(
        IServiceProvider services,
        string sourceId,
        string datasetId,
        string runKeyPrefix,
        TimeSpan timeout,
        bool forceReprocess,
        CancellationToken cancellationToken)
    {
        var runtime = services.GetRequiredService<IExternalDataIngestionRuntime>();
        return await runtime.IngestAsync(new ExternalDataIngestionRequest
        {
            SourceId = sourceId,
            DatasetId = datasetId,
            RunKey = $"{runKeyPrefix}:{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}",
            Timeout = timeout,
            MaxAttempts = 2,
            ForceReprocess = forceReprocess,
        }, cancellationToken);
    }

    private static async Task ImportLicensedBusinessesAsync(
        IReadOnlyList<string> arguments,
        IServiceProvider services,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var parsedArguments = ParseImportArguments(arguments);
        await using var sourceStream = File.OpenRead(parsedArguments.FilePath);
        var sourceHash = Convert.ToHexString(
                await SHA256.HashDataAsync(sourceStream, cancellationToken))
            .ToLowerInvariant();
        sourceStream.Position = 0;

        await using var scope = services.CreateAsyncScope();
        var ingestionDb = scope.ServiceProvider.GetRequiredService<PublicDataIngestionDbContext>();
        await ingestionDb.Database.MigrateAsync(cancellationToken);
        var imported = await scope.ServiceProvider
            .GetRequiredService<지방행정인허가사업장ImportService>()
            .ImportCsvAsync(
                sourceStream,
                new 지방행정인허가사업장ImportRequest(
                    parsedArguments.SourceRevision,
                    sourceHash,
                    parsedArguments.ObservedAtUtc
                    ?? File.GetLastWriteTimeUtc(parsedArguments.FilePath),
                    EncodingName: parsedArguments.EncodingName,
                    SourceId: parsedArguments.SourceId,
                    DatasetId: parsedArguments.DatasetId,
                    DefaultOpenServiceId: parsedArguments.DefaultOpenServiceId,
                    DefaultOpenServiceName: parsedArguments.DefaultOpenServiceName),
                cancellationToken);
        logger.LogInformation(
            "지방행정인허가 사업장 적재 완료. Parsed={Parsed}, Inserted={Inserted}, Existing={Existing}, Rejected={Rejected}, Revision={Revision}, Hash={Hash}",
            imported.ParsedCount,
            imported.InsertedCount,
            imported.ExistingCount,
            imported.RejectedCount,
            parsedArguments.SourceRevision,
            sourceHash);

        if (string.IsNullOrWhiteSpace(parsedArguments.BuildingSourceRevision))
            return;

        var linker = scope.ServiceProvider.GetRequiredService<공개사업장건축물연결Service>();
        var linked = await linker.정확한도로명주소로연결Async(
            parsedArguments.SourceRevision,
            parsedArguments.BuildingSourceRevision,
            cancellationToken);
        var aggregateCount = await linker.건물별집계생성Async(
            parsedArguments.SourceRevision,
            cancellationToken);
        logger.LogInformation(
            "공개 사업장-건축물 연결 완료. Target={Target}, Matched={Matched}, Multiple={Multiple}, NoCandidate={NoCandidate}, InsufficientAddress={InsufficientAddress}, Existing={Existing}, Aggregates={Aggregates}",
            linked.대상사업장수,
            linked.연결수,
            linked.복수후보수,
            linked.건물후보없음수,
            linked.주소부족수,
            linked.기존판정수,
            aggregateCount);
    }

    private static void LogCollectionResult(
        ILogger logger,
        string datasetName,
        ExternalDataIngestionResult result) =>
        logger.LogInformation(
            "{DatasetName} 수집 완료. Status={Status}, Fetched={Fetched}, Normalized={Normalized}, Rejected={Rejected}, Inserted={Inserted}, Updated={Updated}, Existing={Existing}, Revision={Revision}",
            datasetName,
            result.StatusCode,
            result.FetchedCount,
            result.NormalizedCount,
            result.RejectedCount,
            result.InsertedCount,
            result.UpdatedCount,
            result.ExistingCount,
            result.DataRevision);

    private static bool HasCommand(IEnumerable<string> arguments, string command) =>
        arguments.Any(argument => string.Equals(argument, command, StringComparison.OrdinalIgnoreCase));

    private static string? GetOption(IEnumerable<string> arguments, string prefix) =>
        arguments.FirstOrDefault(argument => argument.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            ?[prefix.Length..]
            .Trim();

    private static DateTimeOffset? ParseDateTimeOffsetOption(
        IEnumerable<string> arguments,
        string prefix)
    {
        var value = GetOption(arguments, prefix);
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (DateTimeOffset.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal,
                out var parsed))
            return parsed;

        throw new ArgumentException($"{prefix.TrimEnd('=')}는 올바른 날짜·시각이어야 합니다.");
    }
}

internal sealed record 공개사업장가져오기Arguments(
    string FilePath,
    string SourceRevision,
    string EncodingName,
    string? BuildingSourceRevision,
    string SourceId,
    string DatasetId,
    string? DefaultOpenServiceId,
    string? DefaultOpenServiceName,
    DateTimeOffset? ObservedAtUtc);

internal sealed record 공간원본Argument(
    string FilePath,
    string SourceId,
    string DatasetId,
    string SourceVersion,
    string DataRevision,
    string ContentType,
    DateTimeOffset? EvidenceAsOfUtc,
    string PrivateStorageLocation);
