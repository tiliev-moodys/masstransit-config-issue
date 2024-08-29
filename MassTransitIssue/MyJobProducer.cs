using MassTransit;

namespace MassTransitIssue;

public interface IMyJobProducer
{
    Task Produce(Guid jobId, int seconds, CancellationToken cancellationToken = default);
}

public class MyJobProducer(IBus bus) : IMyJobProducer
{
    private readonly IBus bus = bus;

    public async Task Produce(Guid jobId, int seconds, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Publishing '{jobId}' for '{seconds}' seconds.");

        var job = new MyJob(jobId, seconds);
        await bus.Publish(job);
    }
}