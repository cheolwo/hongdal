using 홍달.Services.Options;

namespace Hongdal.Application.CommandProcessing;

public interface ICommand기능설정Resolver
{
    Task<CommandProcessingRule> ResolveAsync(string commandName, CancellationToken cancellationToken);

    CommandProcessingRule GetDefaultRule(string commandName);

    void Invalidate(string userId, string commandName);
}
