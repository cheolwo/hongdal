using Ssalddel.Application.Sales;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Sales;
using Ssalddel.Services.Community;

namespace Ssalddel.Tests.Application.Sales;

public sealed class 해외판매자식품시설UseCaseTests
{
    [Fact]
    public async Task 일반해외제조업소는_수출국증빙과국내수입자가확인되면_한국수입준비완료다()
    {
        var useCase = new 해외판매자식품시설UseCase(new FakeLedgerStore());
        var request = CompleteRequest();

        var result = await useCase.저장Async("china-fruit-01", request, "seller-1", false);

        Assert.True(result.IsSuccess);
        Assert.Equal(한국수입식품절차코드.해외제조업소등록, result.Value.적용절차코드);
        Assert.True(result.Value.시설등록준비완료여부);
        Assert.True(result.Value.한국수입준비완료여부);
        Assert.False(result.Value.외부신고발생여부);
        Assert.Equal("Simulation", result.Value.실행모드);
    }

    [Fact]
    public async Task 일반해외제조업소는_수출국증빙이없으면_준비완료가아니다()
    {
        var useCase = new 해외판매자식품시설UseCase(new FakeLedgerStore());
        var request = CompleteRequest();
        request.증빙목록.Clear();

        var result = await useCase.저장Async("us-snack-01", request, "seller-1", false);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.시설등록준비완료여부);
        Assert.Contains(result.Value.차단사유목록, x => x.Contains("수출국이 발급한", StringComparison.Ordinal));
    }

    [Fact]
    public async Task 축산물은_일반해외제조업소가아니라_수출국정부해외작업장경로를적용한다()
    {
        var useCase = new 해외판매자식품시설UseCase(new FakeLedgerStore());
        var request = CompleteRequest();
        request.생산품목코드목록 = [해외판매자식품시설품목코드.축산물];
        request.증빙목록 =
        [
            new 해외판매자식품시설증빙요청
            {
                문서Id = "government-route-01",
                문서명 = "Exporting government establishment confirmation",
                증빙유형 = 해외판매자식품시설증빙유형코드.정부경로확인,
                언어코드 = "en"
            }
        ];

        var result = await useCase.저장Async("us-meat-01", request, "seller-1", false);

        Assert.True(result.IsSuccess);
        Assert.Equal(
            한국수입식품절차코드.축산물해외작업장수출국정부신청,
            result.Value.적용절차코드);
        Assert.True(result.Value.시설등록준비완료여부);
        Assert.Contains("수출국 정부", result.Value.다음조치, StringComparison.Ordinal);
    }

    [Fact]
    public async Task 다른판매자는_시설준비원장을조회하거나변경할수없다()
    {
        var store = new FakeLedgerStore();
        var useCase = new 해외판매자식품시설UseCase(store);
        var saved = await useCase.저장Async("china-tea-01", CompleteRequest(), "seller-1", false);

        var read = await useCase.조회Async("china-tea-01", "seller-2", false);
        var update = await useCase.저장Async(
            "china-tea-01",
            CompleteRequest().WithRevision(saved.Value.Revision),
            "seller-2",
            false);

        Assert.True(read.IsFailed);
        Assert.True(update.IsFailed);
        Assert.Equal(403, read.Errors[0].Metadata["StatusCode"]);
        Assert.Equal(403, update.Errors[0].Metadata["StatusCode"]);
    }

    private static 해외판매자식품시설저장요청 CompleteRequest()
        => new()
        {
            판매자업체명 = "Fresh Export LLC",
            판매자국가코드 = "US",
            판매자현지등록번호 = "US-1234",
            판매자담당자명 = "Alex Kim",
            판매자이메일 = "alex@example.com",
            판매자전화번호 = "+1-555-0100",
            판매자가시설운영자인가 = true,
            시설명 = "Fresh Export LLC",
            시설대표자명 = "Alex Kim",
            시설주소 = "100 Food Road, California, USA",
            시설국가코드 = "US",
            시설전화번호 = "+1-555-0100",
            시설이메일 = "facility@example.com",
            생산품목코드목록 = [해외판매자식품시설품목코드.가공식품],
            업종코드목록 = [해외판매자식품시설업종코드.식품첨가물제조가공],
            국내수입업체명 = "살뜰수입 주식회사",
            국내수입업체주소 = "서울특별시",
            국내수입업체전화번호 = "02-0000-0000",
            국내수입업체이메일 = "importer@example.kr",
            국내수입식품영업등록번호 = "IMPORT-001",
            국내수입업체확인여부 = true,
            현지실사동의여부 = true,
            정보진실성확인여부 = true,
            시설운영자동의여부 = true,
            증빙목록 =
            [
                new 해외판매자식품시설증빙요청
                {
                    문서Id = "permit-01",
                    문서명 = "State food facility permit",
                    증빙유형 = 해외판매자식품시설증빙유형코드.수출국허가등록증빙,
                    발급기관 = "State authority",
                    언어코드 = "en"
                }
            ]
        };

    private sealed class FakeLedgerStore : I커뮤니티원장저장소
    {
        private readonly Dictionary<string, 커뮤니티원장Dto> ledgers =
            new(StringComparer.OrdinalIgnoreCase);

        public Task<커뮤니티원장Dto> 원장저장Async(
            커뮤니티원장저장요청 request,
            string updatedBy,
            CancellationToken cancellationToken = default)
        {
            ledgers.TryGetValue(request.원장Id!, out var existing);
            var currentRevision = existing?.Revision ?? 0;
            if (request.기대Revision.HasValue && request.기대Revision.Value != currentRevision)
            {
                throw new InvalidOperationException("revision conflict");
            }

            var saved = new 커뮤니티원장Dto
            {
                원장Id = request.원장Id!,
                Revision = currentRevision + 1,
                커뮤니티Id = request.커뮤니티Id,
                원장템플릿Key = request.원장템플릿Key,
                제목 = request.제목,
                상태 = request.상태 ?? 커뮤니티원장상태.초안,
                현재단계Key = request.현재단계Key,
                대상OsCode = request.대상OsCode,
                대상OsName = request.대상OsName,
                생성자UserId = request.생성자UserId,
                생성자표시명 = request.생성자표시명 ?? "판매자",
                참여자목록 = request.참여자목록,
                블록목록 = request.블록목록,
                외부참조 = request.외부참조,
                확장속성 = request.확장속성,
                생성시각Utc = existing?.생성시각Utc ?? DateTime.UtcNow,
                수정시각Utc = DateTime.UtcNow
            };
            ledgers[saved.원장Id] = saved;
            return Task.FromResult(saved);
        }

        public Task<커뮤니티원장Dto?> 원장조회Async(
            string 원장Id,
            CancellationToken cancellationToken = default)
            => Task.FromResult(ledgers.GetValueOrDefault(원장Id));

        public Task<IReadOnlyList<커뮤니티원장Dto>> 원장목록조회Async(
            커뮤니티원장조회조건 query,
            CancellationToken cancellationToken = default)
        {
            var items = ledgers.Values
                .Where(x => string.IsNullOrWhiteSpace(query.원장템플릿Key)
                            || string.Equals(
                                x.원장템플릿Key,
                                query.원장템플릿Key,
                                StringComparison.OrdinalIgnoreCase))
                .Where(x => string.IsNullOrWhiteSpace(query.접근UserId)
                            || string.Equals(x.생성자UserId, query.접근UserId, StringComparison.Ordinal)
                            || x.참여자목록.Any(p =>
                                string.Equals(p.UserId, query.접근UserId, StringComparison.Ordinal)))
                .Take(query.Limit)
                .ToArray();
            return Task.FromResult<IReadOnlyList<커뮤니티원장Dto>>(items);
        }

        public Task<커뮤니티원장Dto?> 원장상태변경Async(
            커뮤니티원장상태변경요청 request,
            string updatedBy,
            CancellationToken cancellationToken = default)
            => Task.FromResult<커뮤니티원장Dto?>(null);
    }
}

internal static class 해외판매자식품시설테스트요청확장
{
    public static 해외판매자식품시설저장요청 WithRevision(
        this 해외판매자식품시설저장요청 request,
        long revision)
    {
        request.기대Revision = revision;
        return request;
    }
}
