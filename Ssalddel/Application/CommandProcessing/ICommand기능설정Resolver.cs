using 살뜰.Services.Options;

namespace Ssalddel.Application.CommandProcessing;

public interface ICommand기능설정Resolver
{
    Task<CommandProcessingRule> ResolveAsync(string commandName, CancellationToken cancellationToken);

    Task<CommandProcessingRule> ResolveGlobalRuleAsync(string commandName, CancellationToken cancellationToken);

    CommandProcessingRule GetDefaultRule(string commandName);

    void Invalidate(string userId, string commandName);
}
