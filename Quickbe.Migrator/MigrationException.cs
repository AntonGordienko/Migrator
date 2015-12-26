using System;

namespace Quickbe.Migrator
{
    public class MigrationException : Exception
    {
        public MigrationException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}