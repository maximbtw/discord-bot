using Bot.Contracts.Message;
using Microsoft.Extensions.DependencyInjection;

namespace Bot.Application.Services;

public static class DependencyInjectionExtensions
{
    public static void RegisterServices(this IServiceCollection services)
    {
        services.AddScoped<IMessageService, MessageService>();
    }
}