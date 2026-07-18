using Hongdal.Contracts.Common.Community;
using Hongdal.Contracts.Common.Content;
using Hongdal.Contracts.Common.Operations;
using Hongdal.Ui.Common.Areas.App.Services;
using Hongdal.Ui.Common.Areas.App.ViewModels;

namespace Hongdal.Tests.Ui.Common;

public sealed class CommunityInformationReviewPageViewModelTests
{
    [Fact]
    public async Task Initialize_LoadsSourcesAndCandidatesThroughSelectedFilters()
    {
        var client = new RecordingClient(
            [Source(CommunityInformationSourceKeys.KamisPriceObservations)],
            [KamisCandidate()]);
        using var viewModel = CreateViewModel(client);
        viewModel.CountryCode = "KR";
        viewModel.SearchText = "사과";

        await viewModel.InitializeAsync();

        Assert.Single(viewModel.Sources);
        Assert.Single(viewModel.Candidates);
        Assert.Equal("KR", client.LastQuery?.CountryCode);
        Assert.Equal("사과", client.LastQuery?.SearchText);
        Assert.Equal(100, client.LastQuery?.Take);
    }

    [Fact]
    public void KamisCandidate_PreparesEditableInformationPostDraftWithSourceBoundary()
    {
        using var viewModel = CreateViewModel(new RecordingClient([], []));
        var candidate = KamisCandidate();

        var prepared = viewModel.PrepareDraft(candidate, "운영자 홍길동");

        Assert.True(prepared);
        Assert.True(viewModel.Composer.IsOpen);
        Assert.True(viewModel.Composer.IsSettingsOpen);
        Assert.Equal(CommunityBoardCatalog.Vow.DisplayName, viewModel.Composer.Draft.Category);
        Assert.Equal("운영자 정보 공유", viewModel.Composer.Draft.RoleTag);
        Assert.StartsWith("[공공자료]", viewModel.Composer.Draft.Title);
        Assert.Contains("자료 기준일: 2026-07-17", viewModel.Composer.Draft.Body);
        Assert.Contains("표시 기준: KRW · 10개", viewModel.Composer.Draft.Body);
        Assert.Contains("판매 권고", viewModel.Composer.Draft.Body);
        Assert.Equal(candidate.OriginalUrl, viewModel.Composer.Draft.SharedLinkUrl);
        Assert.Equal(string.Empty, viewModel.Composer.Draft.Password);
    }

    [Fact]
    public void ExistingDraft_IsNotReplacedUntilOperatorConfirms()
    {
        using var viewModel = CreateViewModel(new RecordingClient([], []));
        viewModel.Composer.Draft.Title = "작성 중인 글";
        var video = VideoCandidate();

        var prepared = viewModel.PrepareDraft(video, "관리자");

        Assert.False(prepared);
        Assert.True(viewModel.HasDraftConflict);
        Assert.Equal("작성 중인 글", viewModel.Composer.Draft.Title);

        viewModel.ReplaceDraft("관리자");

        Assert.False(viewModel.HasDraftConflict);
        Assert.Equal(CommunityBoardCatalog.Vow.DisplayName, viewModel.Composer.Draft.Category);
        Assert.StartsWith("[영상 공유]", viewModel.Composer.Draft.Title);
        Assert.Contains("제작자가 작성한 정보", viewModel.Composer.Draft.Body);
    }

    [Fact]
    public void Candidate_CanBeAppendedToCurrentDraftWithoutReplacingExistingText()
    {
        using var viewModel = CreateViewModel(new RecordingClient([], []));
        viewModel.OpenBlankDraft("관리자");
        viewModel.Composer.Draft.Title = "지역 사과를 같이 살펴봅니다";
        viewModel.Composer.Draft.Body = "먼저 적어 둔 생각입니다.";

        var added = viewModel.AppendCandidateToDraft(KamisCandidate(), "관리자");

        Assert.True(added);
        Assert.StartsWith("먼저 적어 둔 생각입니다.", viewModel.Composer.Draft.Body, StringComparison.Ordinal);
        Assert.Contains("참고 자료 · 사과 (후지 · 상품)", viewModel.Composer.Draft.Body, StringComparison.Ordinal);
        Assert.Contains("https://www.kamis.or.kr/service/price/xml.do", viewModel.Composer.Draft.Body, StringComparison.Ordinal);
    }

