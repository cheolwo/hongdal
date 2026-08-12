using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

public sealed class Simulation전차화물운송상위규칙Tests
{
    private readonly Simulation전차화물운송상위규칙 rule = new();

    [Fact]
    public void 전차정방향빠른운송은_기회와부담을함께보정한다()
    {
        var preview = rule.CreateUprightFastTransportPreview(
            Baseline(),
            "turn-closing:session-1:2");

        AssertMetric(preview, Simulation타로운송지표Codes.DurationTicks, 3m, 2m,
            Simulation타로보정의미Codes.Opportunity);
        AssertMetric(preview, Simulation타로운송지표Codes.ThroughputCapacity, 300m, 360m,
            Simulation타로보정의미Codes.Opportunity);
        AssertMetric(preview, Simulation타로운송지표Codes.FuelConsumption, 100m, 110m,
            Simulation타로보정의미Codes.Burden);
        AssertMetric(preview, Simulation타로운송지표Codes.LaborConsumption, 10m, 11m,
            Simulation타로보정의미Codes.Burden);
        AssertMetric(preview, Simulation타로운송지표Codes.RiskPercentPoint, 10m, 15m,
            Simulation타로보정의미Codes.Risk);
    }

    [Fact]
    public void 전차정방향안전운송은_시간과비용을더쓰고위험을낮춘다()
    {
        var preview = rule.CreateUprightResponsePreview(
            Baseline(),
            "turn-closing:session-1:2",
            Simulation전차운송대응StableIds.SafeTransport);

        AssertMetric(preview, Simulation타로운송지표Codes.DurationTicks, 3m, 4m,
            Simulation타로보정의미Codes.Burden);
        AssertMetric(preview, Simulation타로운송지표Codes.ThroughputCapacity, 300m, 300m,
            Simulation타로보정의미Codes.Opportunity);
        AssertMetric(preview, Simulation타로운송지표Codes.FuelConsumption, 100m, 105m,
            Simulation타로보정의미Codes.Burden);
        AssertMetric(preview, Simulation타로운송지표Codes.LaborConsumption, 10m, 12m,
            Simulation타로보정의미Codes.Burden);
        AssertMetric(preview, Simulation타로운송지표Codes.RiskPercentPoint, 10m, 6m,
            Simulation타로보정의미Codes.Opportunity);
    }

    [Fact]
    public void 전차정방향합배송은_시간을더쓰고연료와노동과위험을낮춘다()
    {
        var preview = rule.CreateUprightResponsePreview(
            Baseline(),
            "turn-closing:session-1:2",
            Simulation전차운송대응StableIds.ConsolidatedTransport);

        AssertMetric(preview, Simulation타로운송지표Codes.DurationTicks, 3m, 4m,
            Simulation타로보정의미Codes.Burden);
        AssertMetric(preview, Simulation타로운송지표Codes.ThroughputCapacity, 300m, 300m,
            Simulation타로보정의미Codes.Opportunity);
        AssertMetric(preview, Simulation타로운송지표Codes.FuelConsumption, 100m, 85m,
            Simulation타로보정의미Codes.Opportunity);
        AssertMetric(preview, Simulation타로운송지표Codes.LaborConsumption, 10m, 9m,
            Simulation타로보정의미Codes.Opportunity);
        AssertMetric(preview, Simulation타로운송지표Codes.RiskPercentPoint, 10m, 9m,
            Simulation타로보정의미Codes.Opportunity);
    }

    [Fact]
    public void 등록되지않은전차대응은_상위규칙적용전에차단한다()
    {
        var error = Assert.Throws<SimulationContractException>(() =>
            rule.CreateUprightResponsePreview(
                Baseline(),
                "turn-closing:session-1:2",
                "tarot-response:chariot.unknown"));

        Assert.Equal("SimulationTarotTransportResponseInvalid", error.ErrorCode);
    }

