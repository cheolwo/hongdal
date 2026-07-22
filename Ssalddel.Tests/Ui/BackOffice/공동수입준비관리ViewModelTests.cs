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
        Assert.Single(viewModel.초안.품목분류후보목록);
        Assert.Single(viewModel.초안.국가별검토항목목록);
        Assert.Equal(4, viewModel.초안.책임초안목록.Count);
        Assert.False(viewModel.인계승인됨);
        Assert.False(viewModel.저장가능);
        Assert.Equal(1, client.OperatingStatusReadCount);
        Assert.Equal(1, client.ReadinessReadCount);
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
