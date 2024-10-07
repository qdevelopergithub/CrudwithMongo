using MongoDB.Driver;

namespace CrudWithMongoDB.DataAccess
{
    public class MongoDbServices
    {
        private readonly IConfiguration _configuration;
        private readonly IMongoDatabase? _mongoDatabase;
        public MongoDbServices(IConfiguration configuration)
        {
            _configuration = configuration;
            var connectionString = _configuration.GetConnectionString("DbCon");
            var mongoURL = MongoUrl.Create(connectionString);
            var mongoClient = new MongoClient(mongoURL);
            _mongoDatabase = mongoClient.GetDatabase(mongoURL.DatabaseName);
        }
        public IMongoDatabase? Database => _mongoDatabase; 

    }
}
