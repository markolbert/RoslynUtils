using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace J4JSoftware.Roslyn.Sinks
{
    public class TypeParameterSink : RoslynDbSink<ITypeParameterSymbol, NamedType>
    {
        private readonly ISymbolSink<IAssemblySymbol, Assembly> _assemblySink;
        private readonly ISymbolSink<INamespaceSymbol, Namespace> _nsSink;

        public TypeParameterSink(
            RoslynDbContext dbContext,
            ISymbolSink<IAssemblySymbol, Assembly> assemblySink,
            ISymbolSink<INamespaceSymbol, Namespace> nsSink,
            ISymbolName symbolName,
            IJ4JLogger logger )
            : base( dbContext, symbolName, logger )
        {
            _assemblySink = assemblySink;
            _nsSink = nsSink;
        }

        public override bool InitializeSink( ISyntaxWalker syntaxWalker )
        {
            // mark all the existing assemblies as unsynchronized since we're starting
            // the synchronization process
            foreach( var ns in DbContext.TypeGenericParameters )
            {
                ns.Synchronized = false;
            }

            DbContext.SaveChanges();

            return true;
        }

        public override bool FinalizeSink( ISyntaxWalker syntaxWalker )
        {
            if( !base.FinalizeSink( syntaxWalker ) )
                return false;

            return true;
        }

        public override bool TryGetSunkValue(ITypeParameterSymbol symbol, out NamedType? result)
        {
            var symbolName = SymbolName.GetFullyQualifiedName(symbol);

            var retVal = DbContext.NamedTypes.FirstOrDefault(a => a.FullyQualifiedName == symbolName);

            if (retVal == null)
            {
                result = null;
                return false;
            }

            result = retVal;

            return true;
        }

        protected override SymbolInfo OutputSymbolInternal( ISyntaxWalker syntaxWalker, ITypeParameterSymbol symbol )
        {
            var retVal = base.OutputSymbolInternal( syntaxWalker, symbol );

            if( retVal.AlreadyProcessed )
                return retVal;

            // output the symbol to the database
            if( !_assemblySink.TryGetSunkValue( retVal.Symbol.ContainingAssembly, out var dbAssembly ) )
                return retVal;

            if( !_nsSink.TryGetSunkValue( retVal.Symbol.ContainingNamespace, out var dbNS ) )
                return retVal;

            var dbSymbol = DbContext.NamedTypes
                .Include( nt => nt.GenericConstraints )
                .Include( nt => nt.TypeGenericParameters )
                .FirstOrDefault( nt => nt.FullyQualifiedName == retVal.SymbolName );

            bool isNew = dbSymbol == null;

            dbSymbol ??= new NamedType() { FullyQualifiedName = retVal.SymbolName };

            if( isNew )
                DbContext.NamedTypes.Add( dbSymbol );

            dbSymbol.Synchronized = true;
            dbSymbol.Name = SymbolName.GetName( retVal.OriginalSymbol );
            dbSymbol.AssemblyID = dbAssembly!.ID;
            dbSymbol.NamespaceId = dbNS!.ID;
            dbSymbol.Accessibility = retVal.OriginalSymbol.DeclaredAccessibility;
            dbSymbol.DeclarationModifier = retVal.OriginalSymbol.GetDeclarationModifier();
            dbSymbol.Nature = retVal.TypeKind;
            dbSymbol.InDocumentationScope = syntaxWalker.InDocumentationScope( retVal.Symbol.ContainingAssembly );

            // now output the generictype stuff by deleting whatever is in the database and
            // recreating it
            if( dbSymbol.GenericConstraints.Any() )
                DbContext.GenericConstraints.RemoveRange(
                    DbContext.GenericConstraints.Where( gc => gc.NamedTypeID == dbSymbol.ID ) );

            if( dbSymbol.TypeGenericParameters.Any() )
                DbContext.TypeGenericParameters.RemoveRange(
                    DbContext.TypeGenericParameters.Where( tgp => tgp.NamedTypeID == dbSymbol.ID ) );



            DbContext.SaveChanges();

            retVal.WasOutput = true;

            return retVal;
        }
    }
}
