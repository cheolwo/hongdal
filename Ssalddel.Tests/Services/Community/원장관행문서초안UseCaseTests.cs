using System.Text.Json;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Documents;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Services.Community;

namespace Ssalddel.Tests.Services.Community;

public sealed class 원장관행문서초안UseCaseTests
{
    private static readonly DateTimeOffset 기준시각 = new(2026, 7, 27, 3, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task 같이주문원장은_구매주문서와_집계표를_원장단위그대로_만든다()
    {
        var 원장 = new 커뮤니티원장Dto
        {
            원장Id = "group-order-1",
            Revision = 7,
            원장템플릿Key = CommunityLedgerTemplateKeys.GroupOrder,
            제목 = "청사과 같이 주문",
            생성자UserId = "owner-1",
            생성자표시명 = "동네 주문자",
            외부참조 = new Dictionary<string, string>
            {
                ["ProductKey"] = "apple-a",
                ["ProductName"] = "청사과"
            },
            블록목록 =
            [
                new()
                {
                    BlockId = "individual-order-aggregation",
                    Data = new Dictionary<string, string>
                    {
                        ["ConfirmedOrdererCount"] = "4",
                        ["TotalRequestedQuantity"] = "50",
                        ["QuantityUnit"] = "25kg box",
                        ["TotalReservedPaymentAmount"] = "180000",
                        ["DestinationWarehouseCount"] = "2"
                    }
                }
            ]
        };
        var useCase = UseCase(원장);

        var result = await useCase.생성Async(원장.원장Id, "owner-1");

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.운영문서여부);
        Assert.False(result.Value.외부전송가능여부);
        Assert.Equal(4, result.Value.문서목록.Count);
        Assert.Contains(result.Value.문서목록, document =>
            document.문서종류코드 == 원장관행문서종류코드.견적요청서);
        Assert.Contains(result.Value.문서목록, document =>
            document.문서종류코드 == 원장관행문서종류코드.계약검토자료서);
        var purchaseOrder = Assert.Single(result.Value.문서목록, document =>
            document.문서종류코드 == 원장관행문서종류코드.구매주문서);
        var line = Assert.Single(purchaseOrder.품목행목록);
        Assert.Equal(50m, line.수량);
        Assert.Equal("25kg box", line.수량단위);
        Assert.Null(line.단가);
        Assert.Contains("단가·통화", purchaseOrder.필수입력누락목록);
        Assert.Contains(purchaseOrder.경고목록, warning => warning.Contains("예약 결제 합계"));
        Assert.Contains("DRAFT", purchaseOrder.Html);
    }

    [Fact]
    public async Task 카탈로그는_원장별_발급주체와_기존연계모듈을_제공한다()
    {
        var 원장 = new 커뮤니티원장Dto
        {
            원장Id = "group-import-1",
            원장템플릿Key = CommunityLedgerTemplateKeys.GroupImport,
            생성자UserId = "owner-1"
        };
        var useCase = UseCase(원장);

        var result = await useCase.카탈로그조회Async(원장.원장Id, "owner-1");

        Assert.True(result.IsSuccess);
        Assert.Equal(8, result.Value.문서종류목록.Count);
        var shipmentReference = Assert.Single(result.Value.문서종류목록, item =>
            item.문서종류코드 == 원장관행문서종류코드.선적문서참조표);
        Assert.Equal(원장관행문서발급주체코드.운송사포워더, shipmentReference.발급주체코드);
        Assert.Equal(원장관행문서생성모드코드.외부발급준비자료, shipmentReference.생성모드코드);
        Assert.False(shipmentReference.외부발급원본대체가능여부);
        Assert.Contains("공동구매해외선적추적UseCase", shipmentReference.연계모듈목록);
        Assert.NotEmpty(shipmentReference.공식근거목록);
    }

