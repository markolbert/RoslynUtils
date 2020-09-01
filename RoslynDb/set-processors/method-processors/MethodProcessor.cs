using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.Roslyn
{
    public class MethodProcessor : BaseProcessorDb<IMethodSymbol, IMethodSymbol>
    {
        private readonly List<string> _placeholders = new List<string>();

        public MethodProcessor( 
            RoslynDbContext dbContext, 
            ISymbolNamer symbolNamer, 
            IJ4JLogger logger ) 
            : base( dbContext, symbolNamer, logger )
        {
        }

        protected override bool InitializeProcessor( IEnumerable<IMethodSymbol> inputData )
        {
            if( !base.InitializeProcessor( inputData ) )
                return false;

            // identify the method placeholder entities we need to replace
            _placeholders.Clear();

            var placeholders = GetDbSet<MethodPlaceholderDb>();
            _placeholders.AddRange( placeholders.Select( p => p.FullyQualifiedName ) );

            return true;
        }

        protected override bool FinalizeProcessor( IEnumerable<IMethodSymbol> inputData )
        {
            if( !base.FinalizeProcessor( inputData ) )
                return false;

            var placeholders = GetDbSet<MethodPlaceholderDb>();

            if( placeholders.Any() )
            {
                Logger.Error("MethodPlaceholderDb entities still exist in the database");
                return false;
            }

            return true;
        }

        protected override IEnumerable<IMethodSymbol> ExtractSymbols( object item )
        {
            if (!(item is IMethodSymbol methodSymbol) )
            {
                Logger.Error("Supplied item is not an IMethodSymbol");
                yield break;
            }

            yield return methodSymbol;
        }

        protected override bool ProcessSymbol( IMethodSymbol symbol )
        {
            var fqn = SymbolNamer.GetFullyQualifiedName(symbol);

            var typeDb = GetTypeByFullyQualifiedName( symbol.ContainingType );

            if( typeDb == null )
            {
                Logger.Error<string>( "Couldn't find containing type for IMethod '{0}'",
                    SymbolNamer.GetFullyQualifiedName( symbol ) );

                return false;
            }

            var retValDb = GetTypeByFullyQualifiedName( symbol.ReturnType );

            if( retValDb == null )
            {
                Logger.Error<string, string>( "Couldn't find return type '{0}' in database for method '{1}'",
                    SymbolNamer.GetFullyQualifiedName( symbol.ReturnType ),
                    fqn );

                return false;
            }

            // if this method corresponds to a placeholder method, grab the MethodParametricTypeDb object
            // referencing the placeholder so we can switch it over to the methodDb object we'll be creating
            List<MethodParametricTypeDb> methodParametricDbs = new List<MethodParametricTypeDb>();

            var methodTypeParameters = GetDbSet<MethodParametricTypeDb>();

            if ( _placeholders.Any( p => p.Equals( fqn, StringComparison.Ordinal ) ) )
            {
                var placeholders = GetDbSet<MethodPlaceholderDb>();
                var placeholderDb = placeholders.FirstOrDefault(p => p.FullyQualifiedName.Equals(fqn));

                if( placeholderDb == null )
                {
                    Logger.Error<string>( "Could not find placeholder for '{0}' in the database", fqn );
                    return false;
                }

                // remove the placeholder as otherwise adding the "real" method further
                // down will fail. we have to save the changes right away to avoid a concurrency
                // violation problem.
                placeholders.Remove( placeholderDb );
                SaveChanges();

                methodParametricDbs.AddRange( methodTypeParameters
                    .Where( mtp => mtp.ContainingMethodID == placeholderDb.ID ) );

                if( !methodParametricDbs.Any() )
                {
                    Logger.Error<string>("Could not find placeholder(s) for '{0}' in the database", fqn);
                    return false;
                }
            }


            GetByFullyQualifiedName<MethodDb>( symbol, out var methodDb, true );

            methodDb!.Name = symbol.Name;
            methodDb.Accessibility = symbol.DeclaredAccessibility;
            methodDb.DeclarationModifier = symbol.GetDeclarationModifier();
            methodDb.Kind = symbol.MethodKind;
            methodDb.DefiningTypeID = typeDb!.ID;
            methodDb.ReturnTypeID = retValDb!.ID;

            // replace placeholder referencing this method
            foreach( var methodParametricDb in methodParametricDbs! )
            {
                if( methodDb.ID == 0 )
                    methodParametricDb.ContainingMethod = methodDb;
                else methodParametricDb.ID = methodDb.ID;
            }

            methodTypeParameters.RemoveRange( methodParametricDbs );

            return true;
        }
    }
}
