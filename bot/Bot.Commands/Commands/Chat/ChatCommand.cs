using Bot.Contracts.Message;
using DSharpPlus.Commands;

namespace Bot.Commands.Commands.Chat;

[Command("chat")]
internal partial class ChatCommand : ICommand
{
    private readonly IMessageService _messageService;
    
    public ChatCommand(IMessageService messageService)
    {
        _messageService = messageService;
    }
}