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
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#pragma warning disable 8602
#pragma warning disable 8618

namespace J4JSoftware.DocCompiler
{
    [EntityConfiguration(typeof(DocumentationConfigurator))]
    public class Documentation
    {
        private int? _assemblyID;
        private int? _eventID;
        private int? _fieldID;
        private int? _methodID;
        private int? _argID;
        private int? _namespaceID;
        private int? _namedTypeID;
        private int? _propertyID;
        private DocumentationTarget _docTarget;

        public int ID { get; set; }
        public string? AssetRootPath { get;set; }
        public ICollection<DocumentationEntry> Entries { get; set; }

        public DocumentationTarget DocumentationTarget => _docTarget;

        public int? AssemblyID => _assemblyID;
        public int? EventID => _eventID;
        public int? FieldID => _fieldID;
        public int? MethodID => _methodID;
        public int? ArgumentID => _argID;
        public int? NamespaceID => _namespaceID;
        public int? NamedTypeID => _namedTypeID;
        public int? PropertyID => _propertyID;

        public Assembly? Assembly { get; set; }
        public Event? Event { get; set; }
        public Field? Field { get; set; }
        public Method? Method { get;set; }
        public Argument? Argument { get; set; }
        public Namespace? Namespace { get; set; }
        public NamedType? NamedType { get; set; }
        public Property? Property { get; set; }

        private void ClearAssociatedIDs()
        {
            _assemblyID = null;
            _eventID = null;
            _fieldID = null;
            _methodID = null;
            _argID = null;
            _namedTypeID = null;
            _namespaceID = null;
            _propertyID = null;
        }

        public void AssociateWith( Assembly target )
        {
            ClearAssociatedIDs();
            _assemblyID = target.ID;
            _docTarget = DocumentationTarget.Assembly;
        }

        public void AssociateWith(Namespace target)
        {
            ClearAssociatedIDs();
            _namespaceID = target.ID;
            _docTarget = DocumentationTarget.Namespace;
        }

        public void AssociateWith(NamedType target)
        {
            ClearAssociatedIDs();
            _namedTypeID = target.ID;
            _docTarget = DocumentationTarget.NamedType;
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

        public void AssociateWith( Argument target )
        {
            ClearAssociatedIDs();
            _argID = target.ID;

            _docTarget = target switch
            {
                MethodArgument methodArg => DocumentationTarget.MethodArgument,
                PropertyArgument propArg => DocumentationTarget.PropertyArgument,
                _ => throw new ArgumentException( $"Unsupported Argument type '{target.GetType()}'" )
            };
        }

        public void AssociateWith(Property target)
        {
            ClearAssociatedIDs();
            _propertyID = target.ID;
            _docTarget = DocumentationTarget.Property;
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

            builder.Property(x => x.NamedTypeID)
                .HasField("_namedTypeID");

            builder.HasOne(x => x.NamedType)
                .WithOne(x => x.Documentation)
                .HasForeignKey<Documentation>(x => x.NamedTypeID)
                .HasPrincipalKey<NamedType>(x => x.ID);

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

            builder.Property(x => x.ArgumentID)
                .HasField("_argID");

            builder.HasOne(x => x.Argument)
                .WithOne(x => x.Documentation)
                .HasForeignKey<Documentation>(x => x.ArgumentID)
                .HasPrincipalKey<Argument>(x => x.ID);

            builder.Property(x => x.MethodID)
                .HasField("_methodID");

            builder.HasOne(x => x.Method)
                .WithOne(x => x.Documentation)
                .HasForeignKey<Documentation>(x => x.MethodID)
                .HasPrincipalKey<Method>(x => x.ID);

            builder.Property(x => x.PropertyID)
                .HasField("_propertyID");

            builder.HasOne(x => x.Property)
                .WithOne(x => x.Documentation)
                .HasForeignKey<Documentation>(x => x.PropertyID)
                .HasPrincipalKey<Property>(x => x.ID);
        }
    }
}