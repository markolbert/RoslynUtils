using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.CompilerServices;
using J4JSoftware.Logging;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public class PackageLibrary : LibraryInfo
    {
        public PackageLibrary(
            IJ4JLogger<PackageLibrary> logger
        )
            : base( ReferenceType.Package, logger )
        {
        }

        public string SHA512 { get; private set; }
        public List<string> Files { get; private set; }

        public override bool Initialize( string rawName, ExpandoObject container, ProjectAssetsContext context )
        {
            if( !base.Initialize( rawName, container, context ) )
                return false;

            var okay = GetProperty<string>( container, "SHA512", context, out var sha512 );
            okay &= GetProperty<string>( container, "path", context, out var path );
            okay &= GetProperty<List<string>>( container, "files", context, out var files );

            if( !okay ) return false;

            SHA512 = sha512;
            Files = files;

            return true;
        }
    }
}