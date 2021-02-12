#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'GeneralRoslyn' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public class VersionedText : IEquatable<VersionedText>
    {
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

        public static bool Create( string text, out VersionedText? result )
        {
            result = null;

            var parts = text.Split( '/', StringSplitOptions.RemoveEmptyEntries );

            if( parts == null || parts.Length != 2 )
                return false;


            if( !SemanticVersion.TryParse( parts[ 1 ], out var version ) )
                return false;

            result = new VersionedText
            {
                TextComponent = parts[ 0 ],
                Version = version
            };

            return true;
        }

        public override bool Equals( object? obj )
        {
            if( ReferenceEquals( null, obj ) ) return false;
            if( ReferenceEquals( this, obj ) ) return true;
            if( obj.GetType() != GetType() ) return false;
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