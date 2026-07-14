using Microsoft.Extensions.Options;
using 홍달.Services.External.Google;
using 홍달.Services.Options;

namespace Hongdal.Tests.Services.External;

public sealed class GoogleCloudStorageServiceTests
{
    [Fact]
    public void Constructor_DoesNotResolveApplicationDefaultCredentials()
    {
        var options = Options.Create(new GoogleCloudStorageOptions());

        var exception = Record.Exception(() => new GoogleCloudStorageService(options));

        Assert.Null(exception);
    }
}
