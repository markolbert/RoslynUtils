using System;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public class VersionedText : IEquatable<VersionedText>
    {
        public static bool Create(string text, out VersionedText? result)
        {
            result = null;

            var parts = text.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (parts == null || parts.Length != 2)
                return false;


            if (!SemanticVersion.TryParse(parts[1], out var version))
                return false;

            result = new VersionedText
            {
                TextComponent = parts[0],
                Version = version
            };

            return true;
        }

        protected VersionedText()
        {
        }

        public string TextComponent { get; protected set; } = string.Empty;
        public SemanticVersion Version { get; protected set; } = null!;

        public bool Equals( VersionedText? other )
        {
            if( ReferenceEquals( null, other ) ) return false;
            if( ReferenceEquals( this, other ) ) return true;
            return TextComponent == other.TextComponent && Equals( Version, other.Version );
        }

        public override bool Equals( object? obj )
        {
            if( ReferenceEquals( null, obj ) ) return false;
            if( ReferenceEquals( this, obj ) ) return true;
            if( obj.GetType() != this.GetType() ) return false;
            return Equals( (VersionedText) obj );
        }

        public override int GetHashCode()
        {
            return HashCode.Combine( TextComponent, Version );
        }

        public static bool operator ==( VersionedText left, VersionedText right )
        {
            return Equals( left, right );
        }

        public static bool operator !=( VersionedText left, VersionedText right )
        {
            return !Equals( left, right );
        }
    }
}