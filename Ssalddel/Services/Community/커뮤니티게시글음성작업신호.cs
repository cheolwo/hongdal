using System.Threading.Channels;

namespace Ssalddel.Services.Community;

public interface I커뮤니티게시글음성작업신호
{
    void 알림();
    Task 대기Async(TimeSpan timeout, CancellationToken cancellationToken);
}

public sealed class 커뮤니티게시글음성작업신호 : I커뮤니티게시글음성작업신호
{
    private readonly Channel<bool> _channel = Channel.CreateBounded<bool>(new BoundedChannelOptions(1)
    {
        FullMode = BoundedChannelFullMode.DropWrite,
        SingleReader = true,
        SingleWriter = false
    });

    public void 알림()
        => _channel.Writer.TryWrite(true);

    public async Task 대기Async(TimeSpan timeout, CancellationToken cancellationToken)
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
