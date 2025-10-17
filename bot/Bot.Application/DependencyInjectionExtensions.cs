using System.ClientModel;
using Bot.Application.Ai;
using Bot.Application.Handlers;
using Bot.Application.Infrastructure.Configuration;
using Bot.Application.Shared;
using Bot.Application.UseCases.Ai;
using Bot.Application.UseCases.Misc;
using Bot.Application.UseCases.ServerMessages;
using Bot.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using OpenAI.Chat;

namespace Bot.Application;

public static class DependencyInjectionExtensions
{
    public static void RegisterUseCases(this IServiceCollection services)
    {
        // Messages
        services.AddTransient<DeleteServerMessagesUseCase>();
        services.AddTransient<GetServerMessagesStatsUseCase>();
        services.AddTransient<LoadServerMessagesUseCase>();
        
        // Ai
        services.AddTransient<CreateDatasetUseCase>();
        services.AddTransient<DeleteDatasetUseCase>();
        services.AddTransient<TrainModelByDatasetUseCase>();
        
        // Misc
        services.AddTransient<GetJokeUseCase>();
        
        // Handlers
        services.AddTransient<IMessageCreatedHandler, ChatAiHandler>();
        services.AddTransient<IMessageCreatedHandler, SaveMessageToDbHandler>();

        services.AddSingleton<ICreatedMessageCache, CreatedMessageCache>();
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

        services.AddTransient<IChatService, OpenAiChatService>();
    }
}