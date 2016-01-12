using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Quickbe.Migrator
{
    public abstract class MigrationRunner<T>
        where T : Migration
    {
        private readonly Dictionary<string, T> _migrations = new Dictionary<string, T>();

        public void LoadFromAssemblyFile(string path, Func<Type, bool> filter = null)
        {
            Assembly assembly;

            try
            {
                assembly = Assembly.LoadFile(path);
            }
            catch (Exception ex)
            {
                throw new MigrationException($"Can't load assembly from '{path}'.", ex);
            }

            LoadFromAssembly(assembly, filter);
        }

        public void LoadFromAssembly(Assembly assembly, Func<Type, bool> filter = null)
        {
            IEnumerable<T> migrations = GetMigrationsFromAssembly(assembly, filter);

            int migrationCountBefore = _migrations.Count;

            foreach (T migration in migrations)
            {
                string fullName = migration.GetType().FullName;

                if (!_migrations.ContainsKey(fullName))
                {
                    _migrations.Add(fullName, migration);
                }
            }

            Trace.WriteLine($"{_migrations.Count - migrationCountBefore} migrations loaded " +
                $"from assembly '{assembly.FullName}'.");
        }

        public abstract void Upgrade(Version currentVersion = null, Version targetVersion = null);
        public abstract void Downgrade(Version currentVersion = null, Version targetVersion = null);

        public void Upgrade(string currentVersion = null, string targetVersion = null)
        {
            Upgrade(currentVersion != null ? new Version(currentVersion) : null,
                targetVersion != null ? new Version(targetVersion) : null);
        }

        public void Downgrade(string currentVersion = null, string targetVersion = null)
        {
            Downgrade(currentVersion != null ? new Version(currentVersion) : null,
                targetVersion != null ? new Version(targetVersion) : null);
        }

        protected IEnumerable<T> GetMigrations(Version currentVersion = null, Version targetVersion = null)
        {
            IEnumerable<T> migrations = _migrations.Select(x => x.Value);

            if (currentVersion != null)
            {
                migrations = migrations.Where(x => x.Version > currentVersion);
            }

            if (targetVersion != null)
            {
                migrations = migrations.Where(x => x.Version <= targetVersion);
            }

            return migrations;
        }

        private static IEnumerable<T> GetMigrationsFromAssembly(Assembly assembly, Func<Type, bool> filter = null)
        {
            try
            {
                IEnumerable<Type> types = assembly.GetTypes()
                    .Where(t => typeof(T).IsAssignableFrom(t) && !t.IsAbstract);

                if (filter != null)
                {
                    types = types.Where(filter);
                }

                return types.Select(Activator.CreateInstance).OfType<T>();
            }
            catch (Exception ex)
            {
                throw new MigrationException(
                    $"Can't load migrations from assembly '{assembly.FullName}'.", ex);
            }
        }
    }
}
