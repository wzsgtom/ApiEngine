namespace ApiEngine.Application.Services;

/// <summary>
///     基础服务
/// </summary>
[AllowAnonymous]
public class BaseService : IDynamicApiController, ITransient
{
    private readonly ISqlSugarClient _db;
    private readonly IEventPublisher _eventPublisher;

    public BaseService(ISqlSugarClient db, IEventPublisher eventPublisher)
    {
        _db = db;
        _eventPublisher = eventPublisher;
    }

    /// <summary>
    ///     服务器日期时间
    /// </summary>
    /// <returns></returns>
    public DateTime GetDate()
    {
        return _db.GetDate();
    }

    /// <summary>
    ///     TestEvent
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task TestEvent(string name)
    {
        await _eventPublisher.PublishAsync(new ChannelEventSource("Base:Test", name));
        await _eventPublisher.PublishDelayAsync(new ChannelEventSource("Base:Test", name), 3000);
    }
}