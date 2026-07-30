using Ssalddel.Contracts.Common.AgriculturalFisheries;

namespace Ssalddel.Services.AgriculturalFisheries.ImportReadiness;

public static class 농수산물포장Fcl계산기
{
    private const decimal PlanningFillRate = 0.85m;

    private static readonly IReadOnlyList<ContainerSpec> DryContainers =
    [
        new(
            "20GP",
            "20ft standard dry",
            농수산물포장온도코드.상온,
            5900,
            2352,
            2395,
            33.2m,
            28300m,
            17780m,
            0.9m),
        new(
            "40GP",
            "40ft standard dry",
            농수산물포장온도코드.상온,
            12032,
            2352,
            2395,
            67.7m,
            28870m,
            19960m,
            0.9m),
        new(
            "40HC",
            "40ft high-cube dry",
            농수산물포장온도코드.상온,
            12032,
            2350,
            2700,
            76.4m,
            28690m,
            19820m,
            0.9m)
    ];

    private static readonly IReadOnlyList<ContainerSpec> ReeferContainers =
    [
        new(
            "20RF",
            "20ft reefer",
            농수산물포장온도코드.냉장,
            5450,
            2280,
            2159,
            28.3m,
            27770m,
            15830m,
            0.8m),
        new(
            "40HCRF",
            "40ft high-cube reefer",
            농수산물포장온도코드.냉장,
            11599,
            2290,
            2425,
            67.5m,
            29670m,
            17830m,
            0.8m)
    ];

    public static IReadOnlyList<농수산물Fcl적재추정Response> 계산(
        농수산물대표포장제원 package)
    {
        ArgumentNullException.ThrowIfNull(package);
        Validate(package);

        var containers = package.TemperatureCode == 농수산물포장온도코드.상온
            ? DryContainers
            : ReeferContainers;
        return containers
            .Select(container => Calculate(package, container))
            .ToArray();
    }

    private static 농수산물Fcl적재추정Response Calculate(
        농수산물대표포장제원 package,
        ContainerSpec container)
    {
        var spatialLimit = package.PackingMethodCode == "FlexibleVolume"
            ? CalculateFlexibleVolumeLimit(package, container)
            : CalculateRigidGeometryLimit(package, container);
        var oceanWeightLimit = decimal.ToInt32(decimal.Floor(
            container.OceanEquipmentPayloadKg / package.GrossWeightKg));
        var roadWeightLimit = container.UnitedStatesRoadCargoWeightLimitKg
            .HasValue
            ? decimal.ToInt32(decimal.Floor(
                container.UnitedStatesRoadCargoWeightLimitKg.Value
                / package.GrossWeightKg))
            : oceanWeightLimit;
        var oceanMaximum = Math.Max(1, Math.Min(spatialLimit, oceanWeightLimit));
        var practicalMaximum = Math.Max(
            1,
            Math.Min(oceanMaximum, roadWeightLimit));
        var planningCount = Math.Max(
            1,
            decimal.ToInt32(decimal.Floor(
                practicalMaximum * PlanningFillRate)));
        var limitingFactor = practicalMaximum == roadWeightLimit
            ? "UnitedStatesRoadWeight"
            : practicalMaximum == oceanWeightLimit
                ? "OceanEquipmentPayload"
                : package.PackingMethodCode == "FlexibleVolume"
                    ? "UsableVolume"
                    : "PackageGeometry";

        return new 농수산물Fcl적재추정Response
        {
            ContainerCode = container.Code,
            ContainerName = container.Name,
            TemperatureCode = container.TemperatureCode,
            InternalLengthMm = container.InternalLengthMm,
            InternalWidthMm = container.InternalWidthMm,
            InternalHeightMm = container.InternalHeightMm,
            NominalCapacityCbm = container.NominalCapacityCbm,
            OceanEquipmentPayloadKg = container.OceanEquipmentPayloadKg,
            UnitedStatesRoadCargoWeightLimitKg =
                container.UnitedStatesRoadCargoWeightLimitKg,
            LoadingEfficiencyRate = container.LoadingEfficiencyRate,
            OceanMaximumPackageCount = oceanMaximum,
            OceanMaximumNetWeightKg = Round(
                package.NetContentWeightKg * oceanMaximum),
            OceanMaximumGrossWeightKg = Round(
                package.GrossWeightKg * oceanMaximum),
            PracticalMaximumPackageCount = practicalMaximum,
            PracticalMaximumNetWeightKg = Round(
                package.NetContentWeightKg * practicalMaximum),
            PracticalMaximumGrossWeightKg = Round(
                package.GrossWeightKg * practicalMaximum),
            PracticalMaximumUnitCount = MultiplyUnits(
                package.UnitsPerPackage,
                practicalMaximum),
            PlanningFillRate = PlanningFillRate,
            PlanningFclPackageCount = planningCount,
            PlanningFclNetWeightKg = Round(
                package.NetContentWeightKg * planningCount),
            PlanningFclGrossWeightKg = Round(
                package.GrossWeightKg * planningCount),
            PlanningFclUnitCount = MultiplyUnits(
                package.UnitsPerPackage,
                planningCount),
            LimitingFactorCode = limitingFactor,
            Warnings =
            [
                "동일 포장만 적재하고 팔레트 없이 floor-loading하는 계산이다.",
                "미국 도로 한도에는 제품·포장·팔레트·고정재 중량이 포함되므로 실제 포장명세서와 chassis를 확인해야 한다.",
                package.TemperatureCode == 농수산물포장온도코드.상온
                    ? "식품용 dry container 적합성, 습기·환기·방충 조건은 별도 확인이 필요하다."
                    : "reefer의 냉기 순환선, 환기, set point와 적재 금지선을 반영한 실제 적재계획이 필요하다."
            ]
        };
    }

