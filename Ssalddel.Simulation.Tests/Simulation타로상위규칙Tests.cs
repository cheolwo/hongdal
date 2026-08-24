using Ssalddel.Simulation.Contracts;
using Ssalddel.Simulation.Domain;

namespace Ssalddel.Simulation.Tests;

[Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceResponsibility(
    Ssalddel.Contracts.Common.Metadata.SsalddelEvidenceStage.E3,
    "Simulation·Unity 계약과 결정성 및 회귀 증거를 검증한다.",
    Boundary = "자동 시험 통과와 실제 Play Mode·Game View·E 승격 증거를 구분한다.")]
public sealed class Simulation타로상위규칙Tests
{
    private readonly Simulation타로상위규칙보정기 rule = new();

    [Fact]
    public void 활성카드가없으면_기존하위규칙값을그대로유지한다()
    {
        var result = rule.Apply(Request(3m));

        Assert.Equal(3m, result.BaseValue);
        Assert.Equal(3m, result.FinalValue);
        Assert.Equal(0m, result.AdditiveTotal);
        Assert.Equal(1m, result.MultiplierProduct);
        Assert.Empty(result.AppliedModifierLines);
    }

    [Fact]
    public void 전차정방향은_운송기간에마이너스보정을제공한다()
    {
        var request = Request(3m);
        request.ModifierLines = new[]
        {
            Line("modifier:chariot.fast.duration", -1m,
                Simulation타로보정계산방식Codes.Additive, "turn"),
        };

        var result = rule.Apply(request);

        Assert.Equal(3m, result.BaseValue);
        Assert.Equal(-1m, result.AdditiveTotal);
        Assert.Equal(2m, result.FinalValue);
        Assert.Equal(Simulation타로보정의미Codes.Opportunity,
            Assert.Single(result.AppliedModifierLines).MeaningCode);
    }

    [Fact]
    public void 전차정방향은_연료소비에플러스비율보정을제공한다()
    {
        var request = Request(100m, FuelConnectionPoint());
        request.ModifierLines = new[]
        {
            Line("modifier:chariot.fast.fuel", 1.10m,
                Simulation타로보정계산방식Codes.Multiplier, "ratio",
                FuelConnectionPoint(), Simulation타로보정의미Codes.Burden),
        };

        var result = rule.Apply(request);

        Assert.Equal(1.10m, result.MultiplierProduct);
        Assert.Equal(110m, result.FinalValue);
        Assert.Equal("liter", result.ValueUnitCode);
    }

    [Fact]
    public void 고정값_비율_최소최대제한순서는_입력배열순서와무관하다()
    {
        var connection = DurationConnectionPoint(
            Allowed(Simulation타로보정계산방식Codes.Additive, "turn", -3m, 3m),
            Allowed(Simulation타로보정계산방식Codes.Multiplier, "ratio", 0.5m, 2m),
            Allowed(Simulation타로보정계산방식Codes.MinimumClamp, "turn", 1m, 4m),
            Allowed(Simulation타로보정계산방식Codes.MaximumClamp, "turn", 2m, 10m));
        var request = Request(5m, connection);
        request.ModifierLines = new[]
        {
            Line("modifier:chariot.fast.maximum", 4m,
                Simulation타로보정계산방식Codes.MaximumClamp, "turn", connection),
            Line("modifier:chariot.fast.ratio", 0.5m,
                Simulation타로보정계산방식Codes.Multiplier, "ratio", connection),
            Line("modifier:chariot.fast.add", -1m,
                Simulation타로보정계산방식Codes.Additive, "turn", connection),
            Line("modifier:chariot.fast.minimum", 3m,
                Simulation타로보정계산방식Codes.MinimumClamp, "turn", connection),
        };

        var result = rule.Apply(request);

        Assert.Equal(3m, result.FinalValue);
        Assert.Equal(new[]
        {
            Simulation타로보정계산방식Codes.Additive,
            Simulation타로보정계산방식Codes.Multiplier,
            Simulation타로보정계산방식Codes.MinimumClamp,
            Simulation타로보정계산방식Codes.MaximumClamp,
        }, result.AppliedModifierLines.Select(value => value.CalculationKindCode));
    }

    [Fact]
    public void 하위규칙이선언하지않은보정방식은_거부한다()
    {
        var request = Request(3m);
        request.ModifierLines = new[]
        {
            Line("modifier:chariot.fast.ratio", 0.8m,
                Simulation타로보정계산방식Codes.Multiplier, "ratio"),
        };

        var error = Assert.Throws<SimulationContractException>(() => rule.Apply(request));

        Assert.Equal("SimulationTarotModifierNotAllowed", error.ErrorCode);
    }

    [Fact]
    public void 다른하위규칙이나연결지점을겨냥한보정은_거부한다()
    {
        var request = Request(3m);
        var line = Line("modifier:chariot.fast.duration", -1m,
            Simulation타로보정계산방식Codes.Additive, "turn");
        line.CompatibleLowerRuleStableId = "rule:warehouse.receipt.v1";
        request.ModifierLines = new[] { line };

        var error = Assert.Throws<SimulationContractException>(() => rule.Apply(request));

        Assert.Equal("SimulationTarotModifierTargetMismatch", error.ErrorCode);
    }

