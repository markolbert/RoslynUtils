using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class TypeArgumentProcessor : BaseProcessorDb<ITypeSymbol, INamedTypeSymbol>
    {
        public TypeArgumentProcessor(
            RoslynDbContext dbContext,
            ISymbolNamer symbolInfo,
            IJ4JLogger logger
        )
            : base( dbContext, symbolInfo, logger )
        {
        }

        protected override IEnumerable<INamedTypeSymbol> ExtractSymbols(object item)
        {
            if (!(item is ITypeSymbol typeSymbol))
            {
                Logger.Error("Supplied item is not an ITypeSymbol");
                yield break;
            }

            if (typeSymbol is IDynamicTypeSymbol || typeSymbol is IPointerTypeSymbol)
            {
                Logger.Error<string>("Unhandled ITypeSymbol '{0}'", typeSymbol.Name);
                yield break;
            }

            if (typeSymbol is IErrorTypeSymbol)
            {
                Logger.Error("ITypeSymbol is an IErrorTypeSymbol, ignored");
                yield break;
            }

            // we handle INamedTypeSymbols that have TypeArguments that aren't ITypeParameterSymbols
            if( typeSymbol is INamedTypeSymbol ntSymbol 
                && ntSymbol.TypeArguments.Any( x => !( x is ITypeParameterSymbol ) ) )
                yield return ntSymbol;
        }

        protected override bool ProcessSymbol( INamedTypeSymbol symbol )
        {
            if( !ValidateAssembly( symbol, out var assemblyDb ) )
                return false;

            if( !ValidateNamespace( symbol, out var nsDb ) )
                return false;

            var declaringDb = GetTypeByFullyQualifiedName( symbol );

            if( declaringDb == null )
                return false;

            var allOkay = true;
            var typeArgs = GetDbSet<TypeArgument>();

            for( var ordinal = 0; ordinal < symbol.TypeArguments.Length; ordinal++)
            {
                var typeArgSymbol = symbol.TypeArguments[ ordinal ];

                var typeDb = GetTypeByFullyQualifiedName( typeArgSymbol );

                if( typeDb == null )
                {
                    Logger.Error<string, string>( "", 
                        SymbolInfo.GetFullyQualifiedName( typeArgSymbol ),
                        SymbolInfo.GetFullyQualifiedName( symbol ) );

                    allOkay = false;

                    continue;
                }

                var typeArgDb = typeArgs.FirstOrDefault( ta => ta.ArgumentTypeID == typeDb.ID && ta.Ordinal == ordinal );

                if( typeArgDb == null )
                {
                    typeArgDb = new TypeArgument();

                    typeArgs.Add( typeArgDb );
                }

                typeArgDb.DeclaringTypeID = declaringDb.ID;
                typeArgDb.ArgumentTypeID = typeDb.ID;
                typeArgDb.Ordinal = ordinal;
                typeArgDb.Synchronized = true;
            }

            return allOkay;
        }
    }
}
