namespace Hongdal.FoodApi.Application;

public interface IFoodCommand<TResult>;

public interface IFoodCommandHandler<in TCommand, TResult>
    where TCommand : IFoodCommand<TResult>
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}

public interface IFoodEventHandler<in TEvent>
{
    Task HandleAsync(TEvent appEvent, CancellationToken cancellationToken = default);
}

public interface IFoodEventPublisher
{
    Task PublishAsync<TEvent>(TEvent appEvent, CancellationToken cancellationToken = default)
        where TEvent : notnull;
}
