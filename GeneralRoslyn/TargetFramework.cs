using System;
using System.Text;
using System.Text.RegularExpressions;
using J4JSoftware.Logging;

namespace J4JSoftware.Roslyn
{
    public class TargetFramework : VersionedText, IEquatable<TargetFramework>
    {
        public static bool CreateTargetFramework( string text, out TargetFramework result, IJ4JLogger? logger )
        {
            result = new TargetFramework();

            return result.Initialize( text, logger );
        }

        private bool _isApp = false;

        public CSharpFramework Framework { get; protected set; }

        public bool IsApp
        {
            get => Framework == CSharpFramework.NetCoreApp || _isApp;
            set => _isApp = value;
        }

        public override bool Initialize( string text, IJ4JLogger? logger )
        {
            if( !base.Initialize( text, logger ) )
                return false;

            if( Enum.TryParse<CSharpFramework>( TextComponent, true, out var framework ) )
            {
                Framework = framework;

                return true;
            }

            // some versioned framework names come in an extended format. so check for that before failing
            var pattern = @"\.(\D*),Version=v";
            var matches = Regex.Match( TextComponent, pattern );

            if( !matches.Success || matches.Groups.Count < 2 )
            {
                LastError = $"Couldn't parse framework name '{TextComponent}' to a {nameof( CSharpFramework )}";
                logger?.Error( LastError );

                return false;
            }

            if( !Enum.TryParse<CSharpFramework>( matches.Groups[1].Value, true, out var framework2 ) )
            {
                LastError = $"Couldn't parse framework name '{matches.Groups[ 1 ].Value}' to a {nameof( CSharpFramework )}";
                logger?.Error( LastError );

                return false;
            }

            Framework = framework2;

            return true;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append( Framework );

            if( Framework == CSharpFramework.Net )
                sb.Append( Version.Major );
            else sb.Append( $"{Version.Major}.{Version.Minor}" );

            return sb.ToString();
        }

        public override bool Equals( object? obj )
        {
            if( ReferenceEquals( null, obj ) )
                return false;
            if( ReferenceEquals( this, obj ) )
                return true;
            if( obj.GetType() != this.GetType() )
                return false;

            return Equals( (TargetFramework) obj );
        }

        public bool Equals( TargetFramework? other )
        {
            if( ReferenceEquals( null, other ) )
                return false;
            if( ReferenceEquals( this, other ) )
                return true;
            return Framework == other.Framework && IsApp == other.IsApp && Equals( Version, other.Version );
        }

        public override int GetHashCode()
        {
            return HashCode.Combine( (int) Framework, IsApp, Version );
        }

        public static bool operator ==( TargetFramework? left, TargetFramework? right )
        {
            return Equals( left, right );
        }

        public static bool operator !=( TargetFramework? left, TargetFramework? right )
        {
            return !Equals( left, right );
        }

        public static bool operator >( TargetFramework? left, TargetFramework? right )
        {
            if( left == null && right == null ) return false;
            if( left == null ) return false;
            if( right == null ) return true;

            if( left.Framework != right.Framework )
                return false;

            return left.Version > right.Version;
        }

        public static bool operator <( TargetFramework? left, TargetFramework? right )
        {
            if( left == null && right == null ) return false;
            if( left == null ) return true;
            if( right == null ) return false;

            if( left.Framework != right.Framework )
                return false;

            return left.Version < right.Version;
        }

        public static bool operator >=( TargetFramework? left, TargetFramework? right )
        {
            if( Equals( left, right ) ) return true;

            return left > right;
        }

        public static bool operator <=( TargetFramework? left, TargetFramework? right )
        {
            if( Equals( left, right ) ) return true;

            return left < right;
        }
    }
}