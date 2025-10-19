using DSharpPlus.Commands;

namespace Bot.Application.UseCases.FineTunning;

public class TrainModelByDatasetUseCase
{
    public async ValueTask Execute(
        CommandContext context, 
        CancellationToken ct = default)
    {
        await context.RespondAsync("Команда еще не реализована");
    }
}