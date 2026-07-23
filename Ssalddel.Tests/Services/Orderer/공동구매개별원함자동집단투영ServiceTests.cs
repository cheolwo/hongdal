using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Services.Community;
using Ssalddel.Services.Orderer;

namespace Ssalddel.Tests.Services.Orderer;

public sealed class 공동구매개별원함자동집단투영ServiceTests
{
    [Fact]
    public async Task 활성원함은_Revision멱등키로_비구속수요를저장한뒤_원장참조를연결한다()
    {
        var sequence = new List<string>();
        var store = new ProjectionStore(sequence);
        var os = new ProjectionDemandOs(store, sequence);
        var service = new 공동구매개별원함자동집단투영Service(store, os);
        var ledger = IndividualDemandLedger(revision: 7);

        var result = await service.투영Async(ledger);

        Assert.NotNull(result.자동집단);
        Assert.Null(result.철회);
        Assert.Equal(["save", "link"], sequence);
        var command = Assert.Single(os.RegistrationCommands);
        Assert.StartsWith("wish-projection-save:", command.요청멱등키);
        Assert.Equal("demand:garlic:orderer-1", command.수요출처키);
        Assert.Equal(공동구매자동수요유형코드.관심표시, command.수요유형);
        Assert.Equal(공동구매자동결제상태코드.미결제, command.결제상태);
        Assert.Equal(공동구매자동수요물류방식코드.후속검토, command.물류방식);
        Assert.Equal(
            ledger.원장Id,
            Assert.Single(result.자동집단!.수요목록).개별원함원장Id);
    }

    [Fact]
    public async Task 같은Revision을_재처리하면_같은멱등키와_한수요를사용하고_원장연결을중복하지않는다()
    {
        var store = new ProjectionStore();
        var os = new ProjectionDemandOs(store);
        var service = new 공동구매개별원함자동집단투영Service(store, os);
        var ledger = IndividualDemandLedger(revision: 11);

        var first = await service.투영Async(ledger);
        var retry = await service.투영Async(IndividualDemandLedger(revision: 11));
        await service.투영Async(IndividualDemandLedger(revision: 12));

        Assert.Equal(3, os.RegistrationCommands.Count);
        Assert.Equal(
            os.RegistrationCommands[0].요청멱등키,
            os.RegistrationCommands[1].요청멱등키);
        Assert.NotEqual(
            os.RegistrationCommands[1].요청멱등키,
            os.RegistrationCommands[2].요청멱등키);
        Assert.Equal(2, os.ProcessedRegistrationKeyCount);
        Assert.Equal(1, store.LinkCount);
        Assert.Single(first.자동집단!.수요목록);
        Assert.Same(first.자동집단, retry.자동집단);
    }

    [Fact]
    public async Task 닫힌원함은_같은원함주체의_자동집단수요를철회한다()
    {
        var ledger = IndividualDemandLedger(
            revision: 9,
            state: 커뮤니티원장상태.닫힘);
        var store = new ProjectionStore();
        store.Group = GroupWithDemand(ledger);
        var os = new ProjectionDemandOs(store);
        var service = new 공동구매개별원함자동집단투영Service(store, os);

        var result = await service.투영Async(ledger);

        Assert.Same(store.Group, result.자동집단);
        Assert.NotNull(result.철회);
        var withdrawal = result.철회!;
        var command = Assert.Single(os.WithdrawalCommands);
        Assert.StartsWith("wish-projection-withdraw:", command.요청멱등키);
        Assert.Equal("demand:garlic:orderer-1", command.수요출처키);
        Assert.Equal("orderer-1", command.주문자키);
        Assert.True(withdrawal.철회완료);
        Assert.Equal(ledger.원장Id, withdrawal.개별원함원장Id);
    }

    [Fact]
    public async Task 개별원함_자동집단투영대상이아니면_아무것도변경하지않는다()
    {
        var store = new ProjectionStore();
        var os = new ProjectionDemandOs(store);
        var service = new 공동구매개별원함자동집단투영Service(store, os);
        var ledger = IndividualDemandLedger(revision: 1);
        ledger.확장속성 = new Dictionary<string, string>
        {
            ["ProjectionMode"] = "ManualOnly"
        };

        var result = await service.투영Async(ledger);

        Assert.Null(result.자동집단);
        Assert.Null(result.철회);
        Assert.Empty(os.RegistrationCommands);
        Assert.Empty(os.WithdrawalCommands);
        Assert.Equal(0, store.LinkCount);
        Assert.Equal(0, store.GroupReadCount);
    }

