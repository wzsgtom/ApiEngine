using Serilog;
using ServiceSelf;

// Ϊ.NET ����������Ӧ�ó����ṩ�԰�װΪ������̵�������֧��windows��linuxƽ̨
if (Service.UseServiceSelf(args, "API����"))
{
    var builder = WebApplication.CreateBuilder(args).Inject();
    builder.Host.UseServiceSelf();

    builder.Host.UseSerilog((context, services, config) =>
        config.ReadFrom.Configuration(context.Configuration).ReadFrom.Services(services).Enrich.FromLogContext());

    var app = builder.Build();
    app.Run();
}