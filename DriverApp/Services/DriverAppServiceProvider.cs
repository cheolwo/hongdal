namespace DriverApp.Services;

public static class DriverAppServiceProvider
{
    private static IServiceProvider? _services;

    public static IServiceProvider Services =>
        _services ?? throw new InvalidOperationException("DriverApp 서비스 공급자가 아직 초기화되지 않았습니다.");

    public static void Initialize(IServiceProvider services)
    {
        _services = services;
    }
}
