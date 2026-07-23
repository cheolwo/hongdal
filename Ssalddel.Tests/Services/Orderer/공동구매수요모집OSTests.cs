using Microsoft.Extensions.Options;
using Ssalddel.Contracts.Common.Orderer;
using Ssalddel.Services.Orderer;
using 살뜰.Services.Options;
using 살뜰.Services.Versioning;

namespace Ssalddel.Tests.Services.Orderer;

public sealed class 공동구매수요모집OSTests
{
    private static readonly DateTime 기준시각Utc = new(2026, 7, 23, 1, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task 수요등록은_저장뒤_Batching정책으로_운영큐를조율한다()
    {
        var store = new StubDemandStore();
        var port = new RecordingStateTransitionPort
        {
            StateByGroupId = { ["auto-group-1"] = 공동구매자동집단상태코드.확정대기 }
        };
        var os = CreateOs(store, port);
        var command = new 공동구매자동수요등록Command
        {
            요청멱등키 = "demand-command-1",
            상품키 = "garlic",
            상품명 = "마늘",
            배송권키 = "seoul-mapogu",
            주문자키 = "orderer-1",
            희망수량 = 3
        };

        var result = await os.수요등록조율Async(command);

        Assert.Equal(1, store.RegisterCount);
        Assert.Equal(공동구매자동집단상태코드.확정대기, result.현재상태);
        var call = Assert.Single(port.CoordinationCalls);
        Assert.Equal(공동구매수요모집Os트리거코드.수요변경, call.TriggerCode);
        Assert.Equal("DemandChanged:demand-command-1", call.IdempotencyKey);
        Assert.Equal([공동구매수요모집Os정책코드.수요집단화묶음], call.PolicyCodes);
    }

    [Fact]
    public async Task 마감스캔은_EDF와_Aging으로_검토큐와종료큐를분리한다()
    {
        var port = new RecordingStateTransitionPort
        {
            DueGroupIds = ["ready-group", "closed-group"],
            StateByGroupId =
            {
                ["ready-group"] = 공동구매자동집단상태코드.확정대기,
                ["closed-group"] = 공동구매자동집단상태코드.모집종료목표미달
            }
        };
        var os = CreateOs(new StubDemandStore(), port);

        var result = await os.모집마감스캔Async(기준시각Utc, 10);

        Assert.Equal(2, result.조회건수);
        Assert.Equal(2, result.조율건수);
        Assert.Equal(1, result.확정검토건수);
        Assert.Equal(1, result.모집종료건수);
        Assert.Equal(0, result.실패건수);
        Assert.All(port.CoordinationCalls, call =>
        {
            Assert.Equal(공동구매수요모집Os트리거코드.모집마감점검, call.TriggerCode);
            Assert.Contains(공동구매수요모집Os정책코드.모집마감우선, call.PolicyCodes);
            Assert.Contains(공동구매수요모집Os정책코드.장기모집정체보정, call.PolicyCodes);
        });
    }

    [Fact]
    public async Task 사람의인계승인은_Simulation과_후속기능정지를_상태전이Port에전달한다()
    {
        var port = new RecordingStateTransitionPort();
        var os = CreateOs(
            new StubDemandStore(),
            port,
            customsWorkflowEnabled: false,
            mode: SsalddelExecutionMode.Simulation);

        var result = await os.인계승인Async(
            "auto-group-1",
            new 공동구매수요모집인계승인요청
            {
                요청멱등키 = "approve-1",
                승인사유 = "모집 목표와 비구속 참여 의사를 확인함"
            },
            "admin-1");

        Assert.False(result.이미처리됨);
        Assert.Equal("Simulation", port.LastApprovalMode);
        Assert.False(port.LastApprovalNextWorkflowEnabled);
        Assert.Equal("admin-1", port.LastApproverId);
        Assert.Equal(공동구매수요모집인계상태코드.승인후속대기, result.운영상태.인계상태);
    }

    [Fact]
    public async Task 승인된인계는_1점5_대상원장Id를_상태전이Port에연결한다()
    {
        var port = new RecordingStateTransitionPort();
        var os = CreateOs(
            new StubDemandStore(),
            port,
            customsWorkflowEnabled: true);

        var result = await os.후속원장연결Async(
            "auto-group-1",
            "handoff-1",
            "group-import-ledger-1");

        Assert.Equal("handoff-1", port.LastLinkedHandoffId);
        Assert.Equal("group-import-ledger-1", port.LastLinkedLedgerId);
        Assert.Equal("group-import-ledger-1", result.대상원장Id);
    }

    private static 공동구매수요모집OS CreateOs(
        StubDemandStore store,
        RecordingStateTransitionPort port,
        bool customsWorkflowEnabled = false,
        SsalddelExecutionMode mode = SsalddelExecutionMode.Simulation)
        => new(
            store,
            port,
            new StubFeatureFlags(customsWorkflowEnabled),
            new StubExecutionModePolicy(mode),
            new FixedTimeProvider(new DateTimeOffset(기준시각Utc)),
            new StaticOptionsMonitor<GroupPurchaseDemandOsOptions>(new GroupPurchaseDemandOsOptions
            {
                BatchSize = 100,
                AgingReviewHours = 24
            }));

    private sealed class StubDemandStore : I공동구매자동집단화저장소
    {
        public int RegisterCount { get; private set; }

        public Task<공동구매자동집단응답> 수요등록Async(
            공동구매자동수요등록Command command,
            CancellationToken cancellationToken = default)
        {
            RegisterCount++;
            return Task.FromResult(Group("auto-group-1", 공동구매자동집단상태코드.수요수집중));
        }

        public Task<공동구매자동수요철회응답> 수요철회Async(
            공동구매자동수요철회Command command,
            CancellationToken cancellationToken = default)
            => Task.FromResult(new 공동구매자동수요철회응답
            {
                자동집단Id = "auto-group-1",
                요청멱등키 = command.요청멱등키,
                수요출처키 = command.수요출처키,
                철회완료 = true
            });

        public Task<IReadOnlyList<공동구매자동집단응답>> 집단목록조회Async(
            공동구매자동집단조회조건 조건,
            CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<공동구매자동집단응답>>([]);

        public Task<공동구매자동집단응답?> 집단조회Async(
            string 자동집단Id,
            CancellationToken cancellationToken = default)
            => Task.FromResult<공동구매자동집단응답?>(Group(자동집단Id, 공동구매자동집단상태코드.수요수집중));

        public Task<공동구매자동집단응답> 개별원함원장연결Async(
            string 자동집단Id,
            string 수요Id,
            string 개별원함원장Id,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Group(자동집단Id, 공동구매자동집단상태코드.수요수집중));

        public Task<공동구매자동집단응답> 개별주문원장연결Async(
            string 자동집단Id,
            string 수요Id,
            string 공동구매주문집계원장Id,
            string 개별주문원장Id,
            string 입고예정원장Id,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Group(자동집단Id, 공동구매자동집단상태코드.수요수집중));
    }

    private sealed class RecordingStateTransitionPort : I공동구매수요모집Os상태전이Port
    {
        public List<string> DueGroupIds { get; init; } = [];
        public Dictionary<string, string> StateByGroupId { get; init; } = new(StringComparer.Ordinal);
        public List<CoordinationCall> CoordinationCalls { get; } = [];
        public string LastApprovalMode { get; private set; } = string.Empty;
        public bool LastApprovalNextWorkflowEnabled { get; private set; }
        public string LastApproverId { get; private set; } = string.Empty;
        public string LastLinkedHandoffId { get; private set; } = string.Empty;
        public string LastLinkedLedgerId { get; private set; } = string.Empty;

        public Task<공동구매수요모집Os조율응답> 운영조율Async(
            string 자동집단Id,
            string 트리거코드,
            string 조율멱등키,
            IReadOnlyList<string> 정책코드목록,
            DateTime 기준시각Utc,
            TimeSpan 장기모집점검주기,
            string 실행모드,
            bool 후속워크플로우활성여부,
            CancellationToken cancellationToken)
        {
            CoordinationCalls.Add(new CoordinationCall(
                자동집단Id,
                트리거코드,
                조율멱등키,
                정책코드목록.ToArray()));
            var state = StateByGroupId.GetValueOrDefault(
                자동집단Id,
                공동구매자동집단상태코드.수요수집중);
            return Task.FromResult(new 공동구매수요모집Os조율응답
            {
                집단 = Group(자동집단Id, state),
                운영상태 = OperatingState(자동집단Id, state, 실행모드, 후속워크플로우활성여부)
            });
        }

        public Task<IReadOnlyList<string>> 운영점검대상조회Async(
            DateTime 기준시각Utc,
            int 최대건수,
            CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<string>>(DueGroupIds.Take(최대건수).ToArray());

        public Task<공동구매수요모집Os상태응답?> 운영상태조회Async(
            string 자동집단Id,
            CancellationToken cancellationToken)
            => Task.FromResult<공동구매수요모집Os상태응답?>(OperatingState(
                자동집단Id,
                StateByGroupId.GetValueOrDefault(자동집단Id, 공동구매자동집단상태코드.수요수집중),
                "Simulation",
                false));

        public Task<공동구매수요모집인계승인응답> 인계승인Async(
            string 자동집단Id,
            공동구매수요모집인계승인요청 요청,
            string 승인자키,
            DateTime 승인시각Utc,
            string 실행모드,
            bool 후속워크플로우활성여부,
            CancellationToken cancellationToken)
        {
            LastApprovalMode = 실행모드;
            LastApprovalNextWorkflowEnabled = 후속워크플로우활성여부;
            LastApproverId = 승인자키;
            return Task.FromResult(new 공동구매수요모집인계승인응답
            {
                요청멱등키 = 요청.요청멱등키,
                집단 = Group(자동집단Id, 공동구매자동집단상태코드.확정),
                운영상태 = new 공동구매수요모집Os상태응답
                {
                    자동집단Id = 자동집단Id,
                    집단상태 = 공동구매자동집단상태코드.확정,
                    현재큐 = 공동구매수요모집Os큐코드.인계준비,
                    인계상태 = 공동구매수요모집인계상태코드.승인후속대기,
                    실행모드 = 실행모드,
                    시뮬레이션여부 = 실행모드 == "Simulation",
                    후속워크플로우활성여부 = 후속워크플로우활성여부
                }
            });
        }

        public Task<공동구매수요모집Os상태응답> 후속원장연결Async(
            string 자동집단Id,
            string 인계요청Id,
            string 대상원장Id,
            DateTime 연결시각Utc,
            CancellationToken cancellationToken)
        {
            LastLinkedHandoffId = 인계요청Id;
            LastLinkedLedgerId = 대상원장Id;
            return Task.FromResult(new 공동구매수요모집Os상태응답
            {
                자동집단Id = 자동집단Id,
                인계상태 = 공동구매수요모집인계상태코드.승인후속대기,
                인계요청Id = 인계요청Id,
                대상원장Id = 대상원장Id,
                후속워크플로우활성여부 = true
            });
        }
    }

    private sealed record CoordinationCall(
        string GroupId,
        string TriggerCode,
        string IdempotencyKey,
        IReadOnlyList<string> PolicyCodes);

    private sealed class StubFeatureFlags(bool customsWorkflowEnabled) : IVersionFeatureFlagService
    {
        public bool IsEnabled(string featureKey)
            => featureKey == VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow
               && customsWorkflowEnabled;

        public IReadOnlyDictionary<string, bool> GetAll()
            => new Dictionary<string, bool>
            {
                [VersionFeatureFlagKeys.CustomsAndTradeDataWorkflow] = customsWorkflowEnabled
            };
    }

    private sealed class StubExecutionModePolicy(SsalddelExecutionMode mode) : ISsalddelExecutionModePolicy
    {
        public SsalddelExecutionMode Mode { get; } = mode;
        public bool IsSimulation => Mode == SsalddelExecutionMode.Simulation;
        public bool IsOperational => Mode == SsalddelExecutionMode.Operational;
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class StaticOptionsMonitor<T>(T value) : IOptionsMonitor<T>
    {
        public T CurrentValue => value;
        public T Get(string? name) => value;
        public IDisposable? OnChange(Action<T, string?> listener) => null;
    }

    private static 공동구매자동집단응답 Group(string groupId, string state)
        => new()
        {
            자동집단Id = groupId,
            현재상태 = state
        };

    private static 공동구매수요모집Os상태응답 OperatingState(
        string groupId,
        string state,
        string mode,
        bool nextWorkflowEnabled)
        => new()
        {
            자동집단Id = groupId,
            집단상태 = state,
            현재큐 = state switch
            {
                공동구매자동집단상태코드.확정대기 => 공동구매수요모집Os큐코드.확정검토,
                공동구매자동집단상태코드.모집종료목표미달 => 공동구매수요모집Os큐코드.모집종료,
                공동구매자동집단상태코드.확정 => 공동구매수요모집Os큐코드.인계준비,
                _ => 공동구매수요모집Os큐코드.모집중
            },
            실행모드 = mode,
            시뮬레이션여부 = mode == "Simulation",
            후속워크플로우활성여부 = nextWorkflowEnabled
        };
}
