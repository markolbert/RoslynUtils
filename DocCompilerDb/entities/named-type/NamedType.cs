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
using System.Collections;
using System.Collections.Generic;
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace J4JSoftware.DocCompiler
{
    [EntityConfiguration(typeof(NamedTypeConfigurator))]
    public class NamedType
    {
        private int _containerID;
        private ContainerType _containerType = ContainerType.Undefined;
        private CodeFile? _codeFileContainer;
        private Namespace? _nsContainer;
        private NamedType? _classContainer;

        public int ID { get; set; }
        public string Name { get; set; }

        public bool Deprecated { get; set; }
        public NamedTypeKind Kind { get; set; }

        public string? ExternalUrl { get; set; }
        public bool IsExternal => !string.IsNullOrEmpty( ExternalUrl );

        public int CodeFileID { get; set; }
        public CodeFile? CodeFile { get; set; }
        
        public int ContainerID => _containerID;
        public ContainerType ContainerType => _containerType;

        public object? GetContainer() => _containerType switch
        {
            ContainerType.Namespace => _nsContainer,
            ContainerType.CodeFile => _codeFileContainer,
            ContainerType.Class => _classContainer,
            _ => null
        };

        public void SetContainer( CodeFile codeFile )
        {
            _nsContainer = null;
            _classContainer = null;
            _containerID = codeFile.ID;
            _containerType = ContainerType.CodeFile;
            _codeFileContainer = codeFile;
        }

        public void SetContainer( Namespace ns )
        {
            _codeFileContainer = null;
            _classContainer = null;
            _containerID = ns.ID;
            _containerType = ContainerType.Namespace;
            _nsContainer = ns;
        }

        public void SetContainer( NamedType classEntity )
        {
            if( classEntity.Kind != NamedTypeKind.Class )
                throw new ArgumentException(
                    $"Trying to set a {classEntity.Kind} NamedType as a container, which is not allowed" );

            _codeFileContainer = null;
            _nsContainer = null;
            _containerID = classEntity.ID;
            _containerType = ContainerType.Class;
            _classContainer = classEntity;
        }

        public ICollection<TypeConstraint> UsedInConstraints { get; set; }
        public ICollection<TypeReference> UsedInReferences { get;set; }
        public ICollection<TypeAncestor> UsedInAncestors { get; set; }
        public ICollection<Event> UsedInEvents { get;set; }
        public ICollection<Property> PropertyReturnTypes { get; set; }
        public ICollection<Method> MethodReturnTypes { get; set; }
        public ICollection<Argument> UsedInArguments { get; set; }
        public ICollection<Field> FieldTypes { get; set; }

        public ICollection<Method> Methods { get; set; }
        public ICollection<Event> Events { get; set; }
        public ICollection<TypeParameter> TypeParameters { get; set; }
        public ICollection<TypeArgument> TypeArguments { get; set; }
        public ICollection<TypeAncestor> Ancestors { get; set; }
        public ICollection<Property> Properties { get; set; }
        public ICollection<Field> Fields { get; set; }
    }

    internal class NamedTypeConfigurator : EntityConfigurator<NamedType>
    {
        protected override void Configure( EntityTypeBuilder<NamedType> builder )
        {
            builder.HasOne( x => x.CodeFile )
                .WithMany( x => x.NamedTypes )
                .HasForeignKey( x => x.CodeFileID )
                .HasPrincipalKey( x => x.ID );

            builder.Property("ContainerID")
                .HasField( "_containerID" );

            builder.Property("ContainerType")
                .HasField( "_containerType" )
                .HasConversion<string>();
        }
    }
}