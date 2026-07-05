using Microsoft.Extensions.Options;
using 홍달.Services.Options;

namespace Hongdal.Application.CommandProcessing;

public sealed class Command기능CatalogResolver : ICommand기능CatalogResolver
{
    private readonly IOptionsMonitor<CommandProcessingOptions> _options;

    public Command기능CatalogResolver(IOptionsMonitor<CommandProcessingOptions> options)
    {
        _options = options;
    }

    public IReadOnlyList<Command기능대상> GetDriverCommands()
    {
        var optionCommands = _options.CurrentValue.DriverCommandCatalog
            .Where(x => !string.IsNullOrWhiteSpace(x.CommandName))
            .Select(x => new Command기능대상(
                x.CommandName,
                string.IsNullOrWhiteSpace(x.DisplayName) ? x.CommandName : x.DisplayName,
                x.Category))
            .ToArray();

        return optionCommands.Length > 0 ? optionCommands : Command기능대상Catalog.DriverCommands;
    }

    public IReadOnlyList<Command기능정책> GetFeatures()
    {
        var optionFeatures = _options.CurrentValue.FeatureCatalog
            .Where(x => !string.IsNullOrWhiteSpace(x.FeatureName))
            .Select(x =>
            {
                var defaultPolicy = Command기능정책Catalog.Get(x.FeatureName);
                return new Command기능정책(
                    x.FeatureName,
                    x.IsUserConfigurable ?? defaultPolicy.IsUserConfigurable,
                    x.IsRequired ?? defaultPolicy.IsRequired);
            })
            .ToArray();

        return optionFeatures.Length > 0 ? optionFeatures : Command기능정책Catalog.All;
    }

    public string GetFeatureDisplayName(string featureName)
    {
        var option = _options.CurrentValue.FeatureCatalog.FirstOrDefault(x => string.Equals(x.FeatureName, featureName, StringComparison.Ordinal));
        return string.IsNullOrWhiteSpace(option?.DisplayName) ? Command기능명.표시명(featureName) : option.DisplayName;
    }

    public bool IsSupportedDriverCommand(string commandName)
    {
        return GetDriverCommands().Any(x => string.Equals(x.CommandName, commandName, StringComparison.Ordinal));
    }

    public bool IsSupportedFeature(string featureName)
    {
        return GetFeatures().Any(x => string.Equals(x.FeatureName, featureName, StringComparison.Ordinal));
    }
}
