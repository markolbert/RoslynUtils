﻿using System.Collections.Generic;
using J4JSoftware.EFCoreUtilities;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#pragma warning disable 8618

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration( typeof( GenericTypeDbConfigurator ) )]
    public class GenericTypeDb : ImplementableTypeDb
    {
        public bool IsTupleType { get; set; }
        // list of TypeArguments for this type
        public List<TypeArgumentDb> TypeArguments { get; set; }

    }

    internal class GenericTypeDbConfigurator : EntityConfigurator<GenericTypeDb>
    {
        protected override void Configure( EntityTypeBuilder<GenericTypeDb> builder )
        {
        }
    }
}
