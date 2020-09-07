using System;
using System.Collections.Generic;
using J4JSoftware.EFCoreUtilities;
using J4JSoftware.Roslyn.entities;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration( typeof( MethodPlaceholderDbConfigurator ) )]
    public class MethodPlaceholderDb : MethodBaseDb
    {
    }

    internal class MethodPlaceholderDbConfigurator : EntityConfigurator<MethodPlaceholderDb>
    {
        protected override void Configure( EntityTypeBuilder<MethodPlaceholderDb> builder )
        {
            builder.HasOne( x => x.SharpObject )
                .WithOne( x => x.PlaceholderMethod )
                .HasPrincipalKey<SharpObject>( x => x.ID )
                .HasForeignKey<MethodPlaceholderDb>( x => x.SharpObjectID )
                .OnDelete( DeleteBehavior.NoAction );
        }
    }
}
