using Hongdal.Client.Infrastructure;

namespace Hongdal.Tests.ClientInfrastructure;

public sealed class ClientDataModeOptionsTests
{
    [Fact]
    public void RequireServerLedgerForV1Smoke_disables_sample_fallbacks()
    {
        var options = new ClientDataModeOptions
        {
            AllowSampleFallback = true,
            AllowDevelopmentSnapshotFallback = true,
            RequireServerLedgerForV1Smoke = true
        };

        Assert.False(options.CanUseSampleFallback);
        Assert.False(options.CanUseDevelopmentSnapshotFallback);
    }

    [Fact]
    public void Non_strict_mode_respects_explicit_sample_fallback_flags()
    {
        var options = new ClientDataModeOptions
        {
            AllowSampleFallback = true,
            AllowDevelopmentSnapshotFallback = true,
            RequireServerLedgerForV1Smoke = false
        };

        Assert.True(options.CanUseSampleFallback);
        Assert.True(options.CanUseDevelopmentSnapshotFallback);
    }
}
