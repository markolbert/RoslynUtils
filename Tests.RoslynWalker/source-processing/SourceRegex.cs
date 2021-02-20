#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'Tests.RoslynWalker' is free software: you can redistribute it
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
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;

namespace Tests.RoslynWalker
{
    public static class SourceRegex
    {
        #region regex matchers

        private static string _accessibilityClause = @"private|public|protected internal|protected|internal";

        private static readonly Regex _ancestry = new( @"\s*([^:]+):?\s*(.*)", RegexOptions.Compiled );
        private static readonly Regex _attributeGroup = new( @"\s*(\[.*\])\s*(.*)", RegexOptions.Compiled );
        private static readonly Regex _attributes = new( @$"\[([^]]*)\]", RegexOptions.Compiled );
        private static readonly Regex _typeArgsGroup = new( @$"\s*([^<>]*)<(.*)>", RegexOptions.Compiled );
        private static readonly Regex _eventGroup = new(@"\s*(.*)(?:event)\s+(.*)\s+(.*)\s*");
        private static readonly Regex _methodArgsGroup = new( @$"\s*([^()]*)\(\s*(.*)\)" );
        private static readonly Regex _namedType =new( @$"\s*({_accessibilityClause})?\s*(class|interface)?\s*(.*)" );

        private static readonly Regex _propertyGroup =
            new( @"\s*(.*)\s*(this.*)|\s*(.*)\s*", RegexOptions.Compiled );

        private static readonly Regex _propertyIndexer =
            new( @"(\w+)<.*>|(\w+)", RegexOptions.Compiled );

        private static readonly Regex _property =
            new( @$"\s*({_accessibilityClause})?\s*([\w\[\]\,]+)\s*(\w+)", RegexOptions.Compiled );

        private static readonly Regex _methodGroup = new( @$"\s*({_accessibilityClause})?\s*([^\s]*)\s*([^\s]*)",
            RegexOptions.Compiled );

        private static readonly Regex _namespace = new( @"\s*(namespace)\s*([^\s]*)", RegexOptions.Compiled );

        private static readonly Regex _delegateGroup =
            new( @"\s*([^()]*)\s*(delegate)\s*([^()]+)\(\s*(.*)\)", RegexOptions.Compiled );

        private static readonly Regex _fieldGroup =
            new Regex( @"\s*(private|public)?\s*([^<>]+)(<.+>)?\s*(\w+)\s*=?\s*(.*)?", RegexOptions.Compiled );

        #endregion

        public static NamespaceInfo? ParseNamespace( string text )
        {
            var match = _namespace.Match( text );

            if( !match.Success
                || match.Groups.Count != 3 )
                return null;

            return new NamespaceInfo(match.Groups[2].Value.Trim() );
        }

        #region named types

        public static ClassInfo? ParseClass( string text ) =>
            !ExtractNamedTypeArguments( text, "class", out var ntSource )
                ? null
                : new ClassInfo( ntSource! );

        public static InterfaceInfo? ParseInterface( string text ) =>
            !ExtractNamedTypeArguments( text, "interface", out var ntSource )
                ? null
                : new ClassInfo( ntSource! );

        public static bool ExtractNamedTypeArguments( string text, string nature, out NamedTypeSource? result )
        {
            result = null;

            if( !ExtractAncestry( text, out var fullDecl, out var ancestry ) )
                return false;

            if( !ExtractTypeArguments( fullDecl!, out var baseDecl, out var typeArgs ) )
                return false;

            var match = _namedType.Match( text );

            if( !match.Success
                || match.Groups.Count != 4
                || !match.Groups[2].Value.Trim().Equals(nature, StringComparison.Ordinal) )
                return false;

            result = new NamedTypeSource( match.Groups[ 3 ].Value.Trim(),
                match.Groups[ 1 ].Value.Trim().Replace( " ", "" ),
                ancestry!,
                typeArgs );

            return true;
        }

        public static bool ExtractAncestry( string text, out string? preamble, out string? ancestry )
        {
            preamble = null;
            ancestry = null;

            var match = _ancestry.Match( text );

            if( !match.Success )
                return false;

            if( match.Groups.Count != 3 )
                return false;

            ancestry = match.Groups[ 2 ].Value.Trim();
            preamble = match.Groups[ 1 ].Value.Trim();

            return true;
        }

