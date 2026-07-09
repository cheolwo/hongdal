namespace Microsoft.Maui.Storage;

public sealed class Preferences
{
    private readonly Dictionary<string, string> values = new(StringComparer.Ordinal);

    public static Preferences Default { get; } = new();

    public string Get(string key, string defaultValue)
        => values.TryGetValue(key, out var value) ? value : defaultValue;

    public void Set(string key, string value)
        => values[key] = value;
}

public sealed class SecureStorage
{
    private readonly Dictionary<string, string> values = new(StringComparer.Ordinal);

    public static SecureStorage Default { get; } = new();

    public Task<string?> GetAsync(string key)
        => Task.FromResult(values.TryGetValue(key, out var value) ? value : null);

    public Task SetAsync(string key, string value)
    {
        values[key] = value;
        return Task.CompletedTask;
    }

    public void Remove(string key)
        => values.Remove(key);
}
