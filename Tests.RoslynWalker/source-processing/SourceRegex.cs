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

        #region info object parsers

        public static NamespaceInfo? ParseNamespace( string text )
        {
            var match = _namespace.Match( text );

            if( !match.Success
                || match.Groups.Count != 3 )
                return null;

            return new NamespaceInfo { Name = match.Groups[2].Value.Trim() };
        }

        public static ClassInfo? ParseClass( string text ) => ParseNamedType<ClassInfo>( text );
        public static InterfaceInfo? ParseInterface( string text ) => ParseNamedType<InterfaceInfo>( text );

        private static TNamed? ParseNamedType<TNamed>( string text )
            where TNamed : InterfaceInfo, new()
        {
            if( !ExtractAncestry( text, out var fullDecl, out var ancestry ) )
                return null;

            if( !ExtractTypeArguments( fullDecl!, out var baseDecl, out var typeArgs ) )
                return null;

            var target = typeof(TNamed) == typeof(ClassInfo) ? "class" : "interface";

            if( !ExtractNamedTypeNameAccessibility( baseDecl!, target, out var name, out var accessibility ) )
                return null;

            var retVal= new TNamed
            {
                Accessibility = accessibility,
                Ancestry = ancestry,
                Name = name!
            };

            retVal.TypeArguments.AddRange(typeArgs);

            return retVal;
        }

        public static MethodInfo? ParseMethod( string text )
        {
            if( !ExtractMethodArguments( text, out var fullDecl, out var arguments ) )
                return null;

            if( !ExtractTypeArguments( fullDecl!, out var typeName, out var typeArguments ) )
                return null;

            if( !ExtractMethodNameTypeAccessibility( typeName!, out var name, out var returnType,
                out var accessibility ) )
                return null;

            var retVal = new MethodInfo
            {
                Accessibility = accessibility!,
                Name = name!,
                ReturnType = returnType!
            };

            retVal.TypeArguments.AddRange( typeArguments );
            retVal.Arguments.AddRange( arguments );

            return retVal;
        }

        public static PropertyInfo? ParseProperty( string text )
        {
            if( !ExtractPropertyIndexers( text, out var preamble, out var indexers ) )
                return null;

            if( !ExtractPropertyNameTypeAccessibility( preamble!, 
                out var name,
                out var propertyType, 
                out Accessibility accessibility ) )
                return null;

            var retVal = new PropertyInfo
            {
                Accessibility = accessibility,
                Name = name!,
                PropertyType = propertyType!
            };

            retVal.Arguments.AddRange( indexers );

            return retVal;
        }

        public static FieldInfo? ParseField( string text )
        {
            if( !ExtractFieldComponents( 
                text, 
                out var name, 
                out var type,
                out var assignmentClause,
                out Accessibility accessibility ) )
                return null;

            return new FieldInfo
            {
                Accessibility = accessibility,
                Name = name!,
                FieldType = type!,
                AssignmentClause = assignmentClause!
            };
        }

        public static EventInfo? ParseEvent( string text )
        {
            var match = _eventGroup.Match( text );

            if( !match.Success 
                || match.Groups.Count != 4 
                ||!ParseAccessibility(match.Groups[1].Value.Replace("event",string.Empty).Trim(), out var tempAccessibility)
                || !ExtractTypeArguments(match.Groups[2].Value.Trim(), out var baseType, out var typeArgs) )
                return null;

            var retVal = new EventInfo
            {
                Name = match.Groups[3].Value.Trim(),
                Accessibility = tempAccessibility!,
                EventHandler = baseType!
            };

            retVal.EventHandlerTypeArguments.AddRange( typeArgs );

            return retVal;
        }

        public static DelegateInfo? ParseDelegate( string text )
        {
            if( !ExtractDelegateArguments( text, 
                out var name, 
                out Accessibility accessibility, 
                out var typeArguments,
                out var arguments ) )
                return null;

            var retVal = new DelegateInfo
            {
                Name = name!,
                Accessibility = accessibility,
            };

            retVal.TypeArguments.AddRange( typeArguments );
            retVal.Arguments.AddRange( arguments );

            return retVal;
        }

        #endregion

        #region segment extractors

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

        public static bool ExtractDelegateArguments( 
            string text, 
            out string? name, 
            out Accessibility accessibility,
            out List<string> typeArguments,
            out List<string> arguments )
        {
            name = null;
            accessibility = Accessibility.Undefined;
            typeArguments = new List<string>();
            arguments = new List<string>();

            var groupMatch = _delegateGroup.Match( text );

            if( !groupMatch.Success
                || groupMatch.Groups.Count != 5
                || !groupMatch.Groups[ 2 ].Value.Trim().Equals( "delegate", StringComparison.Ordinal )
                || !ParseAccessibility( groupMatch.Groups[ 1 ].Value.Trim(), out var tempAccessibility )
                || !ExtractTypeArguments( groupMatch.Groups[ 3 ].Value.Trim(), out var tempName, out var tempTypeArgs )
            )
                return false;


            name = tempName;
            typeArguments.AddRange( tempTypeArgs );
            arguments.AddRange( ParseArguments( groupMatch.Groups[ 4 ].Value.Trim(), true ) );
            accessibility = tempAccessibility!;

            return true;
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

        public static bool ExtractFieldComponents( 
            string text, 
            out string? name, 
            out string? type,
            out string? assignmentClause, 
            out Accessibility accessibility )
        {
            name = null;
            type = null;
            assignmentClause = null;
            accessibility = Accessibility.Undefined;

            var groupMatch = _fieldGroup.Match( text );

            if( !groupMatch.Success
                || groupMatch.Groups.Count != 6
                || !ParseAccessibility( groupMatch.Groups[ 2 ].Value.Trim(), out var tempAccessibility )
            )
                return false;

            accessibility = tempAccessibility!;
            name = groupMatch.Groups[ 4 ].Value.Trim();
            assignmentClause = groupMatch.Groups[ 5 ].Value.Trim();

            type = groupMatch.Groups[ 2 ].Value.Trim()
                .Split( " ", StringSplitOptions.RemoveEmptyEntries )
                .Last();

            type += groupMatch.Groups[ 3 ].Value.Trim();

            return true;
        }

        public static bool ExtractNamedTypeNameAccessibility( string text, string nature, out string? name, out Accessibility accessibility )
        {
            accessibility = Accessibility.Undefined;
            name = null;

            var match = _namedType.Match( text );

            if( !match.Success
                || match.Groups.Count != 4
                || !match.Groups[2].Value.Trim().Equals(nature, StringComparison.Ordinal)
                || !ParseAccessibility( match.Groups[ 1 ].Value.Trim().Replace( " ", "" ), out var tempAccessibility ) )
                return false;

            accessibility = tempAccessibility!;
            name = match.Groups[ 3 ].Value.Trim();

            return true;
        }

        public static bool ExtractMethodNameTypeAccessibility( 
            string text, 
            out string? name, 
            out string? returnType,
            out Accessibility accessibility )
        {
            name = null;
            returnType = null;
            accessibility = Accessibility.Undefined;

            var match = _methodGroup.Match( text );

            if( !match.Success
            || match.Groups.Count != 4
            || !ParseAccessibility(match.Groups[ 1 ].Value.Trim().Replace( " ", "" ), out var tempAccessibility))
                return false;

            accessibility = tempAccessibility!;
            returnType = match.Groups[ 2 ].Value.Trim();
            name = match.Groups[ 3 ].Value.Trim();

            return true;
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

        public static bool ExtractPropertyNameTypeAccessibility( 
            string text, 
            out string? name, 
            out string? propertyType,
            out Accessibility accessibility )
        {
            name = null;
            propertyType = null;
            accessibility = Accessibility.Undefined;

            var match = _property.Match( text );

            if( !match.Success )
                return false;

            switch( match.Groups.Count )
            {
                case 3:
                    accessibility = Accessibility.Private;
                    propertyType = match.Groups[ 1 ].Value.Trim();
                    name = match.Groups[ 2 ].Value.Trim();

                    break;

                case 4:
                    if( !ParseAccessibility( match.Groups[ 1 ].Value.Trim(), out var tempAccessibility ) )
                        return false;

                    accessibility = tempAccessibility!;
                    propertyType = match.Groups[ 2 ].Value.Trim();
                    name = match.Groups[ 3 ].Value.Trim();

                    break;

                default:
                    return false;
            }

            return true;
        }

        #endregion

        #region utility methods

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