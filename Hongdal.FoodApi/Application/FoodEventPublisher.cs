namespace Hongdal.FoodApi.Application;

public sealed class FoodEventPublisher : IFoodEventPublisher
{
    private readonly IServiceProvider _serviceProvider;

    public FoodEventPublisher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task PublishAsync<TEvent>(TEvent appEvent, CancellationToken cancellationToken = default)
        where TEvent : notnull
    {
        var handlers = _serviceProvider.GetServices<IFoodEventHandler<TEvent>>();
        foreach (var handler in handlers)
        {
            await handler.HandleAsync(appEvent, cancellationToken);
        }
    }
}
