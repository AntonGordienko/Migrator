using System;
using System.Collections.Generic;
using System.Diagnostics;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Quickbe.Migrator.Mongo
{
    public abstract class MongoMigration : Migration
    {
        public IMongoDatabase Database { get; internal set; }

        protected MongoMigration(Version version) : base(version)
        {

        }

        protected MongoMigration(string version) : base(version)
        {

        }

        public IMongoCollection<T> GetCollection<T>(string name, MongoCollectionSettings settings = null)
        {
            return Database.GetCollection<T>(name, settings);
        }

        public IMongoCollection<BsonDocument> GetCollection(string name, MongoCollectionSettings settings = null)
        {
            return Database.GetCollection<BsonDocument>(name, settings);
        }

        public void CreateCollection(string name, CreateCollectionOptions options = null)
        {
            Database.CreateCollection(name, options);
            Trace.WriteLine($"Collection '{name}' created.");
        }

        public void DropCollection(string name)
        {
            Database.DropCollection(name);
            Trace.WriteLine($"Collection '{name}' dropped.");
        }

        public void RenameCollection(string oldName, string newName, RenameCollectionOptions options = null)
        {
            if (options == null)
            {
                options = new RenameCollectionOptions
                {
                    DropTarget = false
                };
            }

            Database.RenameCollection(oldName, newName, options);
            Trace.WriteLine($"Collection renamed from '{oldName}' to '{newName}'.");
        }

        public void RenameFieldIfExists(string collectionName, string oldFieldName, string newFieldName)
        {
            var updateResult = GetCollection(collectionName)
                .RenameFieldIfExistsAsync(oldFieldName, newFieldName)
                .ConfigureAwait(false).GetAwaiter().GetResult();

            string modifiedCount = updateResult.IsAcknowledged
                ? updateResult.ModifiedCount.ToString()
                : "?";

            Trace.WriteLine($"Field renamed from '{oldFieldName}' to '{newFieldName}'"
                + $" in collection '{collectionName}' {modifiedCount} times.");
        }

        public void UpdateFieldValueIfEquals(string collectionName, string fieldName,
           IReadOnlyDictionary<object, object> fieldValues)
        {
            IMongoCollection<BsonDocument> collection = GetCollection(collectionName);

            foreach (KeyValuePair<object, object> fieldValue in fieldValues)
            {
                var updateResult = collection
                    .UpdateFieldValueIfEqualsAsync(fieldName, fieldValue.Key, fieldValue.Value)
                    .ConfigureAwait(false).GetAwaiter().GetResult();

                string modifiedCount = updateResult.IsAcknowledged
                    ? updateResult.ModifiedCount.ToString()
                    : "?";

                Trace.WriteLine(
                    $"Field '{fieldName}' value updated from '{fieldValue.Key}' to '{fieldValue.Value}' " +
                    $"in collection '{collectionName}' {modifiedCount} times.");
            }
        }

        public void UpdateFieldValueIfEquals(string collectionName, string fieldName,
            object oldFieldValue, object newFieldValue)
        {
            UpdateFieldValueIfEquals(collectionName, fieldName, new Dictionary<object, object>
            {
                { oldFieldValue, newFieldValue }
            });
        }
    }
}
