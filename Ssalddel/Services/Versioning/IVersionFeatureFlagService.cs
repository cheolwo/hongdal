namespace 살뜰.Services.Versioning;

public interface IVersionFeatureFlagService
{
    bool IsEnabled(string featureKey);

    IReadOnlyDictionary<string, bool> GetAll();
}
