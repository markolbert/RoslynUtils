using System;
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class ParametricTypeProcessor : BaseProcessorDb<ITypeSymbol, ITypeParameterSymbol>
    {
        private readonly List<string> _visitedNames = new List<string>();

        public ParametricTypeProcessor(
            EntityFactories factories,
            IJ4JLogger logger
        )
            : base( factories, logger )
        {
        }

        protected override IEnumerable<ITypeParameterSymbol> ExtractSymbols( ISymbol item )
        {
            if( !( item is ITypeSymbol typeSymbol ) )
            {
                Logger.Error( "Supplied item is not an ITypeSymbol" );
                yield break;
            }

            if( typeSymbol is IDynamicTypeSymbol || typeSymbol is IPointerTypeSymbol )
            {
                Logger.Error<string>( "Unhandled ITypeSymbol '{0}'", typeSymbol.Name );
                yield break;
            }

            if( typeSymbol is IErrorTypeSymbol )
            {
                Logger.Error( "ITypeSymbol is an IErrorTypeSymbol, ignored" );
                yield break;
            }

            // we handle ITypeParameterSymbols, which can either be the symbol itself
            // or the ElementType of the symbol if it's an IArrayTypeSymbol
            if( typeSymbol is ITypeParameterSymbol tpSymbol )
                yield return tpSymbol;

            if( typeSymbol is IArrayTypeSymbol arraySymbol
                && arraySymbol.ElementType is ITypeParameterSymbol atpSymbol )
                yield return atpSymbol;

            if( !( typeSymbol is INamedTypeSymbol ntSymbol ) )
                yield break;

            var visitedNames = new List<string>();

            if( !HasParametricTypes( ntSymbol, ref visitedNames ) )
                yield break;

            _visitedNames.Clear();

            foreach( var buried in GetBuriedTypeParameterSymbols( ntSymbol ) )
            {
                yield return buried;
            }
        }

        private IEnumerable<ITypeParameterSymbol> GetBuriedTypeParameterSymbols( INamedTypeSymbol symbol )
        {
            INamedTypeSymbol? curSymbol = symbol;

            while( curSymbol != null )
            {
                foreach( var typeArg in curSymbol.TypeArguments
                    .Where( ta => ta is ITypeParameterSymbol )
                    .Cast<ITypeParameterSymbol>() )
                {
                    yield return typeArg;
                }

                curSymbol = curSymbol!.BaseType;
            }

            foreach( var typeArg in symbol.TypeArguments )
            {
                switch( typeArg )
                {
                    case null:
                        continue;

                    case ITypeParameterSymbol tpSymbol:
                        yield return tpSymbol;
                        continue;

                    case INamedTypeSymbol ntTypeArg
                        when !_visitedNames.Any( n => n.Equals( typeArg.Name, StringComparison.Ordinal ) ):
                    {
                        _visitedNames.Add( typeArg.Name );

                        foreach( var buried in GetBuriedTypeParameterSymbols( ntTypeArg ) )
                        {
                            yield return buried;
                        }

                        break;
                    }
                }
            }

            foreach( var interfaceSymbol in symbol.Interfaces )
            {
                if( !_visitedNames.Any( n => n.Equals( interfaceSymbol.Name, StringComparison.Ordinal ) ) )
                {
                    _visitedNames.Add( interfaceSymbol.Name );

                    foreach( var buried in GetBuriedTypeParameterSymbols( interfaceSymbol ) )
                    {
                        yield return buried;
                    }
                }
            }
        }

        // symbol is guaranteed to be an ITypeParameterSymbol 
        protected override bool ProcessSymbol( ITypeParameterSymbol symbol )
        {
            if (!EntityFactories.Get<AssemblyDb>(symbol.ContainingAssembly, out var assemblyDb))
                return false;

            if (!EntityFactories.Get<NamespaceDb>(symbol.ContainingNamespace, out var nsDb))
                return false;

            if( !EntityFactories.Create<ParametricTypeDb>( symbol, out var dbSymbol ) )
            {
                Logger.Error<string>("Could not retrieve ParametricTypeDb entity for '{0}'",
                    EntityFactories.GetFullName(symbol));

                return false;
            }

            EntityFactories.MarkSynchronized( dbSymbol! );

            dbSymbol!.AssemblyID = assemblyDb!.SharpObjectID;
            dbSymbol.NamespaceID = nsDb!.SharpObjectID;

            return true;
        }
    }
}
