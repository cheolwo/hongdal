using MudBlazor;

namespace HongdalAdmin.Services;

public sealed class FakePaymentSettlementSimulationService
{
    private readonly object _gate = new();
    private readonly List<FakePaymentSettlementScenario> _scenarios = [];
    private int _nextSequence = 1;

    public FakePaymentSettlementSimulationService()
    {
        Seed();
    }

    public IReadOnlyList<FakePaymentSettlementScenario> GetScenarios()
    {
        lock (_gate)
        {
            return _scenarios
                .OrderByDescending(x => x.UpdatedAtUtc)
                .Select(Clone)
                .ToArray();
        }
    }

    public FakePaymentSettlementScenario? GetScenario(Guid id)
    {
        lock (_gate)
        {
            var scenario = _scenarios.FirstOrDefault(x => x.Id == id);
            return scenario is null ? null : Clone(scenario);
        }
    }

    public FakePaymentSettlementScenario CreateScenario(
        string requestId,
        string shipperId,
        string driverId,
        int amount,
        string settlementTiming)
    {
        lock (_gate)
        {
            var scenario = NewScenario(requestId, shipperId, driverId, amount, settlementTiming);
            _scenarios.Add(scenario);
            return Clone(scenario);
        }
    }

    public FakePaymentSettlementScenario? Run(Guid id, FakePaymentSettlementAction action, string actor)
    {
        lock (_gate)
        {
            var scenario = _scenarios.FirstOrDefault(x => x.Id == id);
            if (scenario is null)
            {
                return null;
            }

            Apply(scenario, action, actor);
            return Clone(scenario);
        }
    }

    public IReadOnlyList<FakePaymentSettlementScenario> Reset()
    {
        lock (_gate)
        {
            _scenarios.Clear();
            _nextSequence = 1;
            Seed();
            return _scenarios.Select(Clone).ToArray();
        }
    }

    private void Seed()
    {
        _scenarios.Add(NewScenario("SIM-REQ-001", "SIM-SHIPPER-001", "SIM-DRV-001", 98000, "선결제"));
        _scenarios.Add(NewScenario("SIM-REQ-002", "SIM-SHIPPER-002", "SIM-DRV-002", 143000, "하차완료후정산"));
    }

