namespace RestaurantDeskApp.Services;

public sealed class 주문알림Service : I주문알림Service
{
    public Task 신규주문알림재생Async()
    {
#if WINDOWS
        System.Media.SystemSounds.Exclamation.Play();
#endif
        return Task.CompletedTask;
    }
}
