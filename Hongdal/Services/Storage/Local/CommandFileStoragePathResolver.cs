using Microsoft.Extensions.Options;
using 홍달.Services.Options;

namespace 홍달.Services.Storage.Local;

public interface ICommandFileStoragePathResolver
{
    string ResolveCommandFolder(string commandName, string? requestId = null);
    string ResolveAdminFilePodFolder(string fileType, string? requestId = null);
}

public sealed class CommandFileStoragePathResolver : ICommandFileStoragePathResolver
{
    private readonly IOptionsMonitor<CommandFileStorageOptions> _options;

    public CommandFileStoragePathResolver(IOptionsMonitor<CommandFileStorageOptions> options)
    {
        _options = options;
    }

    public string ResolveCommandFolder(string commandName, string? requestId = null)
    {
        if (string.IsNullOrWhiteSpace(commandName))
        {
            throw new InvalidOperationException("commandName is required.");
        }

        var options = _options.CurrentValue;
        var defaultRule = options.DefaultCommand ?? new CommandFileStorageRule();
        var rule = options.Commands.TryGetValue(commandName, out var configuredRule)
            ? configuredRule
            : new CommandFileStorageRule
            {
                Folder = CombineSegments(defaultRule.Folder, commandName),
                IncludeRequestIdSegment = defaultRule.IncludeRequestIdSegment
            };

        var baseFolder = string.IsNullOrWhiteSpace(rule.Folder)
            ? CombineSegments(defaultRule.Folder, commandName)
            : rule.Folder;

        return BuildFolder(options.RootFolder, baseFolder, rule.IncludeRequestIdSegment ? requestId : null);
    }

    public string ResolveAdminFilePodFolder(string fileType, string? requestId = null)
    {
        var options = _options.CurrentValue;
        var rule = options.AdminFilePod ?? new CommandFileStorageRule();
        var baseFolder = string.IsNullOrWhiteSpace(rule.Folder)
            ? "admin/files-pod"
            : rule.Folder;

        return BuildFolder(options.RootFolder, CombineSegments(baseFolder, fileType), rule.IncludeRequestIdSegment ? requestId : null);
    }

    private static string BuildFolder(string? rootFolder, string? baseFolder, string? requestId)
    {
        var segments = new List<string>();
        AddSegment(segments, rootFolder);
        AddSegment(segments, baseFolder);
        AddSegment(segments, requestId);

        return string.Join('/', segments);
    }

    private static string CombineSegments(string? first, string? second)
    {
        var segments = new List<string>();
        AddSegment(segments, first);
        AddSegment(segments, second);
        return string.Join('/', segments);
    }

    private static void AddSegment(List<string> segments, string? value)
    {
        var normalized = NormalizeSegment(value);
        if (!string.IsNullOrWhiteSpace(normalized))
        {
            segments.Add(normalized);
        }
    }

    private static string NormalizeSegment(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Trim().Replace(' ', '-').Replace('\\', '-').Replace('/', '-');
        var buffer = new char[normalized.Length];
        var index = 0;

        foreach (var ch in normalized)
        {
            if (char.IsLetterOrDigit(ch) || ch is '-' or '_' or '.')
            {
                buffer[index++] = ch;
            }
        }

        return index == 0 ? string.Empty : new string(buffer, 0, index);
    }
}
