using Quartz;

namespace Bot.Application.Jobs.SteamNewReleasesLoader;

public static class SteamNewReleasesLoaderScheduler
{
    public static JobKey JobKey => new(nameof(SteamNewReleasesLoaderJob));
    
    public static async Task AddJob(IScheduler scheduler, int intervalInMinutes)
    {
        IJobDetail job = JobBuilder.Create<SteamNewReleasesLoaderJob>()
            .WithIdentity(nameof(SteamNewReleasesLoaderJob))
            .Build();
        
        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity($"{nameof(SteamNewReleasesLoaderJob)}-trigger")
            .StartNow()
            .WithSimpleSchedule(x => x
                .WithIntervalInMinutes(intervalInMinutes)
                .RepeatForever())
            .Build();

        await scheduler.ScheduleJob(job, trigger);
    }
}