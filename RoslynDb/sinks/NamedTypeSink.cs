using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public class NamedTypeSink : RoslynDbSink<INamedTypeSymbol, NamedType>
    {
        private readonly ISymbolSink<IAssemblySymbol, Assembly> _assemblySink;
        private readonly ISymbolSink<INamespaceSymbol, Namespace> _nsSink;
        private readonly List<ITypeProcessor> _processors;
        private readonly Dictionary<string, INamedTypeSymbol> _typeSymbols = new Dictionary<string, INamedTypeSymbol>();

        public NamedTypeSink(
            RoslynDbContext dbContext,
            ISymbolSink<IAssemblySymbol, Assembly> assemblySink,
            ISymbolSink<INamespaceSymbol, Namespace> nsSink,
            ISymbolName symbolName,
            IEnumerable<ITypeProcessor> typeProcessors,
            IJ4JLogger logger )
            : base( dbContext, symbolName, logger )
        {
            _assemblySink = assemblySink;
            _nsSink = nsSink;

            var temp = typeProcessors.ToList();

            var error = TopologicalSorter.CreateSequence( temp, out var processors );

            if( error != null )
                Logger.Error( error );

            _processors = processors ?? new List<ITypeProcessor>();
        }

        public override bool InitializeSink( ISyntaxWalker syntaxWalker )
        {
            // clear the collection of processed type symbols
            _typeSymbols.Clear();

            // mark all the existing assemblies as unsynchronized since we're starting
            // the synchronization process
            foreach( var ns in DbContext.NamedTypes )
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

            // we want to add all the parent types for each symbol's inheritance tree.
            // but to do that we first have to add all the relevant assemblies and namespaces
            var allOkay = true;

            var typeList = _typeSymbols.Select(ts => ts.Value)
                .ToList();

            var context = new TypeProcessorContext( syntaxWalker, typeList );

            foreach( var processor in _processors )
            {
                allOkay &= processor.Process( context );
            }

            // now we can add the parent types
            foreach( var typeSymbol in typeList )
            {
                allOkay &= OutputSymbol( syntaxWalker, typeSymbol );
            }

            return allOkay;
        }

        public override bool TryGetSunkValue(INamedTypeSymbol symbol, out NamedType? result)
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

        protected override SymbolInfo OutputSymbolInternal( ISyntaxWalker syntaxWalker, INamedTypeSymbol symbol )
        {
            var retVal = base.OutputSymbolInternal( syntaxWalker, symbol );

            if (retVal.AlreadyProcessed)
                return retVal;

            // output the symbol to the database
            if( !ProcessSymbol( syntaxWalker, retVal ) )
                return retVal;

            // store the processed symbol and all of its ancestor types if we haven't
            // already processed it
            if( !_typeSymbols.ContainsKey( retVal.SymbolName ) )
            {
                _typeSymbols.Add( retVal.SymbolName, (INamedTypeSymbol) retVal.Symbol );

                AddAncestorTypes( symbol );
            }

            retVal.WasOutput = true;

            return retVal;
        }

        private void AddAncestorTypes( INamedTypeSymbol symbol )
        {
            foreach( var interfaceSymbol in symbol.AllInterfaces )
            {
                var symbolName = SymbolName.GetFullyQualifiedName( interfaceSymbol );

                if( _typeSymbols.ContainsKey( symbolName ) ) 
                    continue;

                _typeSymbols.Add(symbolName, interfaceSymbol  );

                AddAncestorTypes(interfaceSymbol);
            }

            var baseSymbol = symbol.BaseType;

            while( baseSymbol != null )
            {
                var symbolInfo = new SymbolInfo( baseSymbol, SymbolName );

                if( !_typeSymbols.ContainsKey( symbolInfo.SymbolName ) )
                    _typeSymbols.Add( symbolInfo.SymbolName, (INamedTypeSymbol) symbolInfo.Symbol );

                baseSymbol = ( (ITypeSymbol) symbolInfo.Symbol ).BaseType;
            }
        }

        private bool ProcessSymbol( ISyntaxWalker syntaxWalker, SymbolInfo symbolInfo )
        {
            switch( symbolInfo.TypeKind )
            {
                case TypeKind.Error:
                    Logger.Error<string>("Unhandled or incorrect type error for named type '{0}'",
                        symbolInfo.SymbolName);

                    return false;

                case TypeKind.Dynamic:
                case TypeKind.Pointer:
                    Logger.Error<string, TypeKind>(
                        "named type '{0}' is a {1} and not supported",
                        symbolInfo.SymbolName, 
                        symbolInfo.TypeKind);

                    return false;
            }

            if ( !_assemblySink.TryGetSunkValue( symbolInfo.Symbol.ContainingAssembly, out var dbAssembly ) )
                return false;

            if( !_nsSink.TryGetSunkValue( symbolInfo.Symbol.ContainingNamespace, out var dbNS ) )
                return false;

            var dbSymbol = DbContext.NamedTypes.FirstOrDefault( nt => nt.FullyQualifiedName == symbolInfo.SymbolName );

            bool isNew = dbSymbol == null;

            dbSymbol ??= new NamedType() { FullyQualifiedName = symbolInfo.SymbolName };

            if (isNew)
                DbContext.NamedTypes.Add(dbSymbol);

            dbSymbol.Synchronized = true;
            dbSymbol.Name = SymbolName.GetName(symbolInfo.OriginalSymbol);
            dbSymbol.AssemblyID = dbAssembly!.ID;
            dbSymbol.NamespaceId = dbNS!.ID;
            dbSymbol.Accessibility = symbolInfo.OriginalSymbol.DeclaredAccessibility;
            dbSymbol.DeclarationModifier = symbolInfo.OriginalSymbol.GetDeclarationModifier();
            dbSymbol.Nature = symbolInfo.TypeKind;
            dbSymbol.InDocumentationScope = syntaxWalker.InDocumentationScope( symbolInfo.Symbol.ContainingAssembly );

            DbContext.SaveChanges();

            return true;
        }
    }
}
