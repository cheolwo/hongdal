using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Services.Community;
using Ssalddel.Services.Orderer;

namespace Ssalddel.Tests.Services.Orderer;

public sealed class 공동수입준비원장ServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 23, 3, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task 승인된수요집단은_근거를한원장에저장하고_후속실행을열지않는다()
    {
        var fixture = CreateFixture();
        var request = CompleteRequest();

        var created = await fixture.Service.저장Async(
            fixture.Group.자동집단Id,
            request,
            "admin-1",
            "운영 관리자");
        var restored = await fixture.Service.조회Async(fixture.Group.자동집단Id);

        Assert.True(created.생성됨);
        Assert.False(created.이미처리됨);
        Assert.Equal(공동수입준비원장상태코드.전문검토자료준비, created.상태코드);
        Assert.Equal(공동구매거래유형코드.B2C, created.거래문맥.거래유형);
        Assert.Equal(공동구매가격표시기준코드.부가세포함, created.거래문맥.가격표시기준);
        Assert.True(created.평가.전문검토인계가능);
        Assert.False(created.평가.계약서명가능);
        Assert.False(created.평가.결제가능);
        Assert.False(created.평가.신고실행가능);
        Assert.False(created.평가.운송지시가능);
        Assert.Contains(created.평가.명시된미확인항목목록, item => item.Contains("관세사", StringComparison.Ordinal));
        Assert.NotNull(restored);
        Assert.Equal(created.원장Id, restored.원장Id);
        Assert.Equal(1, fixture.LedgerStore.SaveCount);
        Assert.Equal(created.원장Id, fixture.DemandProcessManager.LinkedLedgerId);
        Assert.Equal("handoff-1", fixture.DemandProcessManager.LinkedHandoffId);
        Assert.Equal(1, fixture.DemandProcessManager.LinkCount);

        var ledger = Assert.Single(fixture.LedgerStore.Items.Values);
        Assert.Equal("1.5", ledger.확장속성["ReadinessWorkflowVersion"]);
        Assert.Equal(
            "NoForwarderAutoSelectionNoExternalAutoSendNoContractNoPaymentNoFilingNoTransport",
            ledger.확장속성["ExecutionBoundary"]);
        Assert.StartsWith("group-import-", ledger.원장Id, StringComparison.Ordinal);
        Assert.DoesNotContain("readiness", ledger.원장Id, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(커뮤니티원장상태.진행중, ledger.상태);
        Assert.DoesNotContain(ledger.포함원장목록, item =>
            item.원장템플릿Key == CommunityLedgerTemplateKeys.CargoTransport
            || item.원장템플릿Key == CommunityLedgerTemplateKeys.WarehouseInbound);
        var boundary = Assert.Single(ledger.블록목록, block => block.BlockId == "execution-boundary");
        Assert.All(boundary.Data.Values, value => Assert.Equal(bool.FalseString, value));
        Assert.Contains(ledger.블록목록, block => block.BlockId == "forwarder-handoff");
        var transactionContext = Assert.Single(ledger.블록목록, block => block.BlockId == "transaction-context");
        Assert.Equal(공동구매거래유형코드.B2C, transactionContext.Data[공동구매거래문맥원장키.거래유형]);
        Assert.Equal(
            공동구매가격표시기준코드.부가세포함,
            ledger.외부참조[공동구매거래문맥원장키.가격표시기준]);
    }

    [Fact]
    public async Task 기존공동수입원장이있으면_별도준비원장을만들지않고_준비블록만병합한다()
    {
        var fixture = CreateFixture();
        const string existingLedgerId = "group-import-existing-canonical";
        fixture.LedgerStore.Items[existingLedgerId] = new 커뮤니티원장Dto
        {
            원장Id = existingLedgerId,
            Revision = 4,
            커뮤니티Id = "platform",
            원장템플릿Key = CommunityLedgerTemplateKeys.GroupImport,
            제목 = "기존 공동수입 원장",
            상태 = 커뮤니티원장상태.진행중,
            현재단계Key = CommunityGroupImportLedgerStageCodes.Shipment,
            대상OsCode = CommunityLedgerOperatingSystemCodes.GroupPurchaseImport,
            대상OsName = "공동수입 OS",
            블록목록 =
            [
                new 커뮤니티원장블록Dto
                {
                    BlockId = "shipment-customs",
                    BlockType = CommunityLedgerBlockTypes.Generic,
                    Title = "기존 선적·통관 블록",
                    State = "planned"
                }
            ],
            포함원장목록 =
            [
                new 커뮤니티포함원장참조Dto
                {
                    원장Id = fixture.Group.공동구매주문집계원장Id,
                    원장템플릿Key = CommunityLedgerTemplateKeys.GroupPurchase,
                    역할 = 공동수입원장관계역할.원천공동구매,
                    관계유형 = CommunityLedgerRelationTypes.Reference
                }
            ],
            다이어그램스냅샷 = new DiagramSnapshotDto
            {
                DiagramId = "existing-group-import-diagram",
                DiagramName = "기존 공동수입 실행 다이어그램",
                LedgerTemplateKey = CommunityLedgerTemplateKeys.GroupImport
            },
            외부참조 = new Dictionary<string, string> { ["ExistingReference"] = "preserved" },
            확장속성 = new Dictionary<string, string> { ["WorkflowVersion"] = "1.0" },
            생성시각Utc = Now.UtcDateTime.AddDays(-2),
            수정시각Utc = Now.UtcDateTime.AddDays(-1)
        };
        var request = CompleteRequest();
        request.기대Revision = 4;

        var result = await fixture.Service.저장Async(
            fixture.Group.자동집단Id,
            request,
            "admin-1",
            "운영 관리자");

        Assert.False(result.생성됨);
        Assert.Equal(existingLedgerId, result.원장Id);
        var ledger = Assert.Single(fixture.LedgerStore.Items.Values);
        Assert.Equal(CommunityGroupImportLedgerStageCodes.Shipment, ledger.현재단계Key);
        Assert.Equal("existing-group-import-diagram", ledger.다이어그램스냅샷?.DiagramId);
        Assert.Contains(ledger.블록목록, block => block.BlockId == "shipment-customs");
        Assert.Contains(ledger.블록목록, block => block.BlockId == "trade-readiness-request");
        Assert.Contains(ledger.블록목록, block => block.BlockId == "forwarder-handoff");
        Assert.Equal("preserved", ledger.외부참조["ExistingReference"]);
        Assert.Equal("1.0", ledger.확장속성["WorkflowVersion"]);
        Assert.Equal("1.5", ledger.확장속성["ReadinessWorkflowVersion"]);
    }

    [Fact]
    public async Task 미완성자료도_초안으로보존하지만_전문검토준비로표시하지않는다()
    {
        var fixture = CreateFixture();
        var request = new 공동수입준비원장저장요청
        {
            요청멱등키 = "readiness-draft-1",
            출발국가코드 = "CN",
            도착국가코드 = "KR",
            기준통화코드 = "KRW",
            미확인항목목록 = ["공급자 견적 미수신"]
        };

        var result = await fixture.Service.저장Async(
            fixture.Group.자동집단Id,
            request,
            "admin-1",
            "운영 관리자");

        Assert.Equal(공동수입준비원장상태코드.초안, result.상태코드);
        Assert.False(result.평가.전문검토인계가능);
        Assert.NotEmpty(result.평가.차단사유목록);
        Assert.Contains("공급자 견적 미수신", result.평가.명시된미확인항목목록);
        Assert.Equal(커뮤니티원장상태.진행중, Assert.Single(fixture.LedgerStore.Items.Values).상태);
    }

    [Fact]
    public async Task 사람의인계승인이없으면_준비원장을저장하지않는다()
    {
        var fixture = CreateFixture(approved: false);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            fixture.Service.저장Async(
                fixture.Group.자동집단Id,
                CompleteRequest(),
                "admin-1",
                "운영 관리자"));

        Assert.Contains("승인", exception.Message, StringComparison.Ordinal);
        Assert.Empty(fixture.LedgerStore.Items);
    }

    [Fact]
    public async Task 같은멱등키재시도는_중복저장하지않고_다른자료재사용은거부한다()
    {
        var fixture = CreateFixture();
        var request = CompleteRequest();
        var first = await fixture.Service.저장Async(
            fixture.Group.자동집단Id,
            request,
            "admin-1",
            "운영 관리자");

        var retried = await fixture.Service.저장Async(
            fixture.Group.자동집단Id,
            request,
            "admin-1",
            "운영 관리자");

        Assert.True(retried.이미처리됨);
        Assert.Equal(first.Revision, retried.Revision);
        Assert.Equal(1, fixture.LedgerStore.SaveCount);
        Assert.Equal(2, fixture.DemandProcessManager.LinkCount);

        request.출발국가코드 = "VN";
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            fixture.Service.저장Async(
                fixture.Group.자동집단Id,
                request,
                "admin-1",
                "운영 관리자"));
        Assert.Contains("같은 멱등 키", exception.Message, StringComparison.Ordinal);
        Assert.Equal(1, fixture.LedgerStore.SaveCount);
    }

    [Fact]
    public async Task 여러승인수요집단의재료를_한준비원장에연결하고_품목별근거를검증한다()
    {
        var secondGroup = new 공동구매자동집단응답
        {
            자동집단Id = "auto-group-spice-210690-kr-seoul",
            공동구매주문집계원장Id = "group-purchase-demand-ledger-2",
            상품키 = "ingredient-spice",
            상품명 = "혼합 향신료",
            HS코드 = "2106.90",
            온도코드 = "상온",
            물류방식 = "ReviewRequired",
            배송권키 = "KR-11",
            배송권명 = "서울 모집권",
            현재상태 = 공동구매자동집단상태코드.확정,
            수요건수 = 12,
            참여자수 = 12,
            총희망수량 = 720m,
            수량단위 = "kg",
            목표수량 = 600m,
            모집조건충족여부 = true
        };
        var fixture = CreateFixture(additionalGroups: [secondGroup]);
        var request = CompleteRequest();
        request.재료품목목록 =
        [
            new 공동수입준비재료품목 { 원천자동집단Id = fixture.Group.자동집단Id },
            new 공동수입준비재료품목 { 원천자동집단Id = secondGroup.자동집단Id }
        ];
        request.견적목록[0].재료키 = fixture.Group.상품키;
        request.견적목록.Add(new 공동수입견적근거
        {
            견적키 = "quote-2",
            재료키 = secondGroup.상품키,
            공급자후보키 = "supplier-1",
            통화코드 = "KRW",
            수량단위 = "kg",
            최소주문수량 = 600m,
            단가 = 4_100m,
            납기일수 = 25,
            포장조건 = "10kg 식품용 카톤",
            Incoterms후보 = "FCA Qingdao",
            유효기한Utc = Now.AddDays(14),
            원출처명 = "공급자 서면 견적",
            원출처Url = "https://example.com/evidence/quote-2",
            확인시각Utc = Now.AddHours(-1)
        });
        request.품목분류후보목록[0].재료키 = fixture.Group.상품키;
        request.품목분류후보목록.Add(new 공동수입품목분류후보
        {
            후보키 = "classification-kr-spice",
            재료키 = secondGroup.상품키,
            관할국가코드 = "KR",
            분류체계코드 = 공동수입준비품목분류체계코드.한국HsK,
            품목코드 = "2106909099",
            분류근거 = "혼합 원재료와 조제품 가공 상태 대조",
            신뢰도 = 0.76m,
            검토상태코드 = 공동수입준비검토상태코드.전문가검토필요,
            원출처Url = "https://unipass.customs.go.kr/",
            확인시각Utc = Now.AddHours(-1),
            전문가검토필요 = true
        });

        var result = await fixture.Service.저장Async(
            fixture.Group.자동집단Id,
            request,
            "admin-1",
            "운영 관리자");

        Assert.Equal(2, result.준비자료.재료품목목록.Count);
        Assert.Equal(2, result.원천수요목록.Count);
        Assert.True(result.평가.재료품목구조완료);
        Assert.True(result.평가.견적구조완료);
        Assert.True(result.평가.품목분류후보구조완료);
        Assert.True(result.평가.국제운송검토구조완료);
        Assert.Contains(result.평가.경고목록, item => item.Contains("LCL/FCL", StringComparison.Ordinal));
        Assert.Equal(2, fixture.DemandProcessManager.LinkCount);
        var ledger = Assert.Single(fixture.LedgerStore.Items.Values);
        Assert.Equal(2, ledger.포함원장목록.Count);
        Assert.Contains(ledger.블록목록, block => block.BlockId == "material-items");
        Assert.Contains(ledger.블록목록, block => block.BlockId == "international-transport-review");
    }

    [Fact]
    public async Task 거래문맥이다른수요집단은_같은공동수입원장에합치지않는다()
    {
        var businessGroup = Group();
        businessGroup.자동집단Id = "auto-group-business-spice";
        businessGroup.공동구매주문집계원장Id = "group-purchase-demand-ledger-b2b";
        businessGroup.상품키 = "ingredient-business-spice";
        businessGroup.상품명 = "사업용 향신료";
        businessGroup.거래유형 = 공동구매거래유형코드.B2B;
        businessGroup.가격표시기준 = 공동구매가격표시기준코드.부가세별도;
        var fixture = CreateFixture(additionalGroups: [businessGroup]);
        var request = CompleteRequest();
        request.재료품목목록 =
        [
            new 공동수입준비재료품목 { 원천자동집단Id = fixture.Group.자동집단Id },
            new 공동수입준비재료품목 { 원천자동집단Id = businessGroup.자동집단Id }
        ];

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            fixture.Service.미리보기Async(fixture.Group.자동집단Id, request));

        Assert.Contains("거래 문맥별로 원장을 나눠", exception.Message, StringComparison.Ordinal);
        Assert.Empty(fixture.LedgerStore.Items);
    }

    [Fact]
    public async Task 준비자료를갱신해도_같은원장의1_5Os상태블록을보존한다()
    {
        var fixture = CreateFixture();
        var created = await fixture.Service.저장Async(
            fixture.Group.자동집단Id,
            CompleteRequest(),
            "admin-1",
            "운영 관리자");
        var ledger = Assert.Single(fixture.LedgerStore.Items.Values);
        ledger.블록목록 = ledger.블록목록.Append(new 커뮤니티원장블록Dto
        {
            BlockId = "trade-readiness-os-state",
            BlockType = CommunityLedgerBlockTypes.Generic,
            Title = "1.5 OS 상태",
            State = 공동수입준비Os상태코드.전문검토인계준비,
            Data = new Dictionary<string, string> { ["Json"] = "{}" }
        }).ToArray();
        ledger.Revision++;
        var update = CompleteRequest();
        update.요청멱등키 = "readiness-update-with-os-state";
        update.기대Revision = ledger.Revision;

        var updated = await fixture.Service.저장Async(
            fixture.Group.자동집단Id,
            update,
            "admin-1",
            "운영 관리자");

        Assert.Equal(created.Revision + 2, updated.Revision);
        Assert.Contains(
            Assert.Single(fixture.LedgerStore.Items.Values).블록목록,
            block => block.BlockId == "trade-readiness-os-state"
                     && block.Data.GetValueOrDefault("Json") == "{}");
    }

    [Fact]
    public void 한국과미국품목분류와규제항목은_관할국가별로분리된다()
    {
        var group = Group();
        var korean = 공동수입준비원장정책.평가(CompleteRequest("KR"), group, Now);
        var unitedStates = 공동수입준비원장정책.평가(CompleteRequest("US"), group, Now);

        Assert.True(korean.품목분류후보구조완료);
        Assert.True(korean.국가별검토구조완료);
        Assert.True(unitedStates.품목분류후보구조완료);
        Assert.True(unitedStates.국가별검토구조완료);
        Assert.True(korean.전문검토인계가능);
        Assert.True(unitedStates.전문검토인계가능);
    }

    private static Fixture CreateFixture(
        bool approved = true,
        IReadOnlyList<공동구매자동집단응답>? additionalGroups = null)
    {
        var group = Group();
        var groups = new[] { group }.Concat(additionalGroups ?? []).ToArray();
        var states = groups.Select((item, index) => new 공동구매수요모집Os상태응답
        {
            자동집단Id = item.자동집단Id,
            인계상태 = approved
                ? 공동구매수요모집인계상태코드.승인후속대기
                : 공동구매수요모집인계상태코드.승인대기,
            인계요청Id = approved ? $"handoff-{index + 1}" : string.Empty,
            승인자키 = approved ? "admin-approver" : string.Empty,
            승인시각Utc = approved ? Now.UtcDateTime : null,
            후속워크플로우활성여부 = true,
            실행모드 = "Simulation",
            시뮬레이션여부 = true
        }).ToArray();
        var groupStore = new FakeGroupStore(groups);
        var ledgerStore = new FakeLedgerStore();
        var demandProcessManager = new FakeDemandProcessManager(states);
        var service = new 공동수입준비원장Service(
            groupStore,
            demandProcessManager,
            ledgerStore,
            new FixedTimeProvider(Now));
        return new Fixture(service, group, ledgerStore, demandProcessManager);
    }

    private static 공동구매자동집단응답 Group()
        => new()
        {
            자동집단Id = "auto-group-food-210690-kr-seoul",
            공동구매주문집계원장Id = "group-purchase-demand-ledger-1",
            상품키 = "ingredient-sauce",
            상품명 = "간편식 소스",
            HS코드 = "2106.90",
            온도코드 = "상온",
            물류방식 = "LCL",
            배송권키 = "KR-11",
            배송권명 = "서울 모집권",
            현재상태 = 공동구매자동집단상태코드.확정,
            수요건수 = 18,
            참여자수 = 18,
            총희망수량 = 1_800m,
            수량단위 = "kg",
            목표수량 = 1_500m,
            모집조건충족여부 = true
        };

    private static 공동수입준비원장저장요청 CompleteRequest(string destinationCountry = "KR")
    {
        var classificationSystem = destinationCountry == "US"
            ? 공동수입준비품목분류체계코드.미국Hts
            : 공동수입준비품목분류체계코드.한국HsK;
        var classificationCode = destinationCountry == "US" ? "2106.90.9998" : "2106.90-9099";
        var complianceSource = destinationCountry == "US"
            ? "https://www.fda.gov/food/importing-food-products-united-states/importing-human-foods"
            : "https://impfood.mfds.go.kr/";

        return new 공동수입준비원장저장요청
        {
            요청멱등키 = $"readiness-{destinationCountry}-1",
            재료키 = "ingredient-sauce",
            재료명 = "간편식 소스",
            출발국가코드 = "CN",
            도착국가코드 = destinationCountry,
            기준통화코드 = "KRW",
            공급자근거목록 =
            [
                new 공동수입공급자근거
                {
                    공급자후보키 = "supplier-1",
                    조직명 = "공식 이력 공급자 후보",
                    국가코드 = "CN",
                    관계코드 = "ForeignManufacturer",
                    공식식별자 = "MFDS-FACILITY-001",
                    근거요약 = "공식 수입식품 표시 이력에서 재료 관계 확인",
                    원출처명 = "수입식품정보마루",
                    원출처Url = "https://impfood.mfds.go.kr/",
                    확인시각Utc = Now.AddHours(-2),
                    검토자표시명 = "자료 검토자",
                    검토시각Utc = Now.AddHours(-1),
                    최신상태재확인필요 = true,
                    플랫폼자동선정여부 = false
                }
            ],
            견적목록 =
            [
                new 공동수입견적근거
                {
                    견적키 = "quote-1",
                    공급자후보키 = "supplier-1",
                    통화코드 = "KRW",
                    수량단위 = "kg",
                    최소주문수량 = 1_500m,
                    단가 = 3_200m,
                    납기일수 = 30,
                    포장조건 = "20kg 식품용 카톤, lot 표시",
                    Incoterms후보 = "FOB Shanghai",
                    유효기한Utc = Now.AddDays(14),
                    원출처명 = "공급자 서면 견적",
                    원출처Url = "https://example.com/evidence/quote-1",
                    확인시각Utc = Now.AddHours(-1)
                }
            ],
            예상비용목록 = 공동수입준비비용범주코드.필수목록
                .Select((category, index) => new 공동수입예상비용근거
                {
                    비용키 = $"cost-{index + 1}",
                    범주코드 = category,
                    표시명 = category,
                    통화코드 = "KRW",
                    예상금액 = 100_000m * (index + 1),
                    계산근거 = "수량과 공개 요율 또는 서면 견적을 이용한 Simulation",
                    원출처Url = "https://unipass.customs.go.kr/",
                    확인시각Utc = Now.AddHours(-1),
                    유효기한Utc = Now.AddDays(7)
                })
                .ToList(),
            품목분류후보목록 =
            [
                new 공동수입품목분류후보
                {
                    후보키 = $"classification-{destinationCountry}",
                    관할국가코드 = destinationCountry,
                    분류체계코드 = classificationSystem,
                    품목코드 = classificationCode,
                    분류근거 = "원재료 구성과 조제품 가공 상태를 대조한 후보",
                    신뢰도 = 0.82m,
                    검토상태코드 = 공동수입준비검토상태코드.전문가검토필요,
                    원출처Url = destinationCountry == "US"
                        ? "https://hts.usitc.gov/"
                        : "https://unipass.customs.go.kr/",
                    확인시각Utc = Now.AddHours(-1),
                    전문가검토필요 = true
                }
            ],
            국가별검토항목목록 =
            [
                new 공동수입국가별검토항목
                {
                    관할국가코드 = destinationCountry,
                    항목코드 = destinationCountry == "US" ? "US-FDA-FSVP" : "KR-MFDS-IMPORTED-FOOD",
                    표시명 = destinationCountry == "US" ? "FDA·FSVP 준비" : "식약처 수입식품 준비",
                    검토상태코드 = 공동수입준비검토상태코드.근거수집,
                    책임역할코드 = 공동수입준비책임역할코드.수입자,
                    공식원출처Url = complianceSource,
                    확인시각Utc = Now.AddHours(-1)
                },
                new 공동수입국가별검토항목
                {
                    관할국가코드 = destinationCountry,
                    항목코드 = destinationCountry == "US" ? "US-CBP-HTS" : "KR-KCS-HSK",
                    표시명 = destinationCountry == "US" ? "CBP·HTS 품목분류" : "관세청 HSK 품목분류",
                    검토상태코드 = 공동수입준비검토상태코드.전문가검토필요,
                    책임역할코드 = 공동수입준비책임역할코드.관세사,
                    공식원출처Url = destinationCountry == "US"
                        ? "https://www.cbp.gov/trade/rulings"
                        : "https://unipass.customs.go.kr/",
                    확인시각Utc = Now.AddHours(-1),
                    미확인사유 = "자격 있는 관세 전문가의 최종 검토 필요"
                }
            ],
            책임초안목록 =
            [
                Responsibility(공동수입준비책임역할코드.판매자수출자, "해외 공급자 후보", "상품 규격과 공급·포장 근거 제공"),
                Responsibility(공동수입준비책임역할코드.수입자, "수입자 미지정", "수입 적격성과 신고 주체 확인"),
                Responsibility(공동수입준비책임역할코드.관세사, "관세사 미지정", "품목분류와 신고 자료 전문 검토"),
                Responsibility(공동수입준비책임역할코드.플랫폼, "살뜰 플랫폼", "수요와 근거 자료 연결, 거래 당사자 자동 선정 금지")
            ],
            미확인항목목록 = ["관세사 품목분류 최종 검토"]
        };
    }

    private static 공동수입책임초안 Responsibility(string role, string party, string summary)
        => new()
        {
            역할코드 = role,
            당사자표시명 = party,
            책임요약 = summary,
            당사자확인여부 = false
        };

    private sealed record Fixture(
        공동수입준비원장Service Service,
        공동구매자동집단응답 Group,
        FakeLedgerStore LedgerStore,
        FakeDemandProcessManager DemandProcessManager);

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class FakeGroupStore(IReadOnlyList<공동구매자동집단응답> groups) : I공동구매자동집단화저장소
    {
        public Task<공동구매자동집단응답?> 집단조회Async(string 자동집단Id, CancellationToken cancellationToken = default)
            => Task.FromResult(groups.FirstOrDefault(group => string.Equals(
                자동집단Id,
                group.자동집단Id,
                StringComparison.Ordinal)));

        public Task<IReadOnlyList<공동구매자동집단응답>> 집단목록조회Async(공동구매자동집단조회조건 조건, CancellationToken cancellationToken = default)
            => Task.FromResult(groups);

        public Task<공동구매자동집단응답> 수요등록Async(공동구매자동수요등록Command command, CancellationToken cancellationToken = default)
            => Task.FromResult(groups[0]);

        public Task<공동구매자동수요철회응답> 수요철회Async(공동구매자동수요철회Command command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<공동구매자동집단응답> 개별원함원장연결Async(string 자동집단Id, string 수요Id, string 개별원함원장Id, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<공동구매자동집단응답> 개별주문원장연결Async(string 자동집단Id, string 수요Id, string 공동구매주문집계원장Id, string 개별주문원장Id, string 입고예정원장Id, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class FakeDemandProcessManager(IReadOnlyList<공동구매수요모집Os상태응답> states) : I공동구매수요모집ProcessManager
    {
        public string LinkedHandoffId { get; private set; } = string.Empty;
        public string LinkedLedgerId { get; private set; } = string.Empty;
        public int LinkCount { get; private set; }

        public Task<공동구매수요모집Os상태응답?> 운영상태조회Async(string 자동집단Id, CancellationToken cancellationToken = default)
            => Task.FromResult(states.FirstOrDefault(state => string.Equals(
                자동집단Id,
                state.자동집단Id,
                StringComparison.Ordinal)));

        public Task<공동구매자동집단응답> 수요등록조율Async(공동구매자동수요등록Command command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<공동구매자동수요철회응답> 수요철회조율Async(공동구매자동수요철회Command command, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<공동구매수요모집Os조율응답> 집단조율Async(string 자동집단Id, string 트리거코드, DateTime? 기준시각Utc = null, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<공동구매수요모집마감스캔응답> 모집마감스캔Async(DateTime? 기준시각Utc = null, int? 최대건수 = null, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<공동구매수요모집인계승인응답> 인계승인Async(string 자동집단Id, 공동구매수요모집인계승인요청 요청, string 승인자키, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<공동구매수요모집Os상태응답> 후속원장연결Async(
            string 자동집단Id,
            string 인계요청Id,
            string 대상원장Id,
            CancellationToken cancellationToken = default)
        {
            var state = states.Single(item => string.Equals(item.자동집단Id, 자동집단Id, StringComparison.Ordinal));
            LinkCount++;
            LinkedHandoffId = 인계요청Id;
            LinkedLedgerId = 대상원장Id;
            state.대상원장Id = 대상원장Id;
            return Task.FromResult(state);
        }
    }

    private sealed class FakeLedgerStore : I커뮤니티원장저장소
    {
        public Dictionary<string, 커뮤니티원장Dto> Items { get; } = new(StringComparer.Ordinal);
        public int SaveCount { get; private set; }

        public Task<커뮤니티원장Dto> 원장저장Async(커뮤니티원장저장요청 request, string updatedBy, CancellationToken cancellationToken = default)
        {
            var id = request.원장Id ?? throw new InvalidOperationException("원장 ID가 필요합니다.");
            Items.TryGetValue(id, out var existing);
            if (request.기대Revision.HasValue && request.기대Revision.Value != (existing?.Revision ?? 0))
            {
                throw new InvalidOperationException("Revision conflict");
            }

            var now = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
            var item = new 커뮤니티원장Dto
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
                생성자UserId = existing?.생성자UserId ?? request.생성자UserId,
                생성자표시명 = existing?.생성자표시명 ?? request.생성자표시명 ?? "관리자",
                블록목록 = request.블록목록,
                참여자목록 = request.참여자목록,
                포함원장목록 = request.포함원장목록 ?? existing?.포함원장목록 ?? [],
                다이어그램스냅샷 = request.다이어그램스냅샷,
                외부참조 = request.외부참조,
                확장속성 = request.확장속성,
                생성시각Utc = existing?.생성시각Utc ?? now,
                수정시각Utc = now
            };
            Items[id] = item;
            SaveCount++;
            return Task.FromResult(item);
        }

        public Task<커뮤니티원장Dto?> 원장조회Async(string 원장Id, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.GetValueOrDefault(원장Id));

        public Task<IReadOnlyList<커뮤니티원장Dto>> 원장목록조회Async(커뮤니티원장조회조건 query, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<커뮤니티원장Dto>>(Items.Values.ToArray());

        public Task<커뮤니티원장Dto?> 원장상태변경Async(커뮤니티원장상태변경요청 request, string updatedBy, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