    private static 커뮤니티원장Dto IndividualDemandLedger(
        long revision,
        string state = 커뮤니티원장상태.진행중)
        => new()
        {
            원장Id = "individual-demand-ledger-1",
            Revision = revision,
            커뮤니티Id = "platform",
            원장템플릿Key = CommunityLedgerTemplateKeys.IndividualDemand,
            상태 = state,
            생성자UserId = "orderer-1",
            생성자표시명 = "주문자 1",
            외부참조 = new Dictionary<string, string>
            {
                ["DemandSourceKey"] = "demand:garlic:orderer-1",
                ["AutomaticGroupId"] = "auto-group-1",
                ["ProductKey"] = "garlic",
                ["ProductName"] = "마늘",
                ["TemperatureCode"] = "상온",
                ["TransactionType"] = 공동구매거래유형코드.B2C,
                ["PriceBasis"] = 공동구매가격표시기준코드.부가세포함,
                ["OrdererDisplayName"] = "주문자 1",
                ["DeliveryScopeKey"] = "delivery:kr:04000",
                ["DeliveryScopeName"] = "서울 04000",
                ["DesiredQuantity"] = "3",
                ["QuantityUnit"] = "kg"
            },
            확장속성 = new Dictionary<string, string>
            {
                ["ProjectionMode"] = "NonBindingAutomaticGroup"
            },
            생성시각Utc = new DateTime(2026, 7, 23, 1, 0, 0, DateTimeKind.Utc),
            수정시각Utc = new DateTime(2026, 7, 23, 2, 0, 0, DateTimeKind.Utc)
        };

    private static 공동구매자동집단응답 GroupWithDemand(커뮤니티원장Dto ledger)
        => new()
        {
            자동집단Id = ledger.외부참조["AutomaticGroupId"],
            상품키 = ledger.외부참조["ProductKey"],
            상품명 = ledger.외부참조["ProductName"],
            수요목록 =
            [
                new 공동구매자동수요응답
                {
                    수요Id = "demand-1",
                    수요출처키 = ledger.외부참조["DemandSourceKey"],
                    주문자키 = ledger.생성자UserId!,
                    희망수량 = 3,
                    수량단위 = "kg"
                }
            ]
        };

    private sealed class ProjectionDemandOs(
        ProjectionStore store,
        List<string>? sequence = null) : I공동구매수요모집OS
    {
        private readonly HashSet<string> _processedRegistrationKeys = new(StringComparer.Ordinal);

        public List<공동구매자동수요등록Command> RegistrationCommands { get; } = [];
        public List<공동구매자동수요철회Command> WithdrawalCommands { get; } = [];
        public int ProcessedRegistrationKeyCount => _processedRegistrationKeys.Count;

        public Task<공동구매자동집단응답> 수요등록조율Async(
            공동구매자동수요등록Command command,
            CancellationToken cancellationToken = default)
        {
            sequence?.Add("save");
            RegistrationCommands.Add(command);
            if (_processedRegistrationKeys.Add(command.요청멱등키) && store.Group is null)
            {
                store.Group = new 공동구매자동집단응답
                {
                    자동집단Id = "auto-group-1",
                    상품키 = command.상품키,
                    상품명 = command.상품명,
                    수요목록 =
                    [
                        new 공동구매자동수요응답
                        {
                            수요Id = "demand-1",
                            수요출처키 = command.수요출처키,
                            주문자키 = command.주문자키,
                            희망수량 = command.희망수량,
                            수량단위 = command.수량단위
                        }
                    ]
                };
            }

            return Task.FromResult(store.Group!);
        }

        public Task<공동구매자동수요철회응답> 수요철회조율Async(
            공동구매자동수요철회Command command,
            CancellationToken cancellationToken = default)
        {
            WithdrawalCommands.Add(command);
            return Task.FromResult(new 공동구매자동수요철회응답
            {
                요청멱등키 = command.요청멱등키,
                수요출처키 = command.수요출처키,
                자동집단Id = store.Group?.자동집단Id ?? string.Empty,
                철회완료 = true,
                철회시각Utc = DateTime.UtcNow
            });
        }

        public Task<공동구매수요모집Os조율응답> 집단조율Async(
            string 자동집단Id,
            string 트리거코드,
            DateTime? 기준시각Utc = null,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<공동구매수요모집마감스캔응답> 모집마감스캔Async(
            DateTime? 기준시각Utc = null,
            int? 최대건수 = null,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<공동구매수요모집Os상태응답?> 운영상태조회Async(
            string 자동집단Id,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<공동구매수요모집인계승인응답> 인계승인Async(
            string 자동집단Id,
            공동구매수요모집인계승인요청 요청,
            string 승인자키,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<공동구매수요모집Os상태응답> 후속원장연결Async(
            string 자동집단Id,
            string 인계요청Id,
            string 대상원장Id,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class ProjectionStore(
        List<string>? sequence = null) : I공동구매자동집단화저장소
    {
        public 공동구매자동집단응답? Group { get; set; }
        public int LinkCount { get; private set; }
        public int GroupReadCount { get; private set; }

        public Task<공동구매자동집단응답?> 집단조회Async(
            string 자동집단Id,
            CancellationToken cancellationToken = default)
        {
            GroupReadCount++;
            return Task.FromResult(Group);
        }

        public Task<공동구매자동집단응답> 개별원함원장연결Async(
            string 자동집단Id,
            string 수요Id,
            string 개별원함원장Id,
            CancellationToken cancellationToken = default)
        {
            sequence?.Add("link");
            LinkCount++;
            var demand = Group!.수요목록.Single(item => item.수요Id == 수요Id);
            demand.개별원함원장Id = 개별원함원장Id;
            return Task.FromResult(Group);
        }

        public Task<공동구매자동집단응답> 수요등록Async(
            공동구매자동수요등록Command command,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<공동구매자동수요철회응답> 수요철회Async(
            공동구매자동수요철회Command command,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<공동구매자동집단응답>> 집단목록조회Async(
            공동구매자동집단조회조건 조건,
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
