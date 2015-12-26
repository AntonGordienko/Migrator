using System;

namespace Quickbe.Migrator.Mongo
{
    public class MongoMigrationException : MigrationException
    {
        public MongoMigrationException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}