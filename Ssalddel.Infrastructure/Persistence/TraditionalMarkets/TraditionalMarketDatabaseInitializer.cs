using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ssalddel.Infrastructure.Persistence.TraditionalMarkets;

public sealed class TraditionalMarketDatabaseInitializer : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TraditionalMarketDatabaseInitializer> _logger;

    public TraditionalMarketDatabaseInitializer(
        IServiceScopeFactory scopeFactory,
        ILogger<TraditionalMarketDatabaseInitializer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TraditionalMarketDbContext>();
        await db.Database.MigrateAsync(cancellationToken);
        _logger.LogInformation("전통시장 공공데이터 모듈 스키마를 확인했습니다.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
