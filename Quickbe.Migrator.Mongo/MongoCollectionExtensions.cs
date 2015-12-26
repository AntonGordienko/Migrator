using System.Threading.Tasks;
using MongoDB.Driver;

namespace Quickbe.Migrator.Mongo
{
    public static class MongoCollectionExtensions
    {
        public static Task<UpdateResult> RenameFieldIfExistsAsync<T>(this IMongoCollection<T> collection,
            string oldFieldName, string newFieldName)
        {
            FilterDefinition<T> filter = Builders<T>.Filter.Exists(oldFieldName);
            UpdateDefinition<T> update = Builders<T>.Update.Rename(oldFieldName, newFieldName);

            return collection.UpdateManyAsync(filter, update, new UpdateOptions
            {
                IsUpsert = false
            });
        }

        public static Task<UpdateResult> UpdateFieldValueIfEqualsAsync<T>(this IMongoCollection<T> collection,
            string fieldName, object oldFieldValue, object newFieldValue)
        {
            FilterDefinition<T> filter = Builders<T>.Filter.Eq(fieldName, oldFieldValue);
            UpdateDefinition<T> update = Builders<T>.Update.Set(fieldName, newFieldValue);

            return collection.UpdateManyAsync(filter, update, new UpdateOptions
            {
                IsUpsert = false
            });
        }
    }
}