        #endregion

        #region methods 

        public static MethodInfo? ParseMethod( string text )
        {
            if( !ExtractMethodArguments( text, out var fullDecl, out var arguments ) )
                return null;

            if( !ExtractTypeArguments( fullDecl!, out var typeName, out var typeArguments ) )
                return null;

            if( !ExtractMethodElements( typeName!, out var methodSrc ) )
                return null;

            return new MethodInfo( methodSrc!, typeArguments!, arguments! );
        }

        public static bool ExtractMethodArguments( string text, out string? preamble, out List<string> arguments )
        {
            preamble = null;
            arguments = new List<string>();

            var groupMatch = _methodArgsGroup.Match( text );

            if( !groupMatch.Success 
                || groupMatch.Groups.Count!=3)
                return false;

            preamble = groupMatch.Groups[ 1 ].Value.Trim();
            
            var remainder = groupMatch.Groups[ 2 ].Value.Trim();

            // if no arguments we're done
            if( string.IsNullOrEmpty( remainder ) )
                return true;

            arguments.AddRange( ParseArguments( remainder, true ) );

            return true;
        }

        public static bool ExtractMethodElements( 
            string text, 
            out ReturnTypeSource? result )
        {
            result = null;

            var match = _methodGroup.Match( text );

            if( !match.Success
                || match.Groups.Count != 4 )
                return false;

            result = new ReturnTypeSource( 
                match.Groups[ 3 ].Value.Trim(), 
                match.Groups[ 1 ].Value.Trim(),
                match.Groups[ 2 ].Value.Trim() );

            return true;
        }

        #endregion

        #region properties

        public static PropertyInfo? ParseProperty( string text )
        {
            if( !ExtractPropertyIndexers( text, out var preamble, out var indexers ) )
                return null;

            return !ExtractPropertyElements( preamble!, out var propSrc )
                ? null
                : new PropertyInfo( propSrc!, indexers );
        }

