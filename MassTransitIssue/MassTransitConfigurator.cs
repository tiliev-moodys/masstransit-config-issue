using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace MassTransitIssue;

public static class Configurator
{
    public static IServiceCollection ConfigureMassTransit(this IServiceCollection services)
    {
        services.AddMassTransit(bus =>
        {
            bus.AddDelayedMessageScheduler();

            bus.AddConsumer<MyJobConsumer, MyJobConsumerDefinition>();

            bus.AddJobSagaStateMachines(opts =>
            {
                opts.FinalizeCompleted = true;
            })
            .RedisRepository(r =>
            {
                ConfigureRedisDatabase(r);
                r.KeyPrefix = "masstransit-job";
                r.LockSuffix = "-lockage";
                r.RetryPolicy = Retry.Incremental(3, TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(100));
            });

            bus.UsingRabbitMq((context, cfg) =>
            {
                cfg.UseDelayedMessageScheduler();

                cfg.Host("localhost", 5672, "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                    h.PublisherConfirmation = true;
                });

                cfg.Durable = true;
                cfg.PrefetchCount = 50;

                cfg.ServiceInstance(instance =>
                {
                    instance.ConfigureJobServiceEndpoints();

                    instance.ReceiveEndpoint("my-own-endpoint", e =>
                    {
                        e.PrefetchCount = 2;
                        e.ConfigureConsumer<MyJobConsumer>(context);
                    });
                });
            });
        });

        services.AddOptions<MassTransitHostOptions>()
            .Configure(options =>
            {
                options.WaitUntilStarted = true;
                options.StartTimeout = TimeSpan.FromSeconds(30);
                options.StopTimeout = TimeSpan.FromSeconds(30);
                options.ConsumerStopTimeout = TimeSpan.FromSeconds(15);
            });

        return services;
    }

    private static void ConfigureRedisDatabase(IRedisSagaRepositoryConfigurator configurator)
    {
        var options = new ConfigurationOptions
        {
            EndPoints = { "localhost" },
            ClientName = Environment.MachineName,
            AbortOnConnectFail = false,
            AllowAdmin = true
        };

        configurator.DatabaseConfiguration(options);
    }    
}
