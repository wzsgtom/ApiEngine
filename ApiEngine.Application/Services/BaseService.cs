namespace ApiEngine.Application.Services;

/// <summary>
///     基础服务
/// </summary>
[AllowAnonymous]
public class BaseService(ISqlSugarClient db, IEventPublisher eventPublisher) : IDynamicApiController, ITransient
{
    /// <summary>
    ///     服务器日期时间
    /// </summary>
    /// <returns></returns>
    public DateTime GetDate()
    {
        return db.GetDate();
    }

    /// <summary>
    ///     TestEvent
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task TestEvent(string name)
    {
        await eventPublisher.PublishAsync(new ChannelEventSource("Base:Test", name));
        await eventPublisher.PublishDelayAsync(new ChannelEventSource("Base:Test", name), 3000);
    }
}