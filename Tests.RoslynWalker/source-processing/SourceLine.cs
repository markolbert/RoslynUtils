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
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Xunit.Sdk;

namespace Tests.RoslynWalker
{
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

        private void Initialize()
        {
            // not yet true but it will be...
            Initialized = true;

            // we're not interested in BlockClosers
            if( LineType == LineType.BlockCloser )
                return;

            foreach( var parser in new Func<BaseInfo?>[]
            {
                ParseNamespace,
                ParseDelegate,
                ParseClass, 
                ParseInterface,
                ParseEvent,
                ParseMethod, 
                ParseProperty
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

            var fieldInfo = SourceRegex.ParseField( Line );
            if( fieldInfo == null )
                return;

            // fields must be the child of either a class
            fieldInfo!.Parent = GetParent( ElementNature.Class );

            _baseInfo = fieldInfo;
        }

        private NamespaceInfo? ParseNamespace()
        {
            if( LineType != LineType.BlockOpener )
                return null;

            var retVal = SourceRegex.ParseNamespace( Line );
            if( retVal == null )
                return null;

            // namespaces can be nested so look to see if we're a child of a higher-level
            // namespace
            retVal.Parent = (NamespaceInfo?) GetParent( ElementNature.Namespace );

            return retVal;
        }

        private ClassInfo? ParseClass()
        {
            if( LineType != LineType.BlockOpener )
                return null;

            var retVal = SourceRegex.ParseClass( Line );
            if( retVal == null )
                return null;

            retVal.Parent = (ClassInfo?) GetParent( ElementNature.Class ) 
                               ?? (BaseInfo?) GetParent( ElementNature.Namespace, ElementNature.Class );

            if( retVal.Parent == null )
                throw new NullException( $"Failed to find parent/container for class '{retVal.FullName}'" );

            return retVal;
        }

        private InterfaceInfo? ParseInterface()
        {
            if( LineType != LineType.BlockOpener )
                return null;

            var retVal = SourceRegex.ParseInterface( Line );
            if( retVal == null )
                return null;

            // classes can be nested, so look back up the source code tree to see if we
            // are the child of a higher-level ClassInfo. if not we must be the child
            // of a namespace
            retVal.Parent = (ClassInfo?) GetParent( ElementNature.Class ) 
                            ?? (BaseInfo?) GetParent( ElementNature.Namespace, ElementNature.Class );

            if( retVal.Parent == null )
                throw new NullException( $"Failed to find parent/container for class '{retVal.FullName}'" );

            return retVal;
        }

        private ElementInfo? ParseDelegate()
        {
            if( LineType != LineType.Statement )
                return null;

            var retVal = SourceRegex.ParseDelegate( Line );
            if( retVal == null )
                return null;

            // delegates must be a child of a class or an interface
            if( retVal.Parent == null )
                throw new NullException( $"Failed to find parent/container for delegate '{Line}'" );

            return retVal;
        }

        private EventInfo? ParseEvent() =>
            LineType != LineType.Statement ? null : SourceRegex.ParseEvent( Line );

        private ElementInfo? ParseMethod()
        {
            if( LineType != LineType.BlockOpener )
                return null;

            // a delegate would trip the method detection algorithm but we've already
            // handled delegates.
            var retVal = SourceRegex.ParseMethod( Line );
            if( retVal == null )
                return null;

            // methods must be the child of either an interface or a class
            retVal.Parent = GetParent( ElementNature.Class, ElementNature.Interface );

            return retVal;
        }

        private ElementInfo? ParseProperty()
        {
            if( LineType != LineType.BlockOpener )
                return null;

            var retVal = SourceRegex.ParseProperty( Line );
            if( retVal == null )
                return null;

            // properties must be the child of either an interface or a class
            retVal!.Parent = GetParent( ElementNature.Class, ElementNature.Interface );

            return retVal;
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
