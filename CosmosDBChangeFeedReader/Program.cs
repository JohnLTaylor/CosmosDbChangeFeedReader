using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.IO;

namespace CosmosDBChangeFeedReader
{
    class Program
    {
        static void Main(string[] args)
        {
            var devEnvironmentVariable = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
            var isDevelopment = string.IsNullOrEmpty(devEnvironmentVariable) || devEnvironmentVariable.ToLower() == "development";

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            if (isDevelopment) //only add secrets in development
            {
                builder.AddUserSecrets<YourClassName>();
            }

            var configuration = builder.Build();
            
            //Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.
            //Microsoft.Extensions.DependencyInjection.OptionsConfigurationServiceCollectionExtensions.
            var services = new ServiceCollection()
               .Configure<YourClassName>(configuration.GetSection("YourClassName"))
               .AddOptions()
               .BuildServiceProvider();

            var nane = services.GetService<IOptions<YourClassName>>();

            Console.WriteLine($"Hello World!: {nane.Value.Secret1}");
            Console.ReadKey(true);
        }
    }

    public class YourClassName
    {
        public string Secret1 { get; set; }
        public string Secret2 { get; set; }
    }
}
