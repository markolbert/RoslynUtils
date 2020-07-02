using System;
using J4JSoftware.Logging;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public class DependencyInfo : DependencyInfoBase
    {
        public DependencyInfo( IJ4JLogger logger )
            : base( logger )
        {
        }

        public SemanticVersion Version { get; set; } = new SemanticVersion( 0, 0, 0 );

        public virtual bool Initialize( string text )
        {
            var parts = text.Split( '/', StringSplitOptions.RemoveEmptyEntries );

            if( parts.Length != 3 )
            {
                Logger.Error( $"Couldn't parse assembly constraint '{text}'" );

                return false;
            }

            if( !SemanticVersion.TryParse( parts[ 2 ], out var version ) )
            {
                Logger.Error( $"Couldn't parse '{parts[ 2 ]}' as a {nameof( SemanticVersion )}" );

                return false;
            }

            Assembly = parts[ 0 ];
            Version = version;

            return true;
        }
    }
}