using System;

namespace CosmosDBChangeFeedReader
{
    public class LeasesDbSettings
    {
        public Uri Uri { get; set; }
        public string AccountKey { get; set; }
        public string DatabaseName { get; set; }
        public string CollectionName { get; set; }
    }
}
