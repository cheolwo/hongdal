using System.Text.Json;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Services.Orderer;

namespace Ssalddel.Tests.Services.Orderer;

public sealed class 같이수입준비주문자조회UseCaseTests
{
    [Fact]
    public async Task 참여주문자는_경로원장과자동집단이일치할때만_준비자료를조회한다()
    {
        var group = Group();
        var readiness = new 같이수입준비원장응답
        {
            원장Id = "group-import-ledger-1",
            Revision = 17,
            자동집단Id = group.자동집단Id,
            상품키 = "internal-product-key",
            상품명 = "쌀",
            거래문맥 = new 공동구매거래문맥응답
            {
                거래유형 = 공동구매거래유형코드.B2C,
                원천거래문맥원장Id = "internal-context-ledger"
            },
            원천수요목록 =
            [
                new 같이수입준비원천수요응답
                {
                    자동집단Id = "internal-source-group",
                    인계요청Id = "internal-handoff-request",
                    재료키 = "material-rice",
                    재료명 = "쌀",
                    모인수요수량 = 20,
                    수량단위 = "kg"
                }
            ],
            준비자료 = new 같이수입준비원장저장요청
            {
                요청멱등키 = "internal-idempotency",
                기대Revision = 16,
                공급자근거목록 =
                [
                    new 같이수입공급자근거
                    {
                        공급자후보키 = "internal-supplier-key",
                        조직명 = "공개 공급자",
                        국가코드 = "US",
                        원출처명 = "공식 출처",
                        원출처Url = "https://example.gov/supplier",
                        검토자표시명 = "private-reviewer"
                    }
                ],
                포워더인계 = new 같이수입준비포워더인계
                {
                    전달대상업체키 = "internal-forwarder-key",
                    전달대상업체명 = "공개 포워더",
                    정보제공동의확인여부 = true,
                    정보제공동의근거참조 = "private-consent-evidence",
                    인계기록자표시명 = "private-recorder"
                }
            }
        };
        var readinessService = new FakeReadinessService(readiness);
        var useCase = new 같이수입준비주문자조회UseCase(
            new FakeGroupStore(group),
            readinessService);

        var result = await useCase.조회Async(
            readiness.원장Id,
            group.자동집단Id,
            "orderer-1");

        var response = Assert.IsType<같이수입준비주문자조회응답>(result);
        Assert.Equal("쌀", response.상품명);
        Assert.Equal(20, Assert.Single(response.재료집계목록).모인수요수량);
        Assert.Equal("공개 공급자", Assert.Single(response.공급자근거목록).조직명);
        Assert.Equal(해외구매통관목적코드.개인자가사용, response.통관목적안내.수입목적코드);
        Assert.True(response.통관목적안내.개인통관고유부호입력대상);
        Assert.True(response.포워더인계.운영자기록정보제공조건확인여부);
        var json = JsonSerializer.Serialize(response);
        Assert.DoesNotContain("internal-", json, StringComparison.Ordinal);
        Assert.DoesNotContain("private-", json, StringComparison.Ordinal);
        Assert.DoesNotContain("Revision", json, StringComparison.Ordinal);
        Assert.DoesNotContain("동의근거", json, StringComparison.Ordinal);
        Assert.Equal(1, readinessService.ReadCount);
    }

    [Fact]
    public async Task 비참여주문자는_원장존재여부를알수없고_준비자료도조회하지않는다()
    {
        var group = Group();
        var readinessService = new FakeReadinessService(new 같이수입준비원장응답
        {
            원장Id = "group-import-ledger-1"
        });
        var useCase = new 같이수입준비주문자조회UseCase(
            new FakeGroupStore(group),
            readinessService);

        var result = await useCase.조회Async(
            "group-import-ledger-1",
            group.자동집단Id,
            "other-orderer");

        Assert.Null(result);
        Assert.Equal(0, readinessService.ReadCount);
    }

    [Fact]
    public async Task 자동집단에연결된원장과_경로원장이다르면_조회하지않는다()
    {
        var group = Group();
        var readinessService = new FakeReadinessService(new 같이수입준비원장응답
        {
            원장Id = "group-import-ledger-actual"
        });
        var useCase = new 같이수입준비주문자조회UseCase(
            new FakeGroupStore(group),
            readinessService);

        var result = await useCase.조회Async(
            "group-import-ledger-other",
            group.자동집단Id,
            "orderer-1");

        Assert.Null(result);
    }

    private static 공동구매자동집단응답 Group()
        => new()
        {
            자동집단Id = "auto-group-rice-kr-seoul",
            수요목록 =
            [
                new 공동구매자동수요응답
                {
                    수요Id = "demand-1",
                    주문자키 = "orderer-1"
                }
            ]
        };

    private sealed class FakeReadinessService(같이수입준비원장응답? readiness) : I같이수입준비원장Service
    {
        public int ReadCount { get; private set; }

        public Task<같이수입준비원장응답?> 조회Async(
            string 자동집단Id,
            CancellationToken cancellationToken = default)
        {
            ReadCount++;
            return Task.FromResult(readiness);
        }

        public Task<같이수입준비원장응답> 미리보기Async(
            string 자동집단Id,
            같이수입준비원장저장요청 request,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<같이수입준비원장응답> 저장Async(
            string 자동집단Id,
            같이수입준비원장저장요청 request,
            string actorUserId,
            string actorDisplayName,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class FakeGroupStore(공동구매자동집단응답 group) : I공동구매자동집단화저장소
    {
        public Task<공동구매자동집단응답?> 집단조회Async(
            string 자동집단Id,
            CancellationToken cancellationToken = default)
            => Task.FromResult(string.Equals(group.자동집단Id, 자동집단Id, StringComparison.Ordinal)
                ? group
                : null);

        public Task<IReadOnlyList<공동구매자동집단응답>> 집단목록조회Async(
            공동구매자동집단조회조건 조건,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<공동구매자동집단응답> 수요등록Async(
            공동구매자동수요등록Command command,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<공동구매자동수요철회응답> 수요철회Async(
            공동구매자동수요철회Command command,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<공동구매자동집단응답> 개별원함원장연결Async(
            string 자동집단Id,
            string 수요Id,
            string 개별원함원장Id,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<공동구매자동집단응답> 개별주문원장연결Async(
            string 자동집단Id,
            string 수요Id,
            string 공동구매주문집계원장Id,
            string 개별주문원장Id,
            string 입고예정원장Id,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
