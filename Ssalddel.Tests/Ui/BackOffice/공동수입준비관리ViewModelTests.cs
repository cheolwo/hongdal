using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Ui.Common.Areas.BackOffice.Services;
using Ssalddel.Ui.Common.Areas.BackOffice.ViewModels;

namespace Ssalddel.Tests.Ui.BackOffice;

public sealed class 공동수입준비관리ViewModelTests
{
    [Fact]
    public async Task 초기화는_인계대상을선택하고_1_5체크리스트초안을구성한다()
    {
        var client = new FakeClient();
        var viewModel = new 공동수입준비관리ViewModel(client);

        await viewModel.초기화Async("운영 관리자");

        Assert.True(viewModel.초기화됨);
        Assert.Equal("group-1", viewModel.선택집단?.자동집단Id);
        Assert.Equal(5, viewModel.초안.예상비용목록.Count);
        Assert.Single(viewModel.초안.재료품목목록);
        Assert.Equal("ingredient-sauce", viewModel.초안.재료품목목록[0].재료키);
        Assert.Equal(
            [공동수입준비국제운송방식코드.Lcl, 공동수입준비국제운송방식코드.Fcl],
            viewModel.초안.국제운송검토.방식후보목록);
        Assert.Single(viewModel.초안.품목분류후보목록);
        Assert.Single(viewModel.초안.국가별검토항목목록);
        Assert.Equal(4, viewModel.초안.책임초안목록.Count);
        Assert.False(viewModel.인계승인됨);
        Assert.False(viewModel.저장가능);
        Assert.Contains(viewModel.추가가능재료집단목록, group => group.자동집단Id == "group-2");
        Assert.DoesNotContain(viewModel.추가가능재료집단목록, group => group.자동집단Id == "group-b2b");
        Assert.Equal(1, client.OperatingStatusReadCount);
        Assert.Equal(1, client.ReadinessReadCount);
    }

    [Fact]
    public async Task 승인된수요재료를_같은준비묶음에추가하고_포워더인계와회신을별도로기록한다()
    {
        var client = new FakeClient();
        var viewModel = new 공동수입준비관리ViewModel(client);
        await viewModel.초기화Async("운영 관리자");
        viewModel.승인사유 = "기준 수요 승인";
        await viewModel.인계승인Async();

        viewModel.추가재료집단Id = "group-2";
        await viewModel.재료집단추가Async();
        viewModel.초안.포워더인계.전달대상업체명 = "회신 포워더";
        viewModel.포워더인계상태변경(공동수입준비포워더인계상태코드.인계기록됨);
        viewModel.포워더회신방식변경(공동수입준비국제운송방식코드.Fcl);

        Assert.Equal(2, viewModel.초안.재료품목목록.Count);
        Assert.Contains(viewModel.초안.재료품목목록, item => item.재료키 == "ingredient-spice");
        Assert.Equal(2, viewModel.초안.품목분류후보목록.Count);
        Assert.Equal(공동수입준비국제운송방식코드.Fcl, viewModel.초안.국제운송검토.포워더제안방식코드);
        Assert.Equal(공동수입준비국제운송검토상태코드.포워더회신완료, viewModel.초안.국제운송검토.검토상태코드);
        Assert.Equal("회신 포워더", viewModel.초안.국제운송검토.회신업체표시명);
        Assert.Equal("운영 관리자", viewModel.초안.국제운송검토.회신기록자표시명);
        Assert.Equal(공동수입준비포워더인계상태코드.회신기록됨, viewModel.초안.포워더인계.인계상태코드);
    }

    [Fact]
    public async Task 승인후_초안을저장하고_수정시Revision과새멱등키를사용한다()
    {
        var client = new FakeClient();
        var viewModel = new 공동수입준비관리ViewModel(client);
        await viewModel.초기화Async("운영 관리자");
        viewModel.승인사유 = "모집 목표와 공개 수요 근거를 확인했습니다.";

        await viewModel.인계승인Async();
        await viewModel.미리보기Async();
        await viewModel.저장Async();
        viewModel.초안.재료명 = "수정된 상품명";
        viewModel.초안변경됨();
        await viewModel.저장Async();

        Assert.True(viewModel.인계승인됨, viewModel.메시지);
        Assert.Equal(2, client.SaveKeys.Count);
        Assert.NotEqual(client.SaveKeys[0], client.SaveKeys[1]);
        Assert.Null(client.ExpectedRevisions[0]);
        Assert.Equal(1, client.ExpectedRevisions[1]);
        Assert.NotNull(viewModel.저장원장);
        Assert.False(viewModel.저장원장.평가.계약서명가능);
        Assert.False(viewModel.저장원장.평가.결제가능);
        Assert.False(viewModel.저장원장.평가.신고실행가능);
        Assert.False(viewModel.저장원장.평가.운송지시가능);
    }

