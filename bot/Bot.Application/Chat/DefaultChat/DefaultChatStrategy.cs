using Bot.Application.Chat.Services;
using Bot.Contracts.Chat;
using Bot.Domain.Orms.ChatSettings;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

namespace Bot.Application.Chat.DefaultChat;

internal class DefaultChatStrategy : ChatStrategyBase<DefaultChatOptions>
{
    public DefaultChatStrategy( IChatService chatService, ChatClient client, IConfiguration configuration) 
        : base(chatService, client, configuration)
    {
    }

    public override ChatType Type => ChatType.Default;
    
    protected override DefaultChatOptions GetOptions(ChatSettings settings) => settings.DefaultChatOptions;

    protected override ValueTask<List<SystemChatMessage>> CreateSystemChatMessages(
        DiscordClient client, 
        MessageCreatedEventArgs args, 
        GuildChatSettings guildSettings,
        CancellationToken ct)
    {
        var messages = new List<SystemChatMessage>();
        
        DefaultChatOptions options = GetOptions();
        if (!string.IsNullOrEmpty(options.SystemMessage))
        {
            messages.Add(new SystemChatMessage(options.SystemMessage));
        }
        
        return ValueTask.FromResult(messages);
    }
}