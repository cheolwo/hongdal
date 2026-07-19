using Microsoft.Extensions.Options;
using 살뜰.Services.External.Google;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Services.External;

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
