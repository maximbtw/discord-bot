using System.Threading;
using System.Threading.Tasks;
using Bot.Application.Infrastructure.Checks.Access;
using Bot.Application.UseCases.Ai;
using DSharpPlus.Commands;
using Microsoft.Extensions.Logging;

namespace Bot.Host.Commands;

[Command("ai")]
internal class AiCommands : DiscordCommandsGroupBase<AiCommands>
{
    private readonly CreateDatasetUseCase _createDatasetUseCase;
    private readonly DeleteDatasetUseCase _deleteDatasetUseCase;
    private readonly TrainModelByDatasetUseCase _trainModelByDatasetUseCase;

    public AiCommands(
        ILogger<AiCommands> logger,
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
}