using Ssalddel.Controllers.Common;
using Ssalddel.Controllers.Platform;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Ssalddel.Tests.Application;

public sealed class EventHandlerConsistencyTests
{
    [Fact]
    public void ApplicationEventHandlers_UseSingularEventHandlerSuffix()
    {
        var invalidNames = ApplicationEventHandlerTypes()
            .Where(type => !HandlerTypeName(type).EndsWith("EventHandler", StringComparison.Ordinal))
            .Select(type => type.FullName ?? type.Name)
            .ToArray();

        Assert.Empty(invalidNames);
    }

    [Fact]
    public void ApplicationEventHandlers_DoNotUsePluralEventHandlersSuffix()
    {
        var pluralNames = ApplicationEventHandlerTypes()
            .Where(type => HandlerTypeName(type).EndsWith("EventHandlers", StringComparison.Ordinal))
            .Select(type => type.FullName ?? type.Name)
            .ToArray();

        Assert.Empty(pluralNames);
    }

    [Fact]
    public void ApplicationEventHandlers_UseMatchingLoggerGenericType()
    {
        var mismatches = ApplicationEventHandlerTypes()
            .SelectMany(type => LoggerFields(type)
                .Where(field => field.FieldType.GenericTypeArguments[0] != type)
                .Select(field => $"{type.FullName}.{field.Name}:{field.FieldType.GenericTypeArguments[0].FullName}"))
            .ToArray();

        Assert.Empty(mismatches);
    }

    private static IEnumerable<Type> ApplicationEventHandlerTypes()
        => typeof(VersionFeatureFlagsController).Assembly
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false }
                           && type.Namespace?.StartsWith("Ssalddel.Application.", StringComparison.Ordinal) == true
                           && type.GetInterfaces().Any(IsNotificationHandlerInterface))
            .OrderBy(type => type.FullName, StringComparer.Ordinal);

    private static bool IsNotificationHandlerInterface(Type type)
        => type.IsGenericType
           && type.GetGenericTypeDefinition() == typeof(INotificationHandler<>);

    private static string HandlerTypeName(Type type)
        => type.Name.Split('`', 2)[0];

    private static IEnumerable<System.Reflection.FieldInfo> LoggerFields(Type type)
        => type
            .GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)
            .Where(field => field.FieldType.IsGenericType
                            && field.FieldType.GetGenericTypeDefinition() == typeof(ILogger<>));
}
