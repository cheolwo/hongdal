using System.Globalization;
using System.Text;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.Community;
using 살뜰.Services.Options;

namespace Ssalddel.Services.Community;

public sealed record CommunityActivityBoardContentPublishResult(
    int AttemptedCount,
    int CreatedCount);

public interface ICommunityActivityBoardContentService
{
    Task<CommunityActivityBoardContentPublishResult> EnsureAnnouncementsAsync(
        CancellationToken cancellationToken = default);

    Task<CommunityActivityBoardContentPublishResult> SeedTestActivityPostsAsync(
        string scenarioKey,
        int postsPerBoard,
        CancellationToken cancellationToken = default);
}

public sealed class CommunityActivityBoardContentService(
    ICommunityAutomatedPostPublisher publisher,
    TimeProvider timeProvider) : ICommunityActivityBoardContentService
{
    public Task<CommunityActivityBoardContentPublishResult> EnsureAnnouncementsAsync(
        CancellationToken cancellationToken = default)
        => PublishAsync(
            CommunityActivityBoardCatalog.Bundles.Select(BuildAnnouncementDraft),
            cancellationToken);

    public Task<CommunityActivityBoardContentPublishResult> SeedTestActivityPostsAsync(
        string scenarioKey,
        int postsPerBoard,
        CancellationToken cancellationToken = default)
    {
        var normalizedScenarioKey = NormalizeKey(scenarioKey, "development-observation-v1");
        var normalizedPostsPerBoard = Math.Clamp(postsPerBoard, 1, 5);
        var occurredAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        var drafts = CommunityActivityBoardCatalog.Bundles.SelectMany(bundle =>
            Enumerable.Range(1, normalizedPostsPerBoard)
                .Select(sequence => BuildTestActivityDraft(
                    bundle,
                    normalizedScenarioKey,
                    sequence,
                    occurredAtUtc)));
        return PublishAsync(drafts, cancellationToken);
    }

    private async Task<CommunityActivityBoardContentPublishResult> PublishAsync(
        IEnumerable<CommunityAutomatedPostDraft> drafts,
        CancellationToken cancellationToken)
    {
        var attemptedCount = 0;
        var createdCount = 0;
        foreach (var draft in drafts)
        {
            cancellationToken.ThrowIfCancellationRequested();
            attemptedCount++;
            var result = await publisher.PublishIfMissingAsync(draft, cancellationToken);
            if (result.Created)
            {
                createdCount++;
            }
        }

        return new CommunityActivityBoardContentPublishResult(
            attemptedCount,
            createdCount);
    }

    internal static CommunityAutomatedPostDraft BuildAnnouncementDraft(
        CommunityActivityBoardBundleDefinition bundle)
    {
        var body = new StringBuilder()
            .AppendLine("[게시판 안내]")
            .AppendLine($"{bundle.Board.DisplayName} 게시판은 프로젝트에서 발생하는 공개 가능한 활동을 관찰하는 읽기 전용 공간입니다.")
            .AppendLine()
            .AppendLine($"상징: {CommunityActivityBoardBundleDefinition.MountainSymbol} {CommunityActivityBoardBundleDefinition.MountainName}괘 · 산")
            .AppendLine($"로드맵: {bundle.RoadmapStage.FullDisplayName}")
            .AppendLine($"연결 수: Command {bundle.CommandCount} · Event {bundle.EventCount} · 페이지 {bundle.Pages.Count}")
            .AppendLine()
            .AppendLine("관찰 Command·Event:");
        foreach (var activity in bundle.Activities)
        {
            body.AppendLine($"- {activity.SourceKindDisplayName} · {activity.SourceName} · {activity.ActivityDisplayName}");
        }

        body
            .AppendLine()
            .AppendLine("관련 App·페이지:")
            .AppendLine($"- {CommunityActivityBoardCatalog.SurfaceMappingBoundary}");
        foreach (var page in bundle.Pages)
        {
            body.AppendLine($"- {page.Surface} · {page.PageName} · {page.Route} · {page.Responsibility}");
        }

        body
            .AppendLine()
            .AppendLine("기록 원칙:")
            .AppendLine("- 성공 또는 완료된 활동의 발생 사실만 게시합니다.")
            .AppendLine($"- {CommunityActivityBoardCatalog.PrivacyBoundary}")
            .AppendLine("- 테스트 글은 제목과 본문에 테스트 데이터임을 명확히 표시합니다.");

        return new CommunityAutomatedPostDraft(
            $"activity-board-notice-{bundle.Board.Key}",
            "notice-v2",
            bundle.Board.DisplayName,
            $"{bundle.RoadmapStage.ProductName} {bundle.ProductVersion} 활동 관찰",
            "게시판 안내",
            $"[게시판 안내] {bundle.Board.DisplayName}",
            body.ToString().Trim(),
            "살뜰 활동 안내봇",
            IsOperatorPinned: true,
            EnqueueDerivedWork: false,
            PublishCreatedEvent: false);
    }

    internal static CommunityAutomatedPostDraft BuildTestActivityDraft(
        CommunityActivityBoardBundleDefinition bundle,
        string scenarioKey,
        int sequence,
        DateTime occurredAtUtc)
    {
        var definition = bundle.Activities[(sequence - 1) % bundle.Activities.Count];
        var body = string.Join(
            Environment.NewLine,
            "[테스트 데이터 안내] 화면·Command·Event 연결과 게시판 목록 표시를 검증하기 위해 생성한 가상 활동입니다.",
            "실제 주문, 계약, 결제, 통관, 운송 또는 창고 작업이 아닙니다.",
            string.Empty,
            $"로드맵: {definition.RoadmapStage.FullDisplayName}",
            $"발생 유형: {definition.SourceKindDisplayName} · {definition.SourceName}",
            $"관련 App·페이지: {CommunityActivityBoardCatalog.SurfaceMappingBoundary}",
            $"가상 발생 시각(UTC): {occurredAtUtc:yyyy-MM-dd HH:mm}",
            $"테스트 시나리오: {scenarioKey} · #{sequence.ToString(CultureInfo.InvariantCulture)}",
            string.Empty,
            definition.PublicActivitySummary,
            CommunityActivityBoardCatalog.PrivacyBoundary);
        return new CommunityAutomatedPostDraft(
            $"activity-board-test-{bundle.Board.Key}",
            $"{scenarioKey}-{sequence.ToString("00", CultureInfo.InvariantCulture)}",
            bundle.Board.DisplayName,
            $"{definition.RoadmapStage.ProductName} {definition.ProductVersion} 테스트 활동",
            "테스트 관찰",
            $"[테스트 데이터] {bundle.Board.DisplayName} 활동 #{sequence.ToString(CultureInfo.InvariantCulture)}",
            body,
            "살뜰 테스트봇",
            IsOperatorPinned: false,
            EnqueueDerivedWork: false,
            PublishCreatedEvent: false);
    }

    private static string NormalizeKey(string? value, string fallback)
    {
        var normalized = new string((value ?? string.Empty)
            .Trim()
            .ToLowerInvariant()
            .Where(character => char.IsLetterOrDigit(character) || character is '-' or '_')
            .Take(48)
            .ToArray());
        return normalized.Length == 0 ? fallback : normalized;
    }
}

