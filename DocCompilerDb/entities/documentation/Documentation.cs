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
        private int? _assemblyID;
        private int? _eventID;
        private int? _fieldID;
        private int? _methodID;
        private int? _methodArgID;
        private int? _namespaceID;
        private int? _documentedTypeID;
        private int? _propertyID;
        private int? _propertyArgID;
        private DocumentationTarget _docTarget;

        public int ID { get; set; }
        public string? AssetRootPath { get;set; }
        public ICollection<DocumentationEntry> Entries { get; set; }

        public DocumentationTarget DocumentationTarget => _docTarget;

        public int? AssemblyID => _assemblyID;
        public int? EventID => _eventID;
        public int? FieldID => _fieldID;
        public int? MethodID => _methodID;
        public int? MethodArgumentID => _methodArgID;
        public int? NamespaceID => _namespaceID;
        public int? DocumentedTypeID => _documentedTypeID;
        public int? PropertyID => _propertyID;
        public int? PropertyArgumentID => _propertyArgID;

        public Assembly? Assembly { get; set; }
        public Event? Event { get; set; }
        public Field? Field { get; set; }
        public Method? Method { get;set; }
        public MethodArgument? MethodArgument { get; set; }
        public Namespace? Namespace { get; set; }
        public DocumentedType? DocumentedType { get; set; }
        public Property? Property { get; set; }
        public PropertyArgument? PropertyArgument { get; set; }

        private void ClearAssociatedIDs()
        {
            _assemblyID = null;
            _eventID = null;
            _fieldID = null;
            _methodID = null;
            _methodArgID = null;
            _documentedTypeID = null;
            _namespaceID = null;
            _propertyID = null;
            _propertyArgID = null;
        }

        public void AssociateWith( Assembly target )
        {
            ClearAssociatedIDs();
            _assemblyID = target.ID;
            _docTarget = DocumentationTarget.Assembly;
        }

        public void AssociateWith(DocumentedType target)
        {
            ClearAssociatedIDs();
            _documentedTypeID = target.ID;
            _docTarget = DocumentationTarget.DocumentedType;
        }

        public void AssociateWith(Event target)
        {
            ClearAssociatedIDs();
            _eventID = target.ID;
            _docTarget = DocumentationTarget.Event;
        }

        public void AssociateWith(Field target)
        {
            ClearAssociatedIDs();
            _fieldID = target.ID;
            _docTarget = DocumentationTarget.Field;
        }

        public void AssociateWith(Method target)
        {
            ClearAssociatedIDs();
            _methodID = target.ID;
            _docTarget = DocumentationTarget.Method;
        }

        public void AssociateWith(MethodArgument target)
        {
            ClearAssociatedIDs();
            _methodArgID = target.ID;
            _docTarget = DocumentationTarget.MethodArgument;
        }

        public void AssociateWith(Property target)
        {
            ClearAssociatedIDs();
            _propertyID = target.ID;
            _docTarget = DocumentationTarget.Property;
        }

        public void AssociateWith(PropertyArgument target)
        {
            ClearAssociatedIDs();
            _propertyArgID = target.ID;
            _docTarget = DocumentationTarget.PropertyArgument;
        }
    }

    internal class DocumentationConfigurator : EntityConfigurator<Documentation>
    {
        protected override void Configure( EntityTypeBuilder<Documentation> builder )
        {
            builder.Property( x => x.DocumentationTarget )
                .HasField( "_docTarget" );

            builder.Property( x => x.AssemblyID )
                .HasField( "_assemblyID" );

            builder.HasOne( x => x.Assembly )
                .WithOne( x => x.Documentation )
                .HasForeignKey<Documentation>( x => x.AssemblyID )
                .HasPrincipalKey<Assembly>( x => x.ID );

            builder.Property(x => x.NamespaceID)
                .HasField("_namespaceID");

            builder.HasOne(x => x.Namespace)
                .WithOne(x => x.Documentation)
                .HasForeignKey<Documentation>(x => x.NamespaceID)
                .HasPrincipalKey<Namespace>(x => x.ID);

            builder.Property(x => x.DocumentedTypeID)
                .HasField("_documentedTypeID");

            builder.HasOne(x => x.DocumentedType)
                .WithOne(x => x.Documentation)
                .HasForeignKey<Documentation>(x => x.DocumentedTypeID)
                .HasPrincipalKey<DocumentedType>(x => x.ID);

            builder.Property(x => x.EventID)
                .HasField("_eventID");

            builder.HasOne(x => x.Event)
                .WithOne(x => x.Documentation)
                .HasForeignKey<Documentation>(x => x.EventID)
                .HasPrincipalKey<Event>(x => x.ID);

            builder.Property(x => x.FieldID)
                .HasField("_fieldID");

            builder.HasOne(x => x.Field)
                .WithOne(x => x.Documentation)
                .HasForeignKey<Documentation>(x => x.FieldID)
                .HasPrincipalKey<Field>(x => x.ID);

            builder.Property(x => x.MethodArgumentID)
                .HasField("_methodArgID");

            builder.HasOne(x => x.MethodArgument)
                .WithOne(x => x.Documentation)
                .HasForeignKey<Documentation>(x => x.MethodArgumentID)
                .HasPrincipalKey<MethodArgument>(x => x.ID);

            builder.Property(x => x.MethodID)
                .HasField("_methodID");

            builder.HasOne(x => x.Method)
                .WithOne(x => x.Documentation)
                .HasForeignKey<Documentation>(x => x.MethodID)
                .HasPrincipalKey<Method>(x => x.ID);

            builder.Property(x => x.PropertyArgumentID)
                .HasField("_propertyArgID");

            builder.HasOne(x => x.PropertyArgument)
                .WithOne(x => x.Documentation)
                .HasForeignKey<Documentation>(x => x.PropertyArgumentID)
                .HasPrincipalKey<PropertyArgument>(x => x.ID);

            builder.Property(x => x.PropertyID)
                .HasField("_propertyID");

            builder.HasOne(x => x.Property)
                .WithOne(x => x.Documentation)
                .HasForeignKey<Documentation>(x => x.PropertyID)
                .HasPrincipalKey<Property>(x => x.ID);

        }
    }
}