using System;

namespace Quickbe.Migrator
{
    public abstract class Migration
    {
        public Version Version { get; set; }

        protected Migration(Version version)
        {
            Version = version;
        }

        protected Migration(string version)
        {
            Version = new Version(version);
        }

        public abstract void Up();
        public abstract void Down();
    }
}
