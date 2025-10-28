using DSharpPlus;

namespace Bot.Events;

public static class DependencyInjectionExtensions
{
    public static void RegisterEvents(this DiscordClientBuilder builder)
    {
        builder.ConfigureEventHandlers(x =>
        {
            x.AddEventHandlers<MessageCreatedEventHandler>();
        });
    }
}