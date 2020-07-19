using System;
using System.IO;
using J4JSoftware.EFCoreUtilities;

namespace J4JSoftware.Roslyn
{
    public class RoslynDbContextFactoryConfiguration : IDbContextFactoryConfiguration
    {
        public static string DbName = "CSharpMetaData.db";

        private string _dbPath = Path.Combine( Environment.CurrentDirectory, DbName );

        public string DatabasePath
        {
            get => _dbPath;

            set
            {
                string fullPath = Path.GetFullPath( value );

                if( Directory.Exists( fullPath ) )
                    _dbPath = fullPath;
            }
        }
    }
}