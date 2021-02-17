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
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Tests.RoslynWalker
{
    public static class SourceRegex
    {
        private static string _accessibilityClause = @"private|public|protected internal|protected|internal";

        private static readonly Regex _ancestry = new Regex( @"\s*(.+):\s*(.*)", RegexOptions.Compiled );
        private static readonly Regex _attributeGroup = new Regex( @"\s*(\[.*\])\s*(.*)", RegexOptions.Compiled );
        private static readonly Regex _attributes = new Regex( @$"\[([^]]*)\]", RegexOptions.Compiled );
        private static readonly Regex _typeArgsGroup = new Regex( @$"\s*([^<>]*)<(.*)>", RegexOptions.Compiled );
        private static readonly Regex _eventGroup = new Regex(@"\s*(.*)(?:event)\s+(.*)\s+(.*)\s*");
        private static readonly Regex _methodArgsGroup = new Regex( @$"\s*([^()]*)\(\s*(.*)\)" );
        private static readonly Regex _namedType =new Regex( @$"\s*({_accessibilityClause})?\s*(class|interface)?\s*(.*)" );

        private static readonly Regex _methodGroup = new Regex( @$"\s*({_accessibilityClause})?\s*([^\s]*)\s*([^\s]*)",
            RegexOptions.Compiled );

        public static EventInfo? ParseEventInfo( string text )
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

        public static bool ExtractMethodArguments( string text, out string? preamble, out List<string> arguments )
        {
            preamble = null;
            arguments = new List<string>();

            var groupMatch = _methodArgsGroup.Match( text );

            if( !groupMatch.Success || groupMatch.Groups.Count!=3)
                return false;

            preamble = groupMatch.Groups[ 1 ].Value.Trim();
            
            var remainder = groupMatch.Groups[ 2 ].Value.Trim();

            // if no arguments we're done
            if( string.IsNullOrEmpty( remainder ) )
                return true;

            arguments.AddRange( ParseArguments( remainder, true ) );

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

        public static bool ParseMethodNameTypeAccessibility( 
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

    }
}