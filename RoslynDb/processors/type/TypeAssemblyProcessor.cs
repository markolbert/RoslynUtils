using System.Collections.Generic;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class TypeAssemblyProcessor : BaseProcessorDb<ITypeSymbol, IAssemblySymbol>
    {
        public TypeAssemblyProcessor(
            IEntityFactories factories,
            IJ4JLogger logger
        )
            : base( factories, logger )
        {
        }

        protected override bool InitializeProcessor(IEnumerable<ITypeSymbol> inputData)
        {
            if (!base.InitializeProcessor(inputData))
                return false;

            EntityFactories.MarkSharpObjectUnsynchronized<FixedTypeDb>();
            EntityFactories.MarkSharpObjectUnsynchronized<GenericTypeDb>();
            EntityFactories.MarkSharpObjectUnsynchronized<TypeParametricTypeDb>();
            EntityFactories.MarkSharpObjectUnsynchronized<MethodParametricTypeDb>();
            EntityFactories.MarkSharpObjectUnsynchronized<ParametricTypeDb>();
            EntityFactories.MarkUnsynchronized<TypeAncestorDb>();
            EntityFactories.MarkUnsynchronized<TypeArgumentDb>(true);

            return true;
        }

        protected override IEnumerable<IAssemblySymbol> ExtractSymbols( ISymbol item )
        {
            if( !( item is ITypeSymbol typeSymbol ) )
            {
                Logger.Error("Supplied item is not an ITypeSymbol");
                yield break;
            }

            if( typeSymbol.ContainingAssembly == null )
            {
                Logger.Information<string>("ITypeSymbol '{0}' does not have a ContainingAssembly", typeSymbol.Name);
                yield break;
            }

            yield return typeSymbol.ContainingAssembly!;
        }

        protected override bool ProcessSymbol(IAssemblySymbol symbol)
        {
            if( !EntityFactories.Retrieve<AssemblyDb>( symbol, out var assemblyDb, true ) )
            {
                Logger.Error<string>("Couldn't retrieve AssemblyDb for '{0}'",
                    EntityFactories.GetFullName(symbol));

                return false;
            }

            EntityFactories.MarkSynchronized( assemblyDb! );

            return true;
        }
    }
}
