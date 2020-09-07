using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class AssemblyProcessor : BaseProcessorDb<IAssemblySymbol, IAssemblySymbol>
    {
        public AssemblyProcessor( 
            RoslynDbContext dbContext, 
            IEntityFactories factories,
            ISymbolNamer symbolNamer,
            ISharpObjectTypeMapper sharpObjMapper,
            IJ4JLogger logger ) 
            : base( dbContext, factories, symbolNamer, sharpObjMapper, logger )
        {
        }

        protected override IEnumerable<IAssemblySymbol> ExtractSymbols( object item )
        {
            if (!(item is IAssemblySymbol assemblySymbol ))
            {
                Logger.Error("Supplied item is not an IAssemblySymbol");
                yield break;
            }

            yield return assemblySymbol!;
        }

        protected override bool ProcessSymbol( IAssemblySymbol symbol )
        {
            if( !EntityFactories.Retrieve<AssemblyDb>( symbol, out var assemblyDb, true ) )
            {
                Logger.Error<string>( "Couldn't retrieve AssemblyDb for '{0}'",
                    EntityFactories.GetFullyQualifiedName( symbol ) );

                return false;
            }

            MarkSynchronized( assemblyDb! );

            return true;
        }
    }
}
