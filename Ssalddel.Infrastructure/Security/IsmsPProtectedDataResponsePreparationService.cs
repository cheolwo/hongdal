using System.Reflection;
using Ssalddel.Contracts.Common.Privacy;

namespace 살뜰.Infrastructure.Security;

public sealed class IsmsPProtectedDataResponsePreparationService : IIsmsPProtectedDataResponsePreparationService
{
    private readonly IIsmsPProtectedDataCryptoService cryptoService;

    public IsmsPProtectedDataResponsePreparationService(IIsmsPProtectedDataCryptoService cryptoService)
    {
        this.cryptoService = cryptoService;
    }

    public IsmsPProtectedResponseResult<T> PrepareForResponse<T>(T value, bool revealProtectedValues = false)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(value);

        var type = typeof(T);
        var members = IsmsPProtectedDataAttributeReader.Read(type);
        if (members.Count == 0)
        {
            return new IsmsPProtectedResponseResult<T>(value, []);
        }

        var copy = CreateWritableCopy(value);
        var protectedMembers = new List<IsmsPProtectedResponseMemberValue>();

        foreach (var member in members)
        {
            var property = type.GetProperty(member.PropertyName);
            if (property is null || property.PropertyType != typeof(string) || !property.CanRead || !property.CanWrite)
            {
                continue;
            }

            var storedValue = property.GetValue(value) as string;
            if (string.IsNullOrEmpty(storedValue))
            {
                continue;
            }

            var actionCodes = ResolveActionCodes(member);
            var decryptedValue = cryptoService.DecryptAtRest(member.FieldKey, storedValue);
            var wasDecrypted = !string.Equals(storedValue, decryptedValue, StringComparison.Ordinal);
            var shouldMask = !revealProtectedValues &&
                actionCodes.Contains(PersonalDataProtectionActionCode.MaskByDefault, StringComparer.OrdinalIgnoreCase);
            var responseValue = shouldMask
                ? MaskValue(member.FieldKey, decryptedValue)
                : decryptedValue;

            property.SetValue(copy, responseValue);
            protectedMembers.Add(new IsmsPProtectedResponseMemberValue(
                member.PropertyName,
                member.FieldKey,
                actionCodes,
                ResolveAlgorithmCode(storedValue, wasDecrypted),
                wasDecrypted,
                shouldMask));
        }

        return new IsmsPProtectedResponseResult<T>(copy, protectedMembers);
    }

    private static string ResolveAlgorithmCode(string storedValue, bool wasDecrypted)
    {
        if (wasDecrypted)
        {
            return AesGcmIsmsPProtectedDataCryptoService.EncryptionAlgorithmCode;
        }

        if (storedValue.StartsWith(AesGcmIsmsPProtectedDataCryptoService.HashPrefix, StringComparison.Ordinal))
        {
            return AesGcmIsmsPProtectedDataCryptoService.HashAlgorithmCode;
        }

        return "PLAIN-CLASSIFIED";
    }

    private static string MaskValue(string fieldKey, string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        if (string.Equals(fieldKey, PersonalDataFieldKey.PhoneNumber, StringComparison.OrdinalIgnoreCase))
        {
            var last4 = LastDigits(value, 4);
            return string.IsNullOrEmpty(last4) ? "****" : $"***-****-{last4}";
        }

        if (string.Equals(fieldKey, PersonalDataFieldKey.BankAccountNumber, StringComparison.OrdinalIgnoreCase))
        {
            var last4 = LastDigits(value, 4);
            return string.IsNullOrEmpty(last4) ? "****" : $"****{last4}";
        }

        if (string.Equals(fieldKey, PersonalDataFieldKey.Email, StringComparison.OrdinalIgnoreCase))
        {
            var atIndex = value.IndexOf('@', StringComparison.Ordinal);
            if (atIndex <= 1)
            {
                return "***";
            }

            return $"{value[0]}***{value[atIndex..]}";
        }

        if (string.Equals(fieldKey, PersonalDataFieldKey.DetailedAddress, StringComparison.OrdinalIgnoreCase))
        {
            return "상세 주소 비공개";
        }

        if (string.Equals(fieldKey, PersonalDataFieldKey.LocationCoordinate, StringComparison.OrdinalIgnoreCase))
        {
            return "위치 좌표 비공개";
        }

        if (string.Equals(fieldKey, PersonalDataFieldKey.ContractDocument, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(fieldKey, PersonalDataFieldKey.ElectronicSignatureEvidence, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(fieldKey, PersonalDataFieldKey.DeliveryCompletionPhoto, StringComparison.OrdinalIgnoreCase))
        {
            return "보호 증적 비공개";
        }

        if (value.Length <= 2)
        {
            return "***";
        }

        return $"{value[0]}***{value[^1]}";
    }

    private static string LastDigits(string value, int count)
    {
        var digits = value.Where(char.IsDigit).ToArray();
        if (digits.Length == 0)
        {
            return string.Empty;
        }

        return new string(digits.TakeLast(count).ToArray());
    }

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
            throw new InvalidOperationException($"{type.FullName} must have a parameterless constructor to prepare ISMS-P protected response copy.");
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
