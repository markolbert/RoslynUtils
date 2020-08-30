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
    [EntityConfiguration( typeof( FixedTypeDbConfigurator ) )]
    public class FixedTypeDb : ImplementableTypeDb
    {
    }

    internal class FixedTypeDbConfigurator : EntityConfigurator<FixedTypeDb>
    {
        protected override void Configure( EntityTypeBuilder<FixedTypeDb> builder )
        {
        }
    }
}
