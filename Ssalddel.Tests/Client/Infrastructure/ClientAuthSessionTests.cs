using Ssalddel.Client.Infrastructure.Security;

namespace Ssalddel.Tests.Client.Infrastructure;

public sealed class ClientAuthSessionTests
{
    [Fact]
    public async Task 유효한AccessToken은_인증세션으로복원한다()
    {
        var store = new RecordingTokenStore
        {
            Snapshot = CreateSnapshot(
                accessExpiresAtUtc: DateTime.UtcNow.AddMinutes(30),
                refreshExpiresAtUtc: DateTime.UtcNow.AddDays(7))
        };
        var session = new ClientAuthSession(store, new ClientSessionGuard());

        var state = await session.RestoreAsync();

        Assert.Equal(ClientAuthSessionRestoreState.Authenticated, state);
        Assert.True(session.IsAuthenticated);
        Assert.Equal("access-token", session.AccessToken);
        Assert.Equal("warehouse-user", session.UserName);
        Assert.Contains("창고관리자", session.Roles);
        Assert.Equal(0, store.ClearCount);
    }

    [Fact]
    public async Task AccessToken만료와유효한RefreshToken은_갱신필요상태로복원한다()
    {
        var store = new RecordingTokenStore
        {
            Snapshot = CreateSnapshot(
                accessExpiresAtUtc: DateTime.UtcNow.AddMinutes(-10),
                refreshExpiresAtUtc: DateTime.UtcNow.AddDays(1))
        };
        var session = new ClientAuthSession(store, new ClientSessionGuard());

        var state = await session.RestoreAsync();

        Assert.Equal(ClientAuthSessionRestoreState.RefreshRequired, state);
        Assert.False(session.IsAuthenticated);
        Assert.Equal("refresh-token", session.RefreshToken);
        Assert.Equal("warehouse-user-id", session.UserId);
        Assert.Equal(0, store.ClearCount);
    }

    [Fact]
    public async Task 모든Token이만료되면_저장소와메모리세션을비운다()
    {
        var store = new RecordingTokenStore
        {
            Snapshot = CreateSnapshot(
                accessExpiresAtUtc: DateTime.UtcNow.AddHours(-1),
                refreshExpiresAtUtc: DateTime.UtcNow.AddMinutes(-1))
        };
        var session = new ClientAuthSession(store, new ClientSessionGuard());

        var state = await session.RestoreAsync();

        Assert.Equal(ClientAuthSessionRestoreState.Anonymous, state);
        Assert.False(session.IsAuthenticated);
        Assert.Null(session.AccessToken);
        Assert.Null(session.RefreshToken);
        Assert.Equal(1, store.ClearCount);
    }

    [Fact]
    public async Task 새Token적용은_인증상태와보안저장소를함께갱신한다()
    {
        var store = new RecordingTokenStore();
        var session = new ClientAuthSession(store, new ClientSessionGuard());
        var snapshot = CreateSnapshot(
            accessExpiresAtUtc: DateTime.UtcNow.AddMinutes(30),
            refreshExpiresAtUtc: DateTime.UtcNow.AddDays(7));

        await session.ApplyAsync(snapshot);

        Assert.True(session.IsAuthenticated);
        Assert.Same(snapshot, store.SavedSnapshot);
        Assert.Equal("warehouse-user-id", session.UserId);
    }

    private static ClientAuthTokenSnapshot CreateSnapshot(
        DateTime accessExpiresAtUtc,
        DateTime refreshExpiresAtUtc)
        => new(
            "access-token",
            accessExpiresAtUtc,
            "refresh-token",
            refreshExpiresAtUtc,
            "warehouse-user-id",
            "warehouse-user",
            ["창고관리자"]);

    private sealed class RecordingTokenStore : IClientSecureTokenStore
    {
        public ClientAuthTokenSnapshot? Snapshot { get; init; }
        public ClientAuthTokenSnapshot? SavedSnapshot { get; private set; }
        public int ClearCount { get; private set; }

        public Task<ClientAuthTokenSnapshot?> LoadAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Snapshot);

        public Task SaveAsync(
            ClientAuthTokenSnapshot snapshot,
            CancellationToken cancellationToken = default)
        {
            SavedSnapshot = snapshot;
            return Task.CompletedTask;
        }

        public Task ClearAsync(CancellationToken cancellationToken = default)
        {
            ClearCount++;
            return Task.CompletedTask;
        }
    }
}
