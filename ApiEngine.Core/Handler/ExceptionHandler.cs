using Furion.DependencyInjection;
using Furion.FriendlyException;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ApiEngine.Core.Handler;

public class ExceptionHandler : IGlobalExceptionHandler, ISingleton
{
    public Task OnExceptionAsync(ExceptionContext context)
    {
        return Task.CompletedTask;
    }
}