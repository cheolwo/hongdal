using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Ssalddel.Services.Storage;
using Ssalddel.Services.Storage.Azure;
using 살뜰.Services.Options;

namespace Ssalddel.Tests.Services.Storage;

public sealed class ObjectStorageServiceTests
{
    [Fact]
    public void Azure_adapter_constructor_does_not_resolve_managed_identity()
    {
        var options = Options.Create(new AzureBlobStorageOptions
        {
            ServiceUri = "https://storage.example.test"
        });

        var exception = Record.Exception(() => new AzureBlobStorageService(options));

        Assert.Null(exception);
    }

    [Fact]
    public async Task Development_adapter_exposes_only_public_objects_through_static_url()
    {
        using var environment = new TemporaryHostEnvironment();
        var service = new DevelopmentLocalStorageService(
            environment,
            new HttpContextAccessor());

        await using var publicStream = new MemoryStream([1, 2, 3]);
        await using var privateStream = new MemoryStream([4, 5, 6]);
        var publicObject = await service.UploadAsync(
            publicStream,
            "public.png",
            "image/png",
            "community/posts/1",
            ObjectStorageAccess.Public);
        var privateObject = await service.UploadAsync(
            privateStream,
            "private.png",
            "image/png",
            "evidence/1",
            ObjectStorageAccess.Private);

        Assert.Equal(DevelopmentLocalStorageService.PublicContainerName, publicObject.ContainerName);
        Assert.StartsWith(DevelopmentLocalStorageService.PublicRequestPath, publicObject.Url);
        Assert.Equal(DevelopmentLocalStorageService.PrivateContainerName, privateObject.ContainerName);
        Assert.StartsWith("local-storage-private://", privateObject.Url);
        Assert.Equal([1, 2, 3], await service.DownloadAsync(publicObject.ContainerName, publicObject.ObjectName));
        Assert.Equal([4, 5, 6], await service.DownloadAsync(privateObject.ContainerName, privateObject.ObjectName));
    }

    private sealed class TemporaryHostEnvironment : IHostEnvironment, IDisposable
    {
        public TemporaryHostEnvironment()
        {
            ContentRootPath = Path.Combine(Path.GetTempPath(), "ssalddel-object-storage-tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(ContentRootPath);
            ContentRootFileProvider = new PhysicalFileProvider(ContentRootPath);
        }

        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "Ssalddel.Tests";
        public string ContentRootPath { get; set; }
        public IFileProvider ContentRootFileProvider { get; set; }

        public void Dispose()
        {
            (ContentRootFileProvider as IDisposable)?.Dispose();
            if (Directory.Exists(ContentRootPath))
            {
                Directory.Delete(ContentRootPath, recursive: true);
            }
        }
    }
}
