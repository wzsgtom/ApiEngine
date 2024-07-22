using ApiEngine.Core.Extension;
using ServiceSelf;

// Ϊ.NET ����������Ӧ�ó����ṩ�԰�װΪ������̵�������֧��windows��linuxƽ̨
if (Service.UseServiceSelf(args, "�غ��ۺϷ���ƽ̨"))
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