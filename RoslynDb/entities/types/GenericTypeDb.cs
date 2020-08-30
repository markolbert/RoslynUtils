﻿using System;
using System.Collections.Generic;
using J4JSoftware.EFCoreUtilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn
{
    [EntityConfiguration( typeof( GenericTypeDbConfigurator ) )]
    public class GenericTypeDb : ImplementableTypeDb
    {
    }

    internal class GenericTypeDbConfigurator : EntityConfigurator<GenericTypeDb>
    {
        protected override void Configure( EntityTypeBuilder<GenericTypeDb> builder )
        {
        }
    }
}
