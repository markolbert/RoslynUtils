using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#pragma warning disable 8602

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration( typeof( MethodParametricTypeDbConfigurator ) )]
    public class MethodParametricTypeDb : ParametricTypeDb
    {
        public int? ContainingMethodID { get; set; }
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
