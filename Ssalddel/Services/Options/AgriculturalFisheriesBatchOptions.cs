namespace 살뜰.Services.Options;

public sealed class AgriculturalFisheriesBatchOptions
{
    public const string SectionName = "AgriculturalFisheriesBatch";

    public bool Enabled { get; set; }

    public string TimeZoneId { get; set; } = "Asia/Seoul";

    public int ImmediateRetryCount { get; set; } = 1;

    public bool PublishCommunityPriceBriefs { get; set; }

    public bool KamisDailyEnabled { get; set; } = true;

    public string KamisDailyCronExpression { get; set; } = "0 30 6 * * ?";

    public int KamisDailyDaysBehind { get; set; } = 1;

    public bool KamisMonthlyEnabled { get; set; } = true;

    public string KamisMonthlyCronExpression { get; set; } = "0 0 7 2 * ?";

    public int KamisMonthlyLookbackMonths { get; set; } = 12;

    public bool UsdaMonthlyEnabled { get; set; } = true;

    public string UsdaMonthlyCronExpression { get; set; } = "0 30 7 10 * ?";

    public int UsdaLookbackYears { get; set; } = 1;

    public bool IngredientCompanyResearchEnabled { get; set; }

    public string IngredientCompanyResearchCronExpression { get; set; } = "0 0 3 ? * SUN";

    public int IngredientCompanyResearchMaxIngredients { get; set; } = 500;

    public int IngredientCompanyResearchCandidatesPerIngredient { get; set; } = 100;

    public int IngredientCompanyResearchRefreshAfterDays { get; set; } = 30;

    public int IngredientCompanyResearchRequestDelayMilliseconds { get; set; } = 250;
}
