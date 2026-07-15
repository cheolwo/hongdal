using System.Threading.Channels;

namespace Hongdal.Services.Community;

public interface ICommunityKeywordNotificationSignal
{
    void Notify();
    Task WaitAsync(TimeSpan timeout, CancellationToken cancellationToken);
}

public sealed class CommunityKeywordNotificationSignal : ICommunityKeywordNotificationSignal
{
    private readonly Channel<bool> _channel = Channel.CreateBounded<bool>(new BoundedChannelOptions(1)
    {
        FullMode = BoundedChannelFullMode.DropWrite,
        SingleReader = true,
        SingleWriter = false
    });

    public void Notify()
        => _channel.Writer.TryWrite(true);

    public async Task WaitAsync(TimeSpan timeout, CancellationToken cancellationToken)
    {
        var signalTask = _channel.Reader.WaitToReadAsync(cancellationToken).AsTask();
        var delayTask = Task.Delay(timeout, cancellationToken);
        var completed = await Task.WhenAny(signalTask, delayTask);
        if (completed == signalTask && await signalTask)
        {
            while (_channel.Reader.TryRead(out _))
            {
            }
        }
    }
}
