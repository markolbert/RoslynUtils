using System.Collections.Generic;
using J4JSoftware.EFCoreUtilities;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#pragma warning disable 8618
#pragma warning disable 8603

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration( typeof( NamespaceConfigurator ) )]
    public class NamespaceDb : ISharpObject
    {
        public int SharpObjectID { get; set; }
        public SharpObject SharpObject { get; set; }

        public List<AssemblyNamespaceDb>? AssemblyNamespaces { get; set; }
        public List<BaseTypeDb>? Types { get; set; }
    }

    internal class NamespaceConfigurator : EntityConfigurator<NamespaceDb>
    {
        protected override void Configure( EntityTypeBuilder<NamespaceDb> builder )
        {
            builder.HasKey( x => x.SharpObjectID );

            builder.HasOne(x => x.SharpObject)
                .WithOne(x => x.Namespace)
                .HasPrincipalKey<SharpObject>(x => x.ID)
                .HasForeignKey<NamespaceDb>(x => x.SharpObjectID);

            builder.HasMany( x => x.AssemblyNamespaces )
                .WithOne( x => x.Namespace )
                .HasForeignKey( x => x.NamespaceID )
                .HasPrincipalKey( x => x.SharpObjectID );

            builder.HasMany(x => x.Types)
                .WithOne(x => x.Namespace)
                .HasForeignKey(x => x.NamespaceID)
                .HasPrincipalKey(x => x.SharpObjectID);
        }
    }
}
