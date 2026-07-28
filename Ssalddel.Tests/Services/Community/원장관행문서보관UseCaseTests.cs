using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Documents;
using Ssalddel.Services.Community;
using 살뜰.Services.Documents;

namespace Ssalddel.Tests.Services.Community;

public sealed class 원장관행문서보관UseCaseTests
{
    [Fact]
    public async Task 선택한_초안을_기존_암호화문서관리Service에_보관한다()
    {
        var 원장 = new 커뮤니티원장Dto
        {
            원장Id = "group-order-1",
            Revision = 3,
            원장템플릿Key = CommunityLedgerTemplateKeys.GroupOrder,
            제목 = "감자 같이 주문",
            생성자UserId = "owner-1",
            외부참조 = new Dictionary<string, string>
            {
                ["ProductKey"] = "potato",
                ["ProductName"] = "감자"
            },
            블록목록 =
            [
                new()
                {
                    BlockId = "individual-order-aggregation",
                    Data = new Dictionary<string, string>
                    {
                        ["TotalRequestedQuantity"] = "25",
                        ["QuantityUnit"] = "25kg box"
                    }
                }
            ]
        };
        var 초안UseCase = new 원장관행문서초안UseCase(
            new 원장저장소Stub(원장),
            TimeProvider.System);
        var 문서관리 = new 문서관리ServiceStub();
        var useCase = new 원장관행문서보관UseCase(초안UseCase, 문서관리);

        var result = await useCase.보관Async(
            원장.원장Id,
            "owner-1",
            원장관행문서종류코드.구매주문서);

        Assert.True(result.IsSuccess);
        Assert.Equal(91, result.Value.저장문서Id);
        Assert.Equal(원장관행문서종류코드.구매주문서, result.Value.문서종류코드);
        Assert.Equal(원장관행문서정책코드.검토초안, 문서관리.마지막요청?.문서코드);
        Assert.Equal(원장.원장Id, 문서관리.마지막요청?.의뢰Id);
        Assert.Equal(원장.Revision, 문서관리.마지막요청?.원천원장Revision);
        Assert.Equal(원장.원장템플릿Key, 문서관리.마지막요청?.원천원장종류코드);
        Assert.Equal(원장관행문서종류코드.구매주문서, 문서관리.마지막요청?.원천문서종류코드);
        Assert.Equal(문서분류코드.거래명세, 문서관리.마지막요청?.문서분류코드);
        Assert.Equal(문서생명주기상태코드.입력필요, 문서관리.마지막요청?.생명주기상태코드);
        Assert.True(문서관리.마지막요청?.암호화여부);
        Assert.False(string.IsNullOrWhiteSpace(문서관리.마지막요청?.구조화스냅샷Json));
        using (var snapshot = System.Text.Json.JsonDocument.Parse(문서관리.마지막요청!.구조화스냅샷Json!))
        {
            Assert.False(snapshot.RootElement.TryGetProperty("Html", out _));
            Assert.False(snapshot.RootElement.TryGetProperty("PlainText", out _));
            var field = snapshot.RootElement.GetProperty("확인필드목록").EnumerateArray().First();
            Assert.False(field.TryGetProperty("값", out _));
        }
        Assert.Contains("DRAFT", 문서관리.마지막내용);
    }

    private sealed class 문서관리ServiceStub : I문서관리Service
    {
        public 문서생성요청? 마지막요청 { get; private set; }
        public string 마지막내용 { get; private set; } = string.Empty;

        public async Task<문서조회요약응답?> CreateDocumentAsync(
            문서생성요청 request,
            Stream content,
            CancellationToken cancellationToken = default)
        {
            마지막요청 = request;
            using var reader = new StreamReader(content);
            마지막내용 = await reader.ReadToEndAsync(cancellationToken);
            return new 문서조회요약응답
            {
                Id = 91,
                의뢰Id = request.의뢰Id,
                문서코드 = request.문서코드,
                문서명 = request.문서명,
                파일명 = request.파일명,
                생성상태 = 문서상태값.생성완료,
                문서분류코드 = request.문서분류코드 ?? string.Empty,
                생명주기상태코드 = request.생명주기상태코드 ?? string.Empty,
                내용Sha256 = "TEST-HASH",
                암호화됨 = request.암호화여부 == true,
                다운로드허용여부 = request.다운로드허용여부 == true
            };
        }

        public Task<IReadOnlyList<문서정책요약응답>> GetPoliciesAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<문서정책요약응답>>([]);

        public Task<문서정책요약응답?> UpdatePolicyAsync(
            string 문서코드,
            문서정책수정요청 request,
            CancellationToken cancellationToken = default)
            => Task.FromResult<문서정책요약응답?>(null);

        public Task<IReadOnlyList<문서조회요약응답>> ListDocumentsAsync(
            string? 문서코드 = null,
            string? 의뢰Id = null,
            string? 생성상태 = null,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<문서조회요약응답>>([]);

        public Task<문서관계그래프응답> GetRelationshipGraphAsync(
            string 기준StableId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(new 문서관계그래프응답 { 기준StableId = 기준StableId });

        public Task<IReadOnlyList<문서조회로그요약응답>> ListLogsAsync(
            long? 문서Id = null,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<문서조회로그요약응답>>([]);

        public Task<문서조회요약응답?> TransitionLifecycleAsync(
            long id,
            문서생명주기변경요청 request,
            CancellationToken cancellationToken = default)
            => Task.FromResult<문서조회요약응답?>(null);

        public Task<문서다운로드응답?> DownloadAsync(long id, CancellationToken cancellationToken = default)
            => Task.FromResult<문서다운로드응답?>(null);

        public Task SeedDefaultsAsync(CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class 원장저장소Stub(커뮤니티원장Dto 원장) : I커뮤니티원장저장소
    {
        public Task<커뮤니티원장Dto?> 원장조회Async(
            string 원장Id,
            CancellationToken cancellationToken = default)
            => Task.FromResult<커뮤니티원장Dto?>(
                string.Equals(원장.원장Id, 원장Id, StringComparison.OrdinalIgnoreCase) ? 원장 : null);

        public Task<IReadOnlyList<커뮤니티원장Dto>> 원장목록조회Async(
            커뮤니티원장조회조건 query,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<커뮤니티원장Dto>>([원장]);

        public Task<커뮤니티원장Dto> 원장저장Async(
            커뮤니티원장저장요청 request,
            string updatedBy,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<커뮤니티원장Dto?> 원장상태변경Async(
            커뮤니티원장상태변경요청 request,
            string updatedBy,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
