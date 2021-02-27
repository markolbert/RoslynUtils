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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Tests.RoslynWalker
{
    public class NamespaceCollection : IEnumerable<InterfaceInfo>
    {
        private readonly List<NamespaceInfo> _namespaces = new();
        private readonly ParserCollection _parsers;

        private NamespaceInfo? _curNS;
        private InterfaceInfo? _curNamedType;

        public NamespaceCollection( ParserCollection parsers )
        {
            _parsers = parsers;
        }

        public ReadOnlyCollection<NamespaceInfo> Namespaces => _namespaces.AsReadOnly();

        public IEnumerator<InterfaceInfo> GetEnumerator()
        {
            foreach( var curInterface in _namespaces.SelectMany(x=>x.Interfaces))
            {
                yield return curInterface;
            }

            foreach( var curClass in _namespaces.SelectMany(x=>x.Classes))
            {
                yield return curClass;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool ParseFile( string projFilePath, out string? error )
        {
            error = null;

            if( !File.Exists( projFilePath ) )
            {
                error = $"File '{projFilePath}' does not exist";
                return false;
            }

            var projXML = XDocument.Load( File.OpenRead( projFilePath ) );

            var srcDirectory = Path.GetDirectoryName( projFilePath );

            var exclusions = projXML.Document!.Descendants()
                .Where( x => x.Name.LocalName.Equals( "compile", StringComparison.OrdinalIgnoreCase )
                             && x.HasAttributes )
                .SelectMany( x => x.Attributes()
                    .Where( y => y.Name.LocalName.Equals( "remove", StringComparison.OrdinalIgnoreCase ) )
                    .Select( y => Path.Combine( srcDirectory!, y.Value ) ) )
                .ToList();

            foreach( var srcPath in Directory.GetFiles( srcDirectory!, "*.cs", SearchOption.AllDirectories ) )
            {
                if( exclusions.Any( x => srcPath.Equals( x, StringComparison.OrdinalIgnoreCase ) ) )
                    continue;

                var srcFile = new SourceFile( srcPath );
                srcFile.ParseFile( _parsers );

                foreach( var srcLine in srcFile.RootBlock!.Lines )
                {
                    ParseSourceLine( srcLine );
                }
            }

            return true;
        }

        private void ParseSourceLine( SourceLine srcLine )
        {
            if( srcLine.Elements == null )
                return;

            foreach( var curElement in srcLine.Elements )
            {
                switch( curElement )
                {
                    case NamespaceInfo nsInfo:
                        _curNS = _namespaces.FirstOrDefault( x =>
                            x.FullName.Equals( nsInfo.FullName, StringComparison.Ordinal ) );

                        if( _curNS == null )
                        {
                            _namespaces.Add( nsInfo );
                            _curNS = nsInfo;
                        }

                        break;

                    case DelegateInfo dInfo:
                        if( _curNamedType is ClassInfo delClass )
                            delClass.Delegates.Add( dInfo );
                        else
                            throw new ArgumentException(
                                $"Trying to add a DelegateInfo to a {_curNamedType?.GetType()}" );

                        break;

                    case ClassInfo cInfo:
                        _curNamedType = cInfo;
                        break;

                    case InterfaceInfo iInfo:
                        _curNamedType = iInfo;
                        break;

                    case EventInfo eventInfo:
                        _curNamedType!.Events.Add( eventInfo );
                        break;

                    case FieldInfo fieldInfo:
                        if( _curNamedType is ClassInfo classInfo )
                            classInfo.Fields.Add( fieldInfo );
                        else
                            throw new ArgumentException(
                                $"Trying to assign a {nameof(FieldInfo)} to a {_curNamedType!.GetType()}" );

                        break;

                    case MethodInfo methodInfo:
                        _curNamedType!.Methods.Add( methodInfo );
                        break;

                    case PropertyInfo propertyInfo:
                        _curNamedType!.Properties.Add( propertyInfo );
                        break;
                }

                // we only drill into block openers
                if( srcLine is not BlockOpeningLine blockOpeningLine ) 
                    continue;

                foreach( var childLine in blockOpeningLine.ChildBlock.Lines
                                          ?? Enumerable.Empty<SourceLine>() )
                {
                    ParseSourceLine( childLine );
                }
            }
        }
    }
}