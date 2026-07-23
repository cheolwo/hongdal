using Ssalddel.Contracts.Common.Community;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Contracts.Common.Warehouse;
using Ssalddel.Services.Community;
using Ssalddel.Services.Orderer;

namespace Ssalddel.Tests.Services.Orderer;

public sealed class 공동구매자동집단화철회UseCaseTests
{
    [Fact]
    public async Task 기대Revision철회는_개별원함을먼저닫고_닫힌원장을투영한다()
    {
        var sequence = new List<string>();
        var ledgerService = new WithdrawalLedgerService(sequence);
        var projection = new WithdrawalProjection(sequence);
        var store = new WithdrawalStore(sequence, withdrawalCompleted: true);
        var useCase = CreateUseCase(store, ledgerService, projection);
        var command = Withdrawal(expectedRevision: 4);

        var result = await useCase.수요철회Async(command);

        Assert.True(result.성공);
        Assert.Equal(["individual-ledger-close", "projection"], sequence);
        Assert.Equal(4, ledgerService.LastCommand!.개별원함기대Revision);
        Assert.Equal(커뮤니티원장상태.닫힘, projection.ProjectedLedger!.상태);
        Assert.Equal(5, projection.ProjectedLedger.Revision);
        Assert.Equal(0, store.WithdrawalCount);
        Assert.Equal("individual-demand-ledger-1", result.값!.개별원함원장Id);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Legacy철회는_자동집단철회가완료된경우에만_개별원함을닫는다(
        bool withdrawalCompleted)
    {
        var sequence = new List<string>();
        var ledgerService = new WithdrawalLedgerService(sequence);
        var store = new WithdrawalStore(sequence, withdrawalCompleted);
        var useCase = CreateUseCase(store, ledgerService);

        var result = await useCase.수요철회Async(Withdrawal(expectedRevision: null));

        Assert.True(result.성공);
        Assert.Equal(1, store.WithdrawalCount);
        Assert.Equal(withdrawalCompleted, ledgerService.WithdrawalCount == 1);
        Assert.Equal(
            withdrawalCompleted
                ? ["automatic-group-withdraw", "individual-ledger-close"]
                : ["automatic-group-withdraw"],
            sequence);
        Assert.Equal(
            withdrawalCompleted ? "individual-demand-ledger-1" : string.Empty,
            result.값!.개별원함원장Id);
    }

    private static 공동구매자동집단화UseCase CreateUseCase(
        WithdrawalStore store,
        WithdrawalLedgerService ledgerService,
        I공동구매개별원함자동집단투영Service? projection = null)
        => new(
            store,
            new NoopReceivingWarehouseService(),
            ledgerService,
            new NoopIndividualOrderLedgerService(),
            new 공동구매주문자집단화Engine(),
            원함투영Service: projection);

    private static 공동구매자동수요철회Command Withdrawal(long? expectedRevision)
        => new()
        {
            요청멱등키 = "withdraw-1",
            수요출처키 = "demand:garlic:orderer-1",
            주문자키 = "orderer-1",
            개별원함기대Revision = expectedRevision,
            철회사유 = "더 이상 필요하지 않음"
        };

    private sealed class WithdrawalProjection(List<string> sequence)
        : I공동구매개별원함자동집단투영Service
    {
        public 커뮤니티원장Dto? ProjectedLedger { get; private set; }

        public bool 투영대상(커뮤니티원장Dto ledger) => true;

        public Task<공동구매개별원함자동집단투영결과> 투영Async(
            커뮤니티원장Dto ledger,
            CancellationToken cancellationToken = default)
        {
            sequence.Add("projection");
            ProjectedLedger = ledger;
            return Task.FromResult(new 공동구매개별원함자동집단투영결과(
                null,
                new 공동구매자동수요철회응답
                {
                    요청멱등키 = "projection-withdraw-1",
                    수요출처키 = ledger.외부참조["DemandSourceKey"],
                    자동집단Id = ledger.외부참조["AutomaticGroupId"],
                    철회완료 = true
                }));
        }
    }

    private sealed class WithdrawalLedgerService(List<string> sequence)
        : I공동구매개별원함원장Service
    {
        public int WithdrawalCount { get; private set; }
        public 공동구매자동수요철회Command? LastCommand { get; private set; }

        public Task<공동구매개별원함원장결과?> 철회Async(
            공동구매자동수요철회Command command,
            CancellationToken cancellationToken = default)
        {
            sequence.Add("individual-ledger-close");
            WithdrawalCount++;
            LastCommand = command;
            var ledger = new 커뮤니티원장Dto
            {
                원장Id = "individual-demand-ledger-1",
                Revision = 5,
                원장템플릿Key = CommunityLedgerTemplateKeys.IndividualDemand,
                상태 = 커뮤니티원장상태.닫힘,
                생성자UserId = command.주문자키,
                외부참조 = new Dictionary<string, string>
                {
                    ["DemandSourceKey"] = command.수요출처키,
                    ["AutomaticGroupId"] = "auto-group-1"
                },
                확장속성 = new Dictionary<string, string>
                {
                    ["ProjectionMode"] = "NonBindingAutomaticGroup"
                }
            };
            return Task.FromResult<공동구매개별원함원장결과?>(
                new 공동구매개별원함원장결과(
                    ledger.원장Id,
                    ledger.Revision,
                    ledger));
        }

        public Task<공동구매개별원함원장결과> 저장Async(
            공동구매자동수요등록Command command,
            string 자동집단Id,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class WithdrawalStore(
        List<string> sequence,
        bool withdrawalCompleted) : I공동구매자동집단화저장소
    {
        public int WithdrawalCount { get; private set; }

        public Task<공동구매자동수요철회응답> 수요철회Async(
            공동구매자동수요철회Command command,
            CancellationToken cancellationToken = default)
        {
            sequence.Add("automatic-group-withdraw");
            WithdrawalCount++;
            return Task.FromResult(new 공동구매자동수요철회응답
            {
                요청멱등키 = command.요청멱등키,
                수요출처키 = command.수요출처키,
                자동집단Id = "auto-group-1",
                철회완료 = withdrawalCompleted
            });
        }

        public Task<공동구매자동집단응답> 수요등록Async(
            공동구매자동수요등록Command command,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<공동구매자동집단응답>> 집단목록조회Async(
            공동구매자동집단조회조건 조건,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<공동구매자동집단응답?> 집단조회Async(
            string 자동집단Id,
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

    private sealed class NoopReceivingWarehouseService : I공동구매수령창고Service
    {
        public Task<공동구매수령창고배정결과> 확보Async(
            공동구매자동수요등록Command command,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }

    private sealed class NoopIndividualOrderLedgerService : I공동구매개별주문원장Service
    {
        public Task<공동구매개별주문원장연결결과> 생성및연결Async(
            공동구매자동집단응답 group,
            공동구매자동수요응답 demand,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
