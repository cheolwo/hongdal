using Hongdal.Contracts.Common.Community;
using Hongdal.Contracts.Common.ContractManagement;
using Hongdal.Contracts.Common.Orderer;
using Hongdal.Ui.Common.Areas.App.Services;
using Hongdal.Ui.Common.Areas.App.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Hongdal.Tests.Ui.Common;

public sealed class 공동구매화면ViewModelTests
{
    [Fact]
    public async Task 초기화_공동수입을제외하고최신국내공동구매를선택한다()
    {
        var service = new Fake공동구매업무Service();
        var oldCampaign = Campaign("이전 국내 공동구매", DateTime.UtcNow.AddDays(-2), sourcePostId: 11);
        var latestCampaign = Campaign("최신 국내 공동구매", DateTime.UtcNow.AddDays(-1), sourcePostId: 22);
        var importCampaign = Campaign("공동수입", DateTime.UtcNow, hsCode: "0202.30");
        service.목록응답.Items = [oldCampaign, importCampaign, latestCampaign];
        service.상세응답[latestCampaign.Id] = latestCampaign;
        service.의견응답[22] =
        [
            new PlatformCommunityPostCommentResponse
            {
                Id = 1,
                Nickname = "참여자",
                Body = "[이의제기:recruitment] 수령 시간을 조정해 주세요."
            }
        ];

        using var fixture = CreateFixture(service);

        var succeeded = await fixture.ViewModel.초기화Async();

        Assert.True(succeeded);
        Assert.Equal([latestCampaign.Id, oldCampaign.Id], fixture.ViewModel.상태.공동구매목록.Select(x => x.Id));
        Assert.Same(latestCampaign, fixture.ViewModel.상태.선택된공동구매);
        Assert.Single(fixture.ViewModel.상태.의견목록);
        Assert.Equal(1, fixture.ViewModel.모집.이의검토.전체이의수);
    }

    [Fact]
    public async Task 제안등록_투표생성실패시게시글번호를복구정보로남긴다()
    {
        var service = new Fake공동구매업무Service
        {
            생성게시글응답 = new PlatformCommunityPostResponse { Id = 77 },
            공동구매생성예외 = new InvalidOperationException("vote endpoint unavailable")
        };
        using var fixture = CreateFixture(service);
        var proposal = fixture.ViewModel.모집.제안;
        FillProposal(proposal);

        var succeeded = await proposal.등록Async();

        Assert.False(succeeded);
        Assert.Equal(Api작업상태.실패, proposal.상태);
        Assert.True(proposal.제안글만생성됨);
        Assert.Equal(77, proposal.복구할게시글Id);
        Assert.Contains("게시글 번호 77", proposal.오류메시지);
        Assert.NotNull(service.마지막제안글요청);
        Assert.Contains("최소 참여: 3명", service.마지막제안글요청!.Body);
        Assert.Equal(77, service.마지막공동구매생성요청?.SourcePostId);
        Assert.Null(fixture.ViewModel.상태.선택된공동구매);
    }

    [Fact]
    public async Task 제안등록_성공하면목록선택과수요모집단계를한번에적용한다()
    {
        var created = Campaign("여름 채소 공동구매", DateTime.UtcNow, sourcePostId: 77);
        var service = new Fake공동구매업무Service
        {
            생성게시글응답 = new PlatformCommunityPostResponse { Id = 77 },
            공동구매생성응답 = created
        };
        using var fixture = CreateFixture(service);
        var proposal = fixture.ViewModel.모집.제안;
        FillProposal(proposal);
        proposal.수령소명 = "중앙 관리실";
        proposal.수령소주소 = "서울시 테스트구 1";

        var succeeded = await proposal.등록Async();

        Assert.True(succeeded);
        Assert.Same(created, fixture.ViewModel.상태.선택된공동구매);
        Assert.Equal(공동구매절차코드.수요모집, fixture.ViewModel.상태.현재단계코드);
        Assert.Equal(created.Id, Assert.Single(fixture.ViewModel.상태.공동구매목록).Id);
        Assert.False(proposal.제안글만생성됨);
        Assert.Null(proposal.복구할게시글Id);
        var voteRequest = Assert.IsType<CommunityVoteCreateRequest>(service.마지막공동구매생성요청);
        Assert.Equal("platform", voteRequest.GroupPurchase?.ServiceAreaKey);
        Assert.Single(voteRequest.GroupPurchase?.PickupPoints ?? []);
        Assert.True(voteRequest.ResolutionDocumentEnabled);
        Assert.True(voteRequest.SignatureRequired);
    }

