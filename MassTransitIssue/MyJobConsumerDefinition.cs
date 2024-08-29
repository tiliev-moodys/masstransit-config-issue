using MassTransit;

namespace MassTransitIssue;

public class MyJobConsumerDefinition : ConsumerDefinition<MyJobConsumer>
{
    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<MyJobConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        _ = consumerConfigurator.Options<JobOptions<MyJob>>(options => options
            .SetJobTimeout(TimeSpan.FromMinutes(60))
            .SetConcurrentJobLimit(2)
            .SetJobCancellationTimeout(TimeSpan.FromSeconds(15))
            .SetRetry(r => r.Incremental(3, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1))));
    }
}
