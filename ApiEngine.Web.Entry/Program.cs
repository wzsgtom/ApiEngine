using ApiEngine.Core.Extension;
using ServiceSelf;

// 为.NET 泛型主机的应用程序提供自安装为服务进程的能力，支持windows和linux平台
if (Service.UseServiceSelf(args, "曦航综合服务平台"))
{
    var builder = WebApplication.CreateBuilder(args).Inject();
    builder.Host.UseServiceSelf().UseOwnSerilog();

    builder.Configuration.Get<WebHostBuilder>()?.ConfigureKestrel(u =>
    {
        u.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(30);
        u.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(30);
        u.Limits.MaxRequestBodySize = null;
    });

    var app = builder.Build();
    app.Run();
}