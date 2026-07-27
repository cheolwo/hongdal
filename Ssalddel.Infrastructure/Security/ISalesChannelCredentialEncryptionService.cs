namespace 살뜰.Infrastructure.Security;

public interface ISalesChannelCredentialEncryptionService
{
    string Protect(string value);
    string Unprotect(string protectedValue);
    bool IsProtected(string value);
}
