using System.ClientModel;
using Bot.Application.Chat;
using Bot.Application.Chat.OpenAiImpersonationChat;
using Bot.Application.Chat.OpenAiSimpleChat;
using Bot.Application.Infrastructure.Configuration;
using Bot.Application.Services;
using Bot.Contracts.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using OpenAI.Chat;

namespace Bot.Application;

public static class DependencyInjectionExtensions
{
    public static void RegisterServices(this IServiceCollection services)
    {
        services.AddScoped<IMessageService, MessageService>();
    }
    
    public static void RegisterAiChat(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ChatClient>(_ =>
        {
            var settings = configuration.GetSection(nameof(OpenAiSettings)).Get<OpenAiSettings>()!;
            
            var credential = new ApiKeyCredential(settings.ApiKey);
            OpenAIClientOptions options = settings.UseOpenRouter
                ? new OpenAIClientOptions
                {
                    Endpoint = new Uri("https://openrouter.ai/api/v1/")
                }
                : new OpenAIClientOptions();

            return new ChatClient(settings.Model, credential, options);
        });

        services.AddTransient<IChatStrategy, OpenAiImpersonationChatStrategy>();
        services.AddTransient<IChatStrategy, OpenAiSimpleChatStrategy>();
        services.AddTransient<ChatStrategyResolver>();
    }
}