namespace 홍달.Services.Versioning;

public interface IVersionFeatureFlagService
{
    bool IsEnabled(string featureKey);

    IReadOnlyDictionary<string, bool> GetAll();
}
