using Quartz;

namespace Bot.Application.Jobs;

public class TestJob : IJob
{
    public Task Execute(IJobExecutionContext context)
    {
        Console.WriteLine("Job executed!");
        
        return Task.CompletedTask;
    }
}