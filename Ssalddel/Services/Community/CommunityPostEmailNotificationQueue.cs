using System.Collections.Concurrent;
using System.Threading.Channels;
using Microsoft.Extensions.Options;
using 살뜰.Services.Options;

namespace Ssalddel.Services.Community;

public sealed record CommunityPostEmailNotificationWorkItem(long PostId, int Attempt);

public interface ICommunityPostEmailNotificationQueue
{
    bool TryEnqueue(long postId);
    IAsyncEnumerable<CommunityPostEmailNotificationWorkItem> ReadAllAsync(
        CancellationToken cancellationToken);
    ValueTask RetryAsync(
        CommunityPostEmailNotificationWorkItem workItem,
        CancellationToken cancellationToken);
    void Complete(long postId);
}

public sealed class CommunityPostEmailNotificationQueue : ICommunityPostEmailNotificationQueue
{
    private readonly Channel<CommunityPostEmailNotificationWorkItem> _channel;
    private readonly ConcurrentDictionary<long, byte> _pendingPostIds = new();

    public CommunityPostEmailNotificationQueue(
        IOptions<CommunityPostEmailNotificationOptions> options)
    {
        var capacity = Math.Clamp(options.Value.QueueCapacity, 1, 10_000);
        _channel = Channel.CreateBounded<CommunityPostEmailNotificationWorkItem>(
            new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false
            });
    }

    public bool TryEnqueue(long postId)
    {
        if (postId <= 0)
        {
            return false;
        }

        if (!_pendingPostIds.TryAdd(postId, 0))
        {
            return true;
        }

        if (_channel.Writer.TryWrite(new(postId, 1)))
        {
            return true;
        }

        _pendingPostIds.TryRemove(postId, out _);
        return false;
    }

    public IAsyncEnumerable<CommunityPostEmailNotificationWorkItem> ReadAllAsync(
        CancellationToken cancellationToken)
        => _channel.Reader.ReadAllAsync(cancellationToken);

    public ValueTask RetryAsync(
        CommunityPostEmailNotificationWorkItem workItem,
        CancellationToken cancellationToken)
        => _channel.Writer.WriteAsync(workItem, cancellationToken);

    public void Complete(long postId)
        => _pendingPostIds.TryRemove(postId, out _);
}
