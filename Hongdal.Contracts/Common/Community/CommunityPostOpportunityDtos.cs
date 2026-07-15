using Hongdal.Contracts.Common.AgriculturalFisheries;

namespace Hongdal.Contracts.Common.Community;

public sealed class CommunityPostOpportunityListResponse
{
    public long PostId { get; set; }
    public string DisplayLanguageCode { get; set; } = CommunityDisplayLanguageCodes.Korean;
    public CommunitySharedExperiencePolicyResponse ExperiencePolicy { get; set; } = new();
    public IReadOnlyList<CommunityPostOpportunityResponse> Items { get; set; } = [];
}

public sealed class CommunitySharedExperiencePolicyResponse
{
    public string ExperienceScopeCode { get; set; } = CommunityExperienceScopeCodes.SharedCommunity;
    public bool UsesSameCommunityApp { get; set; } = true;
    public bool OperatingProfileAffectsAvailability { get; set; }
    public bool DisplayLanguageAffectsContentOnly { get; set; } = true;
    public bool InfersLanguageFromCountryOrRole { get; set; }
    public IReadOnlyList<string> SupportedDisplayLanguageCodes { get; set; } = CommunityDisplayLanguageCodes.Supported;
}

public sealed class CommunityPostOpportunityResponse
{
    public string Code { get; set; } = string.Empty;
    public string StateCode { get; set; } = CommunityPostOpportunityStateCodes.Suggested;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string WhySuggested { get; set; } = string.Empty;
    public string LedgerTemplateKey { get; set; } = string.Empty;
    public bool CanStart { get; set; }
    public bool AutoStartsWorkflow { get; set; }
    public bool RequiresExplicitConsent { get; set; } = true;
    public bool InformationOnly { get; set; } = true;
    public bool IsBrokerageEnabled { get; set; }
    public string PreviewEndpoint { get; set; } = string.Empty;
    public string StartEndpoint { get; set; } = string.Empty;
    public IReadOnlyList<string> MatchedSignals { get; set; } = [];
    public IReadOnlyList<string> MissingInformationPrompts { get; set; } = [];
}

public sealed class StartCommunityMeatImportReadinessRequest
{
    public string? DisplayLanguageCode { get; set; }
    public bool ConfirmExplicitStart { get; set; }
    public bool ConfirmInformationOnly { get; set; }
    public CreateMeatImportReadinessCaseRequest Case { get; set; } = new();
}

public sealed class StartCommunityMeatImportReadinessResponse
{
    public long PostId { get; set; }
    public string DisplayLanguageCode { get; set; } = CommunityDisplayLanguageCodes.Korean;
    public bool LinkedToCommunityPost { get; set; }
    public CommunityPostOpportunityResponse Opportunity { get; set; } = new();
    public MeatImportReadinessCaseResponse Case { get; set; } = new();
}

public static class CommunityPostOpportunityCodes
{
    public const string MeatImportReadiness = "MeatImportReadiness";
}

public static class CommunityPostOpportunityStateCodes
{
    public const string Suggested = "Suggested";
    public const string Active = "Active";
    public const string BlockedByAnotherLedger = "BlockedByAnotherLedger";
}

public static class CommunityExperienceScopeCodes
{
    public const string SharedCommunity = "SharedCommunity";
}

public static class CommunityDisplayLanguageCodes
{
    public const string Korean = "ko-KR";
    public const string English = "en-US";

    public static IReadOnlyList<string> Supported { get; } = [Korean, English];

    public static string Normalize(string? value)
        => value?.Trim().StartsWith("en", StringComparison.OrdinalIgnoreCase) == true
            ? English
            : Korean;
}
