using Microsoft.Extensions.Options;
using Ssalddel.Services.Storage;
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

    [Fact]
    public void Public_and_private_buckets_can_be_configured_separately()
    {
        var service = new GoogleCloudStorageService(Options.Create(new GoogleCloudStorageOptions
        {
            PublicBucketName = "community-public",
            PrivateBucketName = "platform-private"
        }));

        Assert.True(service.IsConfigured(ObjectStorageAccess.Public));
        Assert.True(service.IsConfigured(ObjectStorageAccess.Private));
    }
}
