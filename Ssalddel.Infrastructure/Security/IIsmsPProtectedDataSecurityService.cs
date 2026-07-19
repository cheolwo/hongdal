using Ssalddel.Contracts.Common.Privacy;

namespace 살뜰.Infrastructure.Security;

public sealed record IsmsPProtectedValue(
    string FieldKey,
    string ProtectionActionCode,
    string AlgorithmCode,
    string StoredValue);

public sealed record IsmsPProtectedMemberValue(
    string PropertyName,
    string FieldKey,
    string OriginalValue,
    string StoredValue,
    IReadOnlyList<string> AppliedActionCodes,
    string AlgorithmCode);

public sealed record IsmsPProtectedObjectResult<T>(
    T Value,
    IReadOnlyList<IsmsPProtectedMemberValue> ProtectedMembers);

public sealed record IsmsPProtectedResponseMemberValue(
    string PropertyName,
    string FieldKey,
    IReadOnlyList<string> AppliedActionCodes,
    string AlgorithmCode,
    bool WasDecrypted,
    bool WasMasked);

public sealed record IsmsPProtectedResponseResult<T>(
    T Value,
    IReadOnlyList<IsmsPProtectedResponseMemberValue> ProtectedMembers);

public interface IIsmsPProtectedDataCryptoService
{
    IsmsPProtectedValue EncryptAtRest(string fieldKey, string value);

    string DecryptAtRest(string fieldKey, string storedValue);

    IsmsPProtectedValue HashForEvidence(string fieldKey, string value);
}

public interface IIsmsPProtectedDataStorePreparationService
{
    IsmsPProtectedObjectResult<T> PrepareForStorage<T>(T value)
        where T : class;
}

public interface IIsmsPProtectedDataResponsePreparationService
{
    IsmsPProtectedResponseResult<T> PrepareForResponse<T>(T value, bool revealProtectedValues = false)
        where T : class;
}

public interface IIsmsPClientTransportProtectionService
{
    IsmsPClientEncryptionPublicKeyResponse GetPublicKey();

    IsmsPDecryptedTransportPayload Decrypt(IsmsPEncryptedTransportEnvelope envelope);
}
