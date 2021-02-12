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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Tests.RoslynWalker
{
    public class TypeInfoCollection : IEnumerable<NamedTypeInfo>
    {
        private readonly Stack<NamedTypeInfo> _namedTypeStack = new();
        private readonly List<NamedTypeInfo> _types = new();

        public ReadOnlyCollection<NamedTypeInfo> NamedTypes => _types.AsReadOnly();

        public IEnumerator<NamedTypeInfo> GetEnumerator()
        {
            foreach( var typeInfo in _types ) yield return typeInfo;
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

            _namedTypeStack.Clear();

            foreach( var srcPath in Directory.GetFiles( srcDirectory!, "*.cs", SearchOption.AllDirectories ) )
            {
                if( exclusions.Any( x => srcPath.Equals( x, StringComparison.OrdinalIgnoreCase ) ) )
                    continue;

                var srcFile = new SourceFile( srcPath );

                foreach( var srcLine in srcFile.RootBlock.Lines ) ParseSourceLine( srcLine );
            }

            return true;
        }

        private void ParseSourceLine( SourceLine srcLine )
        {
            if( srcLine.Nature == ElementNature.Unknown )
                return;

            var element = srcLine.Nature switch
            {
                ElementNature.Class => ClassInfo.Create( srcLine ),
                ElementNature.Delegate => DelegateInfo.Create( srcLine ),
                ElementNature.Event => EventInfo.Create( srcLine ),
                ElementNature.Field => FieldInfo.Create( srcLine ),
                ElementNature.Interface => InterfaceInfo.Create( srcLine ),
                ElementNature.Method => MethodInfo.Create( srcLine ),
                ElementNature.Property => (ICodeElement) PropertyInfo.Create( srcLine ),
                _ => throw new InvalidEnumArgumentException(
                    $"Unsupported {nameof(ElementNature)} '{srcLine.Nature}'" )
            };

            if( element is NamedTypeInfo ntInfo )
            {
                _namedTypeStack.Push( ntInfo );

                foreach( var childLine in srcLine.ChildBlock?.Lines
                                          ?? Enumerable.Empty<SourceLine>() )
                    ParseSourceLine( childLine );

                _namedTypeStack.Pop();

                return;
            }

            var ntContainer = _namedTypeStack.Peek();

            switch( element )
            {
                case EventInfo eventInfo:
                    if( ntContainer is InterfaceInfo interfaceInfo2 )
                        interfaceInfo2.Events.Add( eventInfo );
                    else
                        throw new ArgumentException(
                            $"Trying to assign a {nameof(EventInfo)} to a {ntContainer.GetType()}" );

                    break;

                case FieldInfo fieldInfo:
                    if( ntContainer is ClassInfo classInfo )
                        classInfo.Fields.Add( fieldInfo );
                    else
                        throw new ArgumentException(
                            $"Trying to assign a {nameof(FieldInfo)} to a {ntContainer.GetType()}" );

                    break;

                case MethodInfo methodInfo:
                    if( ntContainer is InterfaceInfo interfaceInfo )
                        interfaceInfo.Methods.Add( methodInfo );
                    else
                        throw new ArgumentException(
                            $"Trying to assign a {nameof(MethodInfo)} to a {ntContainer.GetType()}" );

                    break;

                case PropertyInfo propInfo:
                    if( ntContainer is InterfaceInfo interfaceInfo3 )
                        interfaceInfo3.Properties.Add( propInfo );
                    else
                        throw new ArgumentException(
                            $"Trying to assign a {nameof(PropertyInfo)} to a {ntContainer.GetType()}" );

                    break;
            }
        }
    }
}