    private static int CalculateFlexibleVolumeLimit(
        농수산물대표포장제원 package,
        ContainerSpec container)
    {
        var packageCbm = package.LengthMm / 1000m
                         * (package.WidthMm / 1000m)
                         * (package.HeightMm / 1000m);
        return decimal.ToInt32(decimal.Floor(
            container.NominalCapacityCbm
            * container.LoadingEfficiencyRate
            / packageCbm));
    }

    private static int CalculateRigidGeometryLimit(
        농수산물대표포장제원 package,
        ContainerSpec container)
    {
        var normalPerLayer =
            container.InternalLengthMm / package.LengthMm
            * (container.InternalWidthMm / package.WidthMm);
        var rotatedPerLayer =
            container.InternalLengthMm / package.WidthMm
            * (container.InternalWidthMm / package.LengthMm);
        var layers = Math.Min(
            container.InternalHeightMm / package.HeightMm,
            package.MaxStackLayers);
        var exactGeometryCount = Math.Max(
            normalPerLayer,
            rotatedPerLayer) * layers;
        return decimal.ToInt32(decimal.Floor(
            exactGeometryCount * container.LoadingEfficiencyRate));
    }

    private static long? MultiplyUnits(
        int? unitsPerPackage,
        int packageCount)
        => unitsPerPackage.HasValue
            ? checked((long)unitsPerPackage.Value * packageCount)
            : null;

    private static void Validate(농수산물대표포장제원 package)
    {
        if (package.NetContentWeightKg <= 0
            || package.GrossWeightKg < package.NetContentWeightKg
            || package.LengthMm <= 0
            || package.WidthMm <= 0
            || package.HeightMm <= 0
            || package.MaxStackLayers <= 0)
        {
            throw new ArgumentException(
                "FCL 계산에는 유효한 포장 중량·외형·적층수가 필요합니다.");
        }
    }

    private static decimal Round(decimal value)
        => decimal.Round(value, 3, MidpointRounding.AwayFromZero);

    private sealed record ContainerSpec(
        string Code,
        string Name,
        string TemperatureCode,
        int InternalLengthMm,
        int InternalWidthMm,
        int InternalHeightMm,
        decimal NominalCapacityCbm,
        decimal OceanEquipmentPayloadKg,
        decimal? UnitedStatesRoadCargoWeightLimitKg,
        decimal LoadingEfficiencyRate);
}
