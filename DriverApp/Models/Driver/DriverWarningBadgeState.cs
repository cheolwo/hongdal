using System.Reflection;

namespace DriverApp.Models.Driver;

public sealed record DriverWarningBadgeState(
    bool RequiresSpecialPickup,
    bool RequiresSpecialDropoff,
    bool ShowReceipt);

public static class DriverWarningBadgeStateFactory
{
    public static DriverWarningBadgeState Create(object? source)
    {
        var request = ResolveRequest(source);
        if (request is null)
        {
            return new DriverWarningBadgeState(false, false, false);
        }

        var boolValues = request.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(property => property.PropertyType == typeof(bool))
            .Select(property => (bool)(property.GetValue(request) ?? false))
            .ToArray();

        return new DriverWarningBadgeState(
            boolValues.ElementAtOrDefault(0),
            boolValues.ElementAtOrDefault(1),
            boolValues.ElementAtOrDefault(2));
    }

    private static object? ResolveRequest(object? source)
    {
        if (source is null)
        {
            return null;
        }

        if (source is DriverRequestItem)
        {
            return source;
        }

        return source.GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .FirstOrDefault(property => property.PropertyType == typeof(DriverRequestItem))
            ?.GetValue(source);
    }
}
