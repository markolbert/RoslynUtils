using System.Security.Cryptography.X509Certificates;
using J4JSoftware.EFCoreUtilities;
using J4JSoftware.Roslyn.entities;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration( typeof( TypeDefinitionMethodArgumentConfigurator ) )]
    public class TypeDefinitionMethodArgument : MethodArgument
    {
        public int TypeDefinitionID { get; set; }
        public TypeDefinition TypeDefinition { get; set; }
    }

    internal class TypeDefinitionMethodArgumentConfigurator : EntityConfigurator<TypeDefinitionMethodArgument>
    {
        protected override void Configure(EntityTypeBuilder<TypeDefinitionMethodArgument> builder)
        {
            builder.HasOne( x => x.TypeDefinition )
                .WithMany( x => x.MethodArguments )
                .HasPrincipalKey( x => x.ID )
                .HasForeignKey( x => x.TypeDefinitionID );

            builder.HasIndex( x => new { x.DeclaringMethodID, x.TypeDefinitionID } )
                .IsUnique();
        }
    }
}
