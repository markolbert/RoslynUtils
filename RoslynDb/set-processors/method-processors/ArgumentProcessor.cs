using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class ArgumentProcessor : BaseProcessorDb<IMethodSymbol, IParameterSymbol>
    {
        public ArgumentProcessor( 
            RoslynDbContext dbContext, 
            IEntityFactories factories,
            ISymbolNamer symbolNamer,
            ISharpObjectTypeMapper sharpObjMapper,
            IJ4JLogger logger ) 
            : base( dbContext, factories, symbolNamer, sharpObjMapper, logger )
        {
        }

        protected override IEnumerable<IParameterSymbol> ExtractSymbols( object item )
        {
            if (!(item is IMethodSymbol methodSymbol) )
            {
                Logger.Error("Supplied item is not an IMethodSymbol");
                yield break;
            }

            foreach( var argSymbol in methodSymbol.Parameters )
            {
                yield return argSymbol;
            }
        }

        protected override bool ProcessSymbol( IParameterSymbol symbol )
        {
            var containerSymbol = symbol.ContainingSymbol as IMethodSymbol;

            if ( containerSymbol == null || !EntityFactories.Retrieve<MethodDb>(containerSymbol, out var methodDb))
            {
                Logger.Error<string>("IParameterSymbol '{0}' is not contained withing an IMethodSymbol",
                    SymbolNamer.GetFullyQualifiedName(symbol.ContainingSymbol));

                return false;
            }

            if( !EntityFactories.Retrieve<TypeDb>( symbol.Type, out var typeDb) )
            {
                Logger.Error<int, string>( "Couldn't find type in database for parameter {0} in method '{1}'",
                    symbol.Ordinal,
                    SymbolNamer.GetFullyQualifiedName( symbol.ContainingSymbol ) );

                return false;
            }

            var argDb = DbContext.MethodArguments
                .FirstOrDefault( a => a.Ordinal == symbol.Ordinal && a.DeclaringMethodID == methodDb!.SharpObjectID );

            if( argDb == null )
            {
                argDb = new ArgumentDb()
                {
                    Ordinal = symbol.Ordinal,
                    DeclaringMethodID = methodDb!.SharpObjectID
                };

                DbContext.MethodArguments.Add( argDb );
            }

            argDb.Name = SymbolNamer.GetName( symbol );
            argDb.Synchronized = true;
            argDb.ArgumentTypeID = typeDb!.SharpObjectID;
            argDb.IsDiscard = symbol.IsDiscard;
            argDb.IsOptional = symbol.IsOptional;
            argDb.IsParams = symbol.IsParams;
            argDb.IsThis = symbol.IsThis;

            return true;
        }

    }
}
