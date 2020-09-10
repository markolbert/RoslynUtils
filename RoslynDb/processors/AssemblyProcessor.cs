using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class AssemblyProcessor : BaseProcessorDb<IAssemblySymbol, IAssemblySymbol>
    {
        public AssemblyProcessor( 
            IEntityFactories factories,
            IJ4JLogger logger ) 
            : base( factories, logger )
        {
        }

        protected override bool InitializeProcessor( IEnumerable<IAssemblySymbol> inputData )
        {
            if( !base.InitializeProcessor( inputData ) )
                return false;

            EntityFactories.MarkSharpObjectUnsynchronized<AssemblyDb>( true );

            return true;
        }

        protected override IEnumerable<IAssemblySymbol> ExtractSymbols( ISymbol item )
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
                    EntityFactories.GetFullName( symbol ) );

                return false;
            }

            EntityFactories.MarkSynchronized( assemblyDb! );

            return true;
        }
    }
}
