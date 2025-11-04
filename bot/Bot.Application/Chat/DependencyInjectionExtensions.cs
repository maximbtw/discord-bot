using System.ClientModel;
using Bot.Application.Chat.DefaultChat;
using Bot.Application.Chat.ImpersonationChat;
using Bot.Application.Chat.Services;
using Bot.Application.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using OpenAI.Chat;

namespace Bot.Application.Chat;

public static class DependencyInjectionExtensions
{
    public static void RegisterChat(this IServiceCollection services, IConfiguration configuration)
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

        services.AddTransient<IChatStrategy, ImpersonationChatStrategy>();
        services.AddTransient<IChatStrategy, DefaultChatStrategy>();
        services.AddTransient<ChatStrategyResolver>();
        
        services.AddScoped<IChatService, ChatService>();
    }
}