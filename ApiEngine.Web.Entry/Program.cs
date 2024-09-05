using Serilog;
using ServiceSelf;

// 为.NET 泛型主机的应用程序提供自安装为服务进程的能力，支持windows和linux平台
if (Service.UseServiceSelf(args, "API服务"))
{
    var builder = WebApplication.CreateBuilder(args).Inject();
    builder.Host.UseServiceSelf();

    builder.Host.UseSerilog((context, services, config) =>
        config.ReadFrom.Configuration(context.Configuration).ReadFrom.Services(services).Enrich.FromLogContext());

    var app = builder.Build();
    app.Run();
}