    [Fact]
    public async Task 합의흐름_모집마감부터전원서명까지단계상태를전이한다()
    {
        var openCampaign = Campaign("감자 공동구매", DateTime.UtcNow, sourcePostId: 33);
        var closedCampaign = Campaign(
            openCampaign.Title,
            openCampaign.CreatedAtUtc,
            openCampaign.SourcePostId,
            id: openCampaign.Id,
            status: CommunityVoteStatusCodes.Closed);
        var draftDocument = ResolutionDocument(
            openCampaign.Id,
            CommunityVoteResolutionStatusCodes.LegalReviewRequired);
        var readyDocument = ResolutionDocument(
            openCampaign.Id,
            CommunityVoteResolutionStatusCodes.ReadyToSign,
            signed: false);
        var signedDocument = ResolutionDocument(
            openCampaign.Id,
            CommunityVoteResolutionStatusCodes.Signed,
            signed: true);
        var service = new Fake공동구매업무Service
        {
            모집마감응답 = closedCampaign,
            결의문응답 = draftDocument,
            서명준비응답 = readyDocument,
            전자서명응답 = signedDocument
        };
        using var fixture = CreateFixture(service);
        var viewModel = fixture.ViewModel;
        viewModel.상태.목록적용([openCampaign]);
        viewModel.상태.선택적용(openCampaign);

        Assert.Equal(
            공동구매절차단계상태.진행중,
            viewModel.합의.절차상태.단계상태(공동구매절차코드.수요모집));
        Assert.Equal(
            공동구매절차단계상태.대기,
            viewModel.합의.절차상태.단계상태(공동구매절차코드.확정안));

        viewModel.합의.모집마감.이의검토완료 = true;
        Assert.True(await viewModel.합의.모집마감.마감Async());
        Assert.Equal(CommunityVoteStatusCodes.Closed, viewModel.상태.선택된공동구매?.Status);
        Assert.Equal(공동구매절차코드.확정안, viewModel.상태.현재단계코드);

        Assert.True(await viewModel.합의.결의.결의문작성Async());
        Assert.Same(draftDocument, viewModel.상태.선택된공동구매?.ResolutionDocument);
        Assert.True(await viewModel.합의.결의.서명준비Async());
        Assert.Equal(공동구매절차코드.전자서명, viewModel.상태.현재단계코드);
        Assert.Single(viewModel.합의.전자서명.미서명자);
        Assert.Equal(
            "resolution-1",
            viewModel.실행.주문원장.서명.참고공동구매결의문번호);
        Assert.Equal(
            "document-hash",
            viewModel.실행.주문원장.서명.참고공동구매결의문Hash);
        Assert.Empty(viewModel.실행.주문원장.서명.서명준비초안.계약문서번호);

        Assert.True(viewModel.합의.전자서명.서명자선택("party-1"));
        viewModel.합의.전자서명.결의문동의 = true;
        Assert.True(await viewModel.합의.전자서명.서명제출Async(
            new 공동구매전자서명입력("참여자 1", "data:image/png;base64,test")));

        Assert.Equal(공동구매절차코드.실행, viewModel.상태.현재단계코드);
        Assert.True(viewModel.합의.절차상태.실행준비완료);
        Assert.Equal(
            공동구매절차단계상태.완료,
            viewModel.합의.절차상태.단계상태(공동구매절차코드.전자서명));
        Assert.Equal(
            공동구매절차단계상태.진행중,
            viewModel.합의.절차상태.단계상태(공동구매절차코드.실행));
        Assert.Equal("party-1", service.마지막전자서명요청?.PartyId);
        Assert.True(viewModel.합의.전자서명.전원서명완료);
    }

    [Fact]
    public async Task 수요참여_공동수령방식은수령소선택을요구한다()
    {
        var campaign = Campaign("쌀 공동구매", DateTime.UtcNow);
        using var fixture = CreateFixture(new Fake공동구매업무Service());
        fixture.ViewModel.상태.목록적용([campaign]);
        fixture.ViewModel.상태.선택적용(campaign);
        var demand = fixture.ViewModel.모집.수요참여;
        demand.참여자표시명 = "주문자";
        demand.참여방식코드 = CommunityVoteParticipationMethodCodes.PickupPoint;
        demand.수령소Id = null;

        var succeeded = await demand.참여Async();

        Assert.False(succeeded);
        Assert.Contains("공동 수령소", demand.오류메시지);
    }

