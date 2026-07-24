using Microsoft.AspNetCore.DataProtection;
using 살뜰.Infrastructure.Security;

namespace Ssalddel.Tests.Infrastructure.Security;

public sealed class DataProtectionPersonalDataProtectorTests
{
    [Fact]
    public void ProtectAndUnprotect_RoundTripsWithSameKeyRing()
    {
        var provider = new EphemeralDataProtectionProvider();
        var service = new DataProtectionPersonalDataEncryptionService(provider);

        var protectedValue = service.Protect("010-1234-5678");
        var plainValue = service.Unprotect(protectedValue);

        Assert.NotEqual("010-1234-5678", protectedValue);
        Assert.Equal("010-1234-5678", plainValue);
    }

    [Fact]
    public void Unprotect_ThrowsWhenKeyRingCannotDecryptCiphertext()
    {
        var writer = new DataProtectionPersonalDataEncryptionService(
            new EphemeralDataProtectionProvider());
        var reader = new DataProtectionPersonalDataEncryptionService(
            new EphemeralDataProtectionProvider());
        var protectedValue = writer.Protect("private-value");

        var exception = Assert.Throws<InvalidOperationException>(
            () => reader.Unprotect(protectedValue));

        Assert.Contains("Data Protection", exception.Message, StringComparison.Ordinal);
        Assert.NotNull(exception.InnerException);
    }
}
