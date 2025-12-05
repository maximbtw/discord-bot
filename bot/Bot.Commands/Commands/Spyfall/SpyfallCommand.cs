using System.ComponentModel;
using System.Text.Json;
using System.Text.RegularExpressions;
using Bot.Application.Shared;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;

namespace Bot.Commands.Commands.Spyfall;

[Command("dspy")]
[Description("Шпион по героям доты")]
internal class SpyfallCommand : ICommand
{
    private const string OpenDotaApiUrl = "https://api.opendota.com/api";
    private const int DefaultSpies = 1;

    private static readonly HttpClient HttpClient = new();
    private static readonly Random Random = new();

    private static List<Hero>? _cachedHeroes;


    [Command("start")]
    [Description("Запускает игру со всеми в текущем голосовом канале.")]
    [RequireGuild]
    public async ValueTask ExecuteVoice(CommandContext context, int spies = DefaultSpies)
    {
        await context.DeferResponseAsync();

        if (context.Member?.VoiceState.ChannelId == null)
        {
            await context.RespondAsync("Ты не в голосовом канале.");
            return;
        }

        DiscordChannel channel = await context.Guild!.GetChannelAsync(context.Member.VoiceState.ChannelId.Value);

        DiscordMember[] members = channel.Users.Where(x=> !x.IsBot).ToArray();

        await DistributeRoles(context, spies, members);
    }

    [Command("pick")]
    [Description("Запускает игру с указанием конкертных пользователей.")]
    [RequireGuild]
    public async ValueTask ExecuteSelection(
        CommandContext context,
        [RemainingText] string memberMentions,
        int spies = DefaultSpies)
    {
        await context.DeferResponseAsync();
        
        // TODO: params в библиотеке не рабоатет корректно, поэтому такой хак.
        // В будущем изенить на params
        var members = new HashSet<DiscordMember>();
        MatchCollection matches = DiscordContentMapper.UsernameRegex().Matches(memberMentions);
        foreach (Match match in matches)
        {
            if (ulong.TryParse(match.Groups[1].Value, out ulong userId))
            {
                DiscordMember member = await context.Guild!.GetMemberAsync(userId);

                if (!member.IsBot)
                {
                    members.Add(member);   
                }
            }
        }
        
        await DistributeRoles(context, spies, members.ToArray());
    }

    private async Task DistributeRoles(CommandContext context, int spies, DiscordMember[] members)
    {
        int minRequiredMembers = spies + 2;
        if (minRequiredMembers > members.Length)
        {
            await context.RespondAsync($"Нужно хотя бы {minRequiredMembers} человека для игры.");
            return;
        }
        
        Hero hero;
        try
        {
            List<Hero> heroes = await GetHeroes();
            
            hero = heroes[Random.Next(heroes.Count)];
        }
        catch (HttpRequestException)
        {
            await context.RespondAsync("Не удалось получить список героев Dota. Попробуйте позже.");
            return;
        }

        List<DiscordMember> spyMembers = members.OrderBy(x => Guid.NewGuid()).Take(spies).ToList();

        DiscordEmbedBuilder embed = CreateHeroEmbed(hero);
        
        foreach (DiscordMember member in spyMembers)
        {
            string spyMessage = SpyMessage(spies, spyMembers.Where(x => x.Id != member.Id));
            
            await member.SendMessageAsync(spyMessage);
        }
        
        foreach (DiscordMember member in members)
        {
            if (spyMembers.All(x => x.Id != member.Id))
            {
                await member.SendMessageAsync(embed);   
            }
        }

        await context.RespondAsync("Роли розданы! Проверьте личные сообщения.");
    }

    private string SpyMessage(int spies, IEnumerable<DiscordMember> spyMembers)
    {
        string message = "🕵️‍♂️ **Ты ШПИОН!** Твоя задача — выяснить, о каком герое говорят остальные.";

        if (spies > 1)
        {
            List<string> names = spyMembers.Select(x => x.Mention).ToList();
            
            message += $"\n **Шпионы**: Ты и {string.Join(", ", names)}";
        }
        
        message += "\n Вот вспомогательный инструмент: https://maximbtw.github.io/dota2-spy-tool/";

        return message;
    }

    private static DiscordEmbedBuilder CreateHeroEmbed(Hero hero)
    {
        var embed = new DiscordEmbedBuilder
        {
            Url = hero.Url,
            Title = $"Герой: {hero.LocalizedName}",
            Color = DiscordColor.Blurple,
            ImageUrl = hero.ImageUrl
        };

        return embed;
    }
    

    private async ValueTask<List<Hero>> GetHeroes()
    {
        if (_cachedHeroes != null)
        {
            return _cachedHeroes;
        }
        
        HttpResponseMessage response = await HttpClient.GetAsync($"{OpenDotaApiUrl}/heroes");

        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync();
        _cachedHeroes = JsonSerializer.Deserialize<List<Hero>>(json)!;

        if (_cachedHeroes == null || _cachedHeroes.Count == 0)
        {
            throw new InvalidOperationException("Не удалось десериализовать список героев.");
        }

        return _cachedHeroes;
    }
}