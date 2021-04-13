using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.EFCoreUtilities;

namespace J4JSoftware.DocCompiler
{
    public class Configuration
    {
        public Configuration()
        {
            CaseSensitiveOperatingSystem = OsUtilities.IsFileSystemCaseSensitive();
        }

        public DatabaseConfig Database {get; set; }
        public bool CaseSensitiveOperatingSystem { get; set; }

        public StringComparison OSComparison => CaseSensitiveOperatingSystem
            ? StringComparison.Ordinal
            : StringComparison.OrdinalIgnoreCase;
    }
}
