using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Ssalddel.Domain.Content;
using Ssalddel.Services.Storage;
using 살뜰.Data;
using 살뜰.Services.Options;

namespace 살뜰.Services.Images;

public static class AppContextImageAssetPublishCommandLine
{
    private const string PublishCommand = "--app-context-images-publish";
    private const string PackIdArgument = "--app-context-images-pack-id=";

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
                "Object Storage와 DB 게시에는 --confirm-storage-write=true 확인이 필요합니다.");
        }

        var repositoryRoot = FindRepositoryRoot(contentRootPath);
        var sourceRoot = Path.Combine(
            repositoryRoot,
            "artifacts",
            "local",
            "app-image-batches");
        var promptRoot = Path.Combine(
            repositoryRoot,
            "docs",
            "Content",
            "AppContextImagePrompts",
            "packs");
        var requestedPackId = ReadArgument(args, PackIdArgument);
        var promptCatalog = LoadSceneMetadata(promptRoot, requestedPackId);
        var sceneMetadata = promptCatalog.Scenes;
        var files = Directory.EnumerateFiles(
                sourceRoot,
                "*.jpg",
                SearchOption.AllDirectories)
            .Where(path => string.Equals(
                new DirectoryInfo(Path.GetDirectoryName(path)!).Name,
                "images",
                StringComparison.OrdinalIgnoreCase))
            .Where(path => !string.Equals(
                Path.GetFileName(path),
                "contact-sheet.jpg",
                StringComparison.OrdinalIgnoreCase))
            .Where(path => requestedPackId is null
                || Path.GetFileNameWithoutExtension(path).StartsWith(
                    $"{requestedPackId}--scene-",
                    StringComparison.Ordinal))
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (files.Length != promptCatalog.ExpectedSceneCount)
        {
            throw new InvalidOperationException(
                $"게시 대상 앱 문맥 이미지는 정확히 {promptCatalog.ExpectedSceneCount}장이어야 합니다. 현재 {files.Length}장입니다.");
        }

        await using var scope = services.CreateAsyncScope();
        var storage = scope.ServiceProvider.GetRequiredService<IObjectStorageService>();
        if (!storage.IsConfigured(ObjectStorageAccess.Public))
        {
            throw new InvalidOperationException(
                "공개 Object Storage 설정이 필요합니다.");
        }

        var executionMode = scope.ServiceProvider
            .GetRequiredService<IOptions<SsalddelExecutionOptions>>()
            .Value.Mode;
        if (storage is not DevelopmentLocalStorageService
            && executionMode != SsalddelExecutionMode.Operational)
        {
            throw new InvalidOperationException(
                "외부 Object Storage 게시는 Operational 모드에서만 가능합니다.");
        }

        var db = scope.ServiceProvider.GetRequiredService<SsalddelContext>();
        if (!await db.Database.CanConnectAsync(cancellationToken))
        {
            throw new InvalidOperationException(
                "앱 문맥 이미지 DB에 연결할 수 없습니다. 게시 전에 migration을 별도로 적용해 주세요.");
        }

        var existing = await db.앱문맥이미지자산들
            .ToDictionaryAsync(item => item.장면Key, cancellationToken);
        var uploadedCount = 0;
        var skippedCount = 0;

        foreach (var path in files)
        {
            var sceneKey = Path.GetFileNameWithoutExtension(path);
            if (!sceneMetadata.TryGetValue(sceneKey, out var metadata))
            {
                throw new InvalidOperationException(
                    $"프롬프트 metadata가 없는 이미지입니다: {sceneKey}");
            }

            var bytes = await File.ReadAllBytesAsync(path, cancellationToken);
            var sha256 = Convert.ToHexString(SHA256.HashData(bytes))
                .ToLowerInvariant();
            if (existing.TryGetValue(sceneKey, out var current))
            {
                if (!string.Equals(
                        current.Sha256,
                        sha256,
                        StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"DB에 다른 내용의 장면 key가 존재합니다. 명시적 교체 흐름이 필요합니다: {sceneKey}");
                }

                skippedCount++;
                continue;
            }

            await using var stream = new MemoryStream(bytes, writable: false);
            var uploaded = await storage.UploadAsync(
                stream,
                Path.GetFileName(path),
                "image/jpeg",
                $"app-context-images/{metadata.PackId}/v{metadata.PromptVersion}",
                ObjectStorageAccess.Public,
                cancellationToken);
            var entity = new 앱문맥이미지자산
            {
                장면Key = sceneKey,
                앱PackId = metadata.PackId,
                장면번호 = metadata.Sequence,
                PromptVersion = metadata.PromptVersion,
                제목 = Truncate(metadata.TitleKo, 240),
                대체Text = $"{Truncate(metadata.TitleKo, 470)}를 설명하는 AI 생성 이미지",
                이미지Url = uploaded.Url,
                StorageContainer = uploaded.ContainerName,
                StorageObjectName = uploaded.ObjectName,
                ContentType = "image/jpeg",
                화면비율 = metadata.AspectRatio,
                Sha256 = sha256,
                RouteRefsJson = JsonSerializer.Serialize(metadata.RouteRefs),
                품질상태 = 앱문맥이미지품질상태.미검토,
                활성화여부 = true
            };
            db.앱문맥이미지자산들.Add(entity);
            await db.SaveChangesAsync(cancellationToken);
            existing.Add(sceneKey, entity);
            uploadedCount++;
        }

        logger.LogInformation(
            "앱 문맥 이미지 Object Storage·DB 게시 완료. Uploaded={Uploaded}, Skipped={Skipped}, Total={Total}, Storage={Storage}",
            uploadedCount,
            skippedCount,
            files.Length,
            storage.GetType().Name);
        Console.WriteLine(JsonSerializer.Serialize(new
        {
            mode = "app-context-images-published",
            uploadedCount,
            skippedCount,
            totalCount = files.Length,
            packId = requestedPackId,
            storage = storage.GetType().Name,
            secretLogged = false
        }, new JsonSerializerOptions { WriteIndented = true }));
        return true;
    }

    private static SceneMetadataCatalog LoadSceneMetadata(
        string promptRoot,
        string? requestedPackId)
    {
        var result = new Dictionary<string, SceneMetadata>(StringComparer.Ordinal);
        var expectedSceneCount = 0;
        foreach (var path in Directory.EnumerateFiles(
                     promptRoot,
                     "*.json",
                     SearchOption.TopDirectoryOnly))
        {
            var pack = 앱문맥이미지BatchPromptPackCompiler.Parse(
                File.ReadAllText(path));
            if (requestedPackId is not null
                && !string.Equals(pack.PackId, requestedPackId, StringComparison.Ordinal))
            {
                continue;
            }

            expectedSceneCount += pack.ExpectedSceneCount;
            foreach (var scene in pack.Scenes)
            {
                var key = $"{pack.PackId}--scene-{scene.Sequence:00}";
                if (!result.TryAdd(key, new SceneMetadata(
                        pack.PackId,
                        pack.PromptVersion,
                        scene.Sequence,
                        scene.TitleKo,
                        scene.AspectRatio,
                        scene.RouteRefs)))
                {
                    throw new InvalidOperationException(
                        $"중복 앱 이미지 장면 metadata입니다: {key}");
                }
            }
        }

        if (requestedPackId is not null && expectedSceneCount == 0)
        {
            throw new InvalidOperationException(
                $"프롬프트 pack을 찾을 수 없습니다: {requestedPackId}");
        }

        return new SceneMetadataCatalog(
            result,
            requestedPackId is null ? 650 : expectedSceneCount);
    }

    private static string? ReadArgument(string[] args, string prefix)
    {
        var argument = args.FirstOrDefault(value => value.StartsWith(
            prefix,
            StringComparison.OrdinalIgnoreCase));
        if (argument is null)
        {
            return null;
        }

        var value = argument[prefix.Length..].Trim();
        if (string.IsNullOrWhiteSpace(value)
            || value.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
            || value.Contains(Path.DirectorySeparatorChar)
            || value.Contains(Path.AltDirectorySeparatorChar))
        {
            throw new InvalidOperationException(
                $"유효한 앱 이미지 pack id가 필요합니다: {argument}");
        }

        return value;
    }

    private static string Truncate(string value, int maximumLength)
        => value.Length <= maximumLength
            ? value
            : value[..maximumLength];

    private static string FindRepositoryRoot(string contentRootPath)
    {
        var directory = new DirectoryInfo(contentRootPath);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Ssalddel.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Hongdal 저장소 루트를 찾을 수 없습니다.");
    }

    private sealed record SceneMetadata(
        string PackId,
        int PromptVersion,
        int Sequence,
        string TitleKo,
        string AspectRatio,
        IReadOnlyList<string> RouteRefs);

    private sealed record SceneMetadataCatalog(
        IReadOnlyDictionary<string, SceneMetadata> Scenes,
        int ExpectedSceneCount);
}
