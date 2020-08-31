using System.Collections.Generic;
using J4JSoftware.EFCoreUtilities;
using J4JSoftware.Roslyn.Deprecated;
using J4JSoftware.Roslyn.entities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration( typeof( TypeParametricTypeDbConfigurator ) )]
    public class ParametricTypeDb : ParametricTypeBaseDb
    {
        public int ContainingTypeID { get; set; }
        public TypeDb ContainingType { get; set; }
    }

    internal class TypeParametricTypeDbConfigurator : EntityConfigurator<ParametricTypeDb>
    {
        protected override void Configure( EntityTypeBuilder<ParametricTypeDb> builder )
        {
            builder.HasOne( x => x.ContainingType )
                .WithMany( x => x.ParametricTypes )
                .HasPrincipalKey( x => x.ID )
                .HasForeignKey( x => x.ContainingTypeID );
        }
    }
}
