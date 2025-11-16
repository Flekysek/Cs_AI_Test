using FoodAI.Core;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace FoodAI.Data
{
    public class MongoDBService
    {
        private readonly IMongoDatabase _database;

        public MongoDBService(IConfiguration configuration)
        {
            var connectionString = configuration.GetValue<string>("MongoDB:ConnectionString");
            var databaseName = configuration.GetValue<string>("MongoDB:DatabaseName");
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoCollection<FoodItem> FoodItems => _database.GetCollection<FoodItem>("FoodItems");
        public IMongoCollection<UserPreferences> UserPreferences => _database.GetCollection<UserPreferences>("UserPreferences");
        public IMongoCollection<ChatMessage> ChatMessages => _database.GetCollection<ChatMessage>("ChatMessages");
    }
}
