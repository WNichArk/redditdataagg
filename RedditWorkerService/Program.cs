using RedditWorkerService;
using RedditWorkerService.DataAccessLayer;
using RedditWorkerService.RedditService.Models;
using System.Reflection;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config =>
    config.AddUserSecrets(Assembly.GetExecutingAssembly()))
    .ConfigureServices((hostContext, services) =>
    {
        services.AddHostedService<Worker>();
        services.AddDbContext<RedditContext>();
        services.AddSingleton<RedditContext>();
    })
    .Build();

await host.RunAsync();