    [Fact]
    public async Task 생산자연결_동의된후보를연락요청초안으로변환한다()
    {
        var campaign = Campaign("양파 공동구매", DateTime.UtcNow);
        var supplyService = new Fake공동구매공급Service
        {
            생산자후보응답 = new DomesticProducerCandidateQueryResponse
            {
                IntegrationStatusCode = DomesticProducerDirectoryIntegrationStatuses.Connected,
                Items =
                [
                    new DomesticProducerCandidateResponse
                    {
                        CandidateKey = "producer-1",
                        MaskedDisplayName = "무안 양파 농가 김○○",
                        ProductTags = ["양파"],
                        ThirdPartySharingConsentConfirmed = true,
                        ContactRequestConsentConfirmed = true
                    }
                ]
            },
            연락요청응답 = new DomesticProducerContactRequestDraftResponse
            {
                DraftId = Guid.NewGuid(),
                ProducerCandidateKey = "producer-1"
            }
        };
        using var fixture = CreateFixture(
            new Fake공동구매업무Service(),
            supplyService);
        fixture.ViewModel.상태.목록적용([campaign]);
        fixture.ViewModel.상태.선택적용(campaign);
        var connection = fixture.ViewModel.공급.생산자연결;

        Assert.True(await connection.후보조회Async());
        Assert.True(connection.생산자선택("producer-1"));
        Assert.True(await connection.연락요청저장Async());

        Assert.Equal("producer-1", connection.저장된연락요청?.ProducerCandidateKey);
        Assert.Equal(campaign.Id, supplyService.마지막연락요청?.GroupPurchaseCampaignId);
        Assert.Equal("양파", supplyService.마지막연락요청?.ProductSummary);
    }

    [Fact]
    public async Task 이행계획_확정조건을발주원장초안으로변환한다()
    {
        var campaign = Campaign("쌀 공동구매", DateTime.UtcNow);
        campaign.GroupPurchase!.TotalRequestedQuantity = 100;
        campaign.GroupPurchase.QuantityUnit = "포대";
        var logisticsService = new Fake공동구매물류Service();
        using var fixture = CreateFixture(
            new Fake공동구매업무Service(),
            logisticsService: logisticsService);
        fixture.ViewModel.상태.목록적용([campaign]);
        fixture.ViewModel.상태.선택적용(campaign);
        var fulfillment = fixture.ViewModel.물류.이행계획;
        fulfillment.초안.ProducerTermsAccepted = true;
        fulfillment.초안.BuyerRepresentativeTermsAccepted = true;
        fulfillment.초안.SupplyCompatibilityConfirmed = true;
        fulfillment.입력변경알림();
        logisticsService.발주초안응답 = new DomesticGroupPurchaseFulfillmentOrderDraftResponse
        {
            DraftId = Guid.NewGuid(),
            Plan = fulfillment.계획!
        };

        Assert.True(fulfillment.발주초안생성가능);
        Assert.True(await fulfillment.발주초안저장Async());
        Assert.NotNull(fulfillment.저장된발주초안);
        Assert.Equal(campaign.Id, logisticsService.마지막발주요청?.GroupPurchaseCampaignId);
        Assert.Equal(100, logisticsService.마지막발주요청?.PlannedQuantity);
    }

