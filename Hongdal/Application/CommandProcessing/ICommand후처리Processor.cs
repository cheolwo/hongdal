using 홍달.Services.Options;

namespace Hongdal.Application.CommandProcessing;

public interface ICommand후처리Processor
{
    string Name { get; }

    int Order { get; }

    bool CanProcess(Command후처리Context context);

    Task ProcessAsync(Command후처리Context context, CancellationToken cancellationToken);
}
