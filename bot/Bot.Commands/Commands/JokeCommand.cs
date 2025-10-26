using System.ComponentModel;
using System.Text.RegularExpressions;
using Bot.Commands.Checks.ExecuteInDm;
using DSharpPlus.Commands;

namespace Bot.Commands.Commands;

internal partial class JokeCommand : ICommand
{
    private const string NekdoUrl = "https://nekdo.ru/random/";
    
    private static readonly HttpClient HttpClient = new();
    
    [Command("joke")]
    [Description("Случайная шутка")]
    [ExecuteInDm]
    public async ValueTask Execute(CommandContext context)
    {
        await context.DeferResponseAsync();
        
        string? joke = await FetchJoke(CancellationToken.None);
        
        await context.RespondAsync(joke ?? "Шутки закончились");
    }

    private async Task<string?> FetchJoke(CancellationToken ct)
    {
        HttpResponseMessage response = await HttpClient.GetAsync(NekdoUrl, ct);
        if (!response.IsSuccessStatusCode)
        {
            return null;   
        }

        string html = await response.Content.ReadAsStringAsync(ct);

        Match match = ParseHtml().Match(html);
        if (!match.Success)
        {
            return null;
        }

        string joke = ParseJoke().Replace(match.Groups[1].Value, string.Empty);

        return System.Net.WebUtility.HtmlDecode(joke).Trim();
    }

    [GeneratedRegex(@"<div[^>]*class=""text""[^>]*>(.*?)</div>", RegexOptions.Singleline)]
    private static partial Regex ParseHtml();
    
    [GeneratedRegex("<.*?>")]
    private static partial Regex ParseJoke();
}