    [Fact]
    public void Diagram_AppliesTemplateFlowToEmptyDraft()
    {
        using var viewModel = CreateViewModel(new RecordingClient([], []));

        var applied = viewModel.ApplyDiagramToDraft(
            "관리자",
            CommunityBoardCatalog.PublicBoards.Select(board => board.DisplayName).ToArray());

        Assert.True(applied);
        Assert.True(viewModel.Composer.IsOpen);
        Assert.Equal(CommunityBoardCatalog.Vow.DisplayName, viewModel.Composer.Draft.Category);
        Assert.NotEmpty(viewModel.Composer.Draft.Title);
        Assert.Contains("다이어그램에서 시작한 커뮤니티 글입니다", viewModel.Composer.Draft.Body, StringComparison.Ordinal);
        Assert.NotEmpty(viewModel.Composer.Draft.WorkflowTag);
    }

    [Fact]
    public void Diagram_UsesGroupImportJourneyAndKeepsBusinessEmailOutOfPublicDraft()
    {
        using var viewModel = CreateViewModel(new RecordingClient([], []));
        var diagram = viewModel.Diagram;
        var linkedStep = diagram.Steps.First();
        diagram.SelectedOrganizationNodeKey = linkedStep.Key;
        diagram.NewOrganizationName = "Example Export Co.";
        diagram.NewOrganizationRoleLabel = "해외 공급자";
        diagram.NewOrganizationCountryCode = "CN";
        diagram.NewOrganizationWebsiteUrl = "https://example.com";
        diagram.NewOrganizationPublicBusinessEmail = "trade@example.com";
        diagram.NewOrganizationContactSourceUrl = "https://example.com/contact";
        diagram.NewOrganizationContactSourceReviewed = true;

        var added = diagram.AddOrganizationCandidate();
        var snapshot = diagram.CreateImportJourneyUpdate();
        var postDraft = diagram.CreateCommunityDraft(
            CommunityBoardCatalog.PublicBoards.Select(board => board.DisplayName).ToArray(),
            "Hongdal Admin",
            "운영자");

        Assert.True(added);
        Assert.Equal(CommunityLedgerTemplateKeys.GroupImport, diagram.SelectedTemplateKey);
        Assert.Equal(CommunityLedgerTemplateKeys.GroupImport, snapshot.LedgerTemplateKey);
        var organization = Assert.Single(snapshot.OrganizationCandidates);
        Assert.Equal(linkedStep.Key, organization.DiagramNodeKey);
        Assert.True(organization.ContactSourceReviewed);
        Assert.Contains("Example Export Co.", postDraft.Body, StringComparison.Ordinal);
        Assert.DoesNotContain("trade@example.com", postDraft.Body, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("수동 이메일 초안 준비 가능", diagram.OutreachReadinessLabel);
    }

    [Fact]
    public void MutualBenefitReview_UsesDraftPurposeAndAppendsNonBindingAssessment()
    {
        using var viewModel = CreateViewModel(new RecordingClient([], []));
        viewModel.OpenVowDraft("관리자");
        viewModel.OpenMutualBenefitTool();
        foreach (var role in viewModel.MutualBenefit.Roles)
        {
            role.ParticipantReviewed = true;
        }

        var applied = viewModel.ApplyMutualBenefitToDraft("관리자");

        Assert.True(applied);
        Assert.Equal(CommunityAuthoringTool.MutualBenefit, viewModel.ActiveTool);
        Assert.Equal(viewModel.Composer.Draft.Title.Trim(), viewModel.MutualBenefit.SharedPurpose);
        Assert.Equal(
            CommunityMutualBenefitAssessmentStatusCodes.MutualBenefitCandidate,
            viewModel.MutualBenefit.Assessment?.StatusCode);
        Assert.Contains("상호 이익 사전 검토", viewModel.Composer.Draft.Body, StringComparison.Ordinal);
        Assert.Contains("거래 실행을 대신하지 않습니다", viewModel.Composer.Draft.Body, StringComparison.Ordinal);
        Assert.Contains("공동조달 경제성 계획", viewModel.Composer.Draft.Body, StringComparison.Ordinal);
        Assert.DoesNotContain("공개용 순편익 추정", viewModel.Composer.Draft.Body, StringComparison.Ordinal);
    }

    [Fact]
    public void MutualBenefitReview_ImportsOrganizationRolesFromDiagramWithoutOperationalSelection()
    {
        using var viewModel = CreateViewModel(new RecordingClient([], []));
        var diagram = viewModel.Diagram;
        diagram.SelectedOrganizationNodeKey = diagram.Steps.First().Key;
        diagram.NewOrganizationName = "Example Logistics";
        diagram.NewOrganizationRoleLabel = "수입 물류 후보";
        diagram.NewOrganizationCountryCode = "US";

        Assert.True(diagram.AddOrganizationCandidate());

        var imported = viewModel.ImportDiagramRolesToMutualBenefit();

        Assert.Equal(1, imported);
        var role = Assert.Single(
            viewModel.MutualBenefit.Roles,
            item => item.RoleLabel == "수입 물류 후보");
        Assert.Equal("Example Logistics", role.ParticipantLabel);
        Assert.False(role.ParticipantReviewed);
        Assert.Equal(CommunityAuthoringTool.MutualBenefit, viewModel.ActiveTool);
    }

    [Fact]
    public void EvidenceChart_ImportsWinWinAmountsAndPersistsReadableChartBlockInDraft()
    {
        using var viewModel = CreateViewModel(new RecordingClient([], []));
        viewModel.OpenVowDraft("관리자");
        for (var index = 0; index < viewModel.MutualBenefit.Roles.Count; index++)
        {
            var role = viewModel.MutualBenefit.Roles[index];
            role.ExpectedBenefitAmount = 20_000m + index * 5_000m;
            role.ExpectedBurdenAmount = 10_000m + index * 2_000m;
        }

        var imported = viewModel.ImportMutualBenefitToEvidenceChart();
        var applied = viewModel.ApplyEvidenceChartToDraft("관리자");

        Assert.True(imported);
        Assert.True(applied);
        Assert.Equal(CommunityAuthoringTool.EvidenceChart, viewModel.ActiveTool);
        Assert.NotNull(viewModel.EvidenceChart.Statistics);
        Assert.Equal(3, viewModel.EvidenceChart.Preview?.Points.Count);
        var block = Assert.Single(
            CommunityEvidenceChartTextCodec.DecodeAll(viewModel.Composer.Draft.Body));
        Assert.Equal("역할별 순편익 추정", block.Title);
        Assert.Equal("KRW", block.Unit);
        Assert.All(block.Points, point => Assert.True(point.Value > 0m));
        Assert.DoesNotContain(
            CommunityEvidenceChartTextCodec.StartMarker,
            CommunityEvidenceChartTextCodec.StripBlocks(viewModel.Composer.Draft.Body),
            StringComparison.Ordinal);
    }

    [Fact]
    public async Task Diagram_CanAttachDirectoryProviderToSelectedNodeWithoutContactEmail()
    {
        var provider = new ThirdPartyLogisticsProviderDirectoryItem
        {
            MarketCode = OperatingMarketCodes.UnitedStates,
            ProviderKey = "example-3pl",
            DisplayName = "Example 3PL",
            OfficialWebsiteUrl = "https://example.com/",
            DirectoryStatusCode = ThirdPartyLogisticsProviderDirectoryStatusCodes.ResearchCandidate,
            PlatformRelationshipStatusCode = ThirdPartyLogisticsProviderRelationshipStatusCodes.NoPlatformRelationship,
            CompanySourceVerificationStatusCode = ThirdPartyLogisticsProviderVerificationStatusCodes.OfficialCompanySourceReviewed,
            RegulatoryVerificationStatusCode = ThirdPartyLogisticsProviderVerificationStatusCodes.RegulatoryStatusNotVerified,
            CapabilityCodes = [ThirdPartyLogisticsProviderCapabilityCodes.WarehousingAndDistribution],
            Evidence =
            [
                new ThirdPartyLogisticsProviderEvidence
                {
                    EvidenceTypeCode = ThirdPartyLogisticsProviderEvidenceTypeCodes.OfficialProviderServicePage,
                    SourceTitle = "Example services",
                    SourceUrl = "https://example.com/services",
                    ReviewedOn = new DateOnly(2026, 7, 19)
                }
            ]
        };
        using var diagram = new CommunityAuthoringDiagramViewModel(
            new PlatformCommunityDiagramWorkspaceViewModel(),
            new RecordingOrganizationDirectoryClient(provider));
        var node = diagram.Steps.First();
        diagram.SelectedOrganizationNodeKey = node.Key;

        await diagram.SearchOrganizationDirectoryAsync();
        var attached = diagram.AttachOrganizationDirectoryItem(
            Assert.Single(diagram.OrganizationDirectoryItems));
        var savedJourney = diagram.CreateImportJourneyUpdate();
        var snapshot = diagram.CreateDiagramSnapshot("diagram-1");

        Assert.True(attached);
        var candidate = Assert.Single(savedJourney.OrganizationCandidates);
        Assert.Equal("example-3pl", candidate.SourceReferenceKey);
        Assert.Empty(candidate.PublicBusinessEmail);
        Assert.False(candidate.CanBeSelectedForOperations);
        var organization = Assert.Single(
            snapshot.Nodes.Single(item => item.NodeId == node.Key).OrganizationReferences);
        Assert.Equal("Example 3PL", organization.DisplayName);
        Assert.Equal(
            DiagramOrganizationSourceKindCodes.ThirdPartyLogisticsDirectory,
            organization.SourceKindCode);
        Assert.Equal(
            ThirdPartyLogisticsProviderVerificationStatusCodes.RegulatoryStatusNotVerified,
            organization.RegulatoryVerificationStatusCode);
    }

    [Fact]
    public async Task SavedYouTubeWorkspace_RestoresAndResavesImportJourneyWithOrganizations()
    {
        var response = SocialResearchResponse();
        var journey = ImportJourney();
        var workspace = SocialWorkspace(response) with { ImportJourney = journey };
        var client = new RecordingClient([], [], response.Sources, response, workspace);
        using var viewModel = CreateViewModel(client);
        await viewModel.SocialResearch.InitializeAsync();
        viewModel.SocialResearch.VideoReference = response.Video.OriginalUrl;

        Assert.True(await viewModel.SocialResearch.LoadSavedWorkspaceAsync());
        Assert.True(viewModel.LoadSocialWorkspaceJourney());
        Assert.Equal(journey.Nodes.Count, viewModel.Diagram.Steps.Count);
        Assert.Single(viewModel.Diagram.OrganizationCandidates);

        viewModel.OpenVowDraft("관리자");
        Assert.True(await viewModel.SaveSocialWorkspaceDraftAsync());
        Assert.NotNull(client.LastWorkspaceDraftRequest?.ImportJourney);
        Assert.Equal(
            CommunityLedgerTemplateKeys.GroupImport,
            client.LastWorkspaceDraftRequest!.ImportJourney!.LedgerTemplateKey);
        Assert.Single(client.LastWorkspaceDraftRequest.ImportJourney.OrganizationCandidates);
    }

    [Fact]
    public void VowDraft_UsesTransparentOperatorPersonaAndNonBindingBoundary()
    {
        using var viewModel = CreateViewModel(new RecordingClient([], []));

        viewModel.OpenVowDraft("실명 관리자");
        var firstNickname = viewModel.Composer.Draft.Nickname;
        viewModel.SelectNextWritingPersona();

        Assert.All(
            viewModel.WritingPersona.Personas,
            persona => Assert.EndsWith("· 운영자", persona.Nickname, StringComparison.Ordinal));
        Assert.NotEqual(firstNickname, viewModel.Composer.Draft.Nickname);
        Assert.Equal(CommunityBoardCatalog.Vow.DisplayName, viewModel.Composer.Draft.Category);
        Assert.StartsWith("[서원]", viewModel.Composer.Draft.Title, StringComparison.Ordinal);
        Assert.Contains("함께 알아차리고 싶은 사람·업체", viewModel.Composer.Draft.Body, StringComparison.Ordinal);
        Assert.Contains("주문·계약·결제·배차", viewModel.Composer.Draft.Body, StringComparison.Ordinal);
    }

    [Fact]
    public void VowDraft_UsesSelectedRoadmapVersionWithoutEnablingItsOperations()
    {
        using var viewModel = CreateViewModel(new RecordingClient([], []));
        viewModel.SelectVowVersion("2.0");

        viewModel.OpenVowDraft("관리자");

        Assert.Equal("홍달 2.0 · 국제 물류·통관", viewModel.Composer.Draft.WorkflowTag);
        Assert.StartsWith("[서원][홍달 2.0]", viewModel.Composer.Draft.Title, StringComparison.Ordinal);
        Assert.Contains("HS 코드", viewModel.Composer.Draft.Body, StringComparison.Ordinal);
        Assert.Contains("자격 있는 관세사", viewModel.Composer.Draft.Body, StringComparison.Ordinal);
        Assert.Contains("실행 기능의 활성화", viewModel.Composer.Draft.Body, StringComparison.Ordinal);
    }

    [Fact]
    public void VowVersionCatalog_CoversCurrentRoadmapAndOpenEndedFuture()
    {
        Assert.Equal(
            ["0.0", "1.0", "1.5", "2.0", "2.5", "3.0", "3.5", "future"],
            CommunityVowVersionCatalog.All.Select(version => version.Code));
        Assert.True(CommunityVowVersionCatalog.Current.IsCurrentFocus);
        Assert.True(CommunityVowVersionCatalog.Find("future").IsFutureExploration);
        Assert.Equal(
            CommunityVowVersionCatalog.All.Count,
            CommunityVowVersionCatalog.All.Select(version => version.WorkflowTag).Distinct().Count());
    }

    [Fact]
    public async Task SocialResearch_UsesPastedYouTubeUrlAndAppliesReturnedDraft()
    {
        var response = SocialResearchResponse();
        var client = new RecordingClient(
            [],
            [],
            [new("reddit-public-posts", "Reddit", "Reddit 공개 글", "https://example.com", true, true, false)],
            response);
        using var viewModel = CreateViewModel(client);
        await viewModel.SocialResearch.InitializeAsync();
        viewModel.SocialResearch.VideoReference = "https://www.youtube.com/watch?v=video-1";
        viewModel.SocialResearch.SearchTermsText = "공동구매, 지역 식재료";

        var researched = await viewModel.SocialResearch.ResearchAsync();
        var applied = viewModel.ApplySocialResearchToDraft("관리자");

        Assert.True(researched);
        Assert.True(applied);
        Assert.Equal("video-1", client.LastResearchRequest?.VideoId);
        Assert.Equal("공동구매", viewModel.Composer.Draft.WorkflowTag);
        Assert.Equal(CommunityBoardCatalog.Vow.DisplayName, viewModel.Composer.Draft.Category);
        Assert.Equal(response.Video.OriginalUrl, viewModel.Composer.Draft.SharedLinkUrl);
        Assert.Contains("SNS에서 함께 본 내용", viewModel.Composer.Draft.Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SocialResearch_LoadsSavedYouTubeRootAndNestedSocialSources()
    {
        var response = SocialResearchResponse();
        var workspace = SocialWorkspace(response);
        var client = new RecordingClient(
            [],
            [],
            response.Sources,
            response,
            workspace);
        using var viewModel = CreateViewModel(client);
        await viewModel.SocialResearch.InitializeAsync();
        viewModel.SocialResearch.VideoReference = response.Video.OriginalUrl;

        var loaded = await viewModel.SocialResearch.LoadSavedWorkspaceAsync();

        Assert.True(loaded);
        Assert.True(viewModel.SocialResearch.HasWorkspace);
        Assert.Equal(workspace.WorkspaceId, viewModel.SocialResearch.WorkspaceId);
        Assert.Equal(workspace.Revision, viewModel.SocialResearch.WorkspaceRevision);
        Assert.Single(viewModel.SocialResearch.Result!.Items);
        Assert.Equal(
            "https://www.reddit.com/r/localfood/",
            Assert.Single(viewModel.SocialResearch.Sources).StartUrls.Single());
    }

    private static CommunityInformationReviewPageViewModel CreateViewModel(
        ICommunityInformationReviewClient client)
    {
        var communityService = new PlatformCommunityService(new HttpClient(), null!);
        var composer = new CommunityPostComposerViewModel(
            communityService,
            new InMemoryDraftStore());
        var socialResearch = new CommunityAuthoringSocialResearchViewModel(client);
        var diagram = new CommunityAuthoringDiagramViewModel(new PlatformCommunityDiagramWorkspaceViewModel());
        return new CommunityInformationReviewPageViewModel(
            client,
            composer,
            new CommunityScheduledPostListViewModel(communityService),
            socialResearch,
            diagram,
            new CommunityAuthoringMutualBenefitViewModel(),
            new CommunityAuthoringEvidenceChartViewModel(),
            new CommunityOperatorWritingPersonaViewModel(),
            new CommunityVowVersionViewModel());
    }

    private static CommunityInformationSourceDto Source(string sourceKey)
        => new(
            sourceKey,
            CommunityInformationSourceTypes.PublicData,
            "provider",
            "source",
            CommunityInformationCollectionModes.ScheduledArchive,
            "daily",
            "review",
            "https://example.com/docs",
            true);

    private static CommunityInformationCandidateDto KamisCandidate()
        => new(
            "kamis:apple",
            CommunityInformationSourceKeys.KamisPriceObservations,
            CommunityInformationSourceTypes.PublicData,
            "KAMIS 농산물 유통정보",
            "사과 (후지 · 상품)",
            "소매 · 과일류 · 25,000원/10개",
            "https://www.kamis.or.kr/service/price/xml.do",
            null,
            null,
            new DateOnly(2026, 7, 17),
            new DateTime(2026, 7, 18, 1, 0, 0, DateTimeKind.Utc),
            "KR",
            "ko",
            "KRW",
            "10개",
            CommunityInformationReviewStates.OfficialObservation,
            ["농수산물", "과일류"],
            "KAMIS Open API에서 수집한 관측값입니다.",
            "전체 시장 평균이나 판매 권고가 아닙니다.");

    private static CommunityInformationCandidateDto VideoCandidate()
        => new(
            "youtube:food",
            CommunityInformationSourceKeys.YouTubeChannelVideos,
            CommunityInformationSourceTypes.Video,
            "음식 채널",
            "새로운 사과 요리",
            "사과를 활용한 공개 영상입니다.",
            "https://www.youtube.com/watch?v=food",
            "https://i.ytimg.com/vi/food/hqdefault.jpg",
            new DateTime(2026, 7, 18, 1, 0, 0, DateTimeKind.Utc),
            new DateOnly(2026, 7, 18),
            new DateTime(2026, 7, 18, 2, 0, 0, DateTimeKind.Utc),
            "KR",
            "ko",
            null,
            null,
            CommunityInformationReviewStates.PendingReview,
            ["음식", "CookingIngredient"],
            "YouTube 공개 메타데이터입니다.",
            "제목과 설명은 영상 제작자가 작성한 정보입니다.");

    private static YouTubeSocialContextResearchResponse SocialResearchResponse()
    {
        var video = new YouTubeSocialContextVideoDto(
            "video-1",
            "Food channel",
            "지역 식재료 영상",
            "영상 요약",
            "https://www.youtube.com/watch?v=video-1",
            null,
            new DateTime(2026, 7, 18, 1, 0, 0, DateTimeKind.Utc),
            "US",
            "en");
        var source = new SocialMediaResearchSourceDto(
            "reddit-public-posts",
            "Reddit",
            "Reddit 공개 글",
            "https://developers.reddit.com/",
            true,
            true,
            false);
        var item = new CommunityInformationCandidateDto(
            "reddit:local-food",
            source.SourceKey,
            CommunityInformationSourceTypes.SocialMedia,
            source.Provider,
            "Local food discussion",
            "Public discussion summary",
            "https://www.reddit.com/r/localfood/comments/1",
            null,
            new DateTime(2026, 7, 18, 1, 10, 0, DateTimeKind.Utc),
            null,
            new DateTime(2026, 7, 18, 1, 20, 0, DateTimeKind.Utc),
            "US",
            "en",
            null,
            null,
            CommunityInformationReviewStates.PendingReview,
            ["지역 식재료"],
            "Reddit public post",
            "Editorial review required");
        return new YouTubeSocialContextResearchResponse(
            new DateTime(2026, 7, 18, 2, 0, 0, DateTimeKind.Utc),
            video,
            ["공동구매"],
            ["지역 식재료"],
            [source],
            [item],
            [],
            new YouTubeSocialContextPostDraftDto(
                "[함께 보기] 지역 식재료 영상",
                "SNS에서 함께 본 내용",
                new YouTubeSocialContextCollectiveActionDraftDto(
                    "공동구매",
                    CommunityCollectiveIntentTypeCodes.GroupPurchase,
                    [CommunityCollectiveIntentTypeCodes.GroupPurchase],
                    "함께하기",
                    "비구속적 관심 표시",
                    "/api/v1/community/posts/{postId}/opportunities")));
    }

    private static YouTubeSocialContextWorkspaceDto SocialWorkspace(
        YouTubeSocialContextResearchResponse response)
    {
        var now = response.GeneratedAtUtc;
        return new YouTubeSocialContextWorkspaceDto(
            "youtube-video-1",
            3,
            YouTubeSocialContextWorkspaceStatusCodes.DraftEdited,
            response.Video,
            response.SearchTerms,
            response.AdjacentTopics,
            [new SocialMediaResearchTargetDto("reddit-public-posts", ["https://www.reddit.com/r/localfood/"])],
            8,
            [new YouTubeSocialContextSourceGroupDto(response.Sources[0], response.Items)],
            response.Failures,
            new YouTubeSocialContextWorkspaceDraftDto(
                "홍달 운영자",
                CommunityBoardCatalog.InformationPrices.DisplayName,
                response.Draft.CollectiveAction.WorkflowTag,
                "운영자 정보 공유",
                response.Draft.Title,
                response.Draft.Body,
                response.Video.OriginalUrl,
                response.Draft.CollectiveAction,
                true,
                now),
            null,
            [],
            now,
            now,
            now,
            "홍달 운영자");
    }

    private static YouTubeImportJourneyDraftDto ImportJourney()
        => new(
            CommunityLedgerTemplateKeys.GroupImport,
            [
                new YouTubeImportJourneyNodeDto(
                    "supplier-order",
                    "해외 공급자 발주",
                    "공급자와 발주 조건을 확인합니다.",
                    "해외 발주",
                    "work"),
                new YouTubeImportJourneyNodeDto(
                    "customs",
                    "수입 통관",
                    "통관 조건을 확인합니다.",
                    "통관",
                    "work")
            ],
            [new YouTubeImportJourneyEdgeDto("supplier-order", "customs", "선적 뒤 통관")],
            [
                new YouTubeImportOrganizationCandidateDto(
                    "organization-example",
                    "supplier-order",
                    "Example Export Co.",
                    "해외 공급자",
                    "CN",
                    "https://example.com/",
                    "trade@example.com",
                    "https://example.com/contact",
                    true)
            ],
            YouTubeImportOutreachReadinessCodes.ReadyForManualDraft,
            new DateTime(2026, 7, 18, 3, 0, 0, DateTimeKind.Utc));

    private sealed class RecordingClient(
        IReadOnlyList<CommunityInformationSourceDto> sources,
        IReadOnlyList<CommunityInformationCandidateDto> candidates,
        IReadOnlyList<SocialMediaResearchSourceDto>? socialSources = null,
        YouTubeSocialContextResearchResponse? socialResearchResponse = null,
        YouTubeSocialContextWorkspaceDto? socialWorkspace = null) : ICommunityInformationReviewClient
    {
        public CommunityInformationCollectionQuery? LastQuery { get; private set; }
        public YouTubeSocialContextResearchRequest? LastResearchRequest { get; private set; }
        public YouTubeSocialContextWorkspaceDraftUpdateRequest? LastWorkspaceDraftRequest { get; private set; }

        public Task<IReadOnlyList<CommunityInformationSourceDto>> GetSourcesAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult(sources);

        public Task<CommunityInformationCollectionResponse> GetCandidatesAsync(
            CommunityInformationCollectionQuery query,
            CancellationToken cancellationToken = default)
        {
            LastQuery = query;
            return Task.FromResult(new CommunityInformationCollectionResponse(
                DateTime.UtcNow,
                sources,
                candidates,
                []));
        }

        public Task<IReadOnlyList<SocialMediaResearchSourceDto>> GetSocialMediaSourcesAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<SocialMediaResearchSourceDto>>(socialSources ?? []);

        public Task<YouTubeSocialContextResearchResponse> ResearchYouTubeSocialContextAsync(
            YouTubeSocialContextResearchRequest request,
            CancellationToken cancellationToken = default)
        {
            LastResearchRequest = request;
            return Task.FromResult(
                socialResearchResponse
                ?? throw new InvalidOperationException("SNS 조사 응답이 준비되지 않았습니다."));
        }

        public Task<YouTubeSocialContextWorkspaceDto?> GetYouTubeSocialContextWorkspaceByVideoAsync(
            string videoId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(socialWorkspace);

        public Task<IReadOnlyList<YouTubeSocialContextWorkspaceSummaryDto>> GetYouTubeSocialContextWorkspacesAsync(
            int take = 50,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<YouTubeSocialContextWorkspaceSummaryDto>>([]);

        public Task<YouTubeSocialContextWorkspaceDto> SaveYouTubeSocialContextWorkspaceDraftAsync(
            string workspaceId,
            YouTubeSocialContextWorkspaceDraftUpdateRequest request,
            CancellationToken cancellationToken = default)
        {
            LastWorkspaceDraftRequest = request;
            return Task.FromResult(
                socialWorkspace
                ?? throw new InvalidOperationException("SNS 작업공간이 준비되지 않았습니다."));
        }

        public Task<YouTubeSocialContextWorkspaceDto> LinkYouTubeSocialContextPublicationAsync(
            string workspaceId,
            YouTubeSocialContextPublicationLinkRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(
                socialWorkspace
                ?? throw new InvalidOperationException("SNS 작업공간이 준비되지 않았습니다."));
    }

    private sealed class InMemoryDraftStore : ICommunityPostComposerDraftStore
    {
        public Task<CommunityPostComposerSnapshot?> LoadAsync(
            string appKey,
            CancellationToken cancellationToken = default)
            => Task.FromResult<CommunityPostComposerSnapshot?>(null);

        public Task SaveAsync(
            string appKey,
            CommunityPostComposerSnapshot snapshot,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task ClearAsync(
            string appKey,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class RecordingOrganizationDirectoryClient(
        params ThirdPartyLogisticsProviderDirectoryItem[] providers)
        : IDiagramOrganizationDirectoryClient
    {
        public Task<ThirdPartyLogisticsProviderDirectoryResponse> SearchThirdPartyLogisticsAsync(
            string? searchText,
            int pageSize = 12,
            CancellationToken cancellationToken = default)
            => Task.FromResult(new ThirdPartyLogisticsProviderDirectoryResponse
            {
                Success = true,
                MarketCode = OperatingMarketCodes.UnitedStates,
                Page = 1,
                PageSize = pageSize,
                TotalCount = providers.Length,
                Items = providers
            });
    }
}
