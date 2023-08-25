namespace ApiEngine.Application.EventBus;

public class BaseEventSubscriber : IEventSubscriber, ISingleton
{
    [EventSubscribe("Base:Test")]
    public async Task Test(EventHandlerExecutingContext context)
    {
        await Task.Delay(3000);
        context.Source.ToJson().LogInformation();
    }
}