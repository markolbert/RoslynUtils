using System;
using System.Collections.Generic;
using J4JSoftware.EFCoreUtilities;
using J4JSoftware.Roslyn.Deprecated;
using J4JSoftware.Roslyn.entities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace J4JSoftware.Roslyn.Deprecated
{

    [EntityConfiguration( typeof( PlaceholderTypeDbConfigurator ) )]
    public class PlaceholderTypeDb : TypeDb, IParametricTypeContainer
    {
        // the type of symbol that gave rise to this place holder
        public SymbolKind ContainerKind { get; set; }

        public Type ContainerType => typeof(PlaceholderTypeDb);

        int IParametricTypeContainer.ContainerID => SharpObjectID;
        object IParametricTypeContainer.Container => this;
    }

    internal class PlaceholderTypeDbConfigurator : EntityConfigurator<PlaceholderTypeDb>
    {
        protected override void Configure( EntityTypeBuilder<PlaceholderTypeDb> builder )
        {
        }
    }
}
