using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Tests.RoslynWalker
{
    public class ParseProperty : ParseBase<PropertyInfo>
    {
        private static readonly Regex _rxGroup = new( @"\s*(.*)\s*(this.*)|\s*(.*)\s*", RegexOptions.Compiled );
        private static readonly Regex _rxIndexers = new( @"(\w+)<.*>|(\w+)", RegexOptions.Compiled );

        private static readonly Regex _rxProperty =
            new( @$"\s*({AccessibilityClause})?\s*([\w\[\]\,]+)\s*(\w+)", RegexOptions.Compiled );

        public ParseProperty()
            : base( ElementNature.Property, 
                @".*\s+get\s*|^get\s*$|.*\s+set\s*|^set\s*$",
                ParserFocus.FirstChildSourceLine, 
                LineType.BlockOpener )
        {
        }

        public override bool HandlesLine( SourceLine srcLine )
        {
            // to determine if this is a property line we check to see if the 
            // immediate child line is "get" or "set"
            if( srcLine.LineType != LineType.BlockOpener )
                return false;

            var toCheck = srcLine.ChildBlock?.Lines.FirstOrDefault();
            return toCheck != null && toCheck.LineType == LineType.BlockOpener && HandlesLine( toCheck );
        }

        protected override PropertyInfo? Parse( SourceLine srcLine )
        {
            if( !ExtractPropertyIndexers( srcLine.Line, out var preamble, out var indexers ) )
                return null;

            // properties must be the child of either an interface or a class
            return !ExtractPropertyElements( preamble!, out var propSrc )
                ? null
                : new PropertyInfo( propSrc!, indexers )
                {
                    Parent = GetParent( srcLine, ElementNature.Class, ElementNature.Interface )
                };
        }

        private bool ExtractPropertyIndexers( string text, out string? preamble, out List<string> indexers )
        {
            preamble = null;
            indexers = new List<string>();

            var groupMatch = _rxGroup.Match( text );

            if( !groupMatch.Success
                || groupMatch.Groups.Count != 4 )
                return false;

            var firstNonEmptyGroup = groupMatch.Groups.Values
                .Select( ( x, i ) => new { Group = x, Index = i } )
                .FirstOrDefault( x => x.Index > 0 && !string.IsNullOrEmpty( x.Group.Value ) );

            if( firstNonEmptyGroup == null )
                return false;

            preamble = firstNonEmptyGroup.Group.Value.Trim();

            var secondNonEmptyGroup = groupMatch.Groups.Values
                .Select( ( x, i ) => new { Group = x, Index = i } )
                .FirstOrDefault( x => !string.IsNullOrEmpty( x.Group.Value ) && x.Index > firstNonEmptyGroup.Index );

            // if there isn't an indexer clause, we're done
            if( secondNonEmptyGroup == null )
                return true;

            var indexerMatch = _rxIndexers.Match(secondNonEmptyGroup.Group.Value.Trim());

            if( !indexerMatch.Success
                || !indexerMatch.Value.Trim().Equals( "this", StringComparison.Ordinal )
            )
                return false;

            var typeMatch = indexerMatch;

            while( ( typeMatch = typeMatch.NextMatch() ).Success )
            {
                var nameMatch = typeMatch.NextMatch();

                if( !nameMatch.Success )
                    return false;

                indexers.Add( $"{typeMatch.Value.Trim()} {nameMatch.Value.Trim()}" );

                typeMatch = nameMatch;
            }

            return true;
        }

        private bool ExtractPropertyElements( string text, out ReturnTypeSource? result )
        {
            result = null;

            var match = _rxProperty.Match( text );

            if( !match.Success )
                return false;

            switch( match.Groups.Count )
            {
                case 3:
                    result = new ReturnTypeSource( match.Groups[ 2 ].Value.Trim(), 
                        string.Empty,
                        match.Groups[ 1 ].Value.Trim() );

                    break;

                case 4:
                    result = new ReturnTypeSource( match.Groups[ 3 ].Value.Trim(), 
                        match.Groups[ 1 ].Value.Trim(),
                        match.Groups[ 2 ].Value.Trim() );

                    break;

                default:
                    return false;
            }

            return true;
        }
    }
}