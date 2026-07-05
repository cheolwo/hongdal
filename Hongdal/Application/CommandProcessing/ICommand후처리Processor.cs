using 홍달.Services.Options;

namespace Hongdal.Application.CommandProcessing;

public interface ICommand후처리Processor
{
    string Name { get; }

    bool CanProcess(CommandProcessingRule rule);

    Task ProcessAsync(Command후처리Context context, CancellationToken cancellationToken);
}
