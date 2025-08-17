using System.Text.RegularExpressions;
using DSharpPlus.Commands;

namespace Bot.Application.UseCases.Misc;

public partial class GetJokeUseCase
{
    private const string NekdoUrl = "https://nekdo.ru/random/";
    
    private static readonly HttpClient HttpClient = new();

    public async ValueTask Execute(CommandContext context, CancellationToken ct = default)
    {
        string? joke = await FetchRandomJokeFromNekdo(ct);
        
        await context.RespondAsync(joke ?? "Не удалось получить шутку. Попробуй ещё раз позже.");
    }

    private async Task<string?> FetchRandomJokeFromNekdo(CancellationToken ct)
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