    [Fact]
    public void 미리보기는_전차출처와상하위규칙계보를보존하고_원장을적용하지않는다()
    {
        var baseline = Baseline();

        var preview = rule.CreateUprightFastTransportPreview(
            baseline,
            "turn-closing:session-1:2");

        Assert.Equal("tarot:major.chariot", preview.SourceCardStableId);
        Assert.Equal(Simulation타로카드방향Codes.Upright, preview.CardOrientationCode);
        Assert.Equal("tarot-response:chariot.fast-transport", preview.ResponseStableId);
        Assert.True(preview.IsCandidateOnly);
        Assert.True(preview.DoesNotApplyResourceLedgers);
        Assert.All(preview.Metrics.SelectMany(value => value.ModifierLines), line =>
        {
            Assert.Equal("rule:transport.travel.v1", line.CompatibleLowerRuleStableId);
            Assert.Equal("turn-closing:session-1:2", line.SourceTurnClosingStableId);
        });
        Assert.Equal(3, baseline.DurationTicks);
        Assert.Equal(300m, baseline.CargoQuantity);
        Assert.Equal(300m, baseline.ThroughputCapacity);
    }

    [Fact]
    public void 처리량보정이차량용량을넘으면_운송불변조건으로차단한다()
    {
        var baseline = Baseline();
        baseline.VehicleCapacity = 350m;

        var error = Assert.Throws<SimulationContractException>(() =>
            rule.CreateUprightFastTransportPreview(
                baseline,
                "turn-closing:session-1:2"));

        Assert.Equal("SimulationTarotModifierResultOutOfRange", error.ErrorCode);
    }

    [Fact]
    public void 위험보정이백퍼센트를넘으면_위험범위불변조건으로차단한다()
    {
        var baseline = Baseline();
        baseline.RiskPercentPoint = 98m;

        var error = Assert.Throws<SimulationContractException>(() =>
            rule.CreateUprightFastTransportPreview(
                baseline,
                "turn-closing:session-1:2"));

        Assert.Equal("SimulationTarotModifierResultOutOfRange", error.ErrorCode);
    }

    [Fact]
    public void 운송기준후보가먼저유효하지않으면_타로보정으로복구하지않는다()
    {
        var baseline = Baseline();
        baseline.DurationTicks = 0;

        var error = Assert.Throws<SimulationContractException>(() =>
            rule.CreateUprightFastTransportPreview(
                baseline,
                "turn-closing:session-1:2"));

        Assert.Equal("SimulationTarotTransportBaselineInvalid", error.ErrorCode);
    }

    private static void AssertMetric(
        Simulation타로운송보정PreviewSnapshot preview,
        string metricCode,
        decimal baseValue,
        decimal finalValue,
        string meaningCode)
    {
        var metric = Assert.Single(preview.Metrics, value => value.MetricCode == metricCode);
        Assert.Equal(baseValue, metric.BaseValue);
        Assert.Equal(finalValue, metric.FinalValue);
        Assert.Equal(meaningCode, Assert.Single(metric.ModifierLines).MeaningCode);
    }

    private static Simulation타로운송기준후보Snapshot Baseline()
        => new()
        {
            TransportRequestStableId = "freight-transport:sim.potato-1",
            LowerRuleStableId = "rule:transport.travel.v1",
            LowerRuleRevision = 1,
            CurrentTurnNumber = 3,
            DurationTicks = 3,
            CargoQuantity = 300m,
            ThroughputCapacity = 300m,
            VehicleCapacity = 400m,
            QuantityUnitCode = "KGM",
            FuelConsumption = 100m,
            FuelUnitCode = "liter",
            LaborConsumption = 10m,
            LaborUnitCode = "labor-hour",
            RiskPercentPoint = 10m,
            SourceStableIds = new[]
            {
                "freight-transport:sim.potato-1",
                "rule:transport.travel.v1",
            },
        };
}
