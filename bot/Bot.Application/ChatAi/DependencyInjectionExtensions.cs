using Bot.Application.ChatAi.OpenRouter;
using Bot.Application.Infrastructure.Configuration.AiChat;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bot.Application.ChatAi;

internal static class DependencyInjectionExtensions
{
    internal static void RegisterAiChat(this IServiceCollection services, AiChatOptions options)
    {
        if (options.Strategy == AiChatStrategy.OpenRouter)
        {
            AiChatOpenRouterSettings settings = options.OpenRouterSettings!;
            
            services.AddSingleton(settings);
            services.AddSingleton<ChatAiOpenRouteLogger>(sp =>
                new ChatAiOpenRouteLogger(sp.GetRequiredService<ILogger<ChatAiOpenRouteLogger>>()));
            
            services.AddHttpClient<ChatAiOpenRouterClient>(client =>
            {
                client.DefaultRequestHeaders.Add("Authorization",$"Bearer {settings.ApiKey}");
            });
        
            services.AddTransient<IChatAiStrategy, ChatAiOpenRouterStrategy>();    
        }
        
        services.AddTransient<ChatAiResolver>();
    }
}