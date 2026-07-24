namespace Ssalddel.Contracts.Common.Community;

public sealed record CommunityImportedFoodCountryFilterDefinition(
    string CountryCode,
    string DisplayName,
    string WorkflowTag);

public static class CommunityImportedFoodCountryFilterCatalog
{
    public static CommunityImportedFoodCountryFilterDefinition China { get; } =
        new("CN", "중국", "중국 수입식품 공개근거");

    public static CommunityImportedFoodCountryFilterDefinition UnitedStates { get; } =
        new("US", "미국", "미국 수입식품 공개근거");

    public static IReadOnlyList<CommunityImportedFoodCountryFilterDefinition> All { get; } =
    [
        China,
        UnitedStates
    ];

    public static CommunityImportedFoodCountryFilterDefinition? FindByWorkflowTag(
        string? workflowTag)
    {
        var normalized = workflowTag?.Trim();
        return All.FirstOrDefault(item => string.Equals(
            item.WorkflowTag,
            normalized,
            StringComparison.OrdinalIgnoreCase));
    }
}