public sealed class CommunityActivityBoardContentWorker(
    IServiceScopeFactory scopeFactory,
    IOptionsMonitor<CommunityActivityBoardContentOptions> options,
    IHostEnvironment environment,
    ILogger<CommunityActivityBoardContentWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var currentOptions = options.CurrentValue;
        var shouldSeedTestPosts = CanSeedTestActivityPosts(
            environment.EnvironmentName,
            currentOptions.SeedTestActivityPosts);
        if (!currentOptions.EnsureAnnouncementsAtStartup && !shouldSeedTestPosts)
        {
            return;
        }

        var delay = TimeSpan.FromSeconds(Math.Clamp(
            currentOptions.StartupDelaySeconds,
            0,
            60));
        if (delay > TimeSpan.Zero)
        {
            await Task.Delay(delay, stoppingToken);
        }

        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var service = scope.ServiceProvider
                .GetRequiredService<ICommunityActivityBoardContentService>();
            if (currentOptions.EnsureAnnouncementsAtStartup)
            {
                var announcementResult = await service.EnsureAnnouncementsAsync(stoppingToken);
                logger.LogInformation(
                    "활동 게시판 공지를 확인했습니다. Attempted={AttemptedCount} Created={CreatedCount}",
                    announcementResult.AttemptedCount,
                    announcementResult.CreatedCount);
            }

            if (shouldSeedTestPosts)
            {
                var testResult = await service.SeedTestActivityPostsAsync(
                    currentOptions.TestScenarioKey,
                    currentOptions.TestPostsPerBoard,
                    stoppingToken);
                logger.LogInformation(
                    "개발·테스트 활동 게시글을 확인했습니다. Attempted={AttemptedCount} Created={CreatedCount} Scenario={ScenarioKey}",
                    testResult.AttemptedCount,
                    testResult.CreatedCount,
                    currentOptions.TestScenarioKey);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "활동 게시판 공지 또는 개발·테스트 글 생성에 실패했습니다. 다음 앱 시작에서 멱등 재시도합니다.");
        }
    }

    internal static bool CanSeedTestActivityPosts(
        string environmentName,
        bool enabled)
        => enabled
           && (string.Equals(
                   environmentName,
                   Environments.Development,
                   StringComparison.OrdinalIgnoreCase)
               || string.Equals(
                   environmentName,
                   "Testing",
                   StringComparison.OrdinalIgnoreCase));
}
