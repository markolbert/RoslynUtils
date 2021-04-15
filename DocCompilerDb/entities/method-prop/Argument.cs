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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#pragma warning disable 8618

namespace J4JSoftware.DocCompiler
{
    [EntityConfiguration(typeof(ArgumentConfigurator))]
    public class Argument 
    {
        protected Argument()
        {
        }

        public int ID { get; set; }
        public bool Deprecated { get; set; }
        public bool HasThis {get; set; }
        public int ArgumentTypeID { get; set; }
        public NamedType ArgumentType { get; set; }
        public Documentation Documentation { get; set; }
    }

    internal class ArgumentConfigurator : EntityConfigurator<Argument>
    {
        protected override void Configure( EntityTypeBuilder<Argument> builder )
        {
            builder.ToTable( "Arguments" );

            builder.HasOne( x => x.ArgumentType )
                .WithMany( x => x.UsedInArguments )
                .HasForeignKey( x => x.ArgumentTypeID )
                .HasPrincipalKey( x => x.ID );
        }
    }
}