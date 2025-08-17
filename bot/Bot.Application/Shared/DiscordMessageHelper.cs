using DSharpPlus.Entities;

namespace Bot.Application.Shared;

public static class DiscordMessageHelper
{
    public static bool MessageIsValid(DiscordMessage message, ulong currentBotId)
    {
        return !string.IsNullOrWhiteSpace(message.Content) &&
               message.Author != null &&
               (!message.Author.IsBot || message.Author.Id == currentBotId) &&
               !message.Content.StartsWith($"/");
    }
}