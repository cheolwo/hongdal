using Ssalddel.Domain.AgriculturalFisheries;
using Ssalddel.Domain.Community;
using Ssalddel.Domain.TraditionalMarkets;
using Microsoft.EntityFrameworkCore;
using 살뜰.Data;
using 살뜰.Infrastructure.Security;

namespace Ssalddel.Tests.Infrastructure.Persistence;

public sealed class SsalddelContextModelIsolationTests
{
    [Fact]
    public void MainContext_ExcludesEntitiesOwnedByDedicatedContexts()
    {
        var options = new DbContextOptionsBuilder<SsalddelContext>()
            .UseInMemoryDatabase($"ssalddel-context-isolation-{Guid.NewGuid():N}")
            .Options;
        using var context = new SsalddelContext(
            options,
            new DummyPersonalDataEncryptionService());

        Assert.NotNull(context.Model.FindEntityType(typeof(PlatformCommunityPost)));
        Assert.Null(context.Model.FindEntityType(typeof(KamisPriceObservation)));
        Assert.Null(context.Model.FindEntityType(typeof(UsdaNassPriceObservation)));
        Assert.Null(context.Model.FindEntityType(typeof(TraditionalMarket)));
        Assert.Null(context.Model.FindEntityType(typeof(TraditionalMarketSyncRun)));
    }

    private sealed class DummyPersonalDataEncryptionService : IPersonalDataEncryptionService
    {
        public string? Protect(string? value) => value;

        public string? Unprotect(string? value) => value;
    }
}
