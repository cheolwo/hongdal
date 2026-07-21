using FluentResults;
using Ssalddel.Application.Mart;
using Ssalddel.Application.Sales;
using Ssalddel.Contracts.Common.Sales;
using Ssalddel.Contracts.Mart;
using 살뜰.Services.Audit;
using 살뜰.Services.Sales;

namespace Ssalddel.Tests.Application.Sales;

public sealed class 판매페이지UseCaseTests
{
    [Fact]
    public async Task 공개상품완료근거는_서버재조회값으로만판매초안에전달된다()
    {
        var mart = new FakeMartReadUseCase(new 마트공개상품상세응답
        {
            Id = 41,
            상품명 = "서버가 확인한 감자",
            구매근거 = new 마트공개상품구매근거응답
            {
                완료원장확인여부 = true,
                공개후기수 = 7,
                근거기준시각Utc = new DateTime(2026, 7, 21, 10, 0, 0, DateTimeKind.Utc),
                공개범위안내 = "비식별 공개 투영"
            }
        });
        var service = new RecordingSalesPageService();
        var useCase = new 판매페이지UseCase(service, new NoOpActivityLog(), mart);

        var result = await useCase.초안생성Async(
            new 판매페이지초안생성요청
            {
                원본공개상품Id = 41,
                판매자표시명 = "판매자",
                상품명 = "편집 가능한 이름"
            },
            Context(),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(41, mart.LastProductId);
        Assert.NotNull(service.VerifiedEvidence);
        Assert.Equal("서버가 확인한 감자", service.VerifiedEvidence.원본공개상품명);
        Assert.Equal(7, service.VerifiedEvidence.공개후기수);
        Assert.True(service.VerifiedEvidence.완료원장확인여부);
    }

    [Fact]
    public async Task 완료되지않은공개상품은_판매초안저장전에차단한다()
    {
        var mart = new FakeMartReadUseCase(new 마트공개상품상세응답
        {
            Id = 41,
            상품명 = "진행 중 상품",
            구매근거 = new 마트공개상품구매근거응답
            {
                완료원장확인여부 = false,
                원장근거상태 = "구매 원장 완료 확인 전"
            }
        });
        var service = new RecordingSalesPageService();
        var useCase = new 판매페이지UseCase(service, new NoOpActivityLog(), mart);

        var result = await useCase.초안생성Async(
            new 판매페이지초안생성요청
            {
                원본공개상품Id = 41,
                판매자표시명 = "판매자",
                상품명 = "진행 중 상품"
            },
            Context(),
            CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.Contains("완료된 구매 원장", result.Errors.Single().Message);
        Assert.Equal(0, service.CreateCallCount);
    }

    private static 판매채널요청Context Context()
        => new("web", "seller-1", "판매자", "Seller", "/sales", "trace", "127.0.0.1", "test");

    private sealed class FakeMartReadUseCase(마트공개상품상세응답 response)
        : I마트공개상품조회UseCase
    {
        public long? LastProductId { get; private set; }

        public Task<Result<마트공개상품목록응답>> 목록Async(
            마트공개상품목록조회요청 request,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<Result<마트공개상품상세응답>> 상세Async(
            long productId,
            CancellationToken cancellationToken)
        {
            LastProductId = productId;
            return Task.FromResult(Result.Ok(response));
        }
    }

    private sealed class RecordingSalesPageService : I판매페이지Service
    {
        public int CreateCallCount { get; private set; }
        public 판매페이지공개구매근거Dto? VerifiedEvidence { get; private set; }

        public Task<판매페이지초안목록응답> 초안목록Async(
            string ownerUserId,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<판매페이지초안응답?> 초안조회Async(
            string pageId,
            string ownerUserId,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<판매페이지초안응답> 초안생성Async(
            판매페이지초안생성요청 request,
            string ownerUserId,
            CancellationToken cancellationToken,
            판매페이지공개구매근거Dto? verifiedPublicEvidence = null)
        {
            CreateCallCount++;
            VerifiedEvidence = verifiedPublicEvidence;
            return Task.FromResult(new 판매페이지초안응답
            {
                페이지Id = "sales-page-1",
                상품명 = request.상품명,
                판매자표시명 = request.판매자표시명,
                공개구매근거 = verifiedPublicEvidence
            });
        }

        public Task<판매페이지초안응답> 초안수정Async(
            string pageId,
            판매페이지초안수정요청 request,
            string ownerUserId,
            CancellationToken cancellationToken)
            => throw new NotSupportedException();
    }

    private sealed class NoOpActivityLog : I사용자행위로그Service
    {
        public Task 기록Async(
            사용자행위로그기록 entry,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
