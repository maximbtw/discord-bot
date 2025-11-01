using Bot.Application.Jobs.SteamNewReleasesLoader.Contracts;
using DSharpPlus.Entities;

namespace Bot.Application.Jobs.SteamNewReleasesLoader;

public static class SteamNewReleasesLoaderDiscordEmbedBuilder
{
    public static DiscordEmbed Build(string appId, SteamAppDetails appDetails)
    {
        var embed = new DiscordEmbedBuilder
        {
            Title = appDetails.Name,
            Url = $"https://store.steampowered.com/app/{appId}/",
            Description = appDetails.ShortDescription,
            Color = DiscordColor.Blurple,
            ImageUrl = appDetails.HeaderImage,
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = "Steam • Новые релизы"
            }
        };

        AddReleaseDateText(embed, appDetails);
        AddGenresText(embed, appDetails);
        AddCategoriesText(embed, appDetails);
        AddPriceText(embed, appDetails);

        return embed.Build();
    }

    private static void AddReleaseDateText(DiscordEmbedBuilder embed, SteamAppDetails appDetails)
    {
        string releaseInfo = appDetails.ReleaseDate is { ComingSoon: true }
            ? $"**Скоро выходит!** ({appDetails.ReleaseDate.Date})"
            : $"**{appDetails.ReleaseDate?.Date ?? "неизвестна"}**";
        
        embed.AddField( "📅 Дата выхода", releaseInfo, inline: false);
    }

    private static void AddGenresText(DiscordEmbedBuilder embed, SteamAppDetails appDetails)
    {
        string genres = string.Join(", ", appDetails.Genres.Select(g => $"{g.Description}"));
        
        embed.AddField("🎮 Жанры", genres, inline: false);
    }
    
    private static void AddCategoriesText(DiscordEmbedBuilder embed, SteamAppDetails appDetails)
    {
        string categories = string.Join(", ", appDetails.Categories.Select(c => $"{c.Description}"));
        
        embed.AddField("⚙️ Категории", categories, inline: false);
    }

    private static void AddPriceText(DiscordEmbedBuilder embed, SteamAppDetails appDetails)
    {
        string priceText = GetPriceText();

        embed.AddField("💵 Цена", priceText, inline: false);
        return;

        string GetPriceText()
        {
            if (appDetails.IsFree)
            {
                return "Бесплатно";
            }

            if (appDetails.PriceOverview is null)
            {
                return "Не указана";
            }

            return appDetails.PriceOverview.DiscountPercent > 0
                ? $"~~{appDetails.PriceOverview.InitialFormatted}~~ → **{appDetails.PriceOverview.FinalFormatted}** (-{appDetails.PriceOverview.DiscountPercent}%)"
                : $"{appDetails.PriceOverview.FinalFormatted}";
        }
    }
}