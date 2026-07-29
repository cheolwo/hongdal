using Ssalddel.Contracts.Common.AgriculturalFisheries;
using Ssalddel.Services.AgriculturalFisheries.ImportReadiness;

namespace Ssalddel.Tests.Services.AgriculturalFisheries;

public sealed class 농수산물포장Fcl계산기Tests
{
    [Fact]
    public void 곡물마대는_해상한도와_미국도로한도를_분리한다()
    {
        var package = CreatePackage(
            netWeightKg: 20m,
            grossWeightKg: 20.35m,
            lengthMm: 600,
            widthMm: 400,
            heightMm: 120,
            temperatureCode: 농수산물포장온도코드.상온,
            maxStackLayers: 18,
            packingMethodCode: "FlexibleVolume");

        var result = 농수산물포장Fcl계산기.계산(package)
            .Single(item => item.ContainerCode == "20GP");

        Assert.Equal(1037, result.OceanMaximumPackageCount);
        Assert.Equal(873, result.PracticalMaximumPackageCount);
        Assert.Equal(742, result.PlanningFclPackageCount);
        Assert.Equal(14840m, result.PlanningFclNetWeightKg);
        Assert.Equal("UnitedStatesRoadWeight", result.LimitingFactorCode);
    }

    [Fact]
    public void 냉장상자는_직교배치와_적층수로_개수까지_계산한다()
    {
        var package = CreatePackage(
            netWeightKg: 13.5m,
            grossWeightKg: 14.3m,
            lengthMm: 500,
            widthMm: 300,
            heightMm: 300,
            temperatureCode: 농수산물포장온도코드.냉장,
            maxStackLayers: 6,
            packingMethodCode: "RigidOrthogonal",
            unitsPerPackage: 85);

        var result = 농수산물포장Fcl계산기.계산(package)
            .Single(item => item.ContainerCode == "20RF");

        Assert.Equal(345, result.PracticalMaximumPackageCount);
        Assert.Equal(293, result.PlanningFclPackageCount);
        Assert.Equal(24905, result.PlanningFclUnitCount);
        Assert.Equal("PackageGeometry", result.LimitingFactorCode);
    }

    [Fact]
    public void 순중량보다_총중량이_작으면_거부한다()
    {
        var package = CreatePackage(
            netWeightKg: 20m,
            grossWeightKg: 19m,
            lengthMm: 600,
            widthMm: 400,
            heightMm: 300,
            temperatureCode: 농수산물포장온도코드.상온,
            maxStackLayers: 6,
            packingMethodCode: "RigidOrthogonal");

        var exception = Assert.Throws<ArgumentException>(
            () => 농수산물포장Fcl계산기.계산(package));

        Assert.Contains("유효한 포장 중량", exception.Message, StringComparison.Ordinal);
    }

    private static 농수산물대표포장제원 CreatePackage(
        decimal netWeightKg,
        decimal grossWeightKg,
        int lengthMm,
        int widthMm,
        int heightMm,
        string temperatureCode,
        int maxStackLayers,
        string packingMethodCode,
        int? unitsPerPackage = null)
        => new(
            PackageTypeCode: "TestPackage",
            PackageUnitLabel: "package",
            NetContentWeightKg: netWeightKg,
            GrossWeightKg: grossWeightKg,
            UnitsPerPackage: unitsPerPackage,
            UnitCountLabel: unitsPerPackage.HasValue ? "개" : null,
            LengthMm: lengthMm,
            WidthMm: widthMm,
            HeightMm: heightMm,
            TemperatureCode: temperatureCode,
            Stackable: true,
            MaxStackLayers: maxStackLayers,
            PackingMethodCode: packingMethodCode,
            EvidenceLevelCode: 농수산물포장근거수준코드.품목군추론,
            ConfidenceScore: 0.5m,
            AssumptionNote: "test",
            Evidence: []);
}
