namespace 홍달.Infrastructure.Security
{
    public interface IPersonalDataEncryptionService
    {
        string? Protect(string? value);
        string? Unprotect(string? value);
    }
}