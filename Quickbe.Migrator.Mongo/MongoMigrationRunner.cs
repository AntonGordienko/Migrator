using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MongoDB.Driver;

namespace Quickbe.Migrator.Mongo
{
    public class MongoMigrationRunner : MigrationRunner<MongoMigration>
    {
        private readonly IMongoDatabase _database;

        public MongoMigrationRunner(string connectionString, string databaseName = null)
        {
            MongoUrl mongoUrl = MongoUrl.Create(connectionString);
            _database = new MongoClient(mongoUrl).GetDatabase(databaseName ?? mongoUrl.DatabaseName);
        }

        public override void Upgrade(Version currentVersion = null, Version targetVersion = null)
        {
            if (currentVersion != null && targetVersion != null && currentVersion >= targetVersion)
            {
                throw new ArgumentException("Current version must be less target version.");
            }

            IEnumerable<MongoMigration> mongoMigrations = GetMigrations(currentVersion, targetVersion)
                .OrderBy(x => x.Version);

            foreach (MongoMigration mongoMigration in mongoMigrations)
            {
                mongoMigration.Database = _database;

                Trace.WriteLine($"{mongoMigration.GetType().FullName}.Up() executing...");
                mongoMigration.Up();
                Trace.WriteLine($"{mongoMigration.GetType().FullName}.Up() executed.");
            }
        }

        public override void Downgrade(Version currentVersion = null, Version targetVersion = null)
        {
            if (currentVersion != null && targetVersion != null && currentVersion <= targetVersion)
            {
                throw new ArgumentException("Current version must be more target version.");
            }

            IEnumerable<MongoMigration> mongoMigrations = GetMigrations(targetVersion, currentVersion)
                .OrderByDescending(x => x.Version);

            foreach (MongoMigration mongoMigration in mongoMigrations)
            {
                mongoMigration.Database = _database;

                Trace.WriteLine($"{mongoMigration.GetType().FullName}.Down() executing...");
                mongoMigration.Down();
                Trace.WriteLine($"{mongoMigration.GetType().FullName}.Down() executed.");
            }
        }
    }
}