    [Fact]
    public async Task 자동집단_선택된공동구매를수요초안으로만들고실행Id를공유한다()
    {
        var campaign = Campaign("햇감자 공동구매", DateTime.UtcNow, sourcePostId: 51);
        campaign.CommunityLedgerId = "community-ledger-51";
        campaign.CreatedByDisplayName = "주문자";
        campaign.Options[0].ProductKey = "potato-10kg";
        campaign.Options[0].RequestedQuantity = 4;
        campaign.GroupPurchase!.ServiceAreaKey = "seoul-east";
        campaign.GroupPurchase.ServiceAreaLabel = "서울 동부권";
        campaign.GroupPurchase.MinimumParticipantCount = 5;
        var executionService = new Fake공동구매실행Service
        {
            자동수요응답 = new 공동구매자동집단응답
            {
                자동집단Id = "auto-potato-seoul",
                상품키 = "potato-10kg",
                배송권키 = "seoul-east",
                현재상태 = 공동구매자동집단상태코드.수요수집중
            }
        };
        using var fixture = CreateFixture(
            new Fake공동구매업무Service(),
            executionService: executionService);
        fixture.ViewModel.상태.선택적용(campaign);
        var automaticGroup = fixture.ViewModel.실행.자동집단;

        Assert.Equal("potato-10kg", automaticGroup.수요초안.상품키);
        Assert.Equal("seoul-east", automaticGroup.수요초안.배송권키);
        Assert.Equal(1, automaticGroup.수요초안.희망수량);
        automaticGroup.수요초안.주문자키 = "orderer-51";
        automaticGroup.수요초안.주문자표시명 = "주문자";

        Assert.True(await automaticGroup.수요등록Async());

        Assert.Equal("auto-potato-seoul", fixture.ViewModel.실행.상태.실행공동구매Id);
        Assert.Same(executionService.자동수요응답, automaticGroup.선택된자동집단);
        Assert.Equal(
            $"community-vote:{campaign.Id:N}:orderer-51",
            executionService.마지막자동수요요청?.수요출처키);
    }

    [Fact]
    public async Task 주문원장_보호조회하위원장연결과서명을독립하위ViewModel로처리한다()
    {
        var campaign = Campaign("사과 공동구매", DateTime.UtcNow);
        campaign.ResolutionDocument = ResolutionDocument(
            campaign.Id,
            CommunityVoteResolutionStatusCodes.ReadyToSign);
        var executionService = new Fake공동구매실행Service
        {
            주문원장보호응답 = ProtectedOrderLedger("order-root-1", revision: 1),
            하위원장연결응답 = OrderLedger("order-root-1", revision: 2, childLedgerId: "sales-ledger-1"),
            서명준비응답 = new 주문원장서명상태공개Dto
            {
                주문원장Id = "order-root-1",
                Revision = 3,
                상태Code = ContractSignatureStatusCode.WaitingForSignature,
                필수서명자수 = 1
            },
            서명등록응답 = new 주문원장서명상태공개Dto
            {
                주문원장Id = "order-root-1",
                Revision = 4,
                상태Code = ContractSignatureStatusCode.Signed,
                필수서명자수 = 1,
                서명완료자수 = 1,
                전체서명완료여부 = true
            }
        };
        using var fixture = CreateFixture(
            new Fake공동구매업무Service(),
            executionService: executionService);
        fixture.ViewModel.상태.선택적용(campaign);
        var orderLedger = fixture.ViewModel.실행.주문원장;
        orderLedger.조회.주문원장선택("order-root-1");

        Assert.True(await orderLedger.조회.조회Async());
        Assert.Equal(1, orderLedger.하위원장.현재Revision);

        orderLedger.하위원장.연결초안.하위원장Id = "sales-ledger-1";
        orderLedger.하위원장.연결초안.역할 = 주문원장포함역할.판매;
        Assert.True(await orderLedger.하위원장.연결Async());
        Assert.Equal(2, orderLedger.하위원장.현재Revision);
        Assert.Equal("sales-ledger-1", executionService.마지막하위원장연결요청?.하위원장Id);

        Assert.Equal("resolution-1", orderLedger.서명.참고공동구매결의문번호);
        orderLedger.서명.서명준비초안.계약문서번호 = "ORDER-2026-0001";
        orderLedger.서명.서명준비초안.문서Hash = "order-document-hash";
        Assert.True(await orderLedger.서명.서명준비Async());
        orderLedger.서명.서명등록초안.동의문Hash = "consent-hash";
        orderLedger.서명.서명등록초안.서명증적Hash = "evidence-hash";
        Assert.True(await orderLedger.서명.서명등록Async());
        Assert.True(orderLedger.서명.전체서명완료);
        Assert.Equal(4, orderLedger.서명.서명상태?.Revision);
    }

