using Hongdal.Contracts.Common.AgriculturalFisheries;
using Hongdal.Contracts.Common.Community;
using Hongdal.Controllers.Common;
using Hongdal.Services.AgriculturalFisheries.ImportReadiness;
using Hongdal.Services.Community;
using Microsoft.AspNetCore.Authorization;

namespace Hongdal.Tests.Services.Community;

public sealed class CommunityPostOpportunityServiceTests
{
    [Fact]
    public void 육류와_국경간거래_신호가_함께있을때만_제안한다()
    {
        var analyzer = new CommunityPostOpportunityAnalyzer();

        var suggested = analyzer.Analyze(
            "미국산 돼지고기 수입을 알아봅니다",
            "해외 작업장과 검역 서류를 어디서 확인하면 좋을까요?");
        var ordinary = analyzer.Analyze(
            "오늘 아파트 장터가 열립니다",
            "이웃들과 반찬을 나눕니다.");

        Assert.True(suggested.SuggestMeatImportReadiness);
        Assert.Contains("돼지고기", suggested.MatchedSignals);
        Assert.Contains("수입", suggested.MatchedSignals);
        Assert.False(ordinary.SuggestMeatImportReadiness);
    }

    [Fact]
    public async Task 같은_커뮤니티기능을_유지하고_표시문구만_사용자언어로_바꾼다()
    {
        var store = new InMemoryPostStore(CreateImportPost());
        var service = CreateService(store);

        var korean = await service.GetAsync(71, "ko-KR");
        var english = await service.GetAsync(71, "en-US");

        var koItem = Assert.Single(korean!.Items);
        var enItem = Assert.Single(english!.Items);
        Assert.Equal(koItem.Code, enItem.Code);
        Assert.Equal(koItem.LedgerTemplateKey, enItem.LedgerTemplateKey);
        Assert.Equal(koItem.StartEndpoint, enItem.StartEndpoint);
        Assert.NotEqual(koItem.Title, enItem.Title);
        Assert.Equal(CommunityExperienceScopeCodes.SharedCommunity, english.ExperiencePolicy.ExperienceScopeCode);
        Assert.True(english.ExperiencePolicy.UsesSameCommunityApp);
        Assert.True(english.ExperiencePolicy.DisplayLanguageAffectsContentOnly);
        Assert.False(english.ExperiencePolicy.OperatingProfileAffectsAvailability);
        Assert.False(english.ExperiencePolicy.InfersLanguageFromCountryOrRole);
        Assert.False(enItem.AutoStartsWorkflow);
        Assert.True(enItem.RequiresExplicitConsent);
        Assert.True(enItem.InformationOnly);
        Assert.False(enItem.IsBrokerageEnabled);
    }

    [Fact]
    public async Task 제안조회만으로는_원장을_만들거나_게시글을_바꾸지않는다()
    {
        var store = new InMemoryPostStore(CreateImportPost());
        var ledgerStore = new InMemoryLedgerStore();
        var service = CreateService(store, ledgerStore);

        var result = await service.GetAsync(71, "en");

        Assert.Single(result!.Items);
        Assert.Null(store.Current.LinkedLedgerId);
        Assert.Equal(0, ledgerStore.Count);
    }

