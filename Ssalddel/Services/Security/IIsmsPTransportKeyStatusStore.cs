using Ssalddel.Contracts.Common.Privacy;

namespace Ssalddel.Services.Security;

public interface IIsmsPTransportKeyStatusStore
{
    Task MarkActiveAsync(
        IsmsPClientEncryptionPublicKeyResponse publicKey,
        CancellationToken cancellationToken = default);

    Task<bool> IsActiveAsync(
        string keyId,
        string algorithmCode,
        CancellationToken cancellationToken = default);
}
