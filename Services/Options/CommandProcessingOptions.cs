using System.Collections.Generic;

namespace 홍달.Services.Options;

public sealed class CommandProcessingOptions
{
    public const string SectionName = "CommandProcessing";

    public CommandProcessingRule Defaults { get; set; } = new();

    public Dictionary<string, CommandProcessingRule> Commands { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public List<CommandCatalogOption> DriverCommandCatalog { get; set; } = [];

    public List<CommandFeatureCatalogOption> FeatureCatalog { get; set; } = [];

    public CommandProcessingRule GetRule(string commandName)
    {
        if (Commands.TryGetValue(commandName, out var rule))
        {
            return Defaults.Merge(rule);
        }

        return Defaults;
    }
}

public sealed class CommandCatalogOption
{
    public string CommandName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;
}

public sealed class CommandFeatureCatalogOption
{
    public string FeatureName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public bool? IsUserConfigurable { get; set; }

    public bool? IsRequired { get; set; }
}

public sealed class CommandProcessingRule
{
    public bool? AuditLogEnabled { get; set; }

    public bool? SmsEnabled { get; set; }

    public bool? SnsEnabled { get; set; }

    public bool? PushEnabled { get; set; }

    public string EventName { get; set; } = string.Empty;

    public string Target { get; set; } = string.Empty;

    public CommandProcessingRule Merge(CommandProcessingRule? overrideRule)
    {
        if (overrideRule is null)
        {
            return Clone(this);
        }

        return new CommandProcessingRule
        {
            AuditLogEnabled = overrideRule.AuditLogEnabled ?? AuditLogEnabled,
            SmsEnabled = overrideRule.SmsEnabled ?? SmsEnabled,
            SnsEnabled = overrideRule.SnsEnabled ?? SnsEnabled,
            PushEnabled = overrideRule.PushEnabled ?? PushEnabled,
            EventName = string.IsNullOrWhiteSpace(overrideRule.EventName) ? EventName : overrideRule.EventName,
            Target = string.IsNullOrWhiteSpace(overrideRule.Target) ? Target : overrideRule.Target
        };
    }

    private static CommandProcessingRule Clone(CommandProcessingRule source)
    {
        return new CommandProcessingRule
        {
            AuditLogEnabled = source.AuditLogEnabled,
            SmsEnabled = source.SmsEnabled,
            SnsEnabled = source.SnsEnabled,
            PushEnabled = source.PushEnabled,
            EventName = source.EventName,
            Target = source.Target
        };
    }
}
