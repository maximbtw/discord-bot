using System.Threading;
using System.Threading.Tasks;
using Bot.Application.Handlers.Chat.OpenAiImpersonationChat;
using Bot.Application.Infrastructure.Checks.Access;
using Bot.Application.UseCases.FineTunning;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace Bot.Host.Commands;

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
    [RoleCheck(Role.SuperUser)]
    public async ValueTask CreateDataset(CommandContext context)
    {
        await ExecuteAsync(context, () => _createDatasetUseCase.Execute(context, CancellationToken.None));
    }
    
    [Command("remove-dataset")]
    [RoleCheck(Role.SuperUser)]
    public async ValueTask RemoveDataset(CommandContext context)
    {
        await ExecuteAsync(context, () => _deleteDatasetUseCase.Execute(context, CancellationToken.None));
    }

    [Command("train")]
    [RoleCheck(Role.SuperUser)]
    public async ValueTask TrainModel(CommandContext context)
    {
        await ExecuteAsync(context, () => _trainModelByDatasetUseCase.Execute(context, CancellationToken.None));
    }
    
    [Command("set-immersion-user")]
    [RoleCheck(Role.SuperUser)]
    public async ValueTask SetImmersionUser(CommandContext context, DiscordUser user)
    {
        OpenAiImpersonationChatOptions.ImpersonationUseId = user.Id;

        await context.RespondAsync("Done!");
    }
}