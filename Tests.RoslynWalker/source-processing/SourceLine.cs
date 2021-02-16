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
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Xunit.Sdk;

namespace Tests.RoslynWalker
{
    public static class SourceRegex
    {
        private static readonly Regex _ancestry = new Regex( @"\s*(.+):\s*(.*)", RegexOptions.Compiled );
        private static readonly Regex _attributes = new Regex( @"\s*(\[.*\])\s*(.*)", RegexOptions.Compiled );
        private static readonly Regex _firstAttr = new Regex( @"\s*\[(.+?)\]\s*(.*)", RegexOptions.Compiled );
        private static readonly Regex _typeArgs = new Regex( @"\s*(.+?)\s*<(.*)>", RegexOptions.Compiled );
        private static readonly Regex _firstTypeArg = new Regex( @"\s*(.+?),(.*)", RegexOptions.Compiled );
        private static readonly Regex _event = new Regex(@"\s*(.*event)\s+(.*)\s+(.*)\s*");
        private static readonly Regex _methodArgs = new Regex( @"\s*(.*)\((.*)\)" );

        public static bool ExtractAncestry( string text, out string? attributedDeclaration, out string? ancestry )
        {
            attributedDeclaration = null;
            ancestry = null;

            var match = _ancestry.Match( text );

            if( !match.Success )
                return false;

            ancestry = match.Groups[ 2 ].Value.Trim();
            attributedDeclaration = match.Groups[ 1 ].Value.Trim();

            return true;
        }

        public static string ExtractAttributes( string text, out List<string> attributes )
        {
            attributes = new List<string>();

            var match = _attributes.Match( text );

            if( !match.Success ) 
                return text.Trim();

            var remainder = match.Groups[ 1 ].Value.Trim();

            do
            {
                var attrMatch = _firstAttr.Match( remainder );

                if( attrMatch.Success )
                {
                    attributes.Add( attrMatch.Groups[ 1 ].Value.Trim() );
                    remainder = attrMatch.Groups.Count > 2 ? attrMatch.Groups[ 2 ].Value.Trim() : null;
                }
            } while( !string.IsNullOrEmpty( remainder ) );

            return match.Groups[ 2 ].Value.Trim();
        }

        public static string ExtractGenericArguments( string text, out List<string> typeArgs )
        {
            typeArgs = new List<string>();

            var match = _typeArgs.Match( text );

            if( !match.Success )
                return text.Trim();

            var remainder = match.Groups[ 2 ].Value.Trim();

            do
            {
                var typeArgMatch = _firstTypeArg.Match( remainder );

                if( typeArgMatch.Success )
                {
                    typeArgs.Add( typeArgMatch.Groups[ 1 ].Value.Trim() );
                    remainder = typeArgMatch.Groups.Count > 2 ? typeArgMatch.Groups[ 2 ].Value.Trim() : null;
                }
                else
                {
                    typeArgs.Add( remainder );
                    remainder = null;
                }
            } while( !string.IsNullOrEmpty( remainder ) );

            return match.Groups[ 1 ].Value.Trim();
        }

        public static bool ParseNameAccessibility( string text, out Accessibility accessibility, out string? name )
        {
            accessibility = Accessibility.Undefined;

            var parts = text.Split( " ", StringSplitOptions.RemoveEmptyEntries );

            switch( parts.Length )
            {
                case 1:
                    name = parts.Last();
                    accessibility = Accessibility.Private;

                    return true;

                case 2:
                    name = parts.Last();

                    if( !ParseAccessibility( parts[ 0 ], out var temp ) ) 
                        return false;

                    accessibility = temp!;
                    return true;

                case 3:
                    name = parts.Last();

                    if( !ParseAccessibility( parts[ 0 ], out var temp2 ) ) 
                        return false;

                    accessibility = temp2!;
                    return true;

                case 4:
                    name = parts.Last();

                    if( !ParseAccessibility( $"{parts[ 0 ]}{parts[1]}", out var temp3 ) ) 
                        return false;

                    accessibility = temp3!;
                    return true;


                default:
                    name = null;
                    return false;
            }
        }

        public static EventInfo? ParseEventInfo( string text )
        {
            var match = _event.Match( text );

            if( !match.Success )
                return null;

            Accessibility accessibility = Accessibility.Private;

            if( match.Groups.Count == 4 )
            {
                if( !ParseNameAccessibility( match.Groups[ 1 ].Value.Trim(), out var temp, out _ ) )
                    return null;

                accessibility = temp;
            }

            var nonGenericHandler = ExtractGenericArguments( match.Groups[ 2 ].Value.Trim(), out var typeArgs );

            var retVal = new EventInfo
            {
                Name = match.Groups[3].Value.Trim(),
                Accessibility = accessibility,
                EventHandler = nonGenericHandler,
            };

            retVal.EventHandlerTypeArguments.AddRange( typeArgs );

            return retVal;
        }

