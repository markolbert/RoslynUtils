using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Tests.RoslynWalker
{
    public class TypeInfoCollection : IEnumerable<NamedTypeInfo>
    {
        private readonly List<NamedTypeInfo> _types = new();
        private readonly Stack<NamedTypeInfo> _namedTypeStack = new();

        public ReadOnlyCollection<NamedTypeInfo> NamedTypes => _types.AsReadOnly();

        public bool ParseFile( string projFilePath, out string? error )
        {
            error = null;

            if( !File.Exists( projFilePath ) )
            {
                error = $"File '{projFilePath}' does not exist";
                return false;
            }

            var srcFile = new SourceFile( projFilePath );

            _namedTypeStack.Clear();

            foreach( var srcLine in srcFile )
            {
                ParseSourceLine( srcLine );
            }

            return true;
        }

        private void ParseSourceLine( SourceLine srcLine )
        {
            if( srcLine.Nature == ElementNature.Unknown )
                return;

            var element = srcLine.Nature switch
            {
                ElementNature.Class => (ICodeElement) ClassInfo.Create( srcLine ),
                ElementNature.Delegate => (ICodeElement) DelegateInfo.Create( srcLine ),
                ElementNature.Event => (ICodeElement) EventInfo.Create( srcLine ),
                ElementNature.Field => (ICodeElement) FieldInfo.Create( srcLine ),
                ElementNature.Interface => (ICodeElement) InterfaceInfo.Create( srcLine ),
                ElementNature.Method => (ICodeElement) MethodInfo.Create( srcLine ),
                ElementNature.Property => (ICodeElement) PropertyInfo.Create( srcLine ),
                _ => throw new InvalidEnumArgumentException(
                    $"Unsupported {nameof(ElementNature)} '{srcLine.Nature}'" )
            };

            if( element is NamedTypeInfo ntInfo )
            {
                _namedTypeStack.Push( ntInfo );

                foreach( var childLine in srcLine.ChildBlock?.Lines
                                          ?? Enumerable.Empty<SourceLine>() )
                {
                    ParseSourceLine( childLine );
                }

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

        public IEnumerator<NamedTypeInfo> GetEnumerator()
        {
            foreach( var typeInfo in _types )
            {
                yield return typeInfo;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}