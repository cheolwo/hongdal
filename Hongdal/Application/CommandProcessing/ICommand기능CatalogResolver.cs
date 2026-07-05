namespace Hongdal.Application.CommandProcessing;

public interface ICommand기능CatalogResolver
{
    IReadOnlyList<Command기능대상> GetDriverCommands();

    IReadOnlyList<Command기능정책> GetFeatures();

    string GetFeatureDisplayName(string featureName);

    bool IsSupportedDriverCommand(string commandName);

    bool IsSupportedFeature(string featureName);
}
