using WorkiomTest.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace WorkiomTest.Services
{
    public class MongoDBService<T> where T : class, IEntity
    {

        private readonly IMongoCollection<T> _collection;

        public MongoDBService(IOptions<MongoDBSettings> mongoDBSettings)
        {
            MongoClient client = new MongoClient(mongoDBSettings.Value.ConnectionURI);
            IMongoDatabase database = client.GetDatabase(mongoDBSettings.Value.DatabaseName);
            
            var _contactCollection = database.GetCollection<T>(mongoDBSettings.Value.ContactCollectionName);
            var _companyCollection = database.GetCollection<T>(mongoDBSettings.Value.CompanyCollectionName);
            if (typeof(T) == typeof(Contact))
            {
                _collection = _contactCollection;
            }
            else
            {
                _collection = _companyCollection;
            }
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            return await _collection.Find(_ => true).ToListAsync();
        }

        public async Task<T> GetById(string id)
        {
            return await _collection.Find(item => item.Id == ObjectId.Parse(id)).FirstOrDefaultAsync();
        }

        public async Task Insert(T item)
        {
            await _collection.InsertOneAsync(item);
        }

        public async Task<bool> Update(string id, T item)
        {

            FilterDefinition<T> filter = Builders<T>.Filter.Eq("Id", id);
            UpdateDefinition<T> update = Builders<T>.Update.AddToSet("items", item);
            var result = await _collection.UpdateOneAsync(filter, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> Delete(string id)
        {
            var result = await _collection.DeleteOneAsync(item => item.Id == ObjectId.Parse(id));
            return result.DeletedCount > 0;
        }


        public void UpdateCollectionSchema(FilterDefinition<T> filter, UpdateDefinition<T> update, UpdateOptions options)
        {
            _collection.UpdateMany(filter, update, options);
        }
    }
}