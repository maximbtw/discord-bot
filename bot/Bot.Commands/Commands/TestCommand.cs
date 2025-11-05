using System.ClientModel;
using System.Net.Http.Json;
using System.Text.Json;
using Bot.Application.Jobs.SteamNewReleasesLoader;
using Bot.Application.Jobs.SteamNewReleasesLoader.Contracts;
using Bot.Application.Jobs.SteamNewReleasesLoader.Service;
using Bot.Commands.Checks.RequireApplicationOwner;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using HtmlAgilityPack;
using OpenAI.Chat;

namespace Bot.Commands.Commands;

public class TestCommand : ICommand
{
    private readonly ISteamNewReleasesService _steamNewReleasesService;
    private readonly ChatClient _client;

    public TestCommand(ISteamNewReleasesService steamNewReleasesService, ChatClient client)
    {
        _steamNewReleasesService = steamNewReleasesService;
        _client = client;
    }

    public enum ChatType
    {
        Default,
        Immersion
    }

    [Command("test2")]
    [MyRequireApplicationOwner]
    public async ValueTask Execute2(CommandContext context, ChatType  chatType)
    {
        return;
    }
        

    [Command("test")]
    [MyRequireApplicationOwner]
    public async ValueTask Execute(CommandContext context)
    {
        await context.DeferResponseAsync();
        
        IAsyncEnumerable<string> appIds = _steamNewReleasesService.GetLastAppIds(1);
        await foreach (string appId in appIds)
        {
            SteamAppDetailsResponse? details = await _steamNewReleasesService.GetAppDetails(appId, "RU", "russia");
            
            string aiMessage = await GetAiMessage(details.Data);
            
            DiscordEmbed embed = SteamNewReleasesLoaderDiscordEmbedBuilder.Build(appId, details.Data);
        
            await context.RespondAsync(aiMessage, embed);
            
            break;
        }
    }

    private async Task<string> GetAiMessage(SteamAppDetails appDetails)
    {
        var inputMessages = new List<ChatMessage>();

        // Системное сообщение — задает стиль и правила
        inputMessages.Add(new SystemChatMessage(
            """
            Ты — игровой Discord-бот. Твоя задача — делать короткие и дружелюбные комментарии о новых играх, которые бот только что нашёл. 
            Начинай сообщение разнообразно: можно 'Нашёл интересную игру!', 'Обратите внимание на', 'Советую попробовать', и т.д. 
            Сделай текст емким, выделяй интересные особенности игры.
            """
        ));

        // Сообщение пользователя — описание игры
        string description = $"{appDetails.Name}: {appDetails.ShortDescription}\n" +
                             $"Жанры: {string.Join(", ", appDetails.Genres.Select(g => g.Description))}\n" +
                             $"Категории: {string.Join(", ", appDetails.Categories.Select(c => c.Description))}\n";

        inputMessages.Add(new UserChatMessage($"Новая игра найдена:\n{description}"));

        var options = new ChatCompletionOptions
        {
            MaxOutputTokenCount = 300,
            Temperature = 0.7f,
            TopP = 0.9f,
            FrequencyPenalty = 0.2f,
            PresencePenalty = 0.3f
        };

        ClientResult<ChatCompletion> result = await _client.CompleteChatAsync(inputMessages, options);

        string responseText = result.Value.Content[0].Text.Trim();
        return responseText;
    }
}