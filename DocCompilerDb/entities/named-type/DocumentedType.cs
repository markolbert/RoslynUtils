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
using System.ComponentModel;
using System.Data;
using System.Linq;
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#pragma warning disable 8602
#pragma warning disable 8618

namespace J4JSoftware.DocCompiler
{
    [EntityConfiguration(typeof(DocumentedTypeConfigurator))]
    public class DocumentedType : NamedType
    {
        private int? _namespaceID;
        private int? _docTypeID;
        private ContainerType _containerType = ContainerType.Undefined;

        public string FullyQualifiedName { get; set; }

        public NamedTypeKind Kind { get; set; }
        public Documentation Documentation { get; set; }

        public Accessibility Accessibility { get; set; }
        public bool IsStatic { get; set; }
        public bool IsSealed { get; set; }
        public bool IsAbstract { get; set; }

        public int? ContainingNamespaceID => _namespaceID;
        public Namespace? ContainingNamespace { get; set; }
        
        public int? ContainingDocumentedTypeID => _docTypeID;
        public DocumentedType? ContainingDocumentedType { get; set; }

        public ContainerType ContainerType => _containerType;

        public void SetContainer( Namespace container )
        {
            _docTypeID = null;
            _namespaceID = container.ID;
            _containerType = ContainerType.Namespace;
        }

        public void SetNotContained()
        {
            _docTypeID = null;
            _namespaceID = null;
            _containerType = ContainerType.Undefined;
        }

        public void SetContainer( DocumentedType container )
        {
            _namespaceID = null;
            _docTypeID = container.ID;

            _containerType = container.Kind switch
            {
                NamedTypeKind.Class => ContainerType.Class,
                NamedTypeKind.Interface=>ContainerType.Interface,
                NamedTypeKind.Record => ContainerType.Record,
                NamedTypeKind.Struct => ContainerType.Struct,
                _ => throw new ArgumentException(
                    $"Attempting to set container on DocumentationType to a {container.Kind}, which is not allowed" )
            };
        }

        public List<NamespaceContext> GetNamespaceContext( List<NamespaceContext>? retVal = null )
        {
            retVal ??= new List<NamespaceContext>();

            var curNS = ContainingNamespace;

            while( curNS != null )
            {
                if( curNS.ChildNamespaces != null )
                {
                    foreach( var childNS in curNS.ChildNamespaces )
                    {
                        if( retVal.All( x => !x.Label.Equals( childNS.Name, StringComparison.Ordinal ) ) )
                            retVal.Add( new NamespaceContext( childNS ) );
                    }
                }

                curNS = curNS.ContainingNamespace;
            }

            return retVal;
        }

        public ICollection<CodeFile>? CodeFiles { get; set; }

        public ICollection<DocumentedType>? ChildTypes { get; set; }
        public ICollection<Method>? Methods { get; set; }
        public ICollection<Event>? Events { get; set; }
        public ICollection<TypeParameter>? TypeParameters { get; set; }
        public ICollection<TypeAncestor>? Ancestors { get; set; }
        public ICollection<Property>? Properties { get; set; }
        public ICollection<Field>? Fields { get; set; }
    }

    internal class DocumentedTypeConfigurator : EntityConfigurator<DocumentedType>
    {
        protected override void Configure( EntityTypeBuilder<DocumentedType> builder )
        {
            builder.HasMany( x => x.CodeFiles )
                .WithMany( x => x.DocumentedTypes );

            builder.Property( x => x.ContainingDocumentedTypeID )
                .HasField( "_docTypeID" );

            builder.HasOne( x => x.ContainingDocumentedType )
                .WithMany( x => x.ChildTypes )
                .HasForeignKey( x => x.ContainingDocumentedTypeID )
                .HasPrincipalKey( x => x.ID );

            builder.Property( x => x.ContainingNamespaceID )
                .HasField( "_namespaceID" );

            builder.HasOne( x => x.ContainingNamespace )
                .WithMany( x => x.DocumentedTypes )
                .HasForeignKey( x => x.ContainingNamespaceID )
                .HasPrincipalKey( x => x.ID );

            builder.Property("ContainerType")
                .HasField("_containerType")
                .HasConversion<string>();

            builder.HasIndex( x => x.FullyQualifiedName )
                .IsUnique();

            builder.Property( x => x.Accessibility )
                .HasConversion<string>()
                .HasDefaultValue( Accessibility.Private );

            builder.Property( x => x.Kind )
                .HasConversion<string>();
        }
    }
}