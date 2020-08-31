using System;
using System.Collections.Generic;
using J4JSoftware.EFCoreUtilities;
using J4JSoftware.Roslyn.entities;
using Microsoft.CodeAnalysis;
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
        }
    }
}