    [Fact]
    public async Task 해외작성자도_명시적으로_확인하면_같은게시글과_원장을_시작한다()
    {
        var store = new InMemoryPostStore(CreateImportPost(authorUserId: "exporter-1"));
        var service = CreateService(store);
        var request = CreateStartRequest(MeatImportReadinessPartySideCodes.Overseas);

        var started = await service.StartMeatImportReadinessAsync(
            71,
            request,
            "exporter-1",
            "Overseas operator");
        var retried = await service.StartMeatImportReadinessAsync(
            71,
            request,
            "exporter-1",
            "Overseas operator");

        Assert.True(started.LinkedToCommunityPost);
        Assert.Equal(MeatImportReadinessCaseIds.FromCommunityPost(71), started.Case.CaseId);
        Assert.Equal(started.Case.CaseId, store.Current.LinkedLedgerId);
        Assert.Equal(MeatImportReadinessPartySideCodes.Overseas, started.Case.InitiatorSideCode);
        Assert.Equal(71, started.Case.SourceCommunityPostId);
        Assert.Contains(started.Case.Participants, participant =>
            participant.UserId == "exporter-1"
            && participant.SideCode == MeatImportReadinessPartySideCodes.Overseas);
        Assert.Contains(started.Case.Participants, participant =>
            participant.UserId == "importer-1"
            && participant.SideCode == MeatImportReadinessPartySideCodes.Korean);
        Assert.Equal(started.Case.CaseId, retried.Case.CaseId);
        Assert.Equal(1, retried.Case.Revision);
    }

    [Fact]
    public async Task 작성자의_두가지_명시적확인없이는_시작하지않는다()
    {
        var store = new InMemoryPostStore(CreateImportPost());
        var ledgerStore = new InMemoryLedgerStore();
        var service = CreateService(store, ledgerStore);
        var request = CreateStartRequest(MeatImportReadinessPartySideCodes.Korean);
        request.ConfirmInformationOnly = false;

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.StartMeatImportReadinessAsync(
            71,
            request,
            "author-1",
            "작성자"));