    [Fact]
    public void 허용범위를넘어불변최소값을깨는보정은_거부한다()
    {
        var connection = DurationConnectionPoint(
            Allowed(Simulation타로보정계산방식Codes.Additive, "turn", -5m, 5m));
        var request = Request(1m, connection);
        request.ModifierLines = new[]
        {
            Line("modifier:chariot.fast.duration", -2m,
                Simulation타로보정계산방식Codes.Additive, "turn", connection),
        };

        var error = Assert.Throws<SimulationContractException>(() => rule.Apply(request));

        Assert.Equal("SimulationTarotModifierResultOutOfRange", error.ErrorCode);
    }

    [Fact]
    public void 활성기간이지난보정은_기존규칙에적용하지않는다()
    {
        var request = Request(3m);
        var line = Line("modifier:chariot.fast.duration", -1m,
            Simulation타로보정계산방식Codes.Additive, "turn");
        line.ActiveFromTurnNumber = 4;
        line.ActiveThroughTurnNumber = 4;
        request.ModifierLines = new[] { line };

        var error = Assert.Throws<SimulationContractException>(() => rule.Apply(request));

        Assert.Equal("SimulationTarotModifierInactive", error.ErrorCode);
    }

    [Fact]
    public void 첫뼈대에서는_서로다른활성카드의보정을중첩하지않는다()
    {
        var connection = DurationConnectionPoint(
            Allowed(Simulation타로보정계산방식Codes.Additive, "turn", -3m, 3m));
        var first = Line("modifier:chariot.fast.duration", -1m,
            Simulation타로보정계산방식Codes.Additive, "turn", connection);
        var second = Line("modifier:tower.delay.duration", 1m,
            Simulation타로보정계산방식Codes.Additive, "turn", connection);
        second.SourceCardStableId = "tarot:major.tower";
        var request = Request(3m, connection);
        request.ModifierLines = new[] { first, second };

        var error = Assert.Throws<SimulationContractException>(() => rule.Apply(request));

        Assert.Equal("SimulationTarotModifierMultipleActiveCardsNotSupported", error.ErrorCode);
    }

    private static Simulation타로규칙보정적용Request Request(
        decimal baseValue,
        Simulation하위규칙보정연결지점Definition? connection = null)
        => new()
        {
            CurrentTurnNumber = 3,
            BaseValue = baseValue,
            ConnectionPoint = connection ?? DurationConnectionPoint(
                Allowed(Simulation타로보정계산방식Codes.Additive, "turn", -2m, 2m)),
        };

    private static Simulation하위규칙보정연결지점Definition DurationConnectionPoint(
        params Simulation타로보정허용범위Definition[] allowed)
        => new()
        {
            ConnectionPointStableId = "modifier-point:transport.duration-turns",
            LowerRuleStableId = "rule:transport.travel.v1",
            LowerRuleRevision = 1,
            RuleDomainCode = Simulation업무규칙영역Codes.Transport,
            ValueUnitCode = "turn",
            MinimumResultValue = 1m,
            MaximumResultValue = 14m,
            AllowedModifiers = allowed,
        };

    private static Simulation하위규칙보정연결지점Definition FuelConnectionPoint()
        => new()
        {
            ConnectionPointStableId = "modifier-point:transport.fuel-consumption",
            LowerRuleStableId = "rule:transport.travel.v1",
            LowerRuleRevision = 1,
            RuleDomainCode = Simulation업무규칙영역Codes.Transport,
            ValueUnitCode = "liter",
            MinimumResultValue = 0m,
            AllowedModifiers = new[]
            {
                Allowed(Simulation타로보정계산방식Codes.Multiplier, "ratio", 0.5m, 2m),
            },
        };

    private static Simulation타로보정허용범위Definition Allowed(
        string calculationKindCode,
        string unitCode,
        decimal minimum,
        decimal maximum)
        => new()
        {
            CalculationKindCode = calculationKindCode,
            ModifierUnitCode = unitCode,
            MinimumModifierValue = minimum,
            MaximumModifierValue = maximum,
        };

    private static Simulation타로규칙보정선Snapshot Line(
        string stableId,
        decimal value,
        string calculationKindCode,
        string unitCode,
        Simulation하위규칙보정연결지점Definition? connection = null,
        string meaningCode = Simulation타로보정의미Codes.Opportunity)
    {
        connection ??= DurationConnectionPoint(
            Allowed(Simulation타로보정계산방식Codes.Additive, "turn", -2m, 2m));
        return new Simulation타로규칙보정선Snapshot
        {
            ModifierLineStableId = stableId,
            UpperRuleStableId = "tarot-rule:major.chariot.transport.v1",
            UpperRuleRevision = 1,
            SourceCardStableId = "tarot:major.chariot",
            SourceCardRevision = "tarot-card:r1",
            CardOrientationCode = Simulation타로카드방향Codes.Upright,
            ResponseStableId = "tarot-response:chariot.fast-transport",
            TargetConnectionPointStableId = connection.ConnectionPointStableId,
            TargetRuleDomainCode = connection.RuleDomainCode,
            CompatibleLowerRuleStableId = connection.LowerRuleStableId,
            CompatibleLowerRuleRevision = connection.LowerRuleRevision,
            CalculationKindCode = calculationKindCode,
            ModifierValue = value,
            ModifierUnitCode = unitCode,
            MeaningCode = meaningCode,
            ActiveFromTurnNumber = 3,
            ActiveThroughTurnNumber = 3,
            SourceTurnClosingStableId = "turn-closing:session-1:2",
            SourceStableIds = new[] { "tarot-publication:major.chariot.r1" },
        };
    }
}
