using Ssalddel.Contracts.Common.Community;
using 살뜰.Services.Options;

namespace Ssalddel.Services.Community;

internal sealed record CommunityEditorialBatchRegistration(
    string SourceKey,
    string? CanonicalBoardKey,
    bool QuartzRegistrationEnabled,
    bool CollectionHandoffEnabled);

internal sealed class CommunityEditorialBatchRegistrationPlan
{
    private readonly IReadOnlyDictionary<string, CommunityEditorialBatchRegistration> _registrations;

    private CommunityEditorialBatchRegistrationPlan(
        IReadOnlyDictionary<string, CommunityEditorialBatchRegistration> registrations)
    {
        _registrations = registrations;
    }

    public CommunityEditorialBatchRegistration Get(string sourceKey)
        => _registrations.TryGetValue(sourceKey, out var registration)
            ? registration
            : throw new InvalidOperationException(
                $"등록되지 않은 커뮤니티 편집 배치 원천입니다. SourceKey={sourceKey}");

    public bool ShouldRegisterQuartz(string sourceKey)
        => Get(sourceKey).QuartzRegistrationEnabled;

    public IReadOnlyCollection<CommunityEditorialBatchRegistration> Registrations
        => _registrations.Values.ToArray();

    public static CommunityEditorialBatchRegistrationPlan Create(
        AgriculturalFisheriesBatchOptions agriculturalBatch,
        CommunityEditorialBatchOptions editorialBatch)
    {
        ArgumentNullException.ThrowIfNull(agriculturalBatch);
        ArgumentNullException.ThrowIfNull(editorialBatch);

        var kamisCollectionHandoff = agriculturalBatch.Enabled
                                     && agriculturalBatch.KamisDailyEnabled
                                     && agriculturalBatch.PublishCommunityPriceBriefs
                                     && editorialBatch.KamisPriceBriefEnabled;
        var usdaCollectionHandoff = agriculturalBatch.Enabled
                                    && agriculturalBatch.UsdaMonthlyEnabled
                                    && agriculturalBatch.PublishCommunityPriceBriefs
                                    && editorialBatch.UsdaNassPriceBriefEnabled;

        var registrations = new[]
        {
            Registration(
                CommunityAutomatedPostSourceKeys.KamisPriceBrief,
                editorialBatch.Enabled
                && editorialBatch.KamisPriceBriefEnabled
                && !kamisCollectionHandoff,
                kamisCollectionHandoff),
            Registration(
                CommunityAutomatedPostSourceKeys.UsdaNassPriceBrief,
                editorialBatch.Enabled
                && editorialBatch.UsdaNassPriceBriefEnabled
                && !usdaCollectionHandoff,
                usdaCollectionHandoff),
            Registration(
                CommunityAutomatedPostSourceKeys.ChinaImportedFoodRegionBrief,
                quartzRegistrationEnabled: false,
                collectionHandoffEnabled:
                    agriculturalBatch.Enabled
                    && agriculturalBatch.IngredientCompanyResearchEnabled
                    && agriculturalBatch.PublishChinaImportedFoodRegionBriefs),
            Registration(
                CommunityAutomatedPostSourceKeys.UnitedStatesImportedFoodStateBrief,
                quartzRegistrationEnabled: false,
                collectionHandoffEnabled:
                    agriculturalBatch.Enabled
                    && agriculturalBatch.IngredientCompanyResearchEnabled
                    && agriculturalBatch.PublishUnitedStatesImportedFoodStateBriefs),
            Registration(
                CommunityAutomatedPostSourceKeys.Reflection,
                editorialBatch.Enabled && editorialBatch.ReflectionEnabled),
            Registration(
                CommunityAutomatedPostSourceKeys.ActivityDigest,
                editorialBatch.Enabled && editorialBatch.ActivityDigestEnabled),
            Registration(
                CommunityAutomatedPostSourceKeys.Prajna,
                editorialBatch.Enabled && editorialBatch.PrajnaPublicationEnabled),
            Registration(
                CommunityAutomatedPostSourceKeys.CultureTransport,
                editorialBatch.Enabled && editorialBatch.CultureTransportEnabled)
        }.ToDictionary(item => item.SourceKey, StringComparer.OrdinalIgnoreCase);

        return new CommunityEditorialBatchRegistrationPlan(registrations);
    }

    private static CommunityEditorialBatchRegistration Registration(
        string sourceKey,
        bool quartzRegistrationEnabled,
        bool collectionHandoffEnabled = false)
        => new(
            sourceKey,
            CommunityPeriodicDataBoardCatalog.CanonicalBoardKeyForPublicationSource(sourceKey),
            quartzRegistrationEnabled,
            collectionHandoffEnabled);
}