    [Fact]
    public async Task 커머스이행_자동집단Id로조회하고현재단계의다음작업을제시한다()
    {
        var executionService = new Fake공동구매실행Service
        {
            공동구매커머스응답 =
            [
                new 공동구매커머스이행계획공개Dto
                {
                    공동구매Id = "auto-group-77",
                    문서관리번호 = "commerce-77",
                    주문자집단배송권키 = "seoul",
                    현재상태코드 = 공동구매커머스이행상태코드.입고완료,
                    판매가능수량 = 80
                }
            ]
        };
        using var fixture = CreateFixture(
            new Fake공동구매업무Service(),
            executionService: executionService);
        fixture.ViewModel.실행.상태.실행공동구매선택("auto-group-77");
        var commerce = fixture.ViewModel.실행.커머스이행;

        Assert.True(await commerce.공동구매별조회Async());

        Assert.Equal("commerce-77", commerce.선택된계획?.문서관리번호);
        Assert.Equal(
            공동구매커머스단계상태.진행중,
            commerce.진행단계.Single(stage =>
                stage.코드 == 공동구매커머스이행상태코드.입고완료).상태);
        Assert.Contains("판매 채널", commerce.다음작업안내);
        Assert.False(commerce.변경가능);
    }

    private static TestFixture CreateFixture(
        Fake공동구매업무Service service,
        Fake공동구매공급Service? supplyService = null,
        Fake공동구매물류Service? logisticsService = null,
        Fake공동구매실행Service? executionService = null)
    {
        var services = new ServiceCollection();
        services.AddSingleton<I공동구매업무Service>(service);
        services.AddSingleton<I공동구매공급Service>(supplyService ?? new Fake공동구매공급Service());
        services.AddSingleton<I공동구매물류Service>(logisticsService ?? new Fake공동구매물류Service());
        services.AddSingleton<I공동구매실행Service>(executionService ?? new Fake공동구매실행Service());
        services.AddHongdalUiCommonAppServices();
        var provider = services.BuildServiceProvider();
        var scope = provider.CreateScope();
        return new TestFixture(
            provider,
            scope,
            scope.ServiceProvider.GetRequiredService<공동구매화면ViewModel>());
    }

    private static void FillProposal(공동구매제안ViewModel proposal)
    {
        proposal.제목 = "여름 채소 공동구매";
        proposal.설명 = "산지 채소를 함께 구매합니다.";
        proposal.상품명 = "감자 10kg";
        proposal.제안자표시명 = "제안자";
        proposal.게시글비밀번호 = "test-password";
    }

