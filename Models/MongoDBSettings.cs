namespace WorkiomTest.Models
{


    public class MongoDBSettings
    {
        public string ConnectionURI { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string ContactCollectionName { get; set; } = null!;
        public string CompanyCollectionName { get; set; } = null!;


    }
}