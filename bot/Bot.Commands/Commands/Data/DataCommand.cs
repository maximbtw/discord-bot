using Bot.Contracts.Message;
using Bot.Domain.Scope;
using DSharpPlus.Commands;

namespace Bot.Commands.Commands.Data;

[Command("data")]
internal partial class DataCommand : ICommand
{
    private readonly IDbScopeProvider _scopeProvider;
    private readonly IMessageService _messageService;

    public DataCommand(
        IDbScopeProvider scopeProvider,
        IMessageService messageService)
    {
        _scopeProvider = scopeProvider;
        _messageService = messageService;
    }
    
}