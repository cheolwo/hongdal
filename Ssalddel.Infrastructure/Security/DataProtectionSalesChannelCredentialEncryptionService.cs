using Microsoft.AspNetCore.DataProtection;

namespace 살뜰.Infrastructure.Security;

public sealed class DataProtectionSalesChannelCredentialEncryptionService
    : ISalesChannelCredentialEncryptionService
{
    private const string Prefix = "sales-credential:v1:";
    private readonly IDataProtector _protector;

    public DataProtectionSalesChannelCredentialEncryptionService(
        IDataProtectionProvider dataProtectionProvider)
    {
        _protector = dataProtectionProvider.CreateProtector(
            "Ssalddel.SalesChannelCredential.v1");
    }

    public string Protect(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return IsProtected(value)
            ? value
            : Prefix + _protector.Protect(value);
    }

    public string Unprotect(string protectedValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(protectedValue);
        if (!IsProtected(protectedValue))
        {
            throw new InvalidOperationException(
                "판매채널 자격증명이 암호화된 형식이 아닙니다. 새 자격증명으로 다시 저장해 주세요.");
        }

        try
        {
            return _protector.Unprotect(protectedValue[Prefix.Length..]);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "판매채널 자격증명을 복호화하지 못했습니다. Data Protection 키 구성을 확인해 주세요.",
                ex);
        }
    }

    public bool IsProtected(string value)
        => value.StartsWith(Prefix, StringComparison.Ordinal);
}
