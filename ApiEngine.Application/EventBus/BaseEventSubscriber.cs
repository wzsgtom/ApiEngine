using Furion.DependencyInjection;
using Furion.EventBus;
using Furion.JsonSerialization;
using Furion.Logging.Extensions;

namespace ApiEngine.Application.EventBus;

public class BaseEventSubscriber(IJsonSerializerProvider jsonSerializer) : IEventSubscriber, ISingleton
{
    [EventSubscribe("Base:Test")]
    public async Task Test(EventHandlerExecutingContext context)
    {
        await Task.Delay(3000);
        jsonSerializer.Serialize(context.Source).LogInformation();
    }
}