//setup our DI
using JackHenry.Service;
using JackHenry.Service.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("Press the Enter key to exit...");
Console.WriteLine($"Application started at : {DateTimeOffset.Now.ToString("0:HH:mm:ss.fff")}");

IConfigurationRoot configuration = new ConfigurationBuilder()
                                    .AddJsonFile(@"C:\Source\Repos\JackHenryAndAssociates\JackHenry.Service\appsettings.json").Build();

ServiceProvider serviceProvider = new ServiceCollection()      
                                    .AddSingleton<IConfiguration>(configuration)
                                    .AddSingleton<ITwitterConfigManager, TwitterConfigManager>()
                                    .AddSingleton<ITwitterStreamService, TwitterStreamService>()
                                    .BuildServiceProvider();

ITwitterStreamService? twitterStreamService = serviceProvider.GetService<ITwitterStreamService>();

if (twitterStreamService != null)
{
    bool isUserAuth = await twitterStreamService.IsUserAuthenticated();
    if (isUserAuth)
    {
        await twitterStreamService.AnalyzeTweets();
    }
   
    Console.ReadLine();
    twitterStreamService.StopAndDisposeTimer();
    Console.WriteLine("Exiting the application..");
}

