using Bot.Application.Handlers.Chat.OpenAiImpersonationChat;
using Bot.Application.UseCases.FineTunning;
using Bot.Commands.Checks.Role;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace Bot.Commands;

[Obsolete]
internal class AdministartorCommands : DiscordCommandsGroupBase<AdministartorCommands>
{
    private readonly CreateDatasetUseCase _createDatasetUseCase;
    private readonly DeleteDatasetUseCase _deleteDatasetUseCase;
    private readonly TrainModelByDatasetUseCase _trainModelByDatasetUseCase;

    public AdministartorCommands(
        ILogger<AdministartorCommands> logger,
        CreateDatasetUseCase createDatasetUseCase, 
        DeleteDatasetUseCase deleteDatasetUseCase, 
        TrainModelByDatasetUseCase trainModelByDatasetUseCase) : base(logger)
    {
        _createDatasetUseCase = createDatasetUseCase;
        _deleteDatasetUseCase = deleteDatasetUseCase;
        _trainModelByDatasetUseCase = trainModelByDatasetUseCase;
    }
    
    [Command("create-dataset")]
    [RoleCheck(Role.Admin)]
    public async ValueTask CreateDataset(CommandContext context)
    {
        await ExecuteAsync(context, () => _createDatasetUseCase.Execute(context, CancellationToken.None));
    }
    
    [Command("remove-dataset")]
    [RoleCheck(Role.Admin)]
    public async ValueTask RemoveDataset(CommandContext context)
    {
        await ExecuteAsync(context, () => _deleteDatasetUseCase.Execute(context, CancellationToken.None));
    }

    [Command("train")]
    [RoleCheck(Role.Admin)]
    public async ValueTask TrainModel(CommandContext context)
    {
        await ExecuteAsync(context, () => _trainModelByDatasetUseCase.Execute(context, CancellationToken.None));
    }
    
    [Command("set-immersion-user")]
    [RoleCheck(Role.Admin)]
    public async ValueTask SetImmersionUser(CommandContext context, DiscordUser user)
    {
        OpenAiImpersonationChatOptions.GuildIdToImpersonationUserIdIndex[context.Guild!.Id] = user.Id;

        await context.RespondAsync("Done!");
    }
}