    [Fact]
    public async Task 같이수입원장은_상업송장과_포장명세서에_견적과_역할근거를_투영한다()
    {
        var 준비자료 = new 같이수입준비원장저장요청
        {
            출발국가코드 = "US",
            도착국가코드 = "KR",
            기준통화코드 = "USD",
            재료품목목록 =
            [
                new()
                {
                    재료키 = "apple-a",
                    재료명 = "Fresh apples",
                    원천Hs코드 = "080810",
                    모인수요수량 = 40,
                    수량단위 = "25kg box"
                }
            ],
            책임초안목록 =
            [
                new()
                {
                    역할코드 = 같이수입준비책임역할코드.판매자수출자,
                    당사자표시명 = "Orchard Export LLC",
                    당사자확인여부 = true
                },
                new()
                {
                    역할코드 = 같이수입준비책임역할코드.수입자,
                    당사자표시명 = "Ssalddel Buyers",
                    당사자확인여부 = true
                }
            ],
            견적목록 =
            [
                new()
                {
                    견적키 = "quote-1",
                    재료키 = "apple-a",
                    통화코드 = "USD",
                    수량단위 = "25kg box",
                    단가 = 32.5m,
                    포장조건 = "40 x 25kg cartons",
                    Incoterms후보 = "FOB Seattle",
                    유효기한Utc = 기준시각.AddDays(10),
                    확인시각Utc = 기준시각.AddDays(-1)
                }
            ]
        };
        var 원장 = 같이수입원장(준비자료);
        var useCase = UseCase(원장);

        var result = await useCase.생성Async(원장.원장Id, "participant-1");

        Assert.True(result.IsSuccess);
        Assert.Equal(8, result.Value.문서목록.Count);
        var invoice = Assert.Single(result.Value.문서목록, document =>
            document.문서종류코드 == 원장관행문서종류코드.상업송장);
        var line = Assert.Single(invoice.품목행목록);
        Assert.Equal("080810", line.Hs코드);
        Assert.Equal(string.Empty, line.원산지국가코드);
        Assert.Equal(32.5m, line.단가);
        Assert.Equal(1300m, line.금액);
        Assert.Equal("USD", line.통화코드);
        Assert.Contains("원산지", invoice.필수입력누락목록);
        Assert.Contains(invoice.경고목록, warning => warning.Contains("출발 국가") && warning.Contains("원산지"));
        Assert.Equal(1300m, Assert.Single(invoice.금액합계목록).금액);

        var packingList = Assert.Single(result.Value.문서목록, document =>
            document.문서종류코드 == 원장관행문서종류코드.포장명세서);
        Assert.Contains("총중량", packingList.필수입력누락목록);
        Assert.Contains("40 x 25kg cartons", packingList.Html);

        var originData = Assert.Single(result.Value.문서목록, document =>
            document.문서종류코드 == 원장관행문서종류코드.원산지증명준비자료서);
        Assert.Equal(원장관행문서생성모드코드.외부발급준비자료, originData.생성모드코드);
        Assert.False(originData.외부발급원본대체가능여부);
        Assert.Contains(originData.경고목록, warning => warning.Contains("원산지증명서") && warning.Contains("대체하지"));

        var customsChecklist = Assert.Single(result.Value.문서목록, document =>
            document.문서종류코드 == 원장관행문서종류코드.수입통관서류점검표);
        Assert.Contains("B/L 또는 AWB 사본", customsChecklist.필수입력누락목록);
    }

    [Fact]
    public async Task 직접참여하지_않은_사용자는_문서초안을_볼수없다()
    {
        var useCase = UseCase(new 커뮤니티원장Dto
        {
            원장Id = "group-order-1",
            원장템플릿Key = CommunityLedgerTemplateKeys.GroupOrder,
            생성자UserId = "owner-1"
        });

        var result = await useCase.생성Async("group-order-1", "other-user");

        Assert.True(result.IsFailed);
        Assert.Equal(403, result.Errors[0].Metadata["StatusCode"]);
    }

    [Fact]
    public async Task 원장종류와_맞지않는_문서종류는_거부한다()
    {
        var useCase = UseCase(new 커뮤니티원장Dto
        {
            원장Id = "group-order-1",
            원장템플릿Key = CommunityLedgerTemplateKeys.GroupOrder,
            생성자UserId = "owner-1"
        });

        var result = await useCase.생성Async(
            "group-order-1",
            "owner-1",
            원장관행문서종류코드.상업송장);

        Assert.True(result.IsFailed);
        Assert.Equal(400, result.Errors[0].Metadata["StatusCode"]);
    }

    private static 원장관행문서초안UseCase UseCase(커뮤니티원장Dto 원장)
        => new(new 원장저장소Stub(원장), new 고정TimeProvider(기준시각));

    private static 커뮤니티원장Dto 같이수입원장(같이수입준비원장저장요청 준비자료)
        => new()
        {
            원장Id = "group-import-1",
            Revision = 4,
            원장템플릿Key = CommunityLedgerTemplateKeys.GroupImport,
            제목 = "사과 같이 수입 준비",
            생성자UserId = "owner-1",
            참여자목록 =
            [
                new() { UserId = "participant-1", DisplayName = "참여자", RoleLabel = "주문자" }
            ],
            블록목록 =
            [
                new()
                {
                    BlockId = "trade-readiness-request",
                    Data = new Dictionary<string, string>
                    {
                        ["Json"] = JsonSerializer.Serialize(
                            준비자료,
                            new JsonSerializerOptions(JsonSerializerDefaults.Web))
                    }
                }
            ]
        };

    private sealed class 고정TimeProvider(DateTimeOffset utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => utcNow;
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
