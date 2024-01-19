namespace ApiEngine.Core;

public sealed class StartupApplicationComponent : IApplicationComponent
{
    public void Load(IApplicationBuilder app, IWebHostEnvironment env, ComponentContext componentContext)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // 跨域
        app.UseCorsAccessor();
        // 限流
        app.UseIpRateLimiting();
        app.UseClientRateLimiting();
        // 健康检查
        app.UseHealthChecks("/healthcheck");
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

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        // 任务看板
        app.UseHangfireDashboard();
        // 日志看板
        app.UseLogDashboard();

        RunWith();
    }

    private static void RunWith()
    {
        #region 日志清理

        var appInfo = App.GetOptions<AppInfoOptions>();
        if (appInfo.Log.LogType == LogTypeEnum.Db)
        {
            var logJob = App.GetService<ILogJob>();
            logJob.RunJob();
            RecurringJob.AddOrUpdate("日志清理", () => logJob.RunJob(),
                Crontab.Monthly.ToString(),
                new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });
        }

        #endregion
    }
}