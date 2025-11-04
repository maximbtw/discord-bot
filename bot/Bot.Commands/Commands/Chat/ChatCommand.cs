using Bot.Application.Chat.Services;
using Bot.Domain.Scope;
using DSharpPlus.Commands;

namespace Bot.Commands.Commands.Chat;

[Command("chat")]
internal partial class ChatCommand : ICommand
{
    private readonly IChatService _chatService;
    private readonly IDbScopeProvider _scopeProvider;
    
    public ChatCommand(IChatService chatService, IDbScopeProvider scopeProvider)
    {
        _chatService = chatService;
        _scopeProvider = scopeProvider;
    }
}