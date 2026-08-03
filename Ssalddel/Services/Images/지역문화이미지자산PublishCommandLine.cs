using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Ssalddel.Domain.Content;
using Ssalddel.Services.Storage;
using 살뜰.Data;
using 살뜰.Services.Options;

namespace 살뜰.Services.Images;

public static class 지역문화이미지자산PublishCommandLine
{
    private const string PublishCommand = "--regional-culture-images-publish";
    private const string PackId = "regional-culture-one-each-v1";

    public static async Task<bool> TryRunAsync(
        string[] args,
        IServiceProvider services,
        string contentRootPath,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        if (!args.Any(value => value.Equals(
                PublishCommand,
                StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        if (!args.Any(value => value.Equals(
                "--confirm-storage-write=true",
                StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                "지역문화 이미지 Blob·DB 반영에는 --confirm-storage-write=true 확인이 필요합니다.");
        }

        var repositoryRoot = FindRepositoryRoot(contentRootPath);
        var sourceRoot = Path.Combine(
            repositoryRoot,
            "artifacts",
            "local",
            "regional-culture-image-generation",
            "all-regions-one-each-2026-08-03");
        var readinessPath = Path.Combine(
            repositoryRoot,
            "docs",
            "Content",
            "RegionalCultureImagePrompts",
            "research-readiness.v1.json");
        var regions = LoadRegions(readinessPath);
        var files = Directory.EnumerateFiles(
                sourceRoot,
                "*-01-2k.jpg",
                SearchOption.AllDirectories)
            .Where(path => !path.Contains(
                $"{Path.DirectorySeparatorChar}rejected{Path.DirectorySeparatorChar}",
                StringComparison.OrdinalIgnoreCase))
            .ToDictionary(
                path => Path.GetFileName(path)[..^"-01-2k.jpg".Length],
                StringComparer.Ordinal);
        var expectedKeys = regions.Select(item => item.RegionKey).ToHashSet(StringComparer.Ordinal);
        if (files.Count != expectedKeys.Count
            || !files.Keys.ToHashSet(StringComparer.Ordinal).SetEquals(expectedKeys))
        {
            throw new InvalidOperationException(
                $"서울 제외 지역문화 이미지는 정확히 {expectedKeys.Count}장이어야 하며 region key가 일치해야 합니다. 현재 {files.Count}장입니다.");
        }

        await using var scope = services.CreateAsyncScope();
        var storage = scope.ServiceProvider.GetRequiredService<IObjectStorageService>();
        if (!storage.IsConfigured(ObjectStorageAccess.Public))
        {
            throw new InvalidOperationException("공개 Object Storage 설정이 필요합니다.");
        }

        var executionMode = scope.ServiceProvider
            .GetRequiredService<IOptions<SsalddelExecutionOptions>>()
            .Value.Mode;
        if (storage is not DevelopmentLocalStorageService
            && executionMode != SsalddelExecutionMode.Operational)
        {
            throw new InvalidOperationException(
                "외부 Object Storage 반영은 Operational 모드에서만 가능합니다.");
        }

        var db = scope.ServiceProvider.GetRequiredService<SsalddelContext>();
        if (!await db.Database.CanConnectAsync(cancellationToken))
        {
            throw new InvalidOperationException("지역문화 이미지 DB에 연결할 수 없습니다.");
        }

        var existing = await db.앱문맥이미지자산들
            .Where(item => item.앱PackId == PackId)
            .ToDictionaryAsync(item => item.장면Key, cancellationToken);
        var uploadedCount = 0;
        var skippedCount = 0;

        for (var index = 0; index < regions.Count; index++)
        {
            var region = regions[index];
            var path = files[region.RegionKey];
            var sceneKey = $"{region.RegionKey}--scene-01";
            var bytes = await File.ReadAllBytesAsync(path, cancellationToken);
            var sha256 = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
            if (existing.TryGetValue(sceneKey, out var current))
            {
                if (!string.Equals(current.Sha256, sha256, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"DB에 다른 내용의 지역문화 장면 key가 존재합니다. 명시적 교체가 필요합니다: {sceneKey}");
                }

                if (!current.활성화여부)
                {
                    current.활성화여부 = true;
                    current.수정시각 = DateTimeOffset.UtcNow;
                    await db.SaveChangesAsync(cancellationToken);
                }

                skippedCount++;
                continue;
            }

            await using var stream = new MemoryStream(bytes, writable: false);
            var uploaded = await storage.UploadAsync(
                stream,
                Path.GetFileName(path),
                "image/jpeg",
                $"regional-culture-images/{region.CountryCode}/{region.RegionKey}/v1",
                ObjectStorageAccess.Public,
                cancellationToken);
            var displayName = ToDisplayName(region.RegionKey);
            var entity = new 앱문맥이미지자산
            {
                장면Key = sceneKey,
                앱PackId = PackId,
                장면번호 = index + 1,
                PromptVersion = 1,
                제목 = $"{displayName} 지역문화 생활 장면",
                대체Text = $"{displayName}의 현재 생활문화를 표현한 AI 생성 이미지",
                이미지Url = uploaded.Url,
                StorageContainer = uploaded.ContainerName,
                StorageObjectName = uploaded.ObjectName,
                ContentType = "image/jpeg",
                화면비율 = "16:9",
                Sha256 = sha256,
                RouteRefsJson = "[]",
                품질상태 = 앱문맥이미지품질상태.미검토,
                활성화여부 = true
            };
            db.앱문맥이미지자산들.Add(entity);
            await db.SaveChangesAsync(cancellationToken);
            existing.Add(sceneKey, entity);
            uploadedCount++;
        }

        logger.LogInformation(
            "지역문화 이미지 Blob·DB 반영 완료. Uploaded={Uploaded}, Skipped={Skipped}, Total={Total}, Storage={Storage}",
            uploadedCount,
            skippedCount,
            regions.Count,
            storage.GetType().Name);
        Console.WriteLine(JsonSerializer.Serialize(new
        {
            mode = "regional-culture-images-published",
            packId = PackId,
            uploadedCount,
            skippedCount,
            totalCount = regions.Count,
            reviewStatus = 앱문맥이미지품질상태.미검토.ToString(),
            active = true,
            storage = storage.GetType().Name,
            secretLogged = false
        }, new JsonSerializerOptions { WriteIndented = true }));
        return true;
    }

    private static IReadOnlyList<RegionItem> LoadRegions(string readinessPath)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(readinessPath));
        var result = new List<RegionItem>();
        foreach (var country in document.RootElement.GetProperty("countries").EnumerateArray())
        {
            var countryCode = country.GetProperty("countryCode").GetString()
                              ?? throw new InvalidOperationException("countryCode가 없습니다.");
            foreach (var item in country.GetProperty("regionKeys").EnumerateArray())
            {
                var regionKey = item.GetString()
                                ?? throw new InvalidOperationException("regionKey가 없습니다.");
                if (regionKey == "kr-seoul")
                {
                    continue;
                }

                result.Add(new RegionItem(countryCode, regionKey));
            }
        }

        if (result.Count != 97)
        {
            throw new InvalidOperationException(
                $"서울 제외 지역 key는 97개여야 합니다. 현재 {result.Count}개입니다.");
        }

        return result;
    }

    private static string ToDisplayName(string regionKey)
    {
        var name = regionKey[(regionKey.IndexOf('-') + 1)..].Replace('-', ' ');
        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(name);
    }

    private static string FindRepositoryRoot(string contentRootPath)
    {
        var current = new DirectoryInfo(contentRootPath);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Ssalddel.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Hongdal repository root를 찾을 수 없습니다.");
    }

    private sealed record RegionItem(string CountryCode, string RegionKey);
}
