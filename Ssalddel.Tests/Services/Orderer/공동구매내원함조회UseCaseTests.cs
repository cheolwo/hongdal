using System.Text.Json;
using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Services.Community;
using Ssalddel.Services.Orderer;

namespace Ssalddel.Tests.Services.Orderer;

public sealed class 공동구매내원함조회UseCaseTests
{
    [Fact]
    public async Task 활성과닫힌_본인원함만_연결집단의공개요약과함께_반환한다()
    {
        var activeLedger = 원함원장(
            ledgerId: "wish-ledger-active",
            ownerId: "orderer-1",
            state: 커뮤니티원장상태.진행중,
            sourceKey: "ingredient:garlic:seoul",
            groupId: "auto-group-active",
            productKey: "garlic",
            productName: "마늘",
            quantity: "12.5");
        var closedLedger = 원함원장(
            ledgerId: "wish-ledger-closed",
            ownerId: "orderer-1",
            state: 커뮤니티원장상태.닫힘,
            sourceKey: "ingredient:onion:seoul",
            groupId: "auto-group-closed",
            productKey: "onion",
            productName: "양파",
            quantity: "4");
        var participantButNotOwner = 원함원장(
            ledgerId: "wish-ledger-other",
            ownerId: "other-orderer",
            state: 커뮤니티원장상태.진행중,
            sourceKey: "ingredient:pepper:seoul",
            groupId: "auto-group-other",
            productKey: "pepper",
            productName: "고추",
            quantity: "7");
        participantButNotOwner.참여자목록 =
        [
            new 커뮤니티원장참여자Dto { UserId = "orderer-1" }
        ];
        var unsupportedDraft = 원함원장(
            ledgerId: "wish-ledger-draft",
            ownerId: "orderer-1",
            state: 커뮤니티원장상태.초안,
            sourceKey: "ingredient:radish:seoul",
            groupId: "auto-group-draft",
            productKey: "radish",
            productName: "무",
            quantity: "3");
        var importLedger = new 커뮤니티원장Dto
        {
            원장Id = "group-import-ledger-1",
            원장템플릿Key = CommunityLedgerTemplateKeys.GroupImport,
            포함원장목록 =
            [
                new 커뮤니티포함원장참조Dto
                {
                    원장Id = "group-purchase-ledger-1"
                }
            ],
            수정시각Utc = new DateTime(2026, 7, 23, 8, 0, 0, DateTimeKind.Utc)
        };
        var ledgerStore = new StubLedgerStore(
            [activeLedger, closedLedger, participantButNotOwner, unsupportedDraft],
            [importLedger]);
        var groupStore = new StubGroupStore(
            new Dictionary<string, 공동구매자동집단응답>(StringComparer.Ordinal)
            {
                ["auto-group-active"] = new()
                {
                    자동집단Id = "auto-group-active",
                    공동구매주문집계원장Id = "group-purchase-ledger-1",
                    상품키 = "garlic",
                    상품명 = "마늘",
                    현재상태 = 공동구매자동집단상태코드.수요수집중,
                    수요건수 = 3,
                    참여자수 = 2,
                    총희망수량 = 20m,
                    수량단위 = "kg",
                    목표수량 = 30m,
                    수요목록 =
                    [
                        new 공동구매자동수요응답
                        {
                            개별원함원장Id = "wish-ledger-active",
                            수요출처키 = "ingredient:garlic:seoul",
                            주문자키 = "orderer-1",
                            목표참여자수 = 7,
                            목표수량 = 110m,
                            공동구매주문집계원장Id = "group-purchase-ledger-1",
                            개별주문원장Id = "individual-order-ledger-1"
                        },
                        new 공동구매자동수요응답
                        {
                            주문자키 = "other-orderer",
                            주문자표시명 = "다른 주문자"
                        }
                    ]
                },
                ["auto-group-closed"] = new()
                {
                    자동집단Id = "auto-group-closed",
                    상품키 = "onion",
                    상품명 = "양파",
                    수요건수 = 1,
                    참여자수 = 1,
                    총희망수량 = 4m,
                    수량단위 = "kg"
                }
            });
        var useCase = new 공동구매내원함조회UseCase(ledgerStore, groupStore);

        var response = await useCase.조회Async("orderer-1");

        Assert.Equal(2, response.전체건수);
        Assert.Equal(1, response.활성건수);
        Assert.Equal(1, response.닫힘건수);
        var active = response.원함목록[0];
        Assert.Equal("wish-ledger-active", active.개별원함원장Id);
        Assert.Equal(공동구매내원함상태코드.활성, active.원함상태);
        Assert.Equal("garlic", active.상품키);
        Assert.Equal(12.5m, active.희망수량);
        Assert.Equal(4, active.Revision);
        Assert.Equal("B2B", active.거래유형);
        Assert.Equal(공동구매가격표시기준코드.부가세별도, active.가격표시기준);
        Assert.Equal("강남 배송권", active.배송권명);
        Assert.Equal("group-purchase-ledger-1", active.공동구매주문집계원장Id);
        Assert.Equal("individual-order-ledger-1", active.개별주문원장Id);
        Assert.Equal("group-import-ledger-1", active.같이수입원장Id);
        Assert.Equal(7, active.목표참여자수);
        Assert.Equal(110m, active.목표수량);
        Assert.Equal(20m, active.자동집단요약!.총희망수량);
        Assert.Equal(30m, active.자동집단요약.목표수량);
        Assert.Equal(
            공동구매내원함상태코드.닫힘,
            response.원함목록[1].원함상태);
        Assert.DoesNotContain(
            "wish-ledger-other",
            response.원함목록.Select(item => item.개별원함원장Id));
        Assert.DoesNotContain("other-orderer", JsonSerializer.Serialize(response), StringComparison.Ordinal);
        Assert.DoesNotContain("다른 주문자", JsonSerializer.Serialize(response), StringComparison.Ordinal);

        Assert.Equal("orderer-1", ledgerStore.WishQuery!.접근UserId);
        Assert.Equal(
            CommunityLedgerTemplateKeys.IndividualDemand,
            ledgerStore.WishQuery.원장템플릿Key);
        Assert.Contains(
            "group-purchase-ledger-1",
            ledgerStore.GroupImportQuery!.포함원장Ids);
    }

