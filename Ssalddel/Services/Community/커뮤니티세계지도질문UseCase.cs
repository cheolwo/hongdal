using FluentResults;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Metadata;

namespace Ssalddel.Services.Community;

public interface I커뮤니티세계지도질문UseCase
{
    Task<커뮤니티세계지도질문초안Response?> 초안생성Async(
        string observationStableId,
        커뮤니티세계지도질문초안Request request,
        CancellationToken cancellationToken = default);

    Task<Result<커뮤니티세계지도질문게시Response>> 게시Async(
        string observationStableId,
        커뮤니티세계지도질문게시Request request,
        CancellationToken cancellationToken = default);
}

[SsalddelCommunityV0Module(
    SsalddelCommunityV0ModuleKeys.Participation,
    SsalddelModuleKind.Application,
    "공개 지도 관측을 출처가 보존된 사용자 질문으로 연결",
    ReleaseStage = SsalddelCommunityV0ReleaseStages.Persistence,
    Boundary = "초안은 저장하지 않고 확인 게시만 저장하며 참여·가원장·주문·계약·배차를 자동 생성하지 않습니다.")]
[SsalddelCodeMetadata(
    SsalddelCodeFeatureKeys.CommunityWorldMapObservation,
    SsalddelCodeLayer.Application,
    "지도 observation을 질문 초안과 출처 영속 게시글로 연결",
    ContractType = typeof(I커뮤니티세계지도질문UseCase),
    FlowOrder = 35,
    Effects = SsalddelCodeEffect.PersistentRead | SsalddelCodeEffect.PersistentWrite,
    Boundary = "사용자 출처 확인 뒤 게시글만 저장하며 가원장은 기존 별도 동의 흐름에서 생성합니다.")]
