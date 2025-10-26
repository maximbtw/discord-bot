using System.ClientModel;
using Bot.Application.Handlers.Chat.OpenAiImpersonationChat;
using Bot.Application.Handlers.EventHandler;
using Bot.Application.Infrastructure.Configuration;
using Bot.Application.Services;
using Bot.Application.Shared;
using Bot.Contracts;
using Bot.Contracts.Handlers;
using Bot.Contracts.Handlers.AiChat;
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
        // Handlers
        services.AddTransient<IMessageCreatedEventHandler, SendMessageToOpenAiEventHandler>();
        services.AddTransient<IMessageCreatedEventHandler, SaveMessageToDbEventHandler>();

        services.AddSingleton<ICreatedMessageCache, CreatedMessageCache>();
        services.AddScoped<IMessageService, MessageService>();
    }
    
    public static void RegisterAiChat(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(nameof(OpenAiSettings)).Get<OpenAiSettings>()!;

        services.AddSingleton<ChatClient>(_ =>
        {
            var credential = new ApiKeyCredential(settings.ApiKey);
            OpenAIClientOptions options = settings.UseOpenRouter
                ? new OpenAIClientOptions
                {
                    Endpoint = new Uri("https://openrouter.ai/api/v1/")
                }
                : new OpenAIClientOptions();

            return new ChatClient(settings.ChatOptions.Model, credential, options);
        });

        services.AddTransient<IAiChatHandler, OpenAiImpersonationChatHandler>();
    }
}