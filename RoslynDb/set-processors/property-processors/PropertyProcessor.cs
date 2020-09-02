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
    public class PropertyProcessor : BaseProcessorDb<IPropertySymbol, IPropertySymbol>
    {
        public PropertyProcessor( 
            RoslynDbContext dbContext, 
            ISymbolNamer symbolNamer, 
            IJ4JLogger logger ) 
            : base( dbContext, symbolNamer, logger )
        {
        }

        protected override IEnumerable<IPropertySymbol> ExtractSymbols( object item )
        {
            if (!(item is IPropertySymbol propSymbol) )
            {
                Logger.Error("Supplied item is not an IPropertySymbol");
                yield break;
            }

            yield return propSymbol;
        }

        protected override bool ProcessSymbol( IPropertySymbol symbol )
        {
            var typeDb = GetTypeByFullyQualifiedName( symbol.ContainingType );

            if( typeDb == null )
            {
                Logger.Error<string>( "Couldn't find containing type for IProperty '{0}'",
                    SymbolNamer.GetFullyQualifiedName( symbol ) );

                return false;
            }

            var propTypeDb = GetTypeByFullyQualifiedName( symbol.Type );

            if( propTypeDb == null )
            {
                Logger.Error<string, string>( "Couldn't find return type '{0}' in database for property '{1}'",
                    SymbolNamer.GetFullyQualifiedName( symbol.Type ),
                    SymbolNamer.GetFullyQualifiedName(symbol) );

                return false;
            }

            GetByFullyQualifiedName<PropertyDb>( symbol, out var propDb, true );

            propDb!.Name = symbol.Name;
            propDb.GetAccessibility= symbol.GetMethod?.DeclaredAccessibility ?? Accessibility.NotApplicable;
            propDb.SetAccessibility = symbol.SetMethod?.DeclaredAccessibility ?? Accessibility.NotApplicable;
            propDb.DeclarationModifier = symbol.GetDeclarationModifier();
            propDb.DefiningTypeID = typeDb!.ID;
            propDb.PropertyTypeID = propTypeDb!.ID;
            propDb.ReturnsByRef = symbol.ReturnsByRef;
            propDb.ReturnsByRefReadOnly = symbol.ReturnsByRefReadonly;
            propDb.IsAbstract = symbol.IsAbstract;
            propDb.IsExtern = symbol.IsExtern;
            propDb.IsIndexer = symbol.IsIndexer;
            propDb.IsOverride = symbol.IsOverride;
            propDb.IsReadOnly = symbol.IsReadOnly;
            propDb.IsSealed = symbol.IsSealed;
            propDb.IsStatic = symbol.IsStatic;
            propDb.IsVirtual = symbol.IsVirtual;
            propDb.IsWriteOnly = symbol.IsWriteOnly;

            return true;
        }
    }
}
