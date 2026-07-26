using System.Text.Json;
using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Services.Community;
using Ssalddel.Services.Orderer;
using 살뜰.Services.Options;
using 살뜰.Services.Versioning;

namespace Ssalddel.Tests.Services.Orderer;

public sealed class 공동수입준비ProcessManagerTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 23, 3, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task 조회는_1_5상태머신과공유배치_실행차단경계를함께반환한다()
    {
        var fixture = CreateFixture();

        var result = await fixture.Service.운영상태조회Async(fixture.Group.자동집단Id);

        Assert.NotNull(result);
        Assert.Equal(공동수입준비Os상태코드.전문검토인계준비, result.상태코드);
        Assert.True(result.전문검토인계가능);
        Assert.False(result.전문검토완료여부);
        Assert.Equal(7, result.작업목록.Count);
        Assert.Contains(result.작업목록, item =>
            item.작업코드 == 공동수입준비Os작업코드.재료묶음운송검토
            && item.상태코드 == 공동수입준비Os작업상태코드.사람검토대기);
        Assert.Contains(result.공유배치목록, item =>
            item.작업코드 == 공동구매수요모집Os배치작업코드.Kamis일별가격수집);
        Assert.False(result.계약서명가능);
        Assert.False(result.결제가능);
        Assert.False(result.신고실행가능);
        Assert.False(result.운송지시가능);
    }

    [Fact]
    public async Task 전체점검은_작업이력과멱등명령을원장에저장하고_같은요청은재사용한다()
    {
        var fixture = CreateFixture();
        var request = new 공동수입준비Os작업실행요청
        {
            요청멱등키 = "os-run-1",
            기대Revision = fixture.Ledger.Revision,
            작업코드 = 공동수입준비Os작업코드.전체준비점검
        };

        var first = await fixture.Service.작업실행Async(
            fixture.Group.자동집단Id,
            request,
            "admin-1",
            "운영 관리자");
        var second = await fixture.Service.작업실행Async(
            fixture.Group.자동집단Id,
            request,
            "admin-1",
            "운영 관리자");

        Assert.Equal(fixture.Ledger.Revision + 1, first.원장Revision);
        Assert.False(first.이미처리됨);
        Assert.True(second.이미처리됨);
        Assert.Equal(1, fixture.Store.SaveCount);
        Assert.All(
            first.작업목록.Where(item => item.수동실행가능여부),
            item => Assert.Equal(1, item.시도횟수));
        Assert.Equal(공동수입준비Os트리거코드.수동점검, first.마지막트리거코드);
        Assert.Equal("운영 관리자", first.마지막조율자표시명);
    }

    [Fact]
    public async Task 같은멱등키의다른작업은거부한다()
    {
        var fixture = CreateFixture();
        await fixture.Service.작업실행Async(
            fixture.Group.자동집단Id,
            new 공동수입준비Os작업실행요청
            {
                요청멱등키 = "same-key",
                기대Revision = fixture.Ledger.Revision,
                작업코드 = 공동수입준비Os작업코드.공급자근거점검
            },
            "admin-1",
            "운영 관리자");

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            fixture.Service.작업실행Async(
                fixture.Group.자동집단Id,
                new 공동수입준비Os작업실행요청
                {
                    요청멱등키 = "same-key",
                    작업코드 = 공동수입준비Os작업코드.견적원가점검
                },
                "admin-1",
                "운영 관리자"));

        Assert.Contains("서로 다른", exception.Message, StringComparison.Ordinal);
        Assert.Equal(1, fixture.Store.SaveCount);
    }

    [Fact]
    public async Task 전문검토인계는_사람수신자와범위를기록하지만_외부실행을열지않는다()
    {
        var fixture = CreateFixture();

        var result = await fixture.Service.전문검토인계Async(
            fixture.Group.자동집단Id,
            new 공동수입준비Os전문검토인계요청
            {
                요청멱등키 = "qualified-handoff-1",
                기대Revision = fixture.Ledger.Revision,
                검토수신자표시명 = "검토 관세사",
                검토범위 = "HSK와 수입식품 규제",
                인계메모 = "공식 원출처와 견적 유효기간을 함께 확인"
            },
            "admin-1",
            "운영 관리자");

        Assert.Equal(공동수입준비Os상태코드.전문검토진행중, result.상태코드);
        Assert.Equal("검토 관세사", result.전문검토인계기록?.검토수신자표시명);
        Assert.Equal("admin-1", result.전문검토인계기록?.인계자UserId);
        Assert.False(result.다음단계인계후보여부);
        Assert.False(result.계약서명가능);
        Assert.False(result.신고실행가능);
    }

    [Fact]
    public async Task 전문검토와당사자확인이원장에완료되면_다음단계인계후보까지만전이한다()
    {
        var request = CompleteRequest();
        request.품목분류후보목록[0].검토상태코드 = 공동수입준비검토상태코드.전문가검토완료;
        request.품목분류후보목록[0].전문가검토필요 = false;
        request.품목분류후보목록[0].검토자표시명 = "검토 관세사";
        request.국가별검토항목목록[0].검토상태코드 = 공동수입준비검토상태코드.전문가검토완료;
        request.책임초안목록.ForEach(item => item.당사자확인여부 = true);
        request.미확인항목목록.Clear();
        request.국제운송검토 = new 공동수입준비국제운송검토
        {
            검토상태코드 = 공동수입준비국제운송검토상태코드.포워더회신완료,
            방식후보목록 = [공동수입준비국제운송방식코드.Lcl, 공동수입준비국제운송방식코드.Fcl],
            포워더제안방식코드 = 공동수입준비국제운송방식코드.Lcl,
            포워더회신요약 = "총 8 CBM과 냉장 혼적 허용 조건, LCL/FCL 비교 견적을 검토",
            회신업체표시명 = "회신 포워더",
            회신기록자표시명 = "물류 회신 기록자",
            회신시각Utc = Now.AddHours(-1)
        };
        request.포워더인계 = new 공동수입준비포워더인계
        {
            인계상태코드 = 공동수입준비포워더인계상태코드.회신기록됨,
            전달대상업체명 = "회신 포워더",
            전달정보범위코드 = 공동수입준비포워더전달정보범위코드.집계수요전용,
            전달항목코드목록 = [.. 공동수입준비포워더전달항목코드.기본집계목록],
            전달범위요약 = "개인 식별정보를 제외한 재료별 합산 수요와 물류 조건",
            전달패키지버전 = "1.0",
            인계기록자표시명 = "운영 관리자",
            인계시각Utc = Now.AddHours(-2)
        };
        var fixture = CreateFixture(request);

        var result = await fixture.Service.전문검토인계Async(
            fixture.Group.자동집단Id,
            new 공동수입준비Os전문검토인계요청
            {
                요청멱등키 = "qualified-complete-handoff",
                기대Revision = fixture.Ledger.Revision,
                검토수신자표시명 = "검토 관세사",
                검토범위 = "HSK와 수입식품 규제",
                인계메모 = "검토 완료 근거와 당사자 확인을 함께 대조"
            },
            "admin-1",
            "운영 관리자");

        Assert.True(result.전문검토완료여부);
        Assert.True(result.다음단계인계후보여부);
        Assert.Equal(공동수입준비Os상태코드.다음단계인계후보, result.상태코드);
        Assert.False(result.계약서명가능);
        Assert.False(result.운송지시가능);
    }

    [Fact]
    public async Task 최신성기준을지난공급자근거는_전문검토인계를차단한다()
    {
        var request = CompleteRequest();
        request.공급자근거목록[0].확인시각Utc = Now.AddDays(-31);
        request.공급자근거목록[0].검토시각Utc = Now.AddDays(-31);
        var fixture = CreateFixture(request);

        var result = await fixture.Service.운영상태조회Async(fixture.Group.자동집단Id);

        Assert.NotNull(result);
        Assert.Equal(공동수입준비Os상태코드.근거재확인필요, result.상태코드);
        Assert.False(result.전문검토인계가능);
        Assert.Contains(result.차단사유목록, item => item.Contains("최신성", StringComparison.Ordinal));
    }

    [Fact]
    public async Task 정기점검은_기한이없는1_5원장을찾아_내부점검만저장한다()
    {
        var fixture = CreateFixture();

        var result = await fixture.Service.정기점검Async();

        Assert.Equal(1, result.조회건수);
        Assert.Equal(1, result.조율건수);
        Assert.Equal(0, result.실패건수);
        Assert.Equal(1, fixture.Store.SaveCount);
    }

    private static Fixture CreateFixture(공동수입준비원장저장요청? request = null)
    {
        var group = Group();
        var ledger = Ledger(group, request ?? CompleteRequest());
        var store = new FakeLedgerStore(ledger);
        var service = new 공동수입준비ProcessManager(
            new FakeSourceGroupReader(group),
            store,
            new FakeEvidenceBatchReader(),
            new StubFeatureFlags(enabled: true),
            new StubExecutionModePolicy(SsalddelExecutionMode.Simulation),
            new StaticOptionsMonitor<GroupImportReadinessOsOptions>(new GroupImportReadinessOsOptions
            {
                Enabled = true,
                ScanIntervalSeconds = 300,
                BatchSize = 100,
                EvidenceFreshnessDays = 30,
                MaxCommandHistory = 50
            }),
            new FixedTimeProvider(Now));
        return new Fixture(service, group, ledger, store);
    }

    private static 공동구매자동집단응답 Group()
        => new()
        {
            자동집단Id = "auto-group-food-210690-kr-seoul",
            상품키 = "ingredient-sauce",
            상품명 = "간편식 소스",
            HS코드 = "2106.90",
            현재상태 = 공동구매자동집단상태코드.확정,
            총희망수량 = 1_800m,
            수량단위 = "kg"
        };

    private static 커뮤니티원장Dto Ledger(
        공동구매자동집단응답 group,
        공동수입준비원장저장요청 request)
        => new()
        {
            원장Id = 공동수입준비원장Service.원장Id생성(group.자동집단Id),
            Revision = 3,
            커뮤니티Id = "platform",
            원장템플릿Key = CommunityLedgerTemplateKeys.GroupImport,
            제목 = "간편식 소스 공급·가격·무역 준비 원장",
            상태 = 커뮤니티원장상태.진행중,
            현재단계Key = 공동수입준비원장상태코드.전문검토자료준비,
            생성자UserId = "admin-1",
            생성자표시명 = "운영 관리자",
            블록목록 =
            [
                new 커뮤니티원장블록Dto
                {
                    BlockId = "trade-readiness-request",
                    BlockType = CommunityLedgerBlockTypes.Generic,
                    Title = "1.5 준비 자료 원본",
                    State = "recorded",
                    Data = new Dictionary<string, string>
                    {
                        ["Json"] = JsonSerializer.Serialize(request, new JsonSerializerOptions(JsonSerializerDefaults.Web))
                    }
                }
            ],
            외부참조 = new Dictionary<string, string>
            {
                ["AutoGroupId"] = group.자동집단Id
            },
            확장속성 = new Dictionary<string, string>
            {
                ["WorkflowVersion"] = "1.5"
            },
            생성시각Utc = Now.UtcDateTime.AddDays(-1),
            수정시각Utc = Now.UtcDateTime.AddHours(-1)
        };

    private static 공동수입준비원장저장요청 CompleteRequest()
        => new()
        {
            요청멱등키 = "readiness-1",
            재료키 = "ingredient-sauce",
            재료명 = "간편식 소스",
            출발국가코드 = "CN",
            도착국가코드 = 공동수입준비국가코드.대한민국,
            기준통화코드 = "KRW",
            공급자근거목록 =
            [
                new 공동수입공급자근거
                {
                    공급자후보키 = "supplier-1",
                    조직명 = "공식 공급자 후보",
                    국가코드 = "CN",
                    관계코드 = "ForeignManufacturer",
                    공식식별자 = "FACILITY-001",
                    근거요약 = "공식 등록 이력 확인",
                    원출처명 = "공식 원천",
                    원출처Url = "https://example.com/supplier-1",
                    확인시각Utc = Now.AddDays(-1),
                    검토자표시명 = "자료 검토자",
                    검토시각Utc = Now.AddHours(-12),
                    최신상태재확인필요 = false,
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
                    포장조건 = "20kg 카톤",
                    Incoterms후보 = "FOB Shanghai",
                    유효기한Utc = Now.AddDays(14),
                    원출처명 = "서면 견적",
                    원출처Url = "https://example.com/quote-1",
                    확인시각Utc = Now.AddDays(-1)
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
                    계산근거 = "공개 요율과 서면 견적 기반 Simulation",
                    원출처Url = "https://example.com/cost-evidence",
                    확인시각Utc = Now.AddDays(-1),
                    유효기한Utc = Now.AddDays(7)
                })
                .ToList(),
            품목분류후보목록 =
            [
                new 공동수입품목분류후보
                {
                    후보키 = "classification-kr",
                    관할국가코드 = "KR",
                    분류체계코드 = 공동수입준비품목분류체계코드.한국HsK,
                    품목코드 = "2106909099",
                    분류근거 = "원재료와 가공 상태 대조",
                    신뢰도 = 0.82m,
                    검토상태코드 = 공동수입준비검토상태코드.전문가검토필요,
                    원출처Url = "https://unipass.customs.go.kr/",
                    확인시각Utc = Now.AddDays(-1),
                    전문가검토필요 = true
                }
            ],
            국가별검토항목목록 =
            [
                new 공동수입국가별검토항목
                {
                    관할국가코드 = "KR",
                    항목코드 = "KR-MFDS-IMPORT-FOOD",
                    표시명 = "수입식품 준비",
                    검토상태코드 = 공동수입준비검토상태코드.근거수집,
                    책임역할코드 = 공동수입준비책임역할코드.수입자,
                    공식원출처Url = "https://impfood.mfds.go.kr/",
                    확인시각Utc = Now.AddDays(-1)
                }
            ],
            책임초안목록 =
            [
                Responsibility(공동수입준비책임역할코드.판매자수출자),
                Responsibility(공동수입준비책임역할코드.수입자),
                Responsibility(공동수입준비책임역할코드.관세사),
                Responsibility(공동수입준비책임역할코드.플랫폼)
            ],
            미확인항목목록 = ["관세사 품목분류 최종 검토"]
        };

    private static 공동수입책임초안 Responsibility(string role)
        => new()
        {
            역할코드 = role,
            당사자표시명 = $"{role} 담당 후보",
            책임요약 = $"{role} 책임 범위 확인",
            당사자확인여부 = false
        };

    private sealed record Fixture(
        공동수입준비ProcessManager Service,
        공동구매자동집단응답 Group,
        커뮤니티원장Dto Ledger,
        FakeLedgerStore Store);

    private sealed class FakeSourceGroupReader(
        공동구매자동집단응답 group) : I공동수입준비SourceGroupReader
    {
        public Task<공동구매자동집단응답?> 조회Async(
            string 자동집단Id,
            CancellationToken cancellationToken = default)
            => Task.FromResult(string.Equals(자동집단Id, group.자동집단Id, StringComparison.Ordinal)
                ? group
                : null);
    }

    private sealed class FakeLedgerStore(
        커뮤니티원장Dto ledger) : I공동수입준비BusinessCaseStore
    {
        private 커뮤니티원장Dto _ledger = ledger;

        public int SaveCount { get; private set; }

        public Task<커뮤니티원장Dto> 저장Async(
            커뮤니티원장저장요청 request,
            string updatedBy,
            CancellationToken cancellationToken = default)
        {
            if (request.기대Revision != _ledger.Revision)
            {
                throw new InvalidOperationException("Revision conflict");
            }

            _ledger = new 커뮤니티원장Dto
            {
                원장Id = _ledger.원장Id,
                Revision = _ledger.Revision + 1,
                커뮤니티Id = request.커뮤니티Id,
                원장템플릿Key = request.원장템플릿Key,
                제목 = request.제목,
                원함 = request.원함,
                상태 = request.상태 ?? _ledger.상태,
                현재단계Key = request.현재단계Key,
                대상OsCode = request.대상OsCode,
                대상OsName = request.대상OsName,
                생성자UserId = request.생성자UserId,
                생성자표시명 = request.생성자표시명 ?? _ledger.생성자표시명,
                블록목록 = request.블록목록,
                참여자목록 = request.참여자목록,
                포함원장목록 = request.포함원장목록 ?? [],
                다이어그램스냅샷 = request.다이어그램스냅샷,
                외부참조 = request.외부참조,
                확장속성 = request.확장속성,
                생성시각Utc = _ledger.생성시각Utc,
                수정시각Utc = Now.UtcDateTime
            };
            SaveCount++;
            return Task.FromResult(_ledger);
        }

        public Task<커뮤니티원장Dto?> 조회Async(
            string caseId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(string.Equals(caseId, _ledger.원장Id, StringComparison.Ordinal)
                ? _ledger
                : null);

        public Task<IReadOnlyList<커뮤니티원장Dto>> 목록조회Async(
            커뮤니티원장조회조건 query,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<커뮤니티원장Dto>>([_ledger]);
    }

    private sealed class FakeEvidenceBatchReader : I공동수입준비EvidenceBatchReader
    {
        public IReadOnlyList<공동구매수요모집Os배치작업응답> 조회()
            =>
            [
                new 공동구매수요모집Os배치작업응답
                {
                    작업코드 = 공동구매수요모집Os배치작업코드.Kamis일별가격수집,
                    작업명 = "KAMIS 일별 가격 근거 수집",
                    등록여부 = true,
                    Os사용활성여부 = true,
                    공유인프라여부 = true,
                    상태코드 = 공동구매수요모집Os배치상태코드.Os활성,
                    데이터출처 = "KAMIS"
                }
            ];
    }

    private sealed class StubFeatureFlags(bool enabled) : IVersionFeatureFlagService
    {
        public bool IsEnabled(string featureKey)
            => featureKey == VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow && enabled;

        public IReadOnlyDictionary<string, bool> GetAll()
            => new Dictionary<string, bool>
            {
                [VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow] = enabled
            };
    }

    private sealed class StubExecutionModePolicy(SsalddelExecutionMode mode) : ISsalddelExecutionModePolicy
    {
        public SsalddelExecutionMode Mode { get; } = mode;
        public bool IsSimulation => Mode == SsalddelExecutionMode.Simulation;
        public bool IsOperational => Mode == SsalddelExecutionMode.Operational;
    }

    private sealed class StaticOptionsMonitor<T>(T value) : IOptionsMonitor<T>
    {
        public T CurrentValue => value;
        public T Get(string? name) => value;
        public IDisposable? OnChange(Action<T, string?> listener) => null;
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
