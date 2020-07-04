using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using J4JSoftware.Logging;

namespace J4JSoftware.Roslyn
{
    public class PackageLibrary : LibraryInfo
    {
        public PackageLibrary(
            IJ4JLogger logger
        )
            : base( ReferenceType.Package, logger )
        {
        }

        public string SHA512 { get; private set; } = string.Empty;
        public List<string> Files { get; } = new List<string>();

        public override bool Initialize( string rawName, ExpandoObject container, ProjectAssetsContext context )
        {
            if( !base.Initialize( rawName, container, context ) )
                return false;

            var okay = container.GetProperty<string>( "SHA512", out var sha512 );
            okay &= container.GetProperty<string>( "path", out var path );
            okay &= container.GetProperty<List<string>>( "files", out var files );

            if( !okay ) return false;

            SHA512 = sha512;

            Files.Clear();
            Files.AddRange( files );

            return true;
        }

        public CompilationReference GetLoadedReference( List<string> pkgFolders, TargetFramework tgtFramework )
        {
            if( GetAbsolutePath( pkgFolders, tgtFramework, out var absPathResult ) )
                return new NamedPathReference( Assembly, absPathResult!.DllPath )
                {
                    IsVirtual = absPathResult.IsVirtual
                };

            return new NamedReference( Assembly );
        }
    }
}