    [Fact]
    public async Task 후속기능이꺼져있으면_승인과원장호출을열지않는다()
    {
        var client = new FakeClient(featureEnabled: false);
        var viewModel = new 공동수입준비관리ViewModel(client);

        await viewModel.초기화Async("운영 관리자");
        viewModel.승인사유 = "승인 사유";

        Assert.False(viewModel.후속기능활성);
        Assert.False(viewModel.인계승인가능);
        Assert.False(viewModel.미리보기가능);
        Assert.False(viewModel.저장가능);
        Assert.Equal(0, client.ReadinessReadCount);
        Assert.Contains("비활성", viewModel.메시지, StringComparison.Ordinal);
    }

    [Fact]
    public async Task 저장된원장만_Os점검과전문검토인계에사용하고_Revision을동기화한다()
    {
        var client = new FakeClient();
        var viewModel = new 공동수입준비관리ViewModel(client);
        await viewModel.초기화Async("운영 관리자");
        viewModel.승인사유 = "모집 근거 확인";
        await viewModel.인계승인Async();
        await viewModel.저장Async();

        viewModel.초안.재료명 = "미저장 변경";
        viewModel.초안변경됨();
        Assert.False(viewModel.Os작업실행가능);

        await viewModel.저장Async();
        var revisionBeforeOs = viewModel.저장원장!.Revision;
        await viewModel.Os작업실행Async(공동수입준비Os작업코드.전체준비점검, 재시도여부: false);

        Assert.Equal(1, client.OsRunCount);
        Assert.Equal(revisionBeforeOs + 1, viewModel.저장원장.Revision);
        viewModel.전문검토수신자 = "검토 관세사";
        viewModel.전문검토범위 = "HSK와 수입 규제";
        viewModel.전문검토인계메모 = "공식 근거 확인 요청";
        await viewModel.전문검토인계Async();

        Assert.Equal(1, client.QualifiedHandoffCount);
        Assert.Equal("검토 관세사", viewModel.준비Os상태?.전문검토인계기록?.검토수신자표시명);
        Assert.False(viewModel.저장원장.평가.계약서명가능);
    }

    private sealed class FakeClient(bool featureEnabled = true) : I공동수입준비관리Client
    {
        private 공동구매수요모집Os상태응답 _state = new()
        {
            자동집단Id = "group-1",
            집단상태 = 공동구매자동집단상태코드.확정대기,
            인계상태 = 공동구매수요모집인계상태코드.승인대기,
            실행모드 = "Simulation",
            시뮬레이션여부 = true,
            후속워크플로우활성여부 = featureEnabled
        };

        public int OperatingStatusReadCount { get; private set; }
        public int ReadinessReadCount { get; private set; }
        public List<string> SaveKeys { get; } = [];
        public List<long?> ExpectedRevisions { get; } = [];
        public int OsRunCount { get; private set; }
        public int QualifiedHandoffCount { get; private set; }

