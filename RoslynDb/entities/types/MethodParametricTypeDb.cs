using System;
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
    [EntityConfiguration( typeof( MethodParametricTypeDbConfigurator ) )]
    public class MethodParametricTypeDb : ParametricTypeDb
    {
        public int ContainingMethodID { get; set; }
        public MethodDb? ContainingMethod { get; set; }
    }

    internal class MethodParametricTypeDbConfigurator : EntityConfigurator<MethodParametricTypeDb>
    {
        protected override void Configure( EntityTypeBuilder<MethodParametricTypeDb> builder )
        {
            builder.HasOne( x => x.ContainingMethod )
                .WithMany( x => x.ParametricTypes )
                .HasPrincipalKey( x => x.SharpObjectID )
                .HasForeignKey( x => x.ContainingMethodID );
        }
    }
}
