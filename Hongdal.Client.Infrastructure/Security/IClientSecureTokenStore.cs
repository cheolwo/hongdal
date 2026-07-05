namespace Hongdal.Client.Infrastructure.Security;

public interface IClientSecureTokenStore
{
    Task<ClientAuthTokenSnapshot?> LoadAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(ClientAuthTokenSnapshot snapshot, CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);
}