        public Task<IReadOnlyList<공동구매자동집단요약응답>> 작업대목록조회Async(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<공동구매자동집단요약응답>>
            ([
                new 공동구매자동집단요약응답
                {
                    자동집단Id = "group-1",
                    상품키 = "ingredient-sauce",
                    상품명 = "간편식 소스",
                    HS코드 = "2106.90",
                    배송권명 = "서울 모집권",
                    현재상태 = 공동구매자동집단상태코드.확정대기,
                    총희망수량 = 1_800m,
                    수량단위 = "kg",
                    참여자수 = 18
                },
                new 공동구매자동집단요약응답
                {
                    자동집단Id = "group-2",
                    상품키 = "ingredient-spice",
                    상품명 = "혼합 향신료",
                    HS코드 = "2106.90",
                    배송권명 = "서울 모집권",
                    현재상태 = 공동구매자동집단상태코드.확정,
                    총희망수량 = 720m,
                    수량단위 = "kg",
                    참여자수 = 12
                },
                new 공동구매자동집단요약응답
                {
                    자동집단Id = "group-b2b",
                    상품키 = "ingredient-business-spice",
                    상품명 = "사업용 향신료",
                    HS코드 = "2106.90",
                    거래유형 = 공동구매거래유형코드.B2B,
                    가격표시기준 = 공동구매가격표시기준코드.부가세별도,
                    배송권명 = "서울 모집권",
                    현재상태 = 공동구매자동집단상태코드.확정,
                    총희망수량 = 2_000m,
                    수량단위 = "kg",
                    참여자수 = 3
                }
            ]);

        public Task<공동구매수요모집Os상태응답?> 운영상태조회Async(string 자동집단Id, CancellationToken cancellationToken = default)
        {
            OperatingStatusReadCount++;
            return Task.FromResult<공동구매수요모집Os상태응답?>(_state);
        }

        public Task<공동구매수요모집인계승인응답> 인계승인Async(string 자동집단Id, 공동구매수요모집인계승인요청 요청, CancellationToken cancellationToken = default)
        {
            _state = new 공동구매수요모집Os상태응답
            {
                자동집단Id = 자동집단Id,
                집단상태 = 공동구매자동집단상태코드.확정,
                인계상태 = 공동구매수요모집인계상태코드.승인후속대기,
                인계요청Id = "handoff-1",
                승인자키 = "admin-1",
                승인시각Utc = DateTime.UtcNow,
                실행모드 = "Simulation",
                시뮬레이션여부 = true,
                후속워크플로우활성여부 = true
            };
            return Task.FromResult(new 공동구매수요모집인계승인응답
            {
                요청멱등키 = 요청.요청멱등키,
                운영상태 = _state,
                안내 = "인계 승인을 기록했습니다."
            });
        }

        public Task<공동수입준비원장응답?> 준비원장조회Async(string 자동집단Id, CancellationToken cancellationToken = default)
        {
            ReadinessReadCount++;
            return Task.FromResult<공동수입준비원장응답?>(null);
        }

        public Task<공동수입준비원장응답> 미리보기Async(string 자동집단Id, 공동수입준비원장저장요청 요청, CancellationToken cancellationToken = default)
            => Task.FromResult(Response(요청, revision: 0));

        public Task<공동수입준비원장응답> 저장Async(string 자동집단Id, 공동수입준비원장저장요청 요청, CancellationToken cancellationToken = default)
        {
            SaveKeys.Add(요청.요청멱등키);
            ExpectedRevisions.Add(요청.기대Revision);
            return Task.FromResult(Response(요청, SaveKeys.Count));
        }

        public Task<공동수입준비Os상태응답?> 준비Os상태조회Async(string 자동집단Id, CancellationToken cancellationToken = default)
            => Task.FromResult<공동수입준비Os상태응답?>(OsResponse(SaveKeys.Count));

        public Task<공동수입준비Os상태응답> 준비Os작업실행Async(string 자동집단Id, 공동수입준비Os작업실행요청 요청, CancellationToken cancellationToken = default)
        {
            OsRunCount++;
            return Task.FromResult(OsResponse((요청.기대Revision ?? 0) + 1));
        }

        public Task<공동수입준비Os상태응답> 전문검토인계Async(string 자동집단Id, 공동수입준비Os전문검토인계요청 요청, CancellationToken cancellationToken = default)
        {
            QualifiedHandoffCount++;
            var response = OsResponse((요청.기대Revision ?? 0) + 1);
            response.전문검토인계기록 = new 공동수입준비Os전문검토인계기록
            {
                검토수신자표시명 = 요청.검토수신자표시명,
                검토범위 = 요청.검토범위,
                인계메모 = 요청.인계메모,
                인계시각Utc = DateTimeOffset.UtcNow
            };
            return Task.FromResult(response);
        }

        private static 공동수입준비Os상태응답 OsResponse(long revision)
            => new()
            {
                자동집단Id = "group-1",
                원장Id = "trade-readiness-1",
                원장Revision = revision,
                기능활성여부 = true,
                OsWorker활성여부 = true,
                실행모드 = "Simulation",
                시뮬레이션여부 = true,
                상태코드 = 공동수입준비Os상태코드.전문검토인계준비,
                전문검토인계가능 = true
            };

        private static 공동수입준비원장응답 Response(공동수입준비원장저장요청 request, long revision)
            => new()
            {
                원장Id = "trade-readiness-1",
                Revision = revision,
                자동집단Id = "group-1",
                준비자료 = request,
                평가 = new 공동수입준비원장평가응답
                {
                    계약서명가능 = false,
                    결제가능 = false,
                    신고실행가능 = false,
                    운송지시가능 = false,
                    차단사유목록 = ["공급자 근거 필요"]
                }
            };
    }
}