    [Fact]
    public async Task 주문자키가없으면_저장소를조회하지않는다()
    {
        var ledgerStore = new StubLedgerStore([], []);
        var useCase = new 공동구매내원함조회UseCase(
            ledgerStore,
            new StubGroupStore(new Dictionary<string, 공동구매자동집단응답>()));

        await Assert.ThrowsAsync<ArgumentException>(() => useCase.조회Async(" "));

        Assert.Null(ledgerStore.WishQuery);
    }

    private static 커뮤니티원장Dto 원함원장(
        string ledgerId,
        string ownerId,
        string state,
        string sourceKey,
        string groupId,
        string productKey,
        string productName,
        string quantity)
        => new()
        {
            원장Id = ledgerId,
            Revision = 4,
            원장템플릿Key = CommunityLedgerTemplateKeys.IndividualDemand,
            생성자UserId = ownerId,
            상태 = state,
            외부참조 = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["DemandSourceKey"] = sourceKey,
                ["AutomaticGroupId"] = groupId,
                ["ProductKey"] = productKey,
                ["ProductName"] = productName,
                ["DesiredQuantity"] = quantity,
                ["QuantityUnit"] = "kg",
                ["DeliveryScopeKey"] = "scope-gangnam",
                ["DeliveryScopeName"] = "강남 배송권",
                ["TemperatureCode"] = "냉장",
                ["LogisticsMode"] = 공동구매자동수요물류방식코드.후속검토,
                ["TransactionType"] = 공동구매거래유형코드.B2B,
                ["PriceBasis"] = 공동구매가격표시기준코드.부가세별도,
                ["PurchasingOrganizationReference"] = "buyer-org-1",
                ["PurchasingOrganizationName"] = "테스트 구매조직",
                ["TaxInvoiceRequired"] = bool.TrueString
            },
            생성시각Utc = new DateTime(2026, 7, 20, 8, 0, 0, DateTimeKind.Utc),
            수정시각Utc = state == 커뮤니티원장상태.진행중
                ? new DateTime(2026, 7, 23, 9, 0, 0, DateTimeKind.Utc)
                : new DateTime(2026, 7, 22, 9, 0, 0, DateTimeKind.Utc)
        };

    private sealed class StubLedgerStore(
        IReadOnlyList<커뮤니티원장Dto> wishLedgers,
        IReadOnlyList<커뮤니티원장Dto> groupImportLedgers) : I커뮤니티원장저장소
    {
        public 커뮤니티원장조회조건? WishQuery { get; private set; }
        public 커뮤니티원장조회조건? GroupImportQuery { get; private set; }

        public Task<IReadOnlyList<커뮤니티원장Dto>> 원장목록조회Async(
            커뮤니티원장조회조건 query,
            CancellationToken cancellationToken = default)
        {
            if (string.Equals(
                    query.원장템플릿Key,
                    CommunityLedgerTemplateKeys.IndividualDemand,
                    StringComparison.Ordinal))
            {
                WishQuery = query;
                return Task.FromResult(wishLedgers);
            }

            GroupImportQuery = query;
            return Task.FromResult(groupImportLedgers);
        }

        public Task<커뮤니티원장Dto?> 원장조회Async(
            string 원장Id,
            CancellationToken cancellationToken = default)
            => Task.FromResult<커뮤니티원장Dto?>(null);

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

    private sealed class StubGroupStore(
        IReadOnlyDictionary<string, 공동구매자동집단응답> groups)
        : I공동구매자동집단화저장소
    {
        public Task<공동구매자동집단응답?> 집단조회Async(
            string 자동집단Id,
            CancellationToken cancellationToken = default)
            => Task.FromResult(groups.GetValueOrDefault(자동집단Id));

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
