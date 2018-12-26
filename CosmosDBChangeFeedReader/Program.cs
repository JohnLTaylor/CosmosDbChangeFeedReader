using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.ChangeFeedProcessor;
using Microsoft.Azure.Documents.ChangeFeedProcessor.FeedProcessing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ChangeFeedObserverCloseReason = Microsoft.Azure.Documents.ChangeFeedProcessor.FeedProcessing.ChangeFeedObserverCloseReason;
using IChangeFeedObserver = Microsoft.Azure.Documents.ChangeFeedProcessor.FeedProcessing.IChangeFeedObserver;

namespace CosmosDBChangeFeedReader
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var devEnvironmentVariable = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
            var isDevelopment = string.IsNullOrEmpty(devEnvironmentVariable) || devEnvironmentVariable.ToLower() == "development";

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            if (isDevelopment) //only add secrets in development
            {
                builder.AddUserSecrets<UserCollectionDbSettings>();
            }

            var configuration = builder.Build();
            
            var services = new ServiceCollection()
               .Configure<UserCollectionDbSettings>(configuration.GetSection("UserCollectionDbSettings"))
               .Configure<LeasesDbSettings>(configuration.GetSection("LeasesDbSettings"))
               .AddOptions()
               .BuildServiceProvider();

            var userCollectionDbSettings = services.GetService<IOptions<UserCollectionDbSettings>>().Value;
            var userCollectionInfo = new DocumentCollectionInfo()
            {
                DatabaseName = userCollectionDbSettings.DatabaseName,
                CollectionName = userCollectionDbSettings.CollectionName,
                Uri = userCollectionDbSettings.Uri,
                MasterKey = userCollectionDbSettings.AccountKey
            };

            var leasesDbSettings = services.GetService<IOptions<LeasesDbSettings>>().Value;
            var leasesCollectionInfo = new DocumentCollectionInfo()
            {
                DatabaseName = leasesDbSettings.DatabaseName,
                CollectionName = leasesDbSettings.CollectionName,
                Uri = leasesDbSettings.Uri,
                MasterKey = leasesDbSettings.AccountKey
            };

            var processor = await new ChangeFeedProcessorBuilder()
                .WithHostName("SampleHost")
                .WithFeedCollection(userCollectionInfo)
                .WithLeaseCollection(leasesCollectionInfo)
                .WithObserver<SampleObserver>()
                .WithProcessorOptions(new ChangeFeedProcessorOptions()
                {
                    StartFromBeginning = true,
                })
                .BuildAsync();

            await processor.StartAsync();

            Console.WriteLine("Change Feed Processor started. Press any key to stop...");
            Console.ReadKey(true);

            await processor.StopAsync();
        }
    }

    class SampleObserver : IChangeFeedObserver
    {
        public Task CloseAsync(IChangeFeedObserverContext context, ChangeFeedObserverCloseReason reason)
        {
            Console.WriteLine($"CloseAsync: {context.PartitionKeyRangeId}, {context.FeedResponse}, {reason}");
            return Task.CompletedTask;  // Note: requires targeting .Net 4.6+.
        }

        public Task OpenAsync(IChangeFeedObserverContext context)
        {
            Console.WriteLine($"OpenAsync: {context.PartitionKeyRangeId}, {context.FeedResponse}");
            return Task.CompletedTask;
        }

        public Task ProcessChangesAsync(IChangeFeedObserverContext context, IReadOnlyList<Document> docs, CancellationToken cancellationToken)
        {
            Console.WriteLine($"ProcessChangesAsync: partition {context.PartitionKeyRangeId}, doc count {docs.Count}");

            foreach (var doc in docs)
            {
                Console.WriteLine($"Document: {doc}");
            }

            return Task.CompletedTask;
        }
    }
}
