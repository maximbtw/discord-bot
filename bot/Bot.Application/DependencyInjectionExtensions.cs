using Bot.Application.Handlers;
using Bot.Application.Shared;
using Bot.Application.UseCases.Ai;
using Bot.Application.UseCases.Misc;
using Bot.Application.UseCases.ServerMessages;
using Bot.Contracts;
using Microsoft.Extensions.DependencyInjection;

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
        services.AddTransient<IMessageCreatedHandler, AskAiHandler>();
        services.AddTransient<IMessageCreatedHandler, SaveMessageToDbHandler>();

        services.AddSingleton<ICreatedMessageCache, CreatedMessageCache>();
    }
}