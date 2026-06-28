using Microsoft.AspNetCore.DataProtection;

namespace 홍달.Infrastructure.Security
{
    public sealed class DataProtectionPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        private const string Prefix = "pd:v1:";
        private readonly IDataProtector _protector;

        public DataProtectionPersonalDataEncryptionService(IDataProtectionProvider dataProtectionProvider)
        {
            _protector = dataProtectionProvider.CreateProtector("Hongdal.PersonalData.v1");
        }

        public string? Protect(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            if (value.StartsWith(Prefix, StringComparison.Ordinal))
            {
                return value;
            }

            return Prefix + _protector.Protect(value);
        }

        public string? Unprotect(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            if (!value.StartsWith(Prefix, StringComparison.Ordinal))
            {
                return value;
            }

            var payload = value[Prefix.Length..];

            try
            {
                return _protector.Unprotect(payload);
            }
            catch
            {
                return value;
            }
        }
    }
}