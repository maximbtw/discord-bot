using System.ComponentModel;
using Bot.Application.UseCases.ServerMessages;
using Bot.Commands.Checks.Role;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace Bot.Commands;

[Command("message")]
internal class MessageCommands : DiscordCommandsGroupBase<MessageCommands>
{
    private readonly LoadServerMessagesUseCase _loadServerMessagesUseCase;
    private readonly DeleteServerMessagesUseCase _deleteServerMessagesUseCase;
    private readonly GetServerMessagesStatsUseCase _getServerMessagesStatsUseCase;

    public MessageCommands(
        ILogger<MessageCommands> logger,
        LoadServerMessagesUseCase loadServerMessagesUseCase,
        DeleteServerMessagesUseCase deleteServerMessagesUseCase,
        GetServerMessagesStatsUseCase getServerMessagesStatsUseCase) : base(logger)
    {
        _loadServerMessagesUseCase = loadServerMessagesUseCase;
        _deleteServerMessagesUseCase = deleteServerMessagesUseCase;
        _getServerMessagesStatsUseCase = getServerMessagesStatsUseCase;
    }

    [Command("stats")]
    [Description("Shows the message statistics on the server. You can optionally specify a user.")]
    public async ValueTask GetMessagesStats(CommandContext context, DiscordUser? user = null)
    {
        await ExecuteAsync(context,
            () => _getServerMessagesStatsUseCase.Execute(context, user, CancellationToken.None));
    }

    [Command("clear")]
    [RoleCheck(Role.Admin)]
    [Description("Deletes all server messages from the database.")]
    public async ValueTask ClearMessages(CommandContext context)
    {
        await ExecuteAsync(context,
            () => _deleteServerMessagesUseCase.Execute(context, context.Guild!.Id, CancellationToken.None));
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