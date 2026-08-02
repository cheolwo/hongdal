using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Extensions.Options;
using 살뜰.Services.External.Gemini;
using 살뜰.Services.Options;

namespace 살뜰.Services.Images;

public static class AppContextImageBatchCommandLine
{
    private const string PreviewCommand = "--app-image-batch-preview";
    private const string SubmitCommand = "--app-image-batch-submit";
    private const string StatusCommand = "--app-image-batch-status";

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web)
        {
            WriteIndented = true
        };

    public static async Task<bool> TryRunAsync(
        string[] args,
        IServiceProvider services,
        string contentRootPath,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(args);
        var command = args.FirstOrDefault(argument =>
            argument.Equals(PreviewCommand, StringComparison.OrdinalIgnoreCase)
            || argument.Equals(SubmitCommand, StringComparison.OrdinalIgnoreCase)
            || argument.Equals(StatusCommand, StringComparison.OrdinalIgnoreCase));
        if (command is null)
        {
            return false;
        }

        var repositoryRoot = FindRepositoryRoot(contentRootPath);
        await using var scope = services.CreateAsyncScope();
        var client = scope.ServiceProvider
            .GetRequiredService<IAppContextImageBatchProviderClient>();
        var executionMode = scope.ServiceProvider
            .GetRequiredService<IOptions<SsalddelExecutionOptions>>()
            .Value
            .Mode;
        if (command.Equals(StatusCommand, StringComparison.OrdinalIgnoreCase))
        {
            await RefreshStatusAsync(
                args,
                repositoryRoot,
                client,
                logger,
                cancellationToken);
            return true;
        }

        var packPath = ResolveRepositoryFile(
            repositoryRoot,
            GetRequiredArgument(args, "--prompt-pack="),
            "프롬프트 팩");
        var pack = 앱문맥이미지BatchPromptPackCompiler.Parse(
            await File.ReadAllTextAsync(packPath, cancellationToken));
        var plan = command.Equals(
            SubmitCommand,
            StringComparison.OrdinalIgnoreCase)
            ? 앱문맥이미지BatchPromptPackCompiler.CompileApproved(pack)
            : 앱문맥이미지BatchPromptPackCompiler.CompilePreview(pack);
        EnsureModelMatches(plan.Model, client.Model);
        var estimate = client.Estimate(plan.Items);

        if (command.Equals(PreviewCommand, StringComparison.OrdinalIgnoreCase))
        {
            WriteSafeOutput(new
            {
                mode = "preview",
                plan.PackId,
                plan.Status,
                plan.PromptVersion,
                estimate.Model,
                estimate.ItemCount,
                estimate.EstimatedOutputUsd,
                estimate.PricingReferenceDate,
                keys = plan.Items.Select(item => item.Key).ToArray(),
                externalCallMade = false
            });
            return true;
        }

        if (!args.Any(argument => argument.Equals(
                "--confirm-billable=true",
                StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                "비용 발생 Batch 제출은 --confirm-billable=true 확인이 필요합니다.");
        }

        if (executionMode != SsalddelExecutionMode.Operational)
        {
            throw new InvalidOperationException(
                "외부 비용이 발생하는 앱 이미지 Batch 제출은 SsalddelExecution:Mode=Operational에서만 가능합니다.");
        }

        var planSha256 = ComputePlanSha256(plan);
        var packArtifactDirectory = GetPackArtifactDirectory(
            repositoryRoot,
            plan.PackId);
        Directory.CreateDirectory(packArtifactDirectory);
        await using var submissionLock = new FileStream(
            Path.Combine(packArtifactDirectory, ".submission.lock"),
            FileMode.OpenOrCreate,
            FileAccess.ReadWrite,
            FileShare.None,
            bufferSize: 1,
            useAsync: true);
        await EnsurePlanWasNotSubmittedAsync(
            packArtifactDirectory,
            planSha256,
            cancellationToken);
        var displayName = $"ssalddel-{plan.PackId}-v{plan.PromptVersion}";
        var submission = await client.SubmitAsync(
            displayName,
            plan.Items,
            cancellationToken);
        var manifest = AppContextImageBatchRunManifest.Create(
            plan,
            planSha256,
            submission,
            DateTimeOffset.UtcNow);
        var manifestPath = GetNewManifestPath(
            repositoryRoot,
            plan.PackId,
            submission.JobName);
        await WriteManifestAsync(
            manifestPath,
            manifest,
            cancellationToken);
        logger.LogInformation(
            "앱 문맥 이미지 Batch 제출 완료. PackId={PackId}, Items={Items}, Model={Model}, EstimatedOutputUsd={EstimatedOutputUsd}, Manifest={Manifest}",
            plan.PackId,
            submission.CostEstimate.ItemCount,
            submission.CostEstimate.Model,
            submission.CostEstimate.EstimatedOutputUsd,
            Path.GetRelativePath(repositoryRoot, manifestPath));
        WriteSafeOutput(new
        {
            mode = "submitted",
            plan.PackId,
            submission.JobName,
            submission.State,
            submission.CostEstimate,
            manifest = Path.GetRelativePath(repositoryRoot, manifestPath),
            apiKeyLogged = false
        });
        return true;
    }

    private static async Task RefreshStatusAsync(
        string[] args,
        string repositoryRoot,
        IAppContextImageBatchProviderClient client,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var manifestPath = ResolveArtifactFile(
            repositoryRoot,
            GetRequiredArgument(args, "--batch-manifest="));
        var manifest = JsonSerializer.Deserialize<AppContextImageBatchRunManifest>(
                           await File.ReadAllTextAsync(
                               manifestPath,
                               cancellationToken),
                           JsonOptions)
                       ?? throw new InvalidOperationException(
                           "Batch manifest를 읽을 수 없습니다.");
        EnsureModelMatches(manifest.Model, client.Model);
        var status = await client.GetAsync(
            manifest.JobName,
            manifest.Keys,
            cancellationToken);
        var resultDirectory = Path.Combine(
            Path.GetDirectoryName(manifestPath)!,
            "images");
        var resultRecords = new List<AppContextImageBatchRunResult>();
        if (status.Results.Count > 0)
        {
            Directory.CreateDirectory(resultDirectory);
        }

        foreach (var result in status.Results)
        {
            if (result.Bytes is null || result.MimeType is null)
            {
                resultRecords.Add(new(
                    result.Key,
                    null,
                    null,
                    result.Error));
                continue;
            }

            var extension = result.MimeType switch
            {
                "image/jpeg" => ".jpg",
                "image/png" => ".png",
                "image/webp" => ".webp",
                _ => throw new InvalidOperationException(
                    "지원하지 않는 Batch 결과 형식입니다.")
            };
            var safeKey = NormalizeFileKey(result.Key);
            var imagePath = Path.Combine(resultDirectory, safeKey + extension);
            await WriteImageWithoutReplacingAsync(
                imagePath,
                result.Bytes,
                cancellationToken);
            resultRecords.Add(new(
                result.Key,
                Path.GetRelativePath(repositoryRoot, imagePath),
                Convert.ToHexString(SHA256.HashData(result.Bytes))
                    .ToLowerInvariant(),
                null));
        }

        var refreshed = manifest with
        {
            State = status.State,
            OutputFileName = status.OutputFileName,
            Error = status.Error,
            RefreshedAtUtc = DateTimeOffset.UtcNow,
            Results = resultRecords
        };
        await WriteManifestAsync(
            manifestPath,
            refreshed,
            cancellationToken);
        logger.LogInformation(
            "앱 문맥 이미지 Batch 상태 갱신. PackId={PackId}, State={State}, Results={Results}, Manifest={Manifest}",
            refreshed.PackId,
            refreshed.State,
            refreshed.Results.Count,
            Path.GetRelativePath(repositoryRoot, manifestPath));
        WriteSafeOutput(new
        {
            mode = "status",
            refreshed.PackId,
            refreshed.JobName,
            refreshed.State,
            resultCount = refreshed.Results.Count,
            failedCount = refreshed.Results.Count(result => result.Error is not null),
            manifest = Path.GetRelativePath(repositoryRoot, manifestPath),
            apiKeyLogged = false
        });
    }

    private static async Task WriteImageWithoutReplacingAsync(
        string path,
        byte[] bytes,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(path))
        {
            await File.WriteAllBytesAsync(path, bytes, cancellationToken);
            return;
        }

        var existing = await File.ReadAllBytesAsync(path, cancellationToken);
        if (!CryptographicOperations.FixedTimeEquals(
                SHA256.HashData(existing),
                SHA256.HashData(bytes)))
        {
            throw new IOException(
                "동일한 Batch key의 이미지 파일이 이미 다른 내용으로 존재합니다.");
        }
    }

    private static async Task WriteManifestAsync(
        string manifestPath,
        AppContextImageBatchRunManifest manifest,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(manifestPath)!);
        var temporaryPath = manifestPath + ".tmp";
        await File.WriteAllTextAsync(
            temporaryPath,
            JsonSerializer.Serialize(manifest, JsonOptions),
            cancellationToken);
        File.Move(temporaryPath, manifestPath, overwrite: true);
    }

    private static string GetNewManifestPath(
        string repositoryRoot,
        string packId,
        string jobName)
    {
        var jobKey = NormalizeFileKey(jobName.Replace('/', '-'));
        return Path.Combine(
            GetPackArtifactDirectory(repositoryRoot, packId),
            $"{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}-{jobKey}.json");
    }

    private static string GetPackArtifactDirectory(
        string repositoryRoot,
        string packId)
        => Path.Combine(
            repositoryRoot,
            "artifacts",
            "local",
            "app-image-batches",
            packId);

    private static string ComputePlanSha256(
        앱문맥이미지BatchPlan plan)
    {
        var canonical = JsonSerializer.Serialize(new
        {
            plan.PackId,
            plan.PromptVersion,
            plan.Model,
            items = plan.Items.Select(item => new
            {
                item.Key,
                item.Prompt,
                item.AspectRatio,
                item.Resolution
            })
        });
        return Convert.ToHexString(SHA256.HashData(
                System.Text.Encoding.UTF8.GetBytes(canonical)))
            .ToLowerInvariant();
    }

    private static async Task EnsurePlanWasNotSubmittedAsync(
        string packArtifactDirectory,
        string planSha256,
        CancellationToken cancellationToken)
    {
        foreach (var manifestPath in Directory.EnumerateFiles(
                     packArtifactDirectory,
                     "*.json",
                     SearchOption.TopDirectoryOnly))
        {
            var manifest = JsonSerializer.Deserialize<AppContextImageBatchRunManifest>(
                await File.ReadAllTextAsync(
                    manifestPath,
                    cancellationToken),
                JsonOptions);
            if (manifest is not null
                && string.Equals(
                    manifest.PlanSha256,
                    planSha256,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "동일한 앱 이미지 Batch plan이 이미 제출되어 중복 비용 발생을 차단했습니다. 변경한 프롬프트는 promptVersion을 올려 새 plan으로 제출해야 합니다.");
            }
        }
    }

    private static string ResolveRepositoryFile(
        string repositoryRoot,
        string path,
        string description)
    {
        var fullPath = Path.GetFullPath(
            Path.IsPathRooted(path)
                ? path
                : Path.Combine(repositoryRoot, path));
        EnsureWithin(fullPath, repositoryRoot, description);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException(
                $"{description} 파일을 찾을 수 없습니다.",
                fullPath);
        }

        return fullPath;
    }

    private static string ResolveArtifactFile(
        string repositoryRoot,
        string path)
    {
        var artifactRoot = Path.GetFullPath(Path.Combine(
            repositoryRoot,
            "artifacts",
            "local",
            "app-image-batches"));
        var fullPath = Path.GetFullPath(
            Path.IsPathRooted(path)
                ? path
                : Path.Combine(repositoryRoot, path));
        EnsureWithin(fullPath, artifactRoot, "Batch manifest");
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException(
                "Batch manifest를 찾을 수 없습니다.",
                fullPath);
        }

        return fullPath;
    }

    private static void EnsureWithin(
        string path,
        string root,
        string description)
    {
        var rootWithSeparator = Path.TrimEndingDirectorySeparator(
                                    Path.GetFullPath(root))
                                + Path.DirectorySeparatorChar;
        if (!path.StartsWith(
                rootWithSeparator,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"{description}은(는) 허용된 저장소 경로 안에 있어야 합니다.");
        }
    }

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

        throw new DirectoryNotFoundException(
            "Hongdal 저장소 루트를 찾을 수 없습니다.");
    }

    private static string GetRequiredArgument(
        IEnumerable<string> args,
        string prefix)
    {
        var argument = args.FirstOrDefault(value =>
            value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
        var value = argument?[prefix.Length..].Trim();
        return string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException($"{prefix}<value> 인자가 필요합니다.")
            : value;
    }

    private static void EnsureModelMatches(
        string packModel,
        string configuredModel)
    {
        if (!string.Equals(
                packModel?.Trim(),
                configuredModel?.Trim(),
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "프롬프트 팩 모델과 GeminiImageBatch 서버 모델이 다릅니다.");
        }
    }

    private static string NormalizeFileKey(string value)
    {
        var normalized = new string(value
            .Trim()
            .Select(character =>
                char.IsAsciiLetterOrDigit(character)
                || character is '-' or '_'
                    ? character
                    : '-')
            .ToArray()).Trim('-');
        if (normalized.Length is 0 or > 160)
        {
            throw new InvalidOperationException(
                "Batch key를 안전한 파일 이름으로 변환할 수 없습니다.");
        }

        return normalized;
    }

    private static void WriteSafeOutput(object value)
        => Console.WriteLine(JsonSerializer.Serialize(value, JsonOptions));

    private sealed record AppContextImageBatchRunManifest(
        int SchemaVersion,
        string PackId,
        int PromptVersion,
        string Model,
        string PlanSha256,
        string JobName,
        string InputFileName,
        string State,
        decimal EstimatedOutputUsd,
        string PricingReferenceDate,
        DateTimeOffset SubmittedAtUtc,
        DateTimeOffset RefreshedAtUtc,
        IReadOnlyList<string> Keys,
        string? OutputFileName,
        string? Error,
        IReadOnlyList<AppContextImageBatchRunResult> Results)
    {
        public static AppContextImageBatchRunManifest Create(
            앱문맥이미지BatchPlan plan,
            string planSha256,
            AppContextImageBatchSubmission submission,
            DateTimeOffset submittedAtUtc)
            => new(
                1,
                plan.PackId,
                plan.PromptVersion,
                submission.CostEstimate.Model,
                planSha256,
                submission.JobName,
                submission.InputFileName,
                submission.State,
                submission.CostEstimate.EstimatedOutputUsd,
                submission.CostEstimate.PricingReferenceDate,
                submittedAtUtc,
                submittedAtUtc,
                plan.Items.Select(item => item.Key).ToArray(),
                null,
                null,
                []);
    }

    private sealed record AppContextImageBatchRunResult(
        string Key,
        string? RelativePath,
        string? Sha256,
        string? Error);
}