    private static CommunityVoteResponse Campaign(
        string title,
        DateTime createdAtUtc,
        long? sourcePostId = null,
        string hsCode = "",
        Guid? id = null,
        string status = CommunityVoteStatusCodes.Open)
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            Title = title,
            CreatedAtUtc = createdAtUtc,
            SourcePostId = sourcePostId,
            Status = status,
            Options =
            [
                new CommunityVoteOptionResponse
                {
                    OptionId = "option-1",
                    Text = title,
                    HsCode = hsCode,
                    QuantityUnit = "개"
                }
            ],
            GroupPurchase = new CommunityGroupPurchaseVoteResponse
            {
                QuantityUnit = "개",
                ServiceAreaLabel = "platform"
            }
        };

    private static CommunityVoteResolutionDocumentResponse ResolutionDocument(
        Guid voteId,
        string status,
        bool? signed = null)
    {
        ContractElectronicSignaturePlan? plan = null;
        if (signed is not null)
        {
            var request = new ContractSignatureRequest(
                "party-1",
                "CommunityParticipant",
                "참여자 1");
            var bundle = new ContractElectronicSignatureBundle(
                "resolution-1",
                "document-hash",
                [request],
                [],
                DateTimeOffset.UtcNow);
            plan = new ContractElectronicSignaturePlan(
                bundle,
                signed.Value ? ContractSignatureStatusCode.Signed : ContractSignatureStatusCode.WaitingForSignature,
                1,
                signed.Value ? 1 : 0,
                signed.Value ? [] : ["party-1"],
                [],
                signed.Value,
                signed.Value ? "서명 완료" : "서명 대기");
        }

        return new CommunityVoteResolutionDocumentResponse
        {
            Id = Guid.NewGuid(),
            VoteId = voteId,
            DocumentNumber = "resolution-1",
            DocumentTitle = "공동구매 확정안",
            ResolutionText = "확정 내용",
            DocumentHash = "document-hash",
            Status = status,
            SignaturePlan = plan
        };
    }

    private static 주문원장통합공개Dto OrderLedger(
        string orderLedgerId,
        long revision,
        string? childLedgerId = null)
        => new()
        {
            주문원장 = new 주문원장원장요약Dto
            {
                원장Id = orderLedgerId,
                Revision = revision,
                상태 = "진행중"
            },
            포함원장목록 = childLedgerId is null
                ? []
                :
                [
                    new 주문포함원장공개Dto
                    {
                        원장Id = childLedgerId,
                        역할 = 주문원장포함역할.판매,
                        필수여부 = true
                    }
                ],
            전체하위원장수 = childLedgerId is null ? 0 : 1
        };

    private static 주문원장역할별조회공개Dto ProtectedOrderLedger(
        string orderLedgerId,
        long revision)
        => new()
        {
            주문원장Id = orderLedgerId,
            조회역할 = "주문자",
            주문원장상태 = "진행중",
            주문원장조회근거 = "소유자",
            주문원장상세 = new 주문원장원장요약Dto
            {
                원장Id = orderLedgerId,
                Revision = revision,
                상태 = "진행중"
            }
        };

    private sealed class TestFixture(
        ServiceProvider provider,
        IServiceScope scope,
        공동구매화면ViewModel viewModel) : IDisposable
    {
        public 공동구매화면ViewModel ViewModel { get; } = viewModel;

        public void Dispose()
        {
            scope.Dispose();
            provider.Dispose();
        }
    }

    private sealed class Fake공동구매업무Service : I공동구매업무Service
    {
        public CommunityVoteListResponse 목록응답 { get; } = new();
        public Dictionary<Guid, CommunityVoteResponse> 상세응답 { get; } = [];
        public Dictionary<long, IReadOnlyList<PlatformCommunityPostCommentResponse>> 의견응답 { get; } = [];
        public PlatformCommunityPostResponse? 생성게시글응답 { get; set; }
        public CommunityVoteResponse? 공동구매생성응답 { get; set; }
        public Exception? 공동구매생성예외 { get; set; }
        public CommunityVoteResponse? 수요참여응답 { get; set; }
        public PlatformCommunityPostCommentResponse? 이의등록응답 { get; set; }
        public CommunityVoteResponse? 모집마감응답 { get; set; }
        public CommunityVoteResolutionDocumentResponse? 결의문응답 { get; set; }
        public CommunityVoteResolutionDocumentResponse? 서명준비응답 { get; set; }
        public CommunityVoteResolutionDocumentResponse? 전자서명응답 { get; set; }
        public PlatformCommunityPostCreateRequest? 마지막제안글요청 { get; private set; }
        public CommunityVoteCreateRequest? 마지막공동구매생성요청 { get; private set; }
        public CommunityVoteResolutionSignRequest? 마지막전자서명요청 { get; private set; }

        public Task<CommunityVoteListResponse> 목록조회Async(
            string? communityScope = null,
            CancellationToken cancellationToken = default)
            => Task.FromResult(목록응답);

        public Task<CommunityVoteResponse?> 상세조회Async(
            Guid voteId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(상세응답.GetValueOrDefault(voteId));

        public Task<PlatformCommunityPostResponse?> 제안글생성Async(
            PlatformCommunityPostCreateRequest request,
            CancellationToken cancellationToken = default)
        {
            마지막제안글요청 = request;
            return Task.FromResult(생성게시글응답);
        }

        public Task<CommunityVoteResponse?> 공동구매생성Async(
            CommunityVoteCreateRequest request,
            CancellationToken cancellationToken = default)
        {
            마지막공동구매생성요청 = request;
            return 공동구매생성예외 is null
                ? Task.FromResult(공동구매생성응답)
                : Task.FromException<CommunityVoteResponse?>(공동구매생성예외);
        }

        public Task<IReadOnlyList<PlatformCommunityPostCommentResponse>> 의견조회Async(
            long postId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(의견응답.GetValueOrDefault(postId) ?? []);

        public Task<CommunityVoteResponse?> 수요참여Async(
            Guid voteId,
            CommunityVoteCastRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(수요참여응답);

        public Task<PlatformCommunityPostCommentResponse?> 이의등록Async(
            long postId,
            PlatformCommunityPostCommentCreateRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(이의등록응답);

        public Task<CommunityVoteResponse?> 모집마감Async(
            Guid voteId,
            CommunityVoteCloseRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(모집마감응답);

        public Task<CommunityVoteResolutionDocumentResponse?> 결의문생성Async(
            Guid voteId,
            CommunityVoteResolutionDraftRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(결의문응답);

        public Task<CommunityVoteResolutionDocumentResponse?> 서명준비Async(
            Guid voteId,
            CommunityVoteResolutionReadyToSignRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(서명준비응답);

        public Task<CommunityVoteResolutionDocumentResponse?> 전자서명Async(
            Guid voteId,
            CommunityVoteResolutionSignRequest request,
            CancellationToken cancellationToken = default)
        {
            마지막전자서명요청 = request;
            return Task.FromResult(전자서명응답);
        }
    }

    private sealed class Fake공동구매공급Service : I공동구매공급Service
    {
        public DomesticProducerCandidateQueryResponse 생산자후보응답 { get; set; } = new();
        public DomesticProducerContactRequestDraftResponse? 연락요청응답 { get; set; }
        public DomesticGroupPurchaseRepresentativeCandidateQueryResponse 대표후보응답 { get; set; } = new();
        public DomesticProducerSupplyOfferDraftResponse? 공급제안응답 { get; set; }
        public DomesticGroupPurchaseSupplyCompatibilityPreviewResponse? 적합성응답 { get; set; }
        public DomesticGroupPurchaseNegotiationTimelineResponse 협상이력응답 { get; set; } = new();
        public DomesticProducerContactRequestDraftRequest? 마지막연락요청 { get; private set; }

        public Task<DomesticProducerCandidateQueryResponse> 생산자후보조회Async(
            Guid campaignId,
            string? search = null,
            string? regionCode = null,
            string? product = null,
            CancellationToken cancellationToken = default)
            => Task.FromResult(생산자후보응답);

        public Task<DomesticProducerContactRequestDraftResponse?> 연락요청초안생성Async(
            Guid campaignId,
            DomesticProducerContactRequestDraftRequest request,
            CancellationToken cancellationToken = default)
        {
            마지막연락요청 = request;
            return Task.FromResult(연락요청응답);
        }

        public Task<DomesticGroupPurchaseRepresentativeCandidateQueryResponse> 대표후보조회Async(
            Guid campaignId,
            string? search = null,
            string? operatingAreaCode = null,
            string? product = null,
            CancellationToken cancellationToken = default)
            => Task.FromResult(대표후보응답);

        public Task<DomesticProducerSupplyOfferDraftResponse?> 공급제안초안생성Async(
            Guid campaignId,
            DomesticProducerSupplyOfferDraftRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(공급제안응답);

        public Task<DomesticGroupPurchaseSupplyCompatibilityPreviewResponse?> 공급적합성미리보기Async(
            Guid campaignId,
            DomesticGroupPurchaseSupplyCompatibilityPreviewRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(적합성응답);

        public Task<DomesticGroupPurchaseNegotiationTimelineResponse> 협상이력조회Async(
            Guid campaignId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(협상이력응답);

        public Task<DomesticGroupPurchaseNegotiationEventResponse?> 협상이벤트등록Async(
            Guid campaignId,
            DomesticGroupPurchaseNegotiationEventRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult<DomesticGroupPurchaseNegotiationEventResponse?>(new());

        public Task<DomesticGroupPurchaseNegotiationIssueResponse?> 협상쟁점등록Async(
            Guid campaignId,
            DomesticGroupPurchaseNegotiationIssueRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult<DomesticGroupPurchaseNegotiationIssueResponse?>(new() { IssueId = Guid.NewGuid() });

        public Task<DomesticGroupPurchaseNegotiationIssueResponse?> 숙고의견등록Async(
            Guid campaignId,
            Guid issueId,
            DomesticGroupPurchaseDeliberationPositionRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult<DomesticGroupPurchaseNegotiationIssueResponse?>(new() { IssueId = issueId });

        public Task<DomesticGroupPurchaseNegotiationIssueResponse?> 협상쟁점합의Async(
            Guid campaignId,
            Guid issueId,
            DomesticGroupPurchaseNegotiationResolutionRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult<DomesticGroupPurchaseNegotiationIssueResponse?>(new()
            {
                IssueId = issueId,
                StatusCode = DomesticGroupPurchaseNegotiationIssueStatusCodes.Resolved
            });
    }

    private sealed class Fake공동구매물류Service : I공동구매물류Service
    {
        public DomesticGroupPurchaseFulfillmentPlanResponse? 미리보기응답 { get; set; }
        public DomesticGroupPurchaseFulfillmentOrderDraftResponse? 발주초안응답 { get; set; }
        public DomesticGroupPurchaseFulfillmentPlanRequest? 마지막발주요청 { get; private set; }

        public Task<DomesticGroupPurchaseFulfillmentPlanResponse?> 이행계획미리보기Async(
            Guid campaignId,
            DomesticGroupPurchaseFulfillmentPlanRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(미리보기응답);

        public Task<DomesticGroupPurchaseFulfillmentOrderDraftResponse?> 발주초안생성Async(
            Guid campaignId,
            DomesticGroupPurchaseFulfillmentPlanRequest request,
            CancellationToken cancellationToken = default)
        {
            마지막발주요청 = request;
            return Task.FromResult(발주초안응답);
        }
    }

    private sealed class Fake공동구매실행Service : I공동구매실행Service
    {
        public IReadOnlyList<공동구매자동집단응답> 자동집단목록응답 { get; set; } = [];
        public 공동구매자동집단응답? 자동수요응답 { get; set; }
        public 공동구매자동수요등록Command? 마지막자동수요요청 { get; private set; }
        public 주문원장역할별조회공개Dto? 주문원장보호응답 { get; set; }
        public 주문원장역할별조회공개Dto? 주문원장역할응답 { get; set; }
        public 주문원장통합공개Dto? 하위원장연결응답 { get; set; }
        public 주문원장통합공개Dto? 하위원장분리응답 { get; set; }
        public 주문하위원장연결ClientRequest? 마지막하위원장연결요청 { get; private set; }
        public 주문원장서명상태공개Dto? 서명상태응답 { get; set; }
        public 주문원장서명상태공개Dto? 서명준비응답 { get; set; }
        public 주문원장서명상태공개Dto? 서명등록응답 { get; set; }
        public IReadOnlyList<공동구매커머스이행계획공개Dto> 공동구매커머스응답 { get; set; } = [];
        public IReadOnlyList<공동구매커머스이행계획공개Dto> 문서커머스응답 { get; set; } = [];

        public Task<IReadOnlyList<공동구매자동집단응답>> 자동집단목록조회Async(
            공동구매자동집단조회조건 condition,
            CancellationToken cancellationToken = default)
            => Task.FromResult(자동집단목록응답);

        public Task<공동구매자동집단응답?> 자동수요등록Async(
            공동구매자동수요등록Command request,
            CancellationToken cancellationToken = default)
        {
            마지막자동수요요청 = request;
            return Task.FromResult(자동수요응답);
        }

        public Task<주문원장역할별조회공개Dto?> 주문원장보호조회Async(
            string orderLedgerId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(주문원장보호응답);

        public Task<주문원장역할별조회공개Dto?> 주문원장역할조회Async(
            string orderLedgerId,
            string viewCode,
            CancellationToken cancellationToken = default)
            => Task.FromResult(주문원장역할응답);

        public Task<주문원장통합공개Dto?> 하위원장연결Async(
            string orderLedgerId,
            주문하위원장연결ClientRequest request,
            CancellationToken cancellationToken = default)
        {
            마지막하위원장연결요청 = request;
            return Task.FromResult(하위원장연결응답);
        }

        public Task<주문원장통합공개Dto?> 하위원장분리Async(
            string orderLedgerId,
            string childLedgerId,
            long? expectedRevision = null,
            CancellationToken cancellationToken = default)
            => Task.FromResult(하위원장분리응답);

        public Task<주문원장서명상태공개Dto?> 주문원장서명상태조회Async(
            string orderLedgerId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(서명상태응답);

        public Task<주문원장서명상태공개Dto?> 주문원장서명준비Async(
            string orderLedgerId,
            주문원장서명준비ClientRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(서명준비응답);

        public Task<주문원장서명상태공개Dto?> 주문원장서명등록Async(
            string orderLedgerId,
            주문원장서명등록ClientRequest request,
            CancellationToken cancellationToken = default)
            => Task.FromResult(서명등록응답);

        public Task<IReadOnlyList<공동구매커머스이행계획공개Dto>> 공동구매별커머스이행조회Async(
            string groupPurchaseId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(공동구매커머스응답);

        public Task<IReadOnlyList<공동구매커머스이행계획공개Dto>> 문서번호로커머스이행조회Async(
            string documentManagementNumber,
            CancellationToken cancellationToken = default)
            => Task.FromResult(문서커머스응답);
    }
}
