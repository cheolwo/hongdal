namespace 살뜰.Infrastructure.Security
{
    public interface IPersonalDataEncryptionService
    {
        string? Protect(string? value);
        string? Unprotect(string? value);
    }
}