        Assert.Null(store.Current.LinkedLedgerId);
        Assert.Equal(0, ledgerStore.Count);
    }

    [Fact]
    public void 조회는_공개이고_시작은_로그인이_필요하다()
    {
        var controller = typeof(CommunityPostOpportunitiesController);
        Assert.NotNull(controller.GetMethod(nameof(CommunityPostOpportunitiesController.Get))!
            .GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true).SingleOrDefault());
        Assert.NotNull(controller.GetMethod(nameof(CommunityPostOpportunitiesController.StartMeatImportReadiness))!
            .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true).SingleOrDefault());
    }

    private static CommunityPostOpportunityService CreateService(
        InMemoryPostStore postStore,
        InMemoryLedgerStore? ledgerStore = null)
        => new(
            postStore,
            new CommunityPostOpportunityAnalyzer(),
            new MeatImportReadinessService(ledgerStore ?? new InMemoryLedgerStore()));

    private static CommunityPostOpportunitySource CreateImportPost(string authorUserId = "author-1")
        => new(
            71,
            "platform",
            "미국산 돼지고기 수입을 함께 검토합니다",
            "해외 작업장과 한국 수입업자가 검역·통관 준비 정보를 함께 확인하고 싶습니다.",
            authorUserId,
            null);

    private static StartCommunityMeatImportReadinessRequest CreateStartRequest(string initiatorSideCode)
        => new()
        {
            DisplayLanguageCode = CommunityDisplayLanguageCodes.English,
            ConfirmExplicitStart = true,
            ConfirmInformationOnly = true,
            Case = new CreateMeatImportReadinessCaseRequest
            {
                InitiatorSideCode = initiatorSideCode,
                Title = "US frozen pork readiness",
                ProductTypeCode = MeatImportReadinessProductTypeCodes.Pork,
                ProductName = "Frozen pork",
                HsCode = "0203299000",
                OriginCountryCode = "US",
                OriginCountryName = "United States",
                KoreanImporterUserId = "importer-1",
                KoreanImporterDisplayName = "Korean importer",
                KoreanImporterOrganizationName = "Korean Importer Co.",
                OverseasCounterparty = new CreateMeatImportReadinessCounterpartyRequest
                {
                    UserId = initiatorSideCode == MeatImportReadinessPartySideCodes.Overseas ? "exporter-1" : "exporter-2",
                    DisplayName = "Overseas operator",
                    OrganizationName = "US Exporter",
                    RoleCode = MeatImportReadinessParticipantRoleCodes.OverseasEstablishment,
                    EstablishmentNumber = "EST-1234"
                }
            }
        };

    private sealed class InMemoryPostStore : ICommunityPostOpportunityStore
    {
        public InMemoryPostStore(CommunityPostOpportunitySource source)
        {
            Current = source;
        }

        public CommunityPostOpportunitySource Current { get; private set; }

        public Task<CommunityPostOpportunitySource?> GetAsync(long postId, CancellationToken cancellationToken = default)
            => Task.FromResult<CommunityPostOpportunitySource?>(Current.PostId == postId ? Current : null);

        public Task<CommunityPostLedgerLinkResult> LinkLedgerAsync(
            long postId,
            string actorUserId,
            string ledgerId,
            CancellationToken cancellationToken = default)
        {
            if (postId != Current.PostId)
            {
                return Task.FromResult(CommunityPostLedgerLinkResult.NotFound);
            }

            if (!string.Equals(actorUserId, Current.AuthorUserId, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(CommunityPostLedgerLinkResult.NotOwner);
            }

            if (Current.LinkedLedgerId is not null)
            {
                return Task.FromResult(string.Equals(Current.LinkedLedgerId, ledgerId, StringComparison.OrdinalIgnoreCase)
                    ? CommunityPostLedgerLinkResult.AlreadyLinked
                    : CommunityPostLedgerLinkResult.ConflictingLedger);
            }

            Current = Current with { LinkedLedgerId = ledgerId };
            return Task.FromResult(CommunityPostLedgerLinkResult.Linked);
        }
    }

    private sealed class InMemoryLedgerStore : I커뮤니티원장저장소
    {
        private readonly Dictionary<string, 커뮤니티원장Dto> _items = new(StringComparer.OrdinalIgnoreCase);

        public int Count => _items.Count;

        public Task<커뮤니티원장Dto> 원장저장Async(
            커뮤니티원장저장요청 request,
            string updatedBy,
            CancellationToken cancellationToken = default)
        {
            var id = request.원장Id ?? $"ledger-{Guid.NewGuid():N}";
            _items.TryGetValue(id, out var existing);
            if (request.기대Revision.HasValue && request.기대Revision.Value != (existing?.Revision ?? 0))
            {
                throw new InvalidOperationException("원장의 현재 상태가 다른 요청에서 먼저 변경되었습니다.");
            }

            var now = DateTime.UtcNow;
            var saved = new 커뮤니티원장Dto
            {
                원장Id = id,
                Revision = (existing?.Revision ?? 0) + 1,
                커뮤니티Id = request.커뮤니티Id,
                원장템플릿Key = request.원장템플릿Key,
                제목 = request.제목,
                원함 = request.원함,
                상태 = request.상태 ?? 커뮤니티원장상태.초안,
                현재단계Key = request.현재단계Key,
                대상OsCode = request.대상OsCode,
                대상OsName = request.대상OsName,
                생성자UserId = request.생성자UserId,
                생성자표시명 = request.생성자표시명 ?? "참여자",
                블록목록 = request.블록목록,
                참여자목록 = request.참여자목록,
                포함원장목록 = request.포함원장목록 ?? [],
                다이어그램스냅샷 = request.다이어그램스냅샷,
                외부참조 = request.외부참조,
                확장속성 = request.확장속성,
                생성시각Utc = existing?.생성시각Utc ?? now,
                수정시각Utc = now
            };
            _items[id] = saved;
            return Task.FromResult(saved);
        }

        public Task<커뮤니티원장Dto?> 원장조회Async(string 원장Id, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.GetValueOrDefault(원장Id));

        public Task<IReadOnlyList<커뮤니티원장Dto>> 원장목록조회Async(
            커뮤니티원장조회조건 query,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<커뮤니티원장Dto>>(_items.Values.ToArray());

        public Task<커뮤니티원장Dto?> 원장상태변경Async(
            커뮤니티원장상태변경요청 request,
            string updatedBy,
            CancellationToken cancellationToken = default)
            => Task.FromResult<커뮤니티원장Dto?>(null);
    }
}