public sealed class 커뮤니티세계지도질문UseCase(
    I커뮤니티세계지도조회UseCase mapUseCase,
    커뮤니티게시글생성Service postCreationService)
    : I커뮤니티세계지도질문UseCase
{
    private const string WorkflowTag = "공개 근거 공동확인";
    private const string RoleTag = "커뮤니티 참여자";

    public async Task<커뮤니티세계지도질문초안Response?> 초안생성Async(
        string observationStableId,
        커뮤니티세계지도질문초안Request request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var resolved = await ResolveAsync(
            observationStableId,
            request.DatasetCode,
            cancellationToken);
        if (resolved is null)
        {
            return null;
        }

        var (snapshot, observation) = resolved.Value;
        var evidence = BuildEvidence(snapshot, observation);
        var questionFocus = NormalizeOptional(request.QuestionFocus, 240);
        var suggestedTitle = string.IsNullOrWhiteSpace(questionFocus)
            ? $"{observation.Title} 공개 근거를 함께 확인해요"
            : questionFocus;

        return new 커뮤니티세계지도질문초안Response
        {
            Evidence = evidence,
            SuggestedPost = BuildPostRequest(
                evidence,
                suggestedTitle,
                BuildSuggestedBody(evidence),
                nickname: string.Empty,
                password: string.Empty,
                originalLanguageCode: CommunityDisplayLanguageCodes.Korean,
                interestGatheringEnabled: true),
            RequiresUserConfirmation = true,
            CreatesPost = false,
            CreatesProvisionalLedger = false,
            BoundaryNotice =
                "이 단계에서는 아무것도 저장하지 않습니다. 게시를 확인해도 질문 글만 만들어지며 참여·연락처 공개·가원장·주문·계약·배차는 각각 별도 동의가 필요합니다."
        };
    }

    public async Task<Result<커뮤니티세계지도질문게시Response>> 게시Async(
        string observationStableId,
        커뮤니티세계지도질문게시Request request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!request.ConfirmSourceReference)
        {
            return Result.Fail<커뮤니티세계지도질문게시Response>(
                "공개 근거의 출처·기준시각·한계가 게시글에 함께 저장되는 것을 확인해야 합니다.");
        }

        var resolved = await ResolveAsync(
            observationStableId,
            request.DatasetCode,
            cancellationToken);
        if (resolved is null)
        {
            return Result.Fail<커뮤니티세계지도질문게시Response>(
                new Error("지도 observation을 찾을 수 없습니다.")
                    .WithMetadata("StatusCode", 404));
        }

        var (snapshot, observation) = resolved.Value;
        var evidence = BuildEvidence(snapshot, observation);
        var postRequest = BuildPostRequest(
            evidence,
            request.Title,
            request.Body,
            request.Nickname,
            request.Password,
            request.OriginalLanguageCode,
            request.IsInterestGatheringEnabled);
        postRequest.IsAuthorDisplayCountryPublic = request.IsAuthorDisplayCountryPublic;
        postRequest.AuthorDisplayCountryCode = request.AuthorDisplayCountryCode;
        postRequest.AuthorDisplayCountryName = request.AuthorDisplayCountryName;

        var created = await postCreationService.CreateAsync(
            postRequest,
            scheduledPublishAtUtc: null,
            evidence,
            cancellationToken);
        if (created.IsFailed)
        {
            return Result.Fail<커뮤니티세계지도질문게시Response>(created.Errors);
        }

        var post = created.Value;
        return Result.Ok(new 커뮤니티세계지도질문게시Response
        {
            Post = post,
            PostHref = $"/community/posts/{post.Id}",
            OpportunitiesHref = $"/api/v1/community/posts/{post.Id}/opportunities",
            ProvisionalLedgerCreated = false,
            NextActionNotice =
                "질문 글만 저장했습니다. 관심 역할 선택 뒤 서로 다른 참여자 2명 이상이 모이고 작성자가 비구속성·알림·가원장 생성을 각각 확인해야 가원장을 만들 수 있습니다."
        });
    }

    private async Task<(커뮤니티세계지도SnapshotDto Snapshot, 커뮤니티세계지도ObservationDto Observation)?> ResolveAsync(
        string observationStableId,
        string? datasetCode,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(observationStableId))
        {
            throw new ArgumentException("지도 observation stable ID가 필요합니다.", nameof(observationStableId));
        }

        var snapshot = await mapUseCase.조회Async(datasetCode, cancellationToken);
        var observation = snapshot.Observations.FirstOrDefault(item => string.Equals(
            item.StableId,
            observationStableId.Trim(),
            StringComparison.Ordinal));
        return observation is null ? null : (snapshot, observation);
    }

    private static 커뮤니티세계지도EvidenceReferenceDto BuildEvidence(
        커뮤니티세계지도SnapshotDto snapshot,
        커뮤니티세계지도ObservationDto observation)
        => new()
        {
            ObservationStableId = observation.StableId,
            DatasetCode = snapshot.DatasetCode,
            SnapshotRevision = snapshot.Revision,
            SourceVersion = observation.SourceVersion,
            LayerCode = observation.LayerCode,
            CountryCode = observation.CountryCode,
            CountryName = observation.CountryName,
            Title = observation.Title,
            Summary = observation.Summary,
            SourceName = observation.SourceName,
            SourceDatasetKey = observation.SourceDatasetKey,
            SourceHref = observation.SourceHref,
            DetailHref = observation.DetailHref,
            MapHref = CommunityPageRoutes.WorldMapFor(
                snapshot.DatasetCode,
                observation.CountryCode,
                observation.LayerCode,
                observation.StableId,
                observation.StableId,
                snapshot.Revision,
                observation.SourceVersion),
            EvidenceAsOfUtc = observation.EvidenceAsOfUtc,
            SourceUpdatedAtUtc = observation.SourceUpdatedAtUtc,
            CollectedAtUtc = observation.CollectedAtUtc,
            UpdateCycle = observation.UpdateCycle,
            LocationPrecisionCode = observation.LocationPrecisionCode,
            BoundaryNotice = observation.BoundaryNotice
        };

    private static PlatformCommunityPostCreateRequest BuildPostRequest(
        커뮤니티세계지도EvidenceReferenceDto evidence,
        string? title,
        string? body,
        string? nickname,
        string? password,
        string? originalLanguageCode,
        bool interestGatheringEnabled)
        => new()
        {
            AppKey = "platform",
            Category = interestGatheringEnabled
                ? CommunityBoardCatalog.Participation.DisplayName
                : PlatformCommunityPostCategories.General,
            WorkflowTag = WorkflowTag,
            RoleTag = RoleTag,
            Title = title?.Trim() ?? string.Empty,
            Body = body?.Trim() ?? string.Empty,
            OriginalLanguageCode = originalLanguageCode,
            SharedLinkUrl = ResolveSharedLink(evidence.SourceHref),
            IsInterestGatheringEnabled = interestGatheringEnabled,
            Nickname = nickname?.Trim() ?? string.Empty,
            Password = password ?? string.Empty
        };

    private static string BuildSuggestedBody(커뮤니티세계지도EvidenceReferenceDto evidence)
    {
        var evidenceDate = evidence.EvidenceAsOfUtc?.ToString("yyyy-MM-dd") ?? "기준일 미제공";
        var boundary = string.IsNullOrWhiteSpace(evidence.BoundaryNotice)
            ? "공개 관측은 공급 가능성·재고·계약 조건을 확정하지 않습니다."
            : evidence.BoundaryNotice.Trim();
        return $"""
               {evidence.Summary}

               함께 확인하고 싶은 점
               - 이 공개 근거가 우리 지역의 생활과 공동행동에 어떤 의미가 있는지 이야기해 주세요.
               - 추가로 확인해야 할 공식 자료, 비용, 노동, 위험과 미정 조건을 남겨 주세요.

               출처: {evidence.SourceName}
               자료 기준: {evidenceDate}
               자료 범위: {evidence.CountryName}
               한계: {boundary}
               """;
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized)
            ? null
            : normalized[..Math.Min(normalized.Length, maxLength)];
    }

    private static string? ResolveSharedLink(string? sourceHref)
        => Uri.TryCreate(sourceHref, UriKind.Absolute, out var uri)
           && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            ? uri.ToString()
            : null;
}
