using 살뜰.Services.Options;

namespace Ssalddel.Application.CommandProcessing;

public interface ICommand후처리Processor
{
    string Name { get; }

    int Order { get; }

    bool CanProcess(Command후처리Context context);

    Task ProcessAsync(Command후처리Context context, CancellationToken cancellationToken);
}
