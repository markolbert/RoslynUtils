using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public abstract class AssemblyProcessorBase<TSource> : BaseProcessorDb<TSource, IAssemblySymbol>
        where TSource : class, ISymbol
    {
        protected AssemblyProcessorBase(
            RoslynDbContext dbContext,
            IEntityFactories factories,
            ISymbolNamer symbolNamer,
            ISharpObjectTypeMapper sharpObjMapper,
            IJ4JLogger logger
        )
            : base( dbContext, factories, symbolNamer, sharpObjMapper, logger )
        {
        }

        protected override bool ProcessSymbol(IAssemblySymbol symbol)
        {
            return GetByFullyQualifiedName<IAssemblySymbol, AssemblyDb>( symbol, out var _, true );
            //var assemblies = GetDbSet<AssemblyDb>();

            //if (!GetByFullyQualifiedNameNG<AssemblyDb>(symbol, out var dbSymbol, true))
            //{
            //    dbSymbol = new AssemblyDb
            //    {
            //        FullyQualifiedName = SymbolNamer.GetFullyQualifiedName(symbol),
            //        Name = SymbolNamer.GetName(symbol),
            //        DotNetVersion = symbol.Identity.Version
            //    };

            //    assemblies.Add(dbSymbol);
            //}

            //dbSymbol!.Synchronized = true;

            //return true;
        }
    }
}
