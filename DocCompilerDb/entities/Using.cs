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

using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace J4JSoftware.DocCompiler
{
    [EntityConfiguration(typeof(UsingConfigurator))]
    public class Using : IDeprecation
    {
        private int _containerID;
        private ContainerType _containerType = ContainerType.Undefined;
        private CodeFile? _codeFileContainer;
        private Namespace? _nsContainer;

        public int ID { get; set; }
        public string Name { get;set; }
        public bool Deprecated { get; set; }

        public int ContainerID => _containerID;
        public ContainerType ContainerType => _containerType;

        public object? GetContainer() => _containerType switch
        {
            ContainerType.Namespace => _nsContainer,
            ContainerType.CodeFile => _codeFileContainer,
            _ => null
        };

        public void SetContainer( CodeFile codeFile )
        {
            _nsContainer = null;
            _containerID = codeFile.ID;
            _containerType = ContainerType.CodeFile;
            _codeFileContainer = codeFile;
        }

        public void SetContainer( Namespace ns )
        {
            _codeFileContainer = null;
            _containerID = ns.ID;
            _containerType = ContainerType.Namespace;
            _nsContainer = ns;
        }
    }

    internal class UsingConfigurator : EntityConfigurator<Using>
    {
        protected override void Configure( EntityTypeBuilder<Using> builder )
        {
            builder.HasIndex( x => x.Name )
                .IsUnique();

            builder.Property( "ContainerID" )
                .HasField( "_containerID" );

            builder.Property( "ContainerType" )
                .HasField("_containerType")
                .HasConversion<string>();
        }
    }
}