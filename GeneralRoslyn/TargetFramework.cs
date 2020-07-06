using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using J4JSoftware.Logging;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public class TargetFramework : VersionedText, IEquatable<TargetFramework>
    {
        public static bool Create(string text, TargetFrameworkTextStyle style, out TargetFramework result)
        {
            result = default!;

            var (fwName, fwVersion ) = style switch
            {
                TargetFrameworkTextStyle.ExplicitVersion => ParseExplicit( text ),
                TargetFrameworkTextStyle.Simple => ParseSimple( text ),
                _ => throw new ArgumentOutOfRangeException( nameof(style), style, null )
            };

            if (!Enum.TryParse(typeof(CSharpFramework), fwName, true, out var framework))
                return false;

            if (!SemanticVersion.TryParse(fwVersion, out var version))
                return false;

            result = new TargetFramework
            {
                TextComponent = fwName,
                Framework = (CSharpFramework)framework!
            };

            // adjust the SemanticVersion for net stuff, which doesn't use periods
            if (result.Framework == CSharpFramework.Net || result.Framework == CSharpFramework.NetFramework)
            {
                if (version.Major < 10)
                    result.Version = new SemanticVersion(version.Major, 0, 0);
                else
                {
                    if (version.Major < 100)
                        result.Version = new SemanticVersion(version.Major / 10, version.Major % 10, 0);
                    else
                        result.Version = new SemanticVersion(
                            version.Major / 100,
                            (version.Major % 100) / 10,
                            (version.Major % 100) % 10
                        );
                }
            }
            else result.Version = version;

            return true;
        }

        private static (string fwName, string fwVersion) ParseSimple( string text )
        {
            var textEnd = -1;

            foreach (var curChar in text)
            {
                textEnd++;

                if (Char.IsDigit(curChar))
                    break;
            }

            // append a trailing patch of 0 if none supplied
            var fwVersion = text.Substring( textEnd );
            fwVersion = fwVersion.Count( c => c == '.' ) < 2 ? $"{fwVersion}.0" : fwVersion;

            return ( text.Substring( 0, textEnd ), fwVersion );
        }

        private static (string fwName, string fwVersion) ParseExplicit( string text )
        {
            var parts = text.Split(",Version=v", StringSplitOptions.RemoveEmptyEntries);

            if (parts == null || parts.Length != 2)
                return (string.Empty, string.Empty);

            // strip off the leading period
            var fwName = parts[ 0 ].Substring( 1 );

            // append a trailing patch of 0 if none supplied
            var fwVersion = parts[ 1 ].Count( c => c == '.' ) < 2 ? $"{parts[ 1 ]}.0" : parts[ 1 ];

            return ( fwName, fwVersion );
        }

        protected TargetFramework()
        {
        }

        public CSharpFramework Framework { get; private set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append( Framework );

            if( Framework == CSharpFramework.Net || Framework == CSharpFramework.NetFramework )
            {
                sb.Append( Version.Major );
                if( Version.Minor > 0 ) sb.Append( Version.Minor );
                if( Version.Patch > 0 ) sb.Append( Version.Patch );
            }
            else
            {
                sb.Append( $"{Version.Major}.{Version.Minor}" );
                if( Version.Patch > 0 ) sb.Append( $".{Version.Patch}" );
            }

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
            return Framework == other.Framework && Equals( Version, other.Version );
        }

        public override int GetHashCode()
        {
            return HashCode.Combine( (int) Framework, Version );
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