        public static bool ParseAccessibility( string toParse, out Accessibility result )
        {
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

    public class SourceLine
    {
        public static readonly string[] AccessTokens =
            { "public", "protected", "private", "internal", "protected internal", string.Empty };

        private BaseInfo? _baseInfo;

        public SourceLine( string line, LineType lineType, LineBlock? lineBlock )
        {
            Line = line;
            LineType = lineType;
            LineBlock = lineBlock;
        }

        public bool Initialized { get; private set; }
        public string Line { get; }
        public LineType LineType { get; }
        public LineBlock? LineBlock { get; }

        public BaseInfo? Element
        {
            get
            {
                if( Initialized )
                    return _baseInfo;

                Initialize();

                return _baseInfo;
            }
        }

        public LineBlock? ChildBlock { get; set; }

        private void TestRegEx( string text )
        {
            var okay = SourceRegex.ExtractAncestry( text, out var attributedDeclaration, out var ancestry );
            var declaration = SourceRegex.ExtractAttributes( attributedDeclaration!, out var attributes );
            var nonGenericDeclaration = SourceRegex.ExtractGenericArguments( declaration, out var typeArgs );
            okay = SourceRegex.ParseNameAccessibility( nonGenericDeclaration, out var accessibility, out var name );
        }

        private void Initialize()
        {
            // not yet true but it will be...
            Initialized = true;

            TestRegEx( " [attr1][attr2]  [attr3]   public class Wow<T1, T2<T3>>  :   AncestryText    " );
            TestRegEx( " [attr1][attr2]  [attr3]   protected internal class Wow<T1, T2<T3>>  :   AncestryText    " );
            TestRegEx( "   public class Wow<T1, T2<T3>>  :   AncestryText    " );
            TestRegEx( "   protected internal class Wow<T1, T2<T3>>  :   AncestryText    " );
            TestRegEx( " [attr1][attr2]  [attr3]   public class Wow  :   AncestryText    " );
            TestRegEx( " [attr1][attr2]  [attr3]   protected internal class Wow  :   AncestryText    " );
            TestRegEx( "   public class Wow  :   AncestryText    " );
            TestRegEx( "   protected internal class Wow  :   AncestryText    " );

            // we're not interested in BlockClosers
            if( LineType == LineType.BlockCloser )
                return;

            foreach( var parser in new Func<BaseInfo?>[]
            {
                ParseAsNamespace,
                ParseAsDelegate,
                ParseAsClass, 
                ParseAsInterface,
                ParseAsEvent,
                ParseAsMethod, 
                ParseAsProperty
            } )
            {
                var parsed = parser();
                if( parsed == null )
                    continue;

                _baseInfo = parsed;
            }

            // we assume all statements at this point are fields (events are statements but
            // we've already handled those). That's only true one level below named types...
            // but we shouldn't ever drill down more than one level below named types
            if( LineType != LineType.Statement ) 
                return;

            var endOfName = GetStartOfDelimited( '(', ')' );
            endOfName--;

            if( endOfName <= 1 )
                return;

            if( !SetNameAndAccessibility<FieldInfo>( endOfName, out var elementInfo ) ) 
                return;

            // fields must be the child of either a class
            elementInfo!.Parent = GetParent( ElementNature.Class );

            _baseInfo = elementInfo;
        }

        private NamespaceInfo? ParseAsNamespace()
        {
            if( LineType != LineType.BlockOpener )
                return null;

            var nsStart = Line.IndexOf( "namespace ", StringComparison.Ordinal );
            if( nsStart < 0 )
                return null;

            var nsParts = Line.Split( " ", StringSplitOptions.RemoveEmptyEntries );

            if( nsParts.Length != 2 )
                return null;

            // namespaces can be nested so look to see if we're a child of a higher-level
            // namespace
            return new NamespaceInfo
            {
                Name = nsParts[ 1 ], 
                Parent = (NamespaceInfo?) GetParent( ElementNature.Namespace )
            };
        }

        private BaseInfo? GetParent( params ElementNature[] nature )
        {
            var curSrcLine = this;
            BaseInfo? retVal = null;

            while( curSrcLine?.LineBlock != null )
            {
                curSrcLine = curSrcLine.LineBlock.ParentLine;

                if( curSrcLine == null )
                    break;

                if( nature.All( x => curSrcLine?.Element?.Nature != x ) ) 
                    continue;

                retVal = curSrcLine.Element;
                break;
            }

            return retVal;
        }

        private ClassInfo? ParseAsClass()
        {
            if( LineType != LineType.BlockOpener )
                return null;

            if( !SourceRegex.ExtractAncestry( Line, out var attributedDeclaration, out var ancestry ) )
                return null;

            var declaration = SourceRegex.ExtractAttributes( attributedDeclaration!, out var attributes );
            var nonGenericDeclaration = SourceRegex.ExtractGenericArguments( declaration, out var typeArgs );

            if( !SourceRegex.ParseNameAccessibility( nonGenericDeclaration, out var accessibility, out var name ) )
                return null;

            // classes can be nested, so look back up the source code tree to see if we
            // are the child of a higher-level ClassInfo. if not we must be the child
            // of a namespace
            var retVal = new ClassInfo
            {
                Accessibility = accessibility,
                Ancestry = ancestry,
                Name = name!
            };

            retVal.Attributes.AddRange( attributes );

            retVal.Parent = (ClassInfo?) GetParent( ElementNature.Class ) 
                               ?? (BaseInfo?) GetParent( ElementNature.Namespace, ElementNature.Class );

            if( retVal.Parent == null )
                throw new NullException( $"Failed to find parent/container for class '{retVal.FullName}'" );

            return retVal;
        }

        private InterfaceInfo? ParseAsInterface()
        {
            if( LineType != LineType.BlockOpener )
                return null;

            if( !SourceRegex.ExtractAncestry( Line, out var attributedDeclaration, out var ancestry ) )
                return null;

            var declaration = SourceRegex.ExtractAttributes( attributedDeclaration!, out var attributes );
            var nonGenericDeclaration = SourceRegex.ExtractGenericArguments( declaration, out var typeArgs );

            if( !SourceRegex.ParseNameAccessibility( nonGenericDeclaration, out var accessibility, out var name ) )
                return null;

            // classes can be nested, so look back up the source code tree to see if we
            // are the child of a higher-level ClassInfo. if not we must be the child
            // of a namespace
            var retVal = new InterfaceInfo
            {
                Accessibility = accessibility,
                Ancestry = ancestry,
                Name = name!
            };

            retVal.Attributes.AddRange( attributes );

            retVal.Parent = (ClassInfo?) GetParent( ElementNature.Class ) 
                            ?? (BaseInfo?) GetParent( ElementNature.Namespace, ElementNature.Class );

            if( retVal.Parent == null )
                throw new NullException( $"Failed to find parent/container for class '{retVal.FullName}'" );

            return retVal;
        }

        private ElementInfo? ParseAsDelegate()
        {
            if( LineType != LineType.Statement )
                return null;

            var methodInfo = ParseAsMethod();
            if( methodInfo == null )
                return null;

            var delegateInfo = new DelegateInfo
            {
                Accessibility = methodInfo.Accessibility,
                Name = methodInfo.Name,
                Parent = GetParent( ElementNature.Class, ElementNature.Interface )
            };

            delegateInfo.Attributes.AddRange( methodInfo.Attributes );

            // delegates must be a child of a class or an interface
            if( delegateInfo.Parent == null )
                throw new NullException( $"Failed to find parent/container for delegate '{delegateInfo.FullName}'" );

            return delegateInfo;
        }

        private (int startOfDeclaration, string? attrText) GetStartOfDeclaration( int natureStart  )
        {
            // the word immediately before the element nature, if any, defines the
            // accessibility of the element...but we need to allow for attributes
            var retVal = Line.LastIndexOf( "]", natureStart, StringComparison.Ordinal );

            return retVal < 0 ? ( 0, null ) : ( retVal++, Line[ ..retVal ].Trim() );
        }

        private (int endOfDeclaration, string? genericText) ExtractGenericArguments( int natureStart )
        {
            // to extract generic argument text we need to see if there is a generic arguments
            // clause starting before eol or any ':' character
            var startOfGenerics = Line.IndexOf( "<", natureStart, StringComparison.Ordinal );

            if( startOfGenerics < 0 )
                return ( startOfGenerics, null );

            // we need to find where the element's name ends, which is either the EOL or the ":"
            // which introduces the ancestry clause for classes and interfaces
            var endOfGenerics = GetEndOfDelimited( startOfGenerics, '<', '>', ':' );

            return ( startOfGenerics--, Line[ startOfGenerics..endOfGenerics ] );
        }

        private EventInfo? ParseAsEvent() =>
            LineType != LineType.Statement ? null : SourceRegex.ParseEventInfo( Line );

        private ElementInfo? ParseAsMethod()
        {
            if( LineType != LineType.BlockOpener )
                return null;

            // a delegate would trip the method detection algorithm but we've already
            // handled delegates. However, we need to ensure we don't detect attributes
            // so we work backwards

            // if there's no trailing parenthesis it's not a method block
            if( Line[ ^1 ] != ')' )
                return null;

            var startOfArgs = GetStartOfDelimited( '(', ')' );
            
            var endOfName = startOfArgs - 1;

            if( endOfName <= 1 )
                return null;

            if( !SetNameAndAccessibility<MethodInfo>( endOfName, out var elementInfo) ) 
                return null;

            elementInfo!.Arguments
                .AddRange( Line[ startOfArgs..^1 ]
                    .Split( ",", StringSplitOptions.RemoveEmptyEntries )
                    .Select( x => x.Trim() ) );

            // methods must be the child of either an interface or a class
            elementInfo.Parent = GetParent( ElementNature.Class, ElementNature.Interface );

            return elementInfo;
        }

        private ElementInfo? ParseAsProperty()
        {
            if( LineType != LineType.BlockOpener )
                return null;

            if( !( LineBlock?.Lines.First().Line.Equals( "get", StringComparison.Ordinal ) ?? false )
                && !( LineBlock?.Lines.First().Line.Equals( "set", StringComparison.Ordinal ) ?? false ) )
                return null;

            var endOfName = GetStartOfDelimited( '[', ']' );
            endOfName--;

            if( endOfName <= 1 )
                return null;

            if( !SetNameAndAccessibility<PropertyInfo>( endOfName, out var elementInfo ) ) 
                return null;

            // properties must be the child of either an interface or a class
            elementInfo!.Parent = GetParent( ElementNature.Class, ElementNature.Interface );

            return elementInfo;
        }

        private int GetStartOfAccessibility( int startOfName )
        {
            // find the next preceding space. If it exists it marks the start of the 
            // accessibility declaration. If it doesn't the method is private.
            // Make sure to allow for attributes
            var endOfAttributes = Line.LastIndexOf( "]", startOfName, StringComparison.Ordinal );

            var retVal = Line.LastIndexOf( " ", startOfName - 2, StringComparison.Ordinal );

            if( retVal < 0 ) 
                return retVal;

            if( endOfAttributes >= 0 && retVal < endOfAttributes )
                retVal = endOfAttributes + 2;
            else retVal++;

            return retVal;
        }

        private int GetStartOfDelimited( char openingDelimiter, char closingDelimiter )
        {
            // walk backwards through the line until the "delimiter count" (+1 for closing,
            // -1 for opening) is zero. That's the opening delimiter.
            var delimiterCount = 1;
            var retVal = Line.Length - 1;

            while( retVal > 0 )
            {
                if( Line[ retVal ] == openingDelimiter )
                    delimiterCount--;
                else
                {
                    if( Line[ retVal ] == closingDelimiter )
                        delimiterCount++;
                }

                if( delimiterCount == 0 )
                    break;

                retVal--;
            }

            return retVal;
        }

        private int GetEndOfDelimited( int start, char openingDelimiter, char closingDelimiter, char stopChar )
        {
            // walk through the line until the "delimiter count" (+1 for opening,
            // -1 for closing) is zero. That's the closing delimiter.
            var delimiterCount = 0;
            var retVal = start;

            while( retVal < Line.Length )
            {
                if( Line[ retVal ] == stopChar )
                {
                    retVal--;
                    break;
                }

                if( Line[ retVal ] == openingDelimiter )
                    delimiterCount++;
                else
                {
                    if( Line[ retVal ] == closingDelimiter )
                        delimiterCount--;
                }

                if( delimiterCount == 0 )
                    break;

                retVal++;
            }

            return retVal;
        }

        private bool SetNameAndAccessibility<TElement>( int endOfName, out TElement? result )
            where TElement : ElementInfo, new()
        {
            result = null;

            var startOfAccessibility = -1;
            var startOfName = Line.LastIndexOf( " ", endOfName, StringComparison.Ordinal );

            if( startOfName < 0 )
                startOfName = 0;
            else
            {
                startOfName++;
                startOfAccessibility = GetStartOfAccessibility( startOfName );
            }

            if( startOfAccessibility < 0 )
            {
                result = create_element( Accessibility.Private );
                return true;
            }

            if( !Enum.TryParse( typeof(Accessibility),
                Line[ startOfAccessibility..( startOfName - 1 ) ],
                true,
                out var temp ) )
            {
                _baseInfo = null;
                return false;
            }

            result = create_element( (Accessibility) temp! );
            return true;

            TElement create_element( Accessibility accessibility ) =>
                new TElement
                {
                    Accessibility = accessibility,
                    Name = Line[ startOfName..^2 ]
                };
        }
    }
}
