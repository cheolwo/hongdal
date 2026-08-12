using System;
using System.Collections.Generic;

namespace Ssalddel.Simulation.Contracts
{

public static class SimulationSharedPublicDataRoutes
{
    public const string KamisPriceObservations =
        "/api/simulation/v1/public-data/kamis-price-observations";
}

public sealed class Simulation공유공공데이터조회결과
{
    public string BoundaryCode { get; set; } =
        "SharedOperationalPublicDataDatabaseReadOnly";

    public string SourceCode { get; set; } = "KAMIS";

    public DateTimeOffset? ReferenceTimeUtc { get; set; }

    public IReadOnlyList<SimulationKamis가격관측> Items { get; set; } =
        Array.Empty<SimulationKamis가격관측>();
}

public sealed class SimulationKamis가격관측
{
    public string StableId { get; set; } = string.Empty;

    public string SurveyDate { get; set; } = string.Empty;

    public string ItemName { get; set; } = string.Empty;

    public string ItemCode { get; set; } = string.Empty;

    public string KindName { get; set; } = string.Empty;

    public string KindCode { get; set; } = string.Empty;

    public string RankName { get; set; } = string.Empty;

    public string Unit { get; set; } = string.Empty;

    public decimal? PriceKrw { get; set; }

    public bool IsPriceMissing { get; set; }

    public string SourcePackageLabel { get; set; } = string.Empty;

    public string SourceUrl { get; set; } = string.Empty;

    public DateTimeOffset LastSeenAtUtc { get; set; }
}
}
