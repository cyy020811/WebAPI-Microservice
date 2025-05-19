using System.Reflection.Metadata;
using MongoDB.Driver;
using WebApi.Entities;

namespace WebApi.Repositories
{
    public class ItemsRepository
    {
        private const string CollectionName = "items";
        private readonly IMongoCollection<Item> _collection;
        private readonly FilterDefinitionBuilder<Item> _filterBuilder = Builders<Item>.Filter;

        public ItemsRepository()
        {
            var mongoClient = new MongoClient("mongodb://localhost:27017");
            var database = mongoClient.GetDatabase("WebApi");
            _collection = database.GetCollection<Item>(CollectionName);
        }


        public async Task<IReadOnlyCollection<Item>> GetAllAsync()
        {
            return await _collection.Find(_filterBuilder.Empty).ToListAsync();
        }

        public async Task<Item> GetAsync(string id)
        {
            FilterDefinition<Item> filter = _filterBuilder.Eq(entity => entity.Id, id);
            return await _collection.Find(filter).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Item entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            await _collection.InsertOneAsync(entity);
        }

        public async Task UpdateAsync(Item entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            FilterDefinition<Item> filter = _filterBuilder.Eq(existingEntity => existingEntity.Id, entity.Id);
            await _collection.ReplaceOneAsync(filter, entity);
        }

        public async Task RemoveAsync(string id)
        {
            FilterDefinition<Item> filter = _filterBuilder.Eq(existingEntity => existingEntity.Id, id);
            await _collection.DeleteOneAsync(filter);
        }
    }
}