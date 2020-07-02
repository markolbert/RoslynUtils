﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using J4JSoftware.Logging;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public class VersionedText : IEquatable<VersionedText>
    {
        public static bool CreateVersionedText( string text, out VersionedText result, IJ4JLogger? logger )
        {
            result = new VersionedText();

            return result.Initialize( text, logger );
        }

        public static bool TryParseSemanticVersion( 
            string text, 
            out SemanticVersion? result,
            IJ4JLogger logger )
        {
            var pattern = @"(\d *)\.?(\d *)?\.?(\d *)?";
            var matches = Regex.Match( text, pattern );

            if( !matches.Success || matches.Groups.Count < 2 )
            {
                logger?.Error( $"Couldn't match '{text}' against the regex pattern '{pattern}' " );
                result = null;

                return false;
            }

            string? unparsedText = null;

            var numbers = matches.Groups.Cast<Group>()
                .Where( ( g, idx ) => idx > 0 && !String.IsNullOrEmpty(g.Value) )
                .Select( g =>
                {
                    if( int.TryParse( g.Value as string, out var number ) )
                        return number;

                    unparsedText = g.Value;

                    return 0;
                } )
                .ToList();

            if( unparsedText != null )
            {
                logger?.Error( $"Couldn't parse version component '{unparsedText}' to a {typeof( int )}" );
                result = null;

                return false;
            }

            for( int idx = 0; idx < 3; idx++ )
            {
                if( idx >= numbers.Count() )
                    numbers.Add( 0 );
            }

            result = new SemanticVersion( numbers[ 0 ], numbers[ 1 ], numbers[ 2 ] );

            return true;
        }

        private readonly List<string> _patterns = new List<string>();
        private readonly string _patternList;

        public VersionedText( string[]? patterns = null )
        {
            if( patterns == null || patterns.Length == 0 )
            {
                _patterns.Add( @"(.*)/(\d*)[.]?(\d)?[.]?(\d)?" );
                _patterns.Add( @"(\D*)(\d*)[.]?(\d)?[.]?(\d)?" );
            }
            else _patterns.AddRange( patterns );

            _patterns = _patterns.Distinct().ToList();
            _patternList = String.Join( " ", _patterns );
        }

        public virtual string TextComponent { get; protected set; } = string.Empty;
        public SemanticVersion Version { get; protected set; } = new SemanticVersion( 0, 0, 0 );
        public string? LastError { get; protected set; }

        public virtual bool Initialize( string text, IJ4JLogger? logger )
        {
            LastError = null;

            Match? result = null;

            foreach( var pattern in _patterns )
            {
                if( TryMatch( text, pattern, out result ) )
                    break;
            }

            if( result == null )
            {
                LastError = $"Couldn't match '{text}' against defined regex patterns {_patternList}";
                logger?.Error( LastError );

                return false;
            }

            TextComponent = result.Groups[ 1 ].Value;

            // the number of groups = number of SemanticVersion 'levels' plus 2 (one for the entire string and
            // one for the text component)
            var svLevels = result.Groups.Cast<Group>().Where( ( g, idx ) => idx > 1 )
                .Select( g =>
                {
                    if( int.TryParse( g.Value as string, out var retVal ) )
                        return retVal;

                    return 0;
                } )
                .ToList();

            // pad out to 3 levels with zeroes
            for( int idx = 0; idx < 3; idx++ )
            {
                if( idx >= svLevels.Count() )
                    svLevels.Add( 0 );
            }

            //// the SemanticVersion parser doesn't like 'partial' versions (e.g., '2.1' rather than '2.1.0')
            //// so use our own
            //if( !TryParseSemanticVersion(result.Groups[2].Value, out var version, logger))
            //{
            //    LastError = $"Couldn't parse version text '{result.Groups[ 2 ].Value}' to a {nameof( SemanticVersion )}";
            //    logger?.Error(LastError);

            //    return false;
            //}

            Version = new SemanticVersion( svLevels[ 0 ], svLevels[ 1 ], svLevels[ 2 ] );

            return true;
        }

        protected bool TryMatch( string text, string pattern, out Match? result )
        {
            var match = Regex.Match( text, pattern );

            // the number of groups = number of SemanticVersion 'levels' plus 2 (one for the entire string and
            // one for the text component), so must be at least 3 (i.e., only a 'major' level)
            if( !match.Success || match.Groups.Count < 3 )
            {
                result = null;

                return false;
            }

            result = match;

            return true;
        }

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