using MassTransit;

namespace MassTransitIssue;

public class MyJobConsumer : IJobConsumer<MyJob>
{
    public async Task Run(JobContext<MyJob> context)
    {
        Console.WriteLine($"Running '{context.Job.Id}' for '{context.Job.DurationInSeconds}'.");

        await Task.Delay(TimeSpan.FromSeconds(context.Job.DurationInSeconds), context.CancellationToken);

        Console.WriteLine($"Finished '{context.Job.Id}' for '{context.Job.DurationInSeconds}'.");
    }
}
