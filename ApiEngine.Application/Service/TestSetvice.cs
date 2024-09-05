﻿using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Furion.DependencyInjection;
using Furion.DynamicApiController;
using Furion.EventBus;
using Furion.FriendlyException;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewLife.Caching;

namespace ApiEngine.Application.Service;

/// <summary>
///     测试
/// </summary>
[ApiDescriptionSettings(Name = "test")]
[AllowAnonymous]
public class TestSetvice(IEventPublisher eventP, ICache cache) : IDynamicApiController, IScoped
{
    public void InitBuy()
    {
        cache.Set("Xxx", 1000);
    }

    public string Buy()
    {
        const string key = "Xxx";
        using var ck = cache.AcquireLock(key + "Lock", 3000);

        if (cache.Get<int>(key) <= 0) throw Oops.Bah("缺货！！！");

        cache.Decrement(key, 1);
        return "下单成功";
    }

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