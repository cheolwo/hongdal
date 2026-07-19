using System.Reflection;
using Ssalddel.Contracts.Common.Privacy;

namespace 살뜰.Infrastructure.Security;

public sealed class IsmsPProtectedDataStorePreparationService : IIsmsPProtectedDataStorePreparationService
{
    private readonly IIsmsPProtectedDataCryptoService cryptoService;

    public IsmsPProtectedDataStorePreparationService(IIsmsPProtectedDataCryptoService cryptoService)
    {
        this.cryptoService = cryptoService;
    }

    public IsmsPProtectedObjectResult<T> PrepareForStorage<T>(T value)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(value);

        var type = typeof(T);
        var members = IsmsPProtectedDataAttributeReader.Read(type);
        if (members.Count == 0)
        {
            return new IsmsPProtectedObjectResult<T>(value, []);
        }

        var copy = CreateWritableCopy(value);
        var protectedMembers = new List<IsmsPProtectedMemberValue>();

        foreach (var member in members)
        {
            var property = type.GetProperty(member.PropertyName);
            if (property is null || property.PropertyType != typeof(string) || !property.CanRead || !property.CanWrite)
            {
                continue;
            }

            var originalValue = property.GetValue(value) as string;
            if (string.IsNullOrEmpty(originalValue))
            {
                continue;
            }

            var protectedValue = ProtectMember(member, originalValue);
            property.SetValue(copy, protectedValue.StoredValue);
            protectedMembers.Add(new IsmsPProtectedMemberValue(
                member.PropertyName,
                member.FieldKey,
                originalValue,
                protectedValue.StoredValue,
                ResolveActionCodes(member),
                protectedValue.AlgorithmCode));
        }

        return new IsmsPProtectedObjectResult<T>(copy, protectedMembers);
    }

    private IsmsPProtectedValue ProtectMember(
        IsmsPProtectedDataMember member,
        string originalValue)
    {
        var actionCodes = ResolveActionCodes(member);
        if (RequiresEncryptionAtRest(member, actionCodes))
        {
            return cryptoService.EncryptAtRest(member.FieldKey, originalValue);
        }

        if (RequiresEvidenceHash(member, actionCodes))
        {
            return cryptoService.HashForEvidence(member.FieldKey, originalValue);
        }

        return new IsmsPProtectedValue(
            member.FieldKey,
            PersonalDataProtectionActionCode.PurposeLimitedCollection,
            "PLAIN-CLASSIFIED",
            originalValue);
    }

    private static bool RequiresEncryptionAtRest(
        IsmsPProtectedDataMember member,
        IReadOnlyList<string> actionCodes)
        => string.Equals(member.FieldRule?.StorageProtectionCode, PersonalDataStorageProtectionCode.EncryptAtRest, StringComparison.OrdinalIgnoreCase) ||
            actionCodes.Contains(PersonalDataProtectionActionCode.EncryptAtRest, StringComparer.OrdinalIgnoreCase);

    private static bool RequiresEvidenceHash(
        IsmsPProtectedDataMember member,
        IReadOnlyList<string> actionCodes)
        => string.Equals(member.FieldRule?.StorageProtectionCode, PersonalDataStorageProtectionCode.HashForEvidence, StringComparison.OrdinalIgnoreCase) ||
            actionCodes.Contains(PersonalDataProtectionActionCode.HashForEvidence, StringComparer.OrdinalIgnoreCase);

    private static IReadOnlyList<string> ResolveActionCodes(IsmsPProtectedDataMember member)
    {
        var explicitCodes = member.RequiredActionCodes
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();

        if (explicitCodes.Length > 0)
        {
            return explicitCodes;
        }

        return member.FieldRule?.RequiredActionCodes ?? [];
    }

    private static T CreateWritableCopy<T>(T value)
        where T : class
    {
        var type = typeof(T);
        var constructor = type.GetConstructor(Type.EmptyTypes);
        if (constructor is null)
        {
            throw new InvalidOperationException($"{type.FullName} must have a parameterless constructor to prepare ISMS-P protected storage copy.");
        }

        var copy = (T)constructor.Invoke(null);
        foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (!property.CanRead || !property.CanWrite)
            {
                continue;
            }

            property.SetValue(copy, property.GetValue(value));
        }

        return copy;
    }
}