    private FakePaymentSettlementScenario NewScenario(
        string requestId,
        string shipperId,
        string driverId,
        int amount,
        string settlementTiming)
    {
        var now = DateTime.UtcNow;
        var sequence = _nextSequence++;
        var scenario = new FakePaymentSettlementScenario
        {
            Id = Guid.NewGuid(),
            Sequence = sequence,
            RequestId = Normalize(requestId, $"SIM-REQ-{sequence:000}"),
            ShipperId = Normalize(shipperId, $"SIM-SHIPPER-{sequence:000}"),
            DriverId = Normalize(driverId, $"SIM-DRV-{sequence:000}"),
            Amount = Math.Max(1000, amount),
            SettlementTiming = Normalize(settlementTiming, "선결제"),
            Provider = "FakePG",
            PaymentStatus = "결제확보대기",
            TransportStatus = "운송대기",
            SettlementStatus = "정산대기전",
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        AddEvent(
            scenario,
            "scenario.created",
            "시뮬레이션",
            "샘플 운송 원장을 만들었습니다.",
            "system",
            Severity.Info);

        return scenario;
    }

    private static void Apply(FakePaymentSettlementScenario scenario, FakePaymentSettlementAction action, string actor)
    {
        switch (action)
        {
            case FakePaymentSettlementAction.SecurePayment:
                SecurePayment(scenario, actor);
                break;
            case FakePaymentSettlementAction.CompletePickup:
                CompletePickup(scenario, actor);
                break;
            case FakePaymentSettlementAction.CompleteDropoff:
                CompleteDropoff(scenario, actor);
                break;
            case FakePaymentSettlementAction.RequestPayout:
                RequestPayout(scenario, actor);
                break;
            case FakePaymentSettlementAction.HoldSettlement:
                HoldSettlement(scenario, actor);
                break;
            case FakePaymentSettlementAction.ReleaseHold:
                ReleaseHold(scenario, actor);
                break;
            case FakePaymentSettlementAction.Refund:
                Refund(scenario, actor);
                break;
            case FakePaymentSettlementAction.ResetScenario:
                ResetScenario(scenario, actor);
                break;
        }
    }

    private static void SecurePayment(FakePaymentSettlementScenario scenario, string actor)
    {
        if (scenario.PaymentStatus == "결제확보됨")
        {
            AddEvent(scenario, "fakepg.payment.duplicate", "Fake PG", "이미 결제 확보 상태입니다.", actor, Severity.Warning);
            return;
        }

        if (scenario.PaymentStatus is "환불대기" or "환불완료")
        {
            AddEvent(scenario, "fakepg.payment.blocked", "Fake PG", "환불 흐름에 들어간 원장은 결제확보로 되돌릴 수 없습니다.", actor, Severity.Warning);
            return;
        }

        scenario.PaymentStatus = "결제확보됨";
        scenario.TransportStatus = "배차대기";
        scenario.SettlementStatus = "정산대기전";
        scenario.PaymentIntentId ??= $"fake_pi_{scenario.Sequence:000000}";
        scenario.ProviderPaymentKey ??= $"fake_pay_{Guid.NewGuid():N}";
        AddEvent(scenario, "fakepg.payment.secured", "Fake PG", "운송료를 Fake PG 보증 상태로 전환했습니다.", actor, Severity.Success);
    }

    private static void CompletePickup(FakePaymentSettlementScenario scenario, string actor)
    {
        if (scenario.PaymentStatus != "결제확보됨")
        {
            AddEvent(scenario, "transport.pickup.blocked", "운송", "결제확보 전에는 상차 완료로 넘기지 않습니다.", actor, Severity.Warning);
            return;
        }

        scenario.TransportStatus = "상차완료";
        scenario.SettlementStatus = scenario.SettlementTiming.Contains("상차", StringComparison.OrdinalIgnoreCase)
            ? "조기정산후보"
            : "정산대기전";
        AddEvent(scenario, "transport.pickup.completed", "운송", "상차 증빙이 접수된 것으로 처리했습니다.", actor, Severity.Success);
    }

    private static void CompleteDropoff(FakePaymentSettlementScenario scenario, string actor)
    {
        if (scenario.TransportStatus != "상차완료")
        {
            AddEvent(scenario, "transport.dropoff.blocked", "운송", "상차 완료 이후에만 하차 완료를 처리합니다.", actor, Severity.Warning);
            return;
        }

        scenario.TransportStatus = "하차완료";
        scenario.SettlementStatus = scenario.IsDisputed ? "정산보류" : "정산대기";
        AddEvent(scenario, "transport.dropoff.completed", "운송", "하차/POD 증빙이 접수된 것으로 처리했습니다.", actor, Severity.Success);
    }

    private static void RequestPayout(FakePaymentSettlementScenario scenario, string actor)
    {
        if (scenario.IsDisputed || scenario.SettlementStatus == "정산보류")
        {
            AddEvent(scenario, "fakesettlement.payout.blocked", "Fake 정산", "보류 중인 원장은 정산할 수 없습니다.", actor, Severity.Warning);
            return;
        }

        if (scenario.TransportStatus != "하차완료" && scenario.SettlementStatus != "조기정산후보")
        {
            AddEvent(scenario, "fakesettlement.payout.blocked", "Fake 정산", "하차 완료 또는 조기 정산 후보 상태가 필요합니다.", actor, Severity.Warning);
            return;
        }

        scenario.SettlementStatus = "정산완료";
        scenario.SettlementInstructionId ??= $"fake_settle_{scenario.Sequence:000000}";
        scenario.ProviderTransferKey ??= $"fake_transfer_{Guid.NewGuid():N}";
        AddEvent(scenario, "fakesettlement.payout.completed", "Fake 정산", "기사 정산을 Fake 정산 완료 상태로 처리했습니다.", actor, Severity.Success);
    }

    private static void HoldSettlement(FakePaymentSettlementScenario scenario, string actor)
    {
        scenario.IsDisputed = true;
        scenario.SettlementStatus = "정산보류";
        AddEvent(scenario, "fakesettlement.hold.created", "Fake 정산", "증빙 또는 분쟁 검토를 위해 정산을 보류했습니다.", actor, Severity.Warning);
    }

    private static void ReleaseHold(FakePaymentSettlementScenario scenario, string actor)
    {
        if (!scenario.IsDisputed && scenario.SettlementStatus != "정산보류")
        {
            AddEvent(scenario, "fakesettlement.hold.none", "Fake 정산", "해제할 보류 상태가 없습니다.", actor, Severity.Info);
            return;
        }

        scenario.IsDisputed = false;
        scenario.SettlementStatus = scenario.TransportStatus == "하차완료" ? "정산대기" : "정산대기전";
        AddEvent(scenario, "fakesettlement.hold.released", "Fake 정산", "정산 보류를 해제했습니다.", actor, Severity.Success);
    }

    private static void Refund(FakePaymentSettlementScenario scenario, string actor)
    {
        if (scenario.SettlementStatus == "정산완료")
        {
            AddEvent(scenario, "fakepg.refund.blocked", "Fake PG", "정산완료 후 환불은 별도 회수 절차가 필요합니다.", actor, Severity.Warning);
            return;
        }

        scenario.PaymentStatus = "환불완료";
        scenario.TransportStatus = "취소";
        scenario.SettlementStatus = "환불종료";
        AddEvent(scenario, "fakepg.refund.completed", "Fake PG", "Fake PG 환불 완료로 원장을 종료했습니다.", actor, Severity.Success);
    }

    private static void ResetScenario(FakePaymentSettlementScenario scenario, string actor)
    {
        scenario.PaymentStatus = "결제확보대기";
        scenario.TransportStatus = "운송대기";
        scenario.SettlementStatus = "정산대기전";
        scenario.PaymentIntentId = null;
        scenario.ProviderPaymentKey = null;
        scenario.SettlementInstructionId = null;
        scenario.ProviderTransferKey = null;
        scenario.IsDisputed = false;
        AddEvent(scenario, "scenario.reset", "시뮬레이션", "원장 상태를 초기 단계로 되돌렸습니다.", actor, Severity.Info);
    }

    private static void AddEvent(
        FakePaymentSettlementScenario scenario,
        string eventType,
        string stage,
        string message,
        string actor,
        Severity severity)
    {
        var now = DateTime.UtcNow;
        scenario.UpdatedAtUtc = now;
        scenario.Events.Insert(0, new FakePaymentSettlementEvent
        {
            OccurredAtUtc = now,
            Stage = stage,
            EventType = eventType,
            Message = message,
            Actor = string.IsNullOrWhiteSpace(actor) ? "admin" : actor,
            Severity = severity,
            Snapshot = $"{scenario.PaymentStatus} / {scenario.TransportStatus} / {scenario.SettlementStatus}"
        });
    }

    private static FakePaymentSettlementScenario Clone(FakePaymentSettlementScenario source)
        => new()
        {
            Id = source.Id,
            Sequence = source.Sequence,
            RequestId = source.RequestId,
            ShipperId = source.ShipperId,
            DriverId = source.DriverId,
            Amount = source.Amount,
            SettlementTiming = source.SettlementTiming,
            Provider = source.Provider,
            PaymentStatus = source.PaymentStatus,
            TransportStatus = source.TransportStatus,
            SettlementStatus = source.SettlementStatus,
            PaymentIntentId = source.PaymentIntentId,
            ProviderPaymentKey = source.ProviderPaymentKey,
            SettlementInstructionId = source.SettlementInstructionId,
            ProviderTransferKey = source.ProviderTransferKey,
            IsDisputed = source.IsDisputed,
            CreatedAtUtc = source.CreatedAtUtc,
            UpdatedAtUtc = source.UpdatedAtUtc,
            Events = source.Events.Select(Clone).ToList()
        };

    private static FakePaymentSettlementEvent Clone(FakePaymentSettlementEvent source)
        => new()
        {
            OccurredAtUtc = source.OccurredAtUtc,
            Stage = source.Stage,
            EventType = source.EventType,
            Message = source.Message,
            Actor = source.Actor,
            Severity = source.Severity,
            Snapshot = source.Snapshot
        };

    private static string Normalize(string? value, string fallback)
        => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
}

public sealed class FakePaymentSettlementScenario
{
    public Guid Id { get; set; }
    public int Sequence { get; set; }
    public string RequestId { get; set; } = string.Empty;
    public string ShipperId { get; set; } = string.Empty;
    public string DriverId { get; set; } = string.Empty;
    public int Amount { get; set; }
    public string SettlementTiming { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string TransportStatus { get; set; } = string.Empty;
    public string SettlementStatus { get; set; } = string.Empty;
    public string? PaymentIntentId { get; set; }
    public string? ProviderPaymentKey { get; set; }
    public string? SettlementInstructionId { get; set; }
    public string? ProviderTransferKey { get; set; }
    public bool IsDisputed { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public List<FakePaymentSettlementEvent> Events { get; set; } = [];
}

public sealed class FakePaymentSettlementEvent
{
    public DateTime OccurredAtUtc { get; set; }
    public string Stage { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Actor { get; set; } = string.Empty;
    public Severity Severity { get; set; }
    public string Snapshot { get; set; } = string.Empty;
}

public enum FakePaymentSettlementAction
{
    SecurePayment,
    CompletePickup,
    CompleteDropoff,
    RequestPayout,
    HoldSettlement,
    ReleaseHold,
    Refund,
    ResetScenario
}
