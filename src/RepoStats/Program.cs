using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RepoStats.Configuration;
using System.IO;

namespace RepoStats
{
    class Program
    {
        static void Main(string[] args)
        {
            var services = new ServiceCollection();
            ConfigureServices(services, args);

            var serviceProvider = services.BuildServiceProvider();

            serviceProvider.GetRequiredService<App>().Run();
            serviceProvider.Dispose();
        }

        private static void ConfigureServices(IServiceCollection services, string[] args)
        {
            services.AddLogging(c =>
                c.AddConsole()
                    .AddDebug()
                    .SetMinimumLevel(LogLevel.Debug));

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .AddCommandLine(args)
                .Build();

            services.AddOptions();
            services.Configure<AppSettings>(configuration);

            services.AddTransient<RepoParser>();
            services.AddTransient<App>();
        }
    }
}