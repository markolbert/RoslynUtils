using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class ArgumentProcessor : BaseProcessorDb<IMethodSymbol, IParameterSymbol>
    {
        public ArgumentProcessor( 
            RoslynDbContext dbContext, 
            ISymbolNamer symbolNamer,
            IDocObjectTypeMapper docObjMapper,
            IJ4JLogger logger ) 
            : base( dbContext, symbolNamer, docObjMapper, logger )
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
            if( !GetByFullyQualifiedName<MethodDb>( symbol.ContainingSymbol, out var methodDb ) )
                return false;

            var typeDb = GetTypeByFullyQualifiedName( symbol.Type );

            if( typeDb == null )
            {
                Logger.Error<int, string>( "Couldn't find type in database for parameter {0} in method '{1}'",
                    symbol.Ordinal,
                    SymbolNamer.GetFullyQualifiedName( symbol.ContainingSymbol ) );

                return false;
            }

            var arguments = GetDbSet<ArgumentDb>();

            var argDb = arguments
                .FirstOrDefault( a => a.Ordinal == symbol.Ordinal && a.DeclaringMethodID == methodDb!.DocObjectID );

            if( argDb == null )
            {
                argDb = new ArgumentDb()
                {
                    Ordinal = symbol.Ordinal,
                    DeclaringMethodID = methodDb!.DocObjectID
                };

                arguments.Add( argDb );
            }

            argDb.Name = SymbolNamer.GetName( symbol );
            argDb.Synchronized = true;
            argDb.ArgumentTypeID = typeDb.DocObjectID;
            argDb.IsDiscard = symbol.IsDiscard;
            argDb.IsOptional = symbol.IsOptional;
            argDb.IsParams = symbol.IsParams;
            argDb.IsThis = symbol.IsThis;

            return true;
        }

    }
}
