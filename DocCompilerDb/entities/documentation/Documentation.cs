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
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace J4JSoftware.DocCompiler
{
    [EntityConfiguration(typeof(DocumentationConfigurator))]
    public class Documentation
    {
        private int _assemblyID;
        private int _eventID;
        private int _fieldID;
        private int _methodID;
        private int _methodArgID;
        private int _namespaceID;
        private int _namedTypeID;
        private int _propertyID;
        private int _propertyArgID;

        public int ID { get; set; }
        public string? AssetRootPath { get;set; }
        public ICollection<DocumentationEntry> Entries { get; set; }

        [DocumentedType("Assembly", "_assemblyID")]
        public int AssemblyID => _assemblyID;
        [DocumentedType("Event", "_eventID")]
        public int EventID => _eventID;
        [DocumentedType("Field", "_fieldID")]
        public int FieldID => _fieldID;
        [DocumentedType("Method", "_methodID")]
        public int MethodID => _methodID;
        [DocumentedType("MethodArgument", "_methodArgID")]
        public int MethodArgumentID => _methodArgID;
        [DocumentedType("Namespace", "_namespaceID")]
        public int NamespaceID => _namespaceID;
        [DocumentedType("NamedType", "_namedTypeID")]
        public int NamedTypeID => _namedTypeID;
        [DocumentedType("Property", "_propertyID")]
        public int PropertyID => _propertyID;
        [DocumentedType("PropertyArgument", "_propertyArgID")]
        public int PropertyArgumentID => _propertyArgID;

        public Assembly? Assembly { get; set; }
        public Event? Event { get; set; }
        public Field? Field { get; set; }
        public Method? Method { get;set; }
        public MethodArgument? MethodArgument { get; set; }
        public Namespace? Namespace { get; set; }
        public NamedType? NamedType { get; set; }
        public Property? Property { get; set; }
        public PropertyArgument? PropertyArgument { get; set; }

        public void AssociateWith( object entity )
        {
            _assemblyID = 0;
            _eventID = 0;
            _fieldID = 0;
            _methodID = 0;
            _methodArgID = 0;
            _namedTypeID = 0;
            _namespaceID = 0;
            _propertyID = 0;
            _propertyArgID = 0;

            switch( entity )
            {
                case Assembly anAssembly:
                    _assemblyID = anAssembly.ID;
                    break;

                case Event anEvent:
                    _eventID = anEvent.ID;
                    break;

                case Field aField:
                    _fieldID = aField.ID;
                    break;

                case Method aMethod:
                    _methodID = aMethod.ID;
                    break;

                case MethodArgument aMethodArg:
                    _methodArgID = aMethodArg.ID;
                    break;

                case Namespace aNamespace:
                    _namespaceID = aNamespace.ID;
                    break;

                case NamedType aNamedType:
                    _namedTypeID = aNamedType.ID;
                    break;

                case Property aProperty:
                    _propertyID = aProperty.ID;
                    break;

                case PropertyArgument aPropertyArg:
                    _propertyArgID = aPropertyArg.ID;
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
            var documentedProps = typeof(Documentation).GetProperties()
                .Where( p => p.GetCustomAttributes( typeof(DocumentedTypeAttribute), false ).Any() )
                .Select( p => new
                {
                    PropertyName = p.Name,
                    AttributeInfo = (DocumentedTypeAttribute) p.GetCustomAttributes( typeof(DocumentedTypeAttribute), false ).First()
                } );

            foreach( var docPropInfo in documentedProps )
            {
                builder.Property( docPropInfo.PropertyName )
                    .HasField( docPropInfo.AttributeInfo.BackingField );

                builder.HasOne( docPropInfo.AttributeInfo.EntityType )
                    .WithOne( "Documentation" )
                    .HasForeignKey( "Documentation", docPropInfo.PropertyName )
                    .HasPrincipalKey( docPropInfo.AttributeInfo.EntityType, "ID" );
            }
        }
    }
}