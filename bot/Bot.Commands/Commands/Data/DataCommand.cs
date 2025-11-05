using Bot.Application.Chat.Services;
using Bot.Domain.Scope;
using DSharpPlus.Commands;

namespace Bot.Commands.Commands.Data;

[Command("data")]
internal partial class DataCommand : ICommand
{
    private readonly IDbScopeProvider _scopeProvider;
    private readonly IChatService _chatService;

    public DataCommand(
        IDbScopeProvider scopeProvider,
        IChatService chatService)
    {
        _scopeProvider = scopeProvider;
        _chatService = chatService;
    }
}