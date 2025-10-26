using System.ComponentModel;
using Bot.Application.UseCases.ServerMessages;
using Bot.Commands.Checks.Role;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace Bot.Commands;

[Obsolete]
[Command("message")]
internal class MessageCommands : DiscordCommandsGroupBase<MessageCommands>
{
    private readonly LoadServerMessagesUseCase _loadServerMessagesUseCase;

    public MessageCommands(
        ILogger<MessageCommands> logger,
        LoadServerMessagesUseCase loadServerMessagesUseCase) : base(logger)
    {
        _loadServerMessagesUseCase = loadServerMessagesUseCase;
    }
    

    [Command("load")]
    [RoleCheck(Role.Admin)]
    [Description("Loads all server messages and saves them to the database.")]
    public async ValueTask LoadMessages(CommandContext context, params DiscordChannel[] channels)
    {
        await ExecuteAsync(context, () => _loadServerMessagesUseCase.Execute(
            context,
            channels.Any() ? channels.ToList() : null,
            CancellationToken.None));
    }
}