        public static bool ExtractPropertyIndexers( string text, out string? preamble, out List<string> indexers )
        {
            preamble = null;
            indexers = new List<string>();

            var groupMatch = _propertyGroup.Match( text );

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

            var indexerMatch = _propertyIndexer.Match(secondNonEmptyGroup.Group.Value.Trim());

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

        public static bool ExtractPropertyElements( string text, out ReturnTypeSource? result )
        {
            result = null;

            var match = _property.Match( text );

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

        #endregion

        #region fields

        public static FieldInfo? ParseField( string text ) => !ExtractFieldComponents( text, out var fieldSrc )
            ? null
            : new FieldInfo( fieldSrc! );

        public static bool ExtractFieldComponents( string text, out FieldSource? result )
        {
            result = null;

            var groupMatch = _fieldGroup.Match( text );

            if( !groupMatch.Success
                || groupMatch.Groups.Count != 6
                || !ParseAccessibility( groupMatch.Groups[ 2 ].Value.Trim(), out var tempAccessibility )
            )
                return false;

            result = new FieldSource( groupMatch.Groups[ 4 ].Value.Trim(),
                groupMatch.Groups[ 2 ].Value.Trim(),
                groupMatch.Groups[ 2 ].Value.Trim()
                    .Split( " ", StringSplitOptions.RemoveEmptyEntries )
                    .Last()
                + groupMatch.Groups[ 3 ].Value.Trim(),
                groupMatch.Groups[ 5 ].Value.Trim() );

            return true;
        }

        #endregion

        #region events

        public static EventInfo? ParseEvent( string text ) => !ExtractEventArguments( text, out var eventSource )
            ? null
            : new EventInfo( eventSource! );

        public static bool ExtractEventArguments( string text, out EventSource? result )
        {
            result = null;

            var match = _eventGroup.Match( text );

            if( !match.Success 
                || match.Groups.Count != 4 
                || !ExtractTypeArguments(match.Groups[2].Value.Trim(), out var baseType, out var typeArgs) )
                return false;

            result = new EventSource( match.Groups[ 3 ].Value.Trim(),
                match.Groups[ 1 ].Value.Replace( "event", string.Empty ).Trim(),
                baseType!,
                typeArgs );

            return true;
        }

        #endregion

        #region delegates

        public static DelegateInfo? ParseDelegate( string text ) =>
            !ExtractDelegateArguments( text, out var delegateSrc )
                ? null
                : new DelegateInfo( delegateSrc! );

        public static bool ExtractDelegateArguments( string text, out DelegateSource? result )
        {
            result = null;

            var groupMatch = _delegateGroup.Match( text );

            if( !groupMatch.Success
                || groupMatch.Groups.Count != 5
                || !groupMatch.Groups[ 2 ].Value.Trim().Equals( "delegate", StringComparison.Ordinal )
                || !ExtractTypeArguments( groupMatch.Groups[ 3 ].Value.Trim(), out var tempName, out var tempTypeArgs )
            )
                return false;


            result = new DelegateSource( tempName!,
                groupMatch.Groups[ 1 ].Value.Trim(),
                tempTypeArgs,
                ParseArguments( groupMatch.Groups[ 4 ].Value.Trim(), true ) );

            return true;
        }

        #endregion

        #region utililty methods

        public static bool ExtractAttributes( string text, out string? postAttribute, out List<string> attributes )
        {
            postAttribute = null;
            attributes = new List<string>();

            var groupMatch = _attributeGroup.Match( text );

            // if there were no attributes just return the text
            if( !groupMatch.Success )
            {
                postAttribute = text.Trim();
                return true;
            }

            switch( groupMatch.Groups.Count )
            {
                case < 2:
                case > 3:
                    return false;

                case > 2:
                    postAttribute = groupMatch.Groups[ 2 ].Value.Trim();
                    break;
            }

            var remainder = groupMatch.Groups[ 1 ].Value.Trim();

            var itemMatches = _attributes.Matches( remainder );

            if( !itemMatches.Any())
                return false;

            for( var idx = 0; idx < itemMatches.Count; idx++ )
            {
                if( itemMatches[ idx ].Groups.Count!=2 )
                    return false;

                attributes.Add( itemMatches[ idx ].Groups[ 1 ].Value.Trim() );
            }

            return true;
        }

        public static bool ExtractTypeArguments( string text, out string? preamble, out List<string> typeArgs )
        {
            preamble = null;
            typeArgs = new List<string>();

            var groupMatch = _typeArgsGroup.Match( text );

            // if no type arguments return the entire text because it's just preamble
            if( !groupMatch.Success )
            {
                preamble = text.Trim();
                return true;
            }

            if( groupMatch.Groups.Count != 3 )
                return false;

            preamble = groupMatch.Groups[ 1 ].Value.Trim();

            typeArgs.AddRange( ParseArguments( groupMatch.Groups[ 2 ].Value.Trim(), false ) );

            return true;
        }

        public static List<string> ParseArguments( string text, bool isMethod )
        {
            var retVal = new List<string>();
            var numLessThan = 0;
            var foundArgStart = false;
            var sb = new StringBuilder();

            foreach( var curChar in text )
            {
                switch( curChar )
                {
                    case ',':
                        if( numLessThan == 0 )
                        {
                            retVal.Add( sb.ToString() );
                            sb.Clear();
                            foundArgStart = false;
                        }
                        else sb.Append( curChar );

                        break;

                    case '<':
                        sb.Append( curChar );
                        numLessThan++;

                        break;

                    case '>':
                        sb.Append( curChar );
                        numLessThan--;
                            
                        break;

                    case ' ':
                        // we only merge types and argument names for parsing method
                        // arguments, since generic type arguments don't have argument names
                        if( isMethod && foundArgStart )
                            sb.Append( curChar );

                        break;

                    default:
                        foundArgStart = true;
                        sb.Append( curChar );

                        break;
                }
            }

            if( sb.Length > 0)
                retVal.Add(sb.ToString());

            return retVal;
        }

        public static bool ParseAccessibility( string toParse, out Accessibility result )
        {
            if( string.IsNullOrEmpty( toParse ) )
            {
                result = Accessibility.Private;
                return true;
            }

            result = Accessibility.Undefined;

            if( !Enum.TryParse( typeof(Accessibility),
                toParse.Replace( " ", string.Empty ),
                true,
                out var parsed ) )
                return false;

            result = (Accessibility) parsed!;

            return true;
        }

        #endregion
    }
}