using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MassTransitIssue;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        await host.StartAsync();

        var jobProducer = host.Services.GetService<IMyJobProducer>()!;

        await PrintHelath(host);

        Console.WriteLine("Enter int number (seconds for a job to run) or 'x' to shut down.");
        while (true)
        {
            var line = await Task.Run(Console.ReadLine);
            if (line == "x")
            {
                break;
            }
            else if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var seconds = int.Parse(line);
            await jobProducer.Produce(Guid.NewGuid(), seconds);
        }

        await host.StopAsync();
    }

    public static async Task PrintHelath(IHost host)
    {
        var healthCheckService = host.Services.GetRequiredService<HealthCheckService>();
        var healthReport = await healthCheckService.CheckHealthAsync();

        // Output the health check results
        Console.WriteLine($"Overall Status: {healthReport.Status}");
        foreach (var entry in healthReport.Entries)
        {
            if (entry.Key.Contains("masstransit", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine($"Component: {entry.Key}, Status: {entry.Value.Status}");
                Console.WriteLine("Endpoints:");
                var endpoints = JsonSerializer.Serialize(entry.Value.Data["Endpoints"]);
                Console.WriteLine(endpoints);
            }
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.Information);
            })
            .ConfigureServices((hostContext, services) =>
            {
                services.ConfigureMassTransit();
                services.AddSingleton<IMyJobProducer, MyJobProducer>();
            });
}