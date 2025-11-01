using Bot.Application.Jobs.SteamNewReleasesLoader;
using Bot.Application.Jobs.SteamNewReleasesLoader.Service;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;

namespace Bot.Application.Jobs;

public static class DependencyInjectionExtensions
{
    public static void RegisterJobs(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(nameof(SteamNewReleasesLoaderSettings))
            .Get<SteamNewReleasesLoaderSettings>()!;
        
        services.AddScoped<ISteamNewReleasesService, SteamNewReleasesService>();
        services.AddQuartz(q =>
        {
            if (settings.Enabled)
            {
                q.ScheduleJob<SteamNewReleasesLoaderJob>(t =>
                    t.WithIdentity(nameof(SteamNewReleasesLoaderJob))
                        .StartNow()
                        .WithSimpleSchedule(x => x.WithIntervalInMinutes(settings.IntervalInMinutes).RepeatForever())
                );
            }
        });
        
        services.AddQuartzHostedService();
    }
}