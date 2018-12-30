using System;

namespace CosmosDBChangeFeedReader
{
    public class UserDbSettings
    {
        public Uri Uri { get; set; }
        public string AccountKey { get; set; }
        public string DatabaseName { get; set; }
        public string CollectionName { get; set; }
    }
}
