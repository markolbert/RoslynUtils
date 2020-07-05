using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using J4JSoftware.Logging;

namespace J4JSoftware.Roslyn
{
    public class PackageLibrary : LibraryInfo
    {
        public PackageLibrary(
            string text,
            ExpandoObject libInfo,
            Func<IJ4JLogger> loggerFactory
        )
            : base( text, loggerFactory, ReferenceType.Package )
        {
            SHA512 = GetProperty<string>( libInfo, "SHA512" );
            Path = GetProperty<string>( libInfo, "path" );
            Files = GetProperty<List<string>>( libInfo, "path" );
        }

        public string SHA512 { get; }
        public string Path { get; }
        public List<string> Files { get; }

        public CompilationReference GetLoadedReference( List<string> pkgFolders, TargetFramework tgtFramework )
        {
            if( GetAbsolutePath( Path, pkgFolders, tgtFramework, out var absPathResult ) )
                return new NamedPathReference( Assembly, absPathResult!.DllPath )
                {
                    IsVirtual = absPathResult.IsVirtual
                };

            return new NamedReference( Assembly );
        }
    }
}