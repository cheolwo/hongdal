using Microsoft.AspNetCore.DataProtection;
using 살뜰.Infrastructure.Security;

namespace Ssalddel.Tests.Infrastructure.Security;

public sealed class DataProtectionSalesChannelCredentialEncryptionServiceTests
{
    [Fact]
    public void 자격증명은_전용Purpose로암복호화되고_평문복호화를거부한다()
    {
        var service = new DataProtectionSalesChannelCredentialEncryptionService(
            new EphemeralDataProtectionProvider());

        var protectedValue = service.Protect("""{"secretKey":"private"}""");

        Assert.True(service.IsProtected(protectedValue));
        Assert.DoesNotContain("private", protectedValue, StringComparison.Ordinal);
        Assert.Equal("""{"secretKey":"private"}""", service.Unprotect(protectedValue));
        Assert.Throws<InvalidOperationException>(() => service.Unprotect("plain-secret"));
    }
}
