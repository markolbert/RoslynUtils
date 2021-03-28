#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'DocCompilerDb' is free software: you can redistribute it
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
using J4JSoftware.EFCoreUtilities;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace J4JSoftware.DocCompiler
{
    public class Documentation
    {
        private DocEntityType _entityType = DocEntityType.Undefined;
        private int _entityID;
        private Assembly? _assembly;
        private Event? _event;
        private Field? _field;
        private Method? _method;
        private Namespace? _namespace;
        private NamedType? _namedType;
        private Property? _property;
        private Using? _using;

        public int ID { get; set; }

        public DocEntityType EntityType => _entityType;
        public int EntityID => _entityID;

        public object? Entity => _entityType switch
        {
            DocEntityType.Assembly => _assembly,
            DocEntityType.Event => _event,
            DocEntityType.Field => _field,
            DocEntityType.Method => _method,
            DocEntityType.Namespace => _namespace,
            DocEntityType.NamedType => _namedType,
            DocEntityType.Property => _property,
            DocEntityType.Using => _using,
            _ => null
        };

        public void AssociateWith( object entity )
        {
            _assembly = null;
            _event = null;
            _field = null;
            _method = null;
            _namespace = null;
            _namedType = null;
            _property = null;
            _using = null;

            _entityType = DocEntityType.Undefined;

            switch( entity )
            {
                case Assembly anAssembly:
                    _assembly = anAssembly;
                    _entityID = anAssembly.ID;
                    _entityType = DocEntityType.Assembly;

                    break;

                case Event anEvent:
                    _event = anEvent;
                    _entityID = anEvent.ID;
                    _entityType = DocEntityType.Event;

                    break;

                case Field aField:
                    _field = aField;
                    _entityID = aField.ID;
                    _entityType = DocEntityType.Field;

                    break;

                case Method aMethod:
                    _method = aMethod;
                    _entityID = aMethod.ID;
                    _entityType = DocEntityType.Method;

                    break;

                case Namespace aNamespace:
                    _namespace = aNamespace;
                    _entityID = aNamespace.ID;
                    _entityType = DocEntityType.Namespace;

                    break;

                case NamedType aNamedType:
                    _namedType = aNamedType;
                    _entityID = aNamedType.ID;
                    _entityType = DocEntityType.NamedType;

                    break;

                case Property aProperty:
                    _property = aProperty;
                    _entityID = aProperty.ID;
                    _entityType = DocEntityType.Property;

                    break;

                case Using aUsing:
                    _using = aUsing;
                    _entityID = aUsing.ID;
                    _entityType = DocEntityType.Using;

                    break;

                default:
                    throw new ArgumentException(
                        $"Trying to associate an unsupported entity type '{entity.GetType()}'with a Documentation instance" );
            }
        }
    }

    internal class DocumentationConfigurator : EntityConfigurator<Documentation>
    {
        protected override void Configure( EntityTypeBuilder<Documentation> builder )
        {
            builder.Ignore( x => x.Entity );
            builder.Ignore( x => x.EntityID );
            builder.Ignore( x => x.EntityType );

            builder.Property( "EntityType" ).HasField( "_entityType" );
            builder.Property( "EntityID" ).HasField( "_entityID" );

            foreach( var entityName in Enum.GetNames<DocEntityType>() )
            {
                builder.Property( entityName ).HasField( $"_{entityName.ToLower()}" );
            }
        }
    }
}