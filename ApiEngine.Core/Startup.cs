using ApiEngine.Core.Aop;
using ApiEngine.Core.Converter;
using ApiEngine.Core.Extension;
using ApiEngine.Core.Gen;
using ApiEngine.Core.Handler;
using ApiEngine.Core.Option;
using ApiEngine.Core.Provider;
using AspNetCoreRateLimit;
using Furion;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Yitter.IdGenerator;

namespace ApiEngine.Core;

[AppStartup(99)]
public class Startup : AppStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // App配置
        services.AddConfigurableOptions<AppInfoOptions>();
        // 允许跨域
        services.AddCorsAccessor();
        // 限流服务
        services.SetRateLimit();
        // 设置缓存
        services.SetCache();
        // 设置数据库
        services.SetSqlSugar();
        // 远程请求
        services.AddRemoteRequest();
        // JWT授权
        services.AddJwt<JwtHandler>(enableGlobalAuthorize: App.GetOptions<AppInfoOptions>().GlobalAuthorize);
        // 审计
        services.AddMvcFilter<AuditFilter>();
        // 控制器.设置JSON.规范化结果
        services.AddControllers().AddNewtonsoftJson(jsonOptions =>
        {
            jsonOptions.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            jsonOptions.SerializerSettings.Converters.AddLongTypeConverters();
            jsonOptions.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
            jsonOptions.SerializerSettings.DateFormatString = GenConst.Time17Format;
            jsonOptions.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            jsonOptions.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            jsonOptions.SerializerSettings.Converters = new List<JsonConverter> { new StringLongConverter() };
        }).AddInjectWithUnifyResult<ResultProvider>();
        // 响应压缩
        services.SetResponseCompression();
        // 事件总线
        services.SetEventBus();
        // 任务队列
        services.SetTaskQueue();
        // 定时任务
        services.SetHangfire();

        // 配置Nginx转发获取客户端真实IP
        // 注1：如果负载均衡不是在本机通过 Loopback 地址转发请求的，一定要加上options.KnownNetworks.Clear()和options.KnownProxies.Clear()
        // 注2：如果设置环境变量 ASPNETCORE_FORWARDEDHEADERS_ENABLED 为 True，则不需要下面的配置代码
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.All;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

        // 跨域
        app.UseCorsAccessor();
        // 限流
        app.UseIpRateLimiting();
        app.UseClientRateLimiting();
        // 默认文件/静态文件
        app.UseDefaultFiles();
        app.UseStaticFiles();
        // 状态码拦截
        app.UseUnifyResultStatusCodes();
        // 重定向
        app.UseHttpsRedirection();
        // 路由
        app.UseRouting();
        // 认证授权
        app.UseAuthentication();
        app.UseAuthorization();
        // Furion 注入
        app.UseInject();
        // 响应压缩
        app.UseResponseCompression();

        app.UseEndpoints(endpoints => endpoints.MapControllers());
        // 任务看板
        app.UseHangfireDashboard();

        RunWith();
    }

    private static void RunWith()
    {
        YitIdHelper.SetIdGenerator(new IdGeneratorOptions(22));
    }
}