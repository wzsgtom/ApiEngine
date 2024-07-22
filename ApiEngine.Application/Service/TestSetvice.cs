using Furion.DependencyInjection;
using Furion.DynamicApiController;
using Furion.EventBus;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace ApiEngine.Application.Service;

/// <summary>
///     测试
/// </summary>
[ApiDescriptionSettings(Name = "test")]
public class TestSetvice(IEventPublisher eventP) : IDynamicApiController, IScoped
{
    public object Test(bool gc = false)
    {
        if (gc) GC.Collect();

        var process = Process.GetCurrentProcess();

        var workingSet = process.WorkingSet64; // 工作集大小（字节）
        var privateBytes = process.PrivateMemorySize64; // 私有的内存大小（字节）

        return new { workingSetMb = workingSet / 1024.0 / 1024.0, privateBytesMb = privateBytes / 1024.0 / 1024.0 };
    }

    public async Task TestPublishEvent([Required] string name)
    {
        await eventP.PublishAsync(new ChannelEventSource("Base:Test", name));
        await eventP.PublishDelayAsync(new ChannelEventSource("Base:Test", name), 3000);
    }
}