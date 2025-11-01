using Bot.Application.Jobs.SteamNewReleasesLoader.Contracts;
using DSharpPlus.Entities;

namespace Bot.Application.Jobs.SteamNewReleasesLoader;

public static class SteamNewReleasesLoaderDiscordEmbedBuilder
{
    public static DiscordEmbed Build(SteamAppDetails appDetails)
    {
        string priceText = appDetails.IsFree
            ? "🆓 Бесплатно"
            : appDetails.PriceOverview is not null
                ? (int.TryParse(appDetails.PriceOverview.DiscountPercent, out int discount) && discount > 0
                    ? $"💸 ~~{appDetails.PriceOverview.InitialFormatted}~~ → **{appDetails.PriceOverview.FinalFormatted}** (-{discount}%)"
                    : $"💰 {appDetails.PriceOverview.FinalFormatted}")
                : "💰 Не указана";

        string genres = appDetails.Genres.Count > 0
            ? string.Join(", ", appDetails.Genres.Select(g => g.Description))
            : "Не указаны";

        string categories = appDetails.Categories.Count > 0
            ? string.Join(", ", appDetails.Categories.Select(c => c.Description))
            : "Не указаны";

        string releaseInfo = appDetails.ReleaseDate is { ComingSoon: true }
            ? $"📅 **Скоро выходит!** ({appDetails.ReleaseDate.Date})"
            : $"📅 Дата выхода: **{appDetails.ReleaseDate?.Date ?? "неизвестна"}**";

        string screenshotsSection = "";
        if (appDetails.Screenshots is { Count: > 0 })
        {
            // Покажем максимум 3 скриншота, в виде ссылок
            List<string> screenshots = appDetails.Screenshots
                .Take(3)
                .Select(s => $"[Скриншот]({s.FullPath})")
                .ToList();
            
            screenshotsSection = string.Join(" • ", screenshots);
        }

        var embed = new DiscordEmbedBuilder
        {
            Title = appDetails.Name,
            Url = $"https://store.steampowered.com/app/{GetAppIdFromImage(appDetails.HeaderImage)}/",
            Description = $"{appDetails.ShortDescription.Truncate(300)}\n\n{releaseInfo}",
            Color = DiscordColor.Blurple,
            Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
            {
                Url = appDetails.HeaderImage
            },
            ImageUrl = appDetails.Screenshots?.FirstOrDefault()?.FullPath ?? appDetails.HeaderImage,
            Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = "Steam • Новые релизы"
            }
        };

        embed.AddField("🎮 Жанры", genres, inline: true);
        embed.AddField("⚙️ Категории", categories, inline: true);
        embed.AddField("💵 Цена", priceText, inline: true);

        if (!string.IsNullOrEmpty(screenshotsSection))
        {
            embed.AddField("📸 Скриншоты", screenshotsSection, inline: false);
        }

        return embed.Build();
    }

    private static string GetAppIdFromImage(string headerImageUrl)
    {
        var parts = headerImageUrl.Split('/');
        int index = Array.IndexOf(parts, "apps");
        return (index >= 0 && index + 1 < parts.Length) ? parts[index + 1] : "0";
    }

    private static string Truncate(this string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        return text.Length <= maxLength ? text : text[..(maxLength - 3)] + "...";
    }
}