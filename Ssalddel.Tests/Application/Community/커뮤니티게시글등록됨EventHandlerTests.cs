using Ssalddel.Application.Community;
using Ssalddel.Services.Community;

namespace Ssalddel.Tests.Application.Community;

public sealed class 커뮤니티게시글등록됨EventHandlerTests
{
    [Fact]
    public async Task Handle_음성Worker에_즉시처리신호를_보낸다()
    {
        var signal = new FakeSignal();
        var sut = new 커뮤니티게시글등록됨EventHandler(signal);

        await sut.Handle(new 커뮤니티게시글등록됨Event(15), CancellationToken.None);

        Assert.Equal(1, signal.Count);
    }

    private sealed class FakeSignal : I커뮤니티게시글음성작업신호
    {
        public int Count { get; private set; }

        public void 알림() => Count++;

        public Task 대기Async(TimeSpan timeout, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}
