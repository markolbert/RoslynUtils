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
    public class TypeSink : RoslynDbSink<ITypeSymbol, NamedType>
    {
        private readonly ISymbolSink<IAssemblySymbol, Assembly> _assemblySink;
        private readonly ISymbolSink<INamespaceSymbol, Namespace> _nsSink;
        private readonly List<ITypeProcessor> _processors;
        private readonly Dictionary<string, ITypeSymbol> _typeSymbols = new Dictionary<string, ITypeSymbol>();

        public TypeSink(
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

            var typeList = _typeSymbols.Select( ts => ts.Value )
                .ToList();

            foreach( var processor in _processors )
            {
                allOkay &= processor.Process( syntaxWalker, typeList );
            }

            // now we can add the parent types
            foreach( var typeSymbol in typeList )
            {
                allOkay &= OutputSymbol( syntaxWalker, typeSymbol );
            }

            return allOkay;
        }

        public override bool TryGetSunkValue(ITypeSymbol symbol, out NamedType? result)
        {
            var symbolName = SymbolName.GetSymbolName(symbol);

            var retVal = DbContext.NamedTypes.FirstOrDefault(a => a.FullyQualifiedName == symbolName);

            if (retVal == null)
            {
                result = null;
                return false;
            }

            result = retVal;

            return true;
        }

        protected override SymbolInfo OutputSymbolInternal( ISyntaxWalker syntaxWalker, ITypeSymbol symbol )
        {
            var retVal = base.OutputSymbolInternal( syntaxWalker, symbol );

            if (retVal.AlreadyProcessed)
                return retVal;

            // output the symbol to the database
            if( !ProcessSymbol( syntaxWalker, (ITypeSymbol) retVal.Symbol, retVal.SymbolName ) )
                return retVal;

            // store the processed symbol and all of its ancestor types if we haven't
            // already processed it
            if( !_typeSymbols.ContainsKey( retVal.SymbolName ) )
            {
                _typeSymbols.Add( retVal.SymbolName, (ITypeSymbol) retVal.Symbol );

                AddAncestorTypes( symbol );
            }

            retVal.WasOutput = true;

            return retVal;
        }

        private void AddAncestorTypes( ITypeSymbol symbol )
        {
            foreach( var interfaceSymbol in symbol.AllInterfaces )
            {
                var symbolName = SymbolName.GetSymbolName( interfaceSymbol );

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
                    _typeSymbols.Add( symbolInfo.SymbolName, (ITypeSymbol) symbolInfo.Symbol );

                baseSymbol = ( (ITypeSymbol) symbolInfo.Symbol ).BaseType;
            }
        }

        private bool ProcessSymbol( ISyntaxWalker syntaxWalker, ITypeSymbol symbol, string symbolName )
        {
            var nature = symbol switch
            {
                INamedTypeSymbol ntSymbol => ntSymbol.TypeKind,
                IArrayTypeSymbol arSymbol => TypeKind.Array,
                IDynamicTypeSymbol dynSymbol => TypeKind.Dynamic,
                IPointerTypeSymbol ptrSymbol => TypeKind.Pointer,
                _ => TypeKind.Error
            };

            switch( nature )
            {
                case TypeKind.Array:
                    symbol = ( (IArrayTypeSymbol) symbol ).ElementType;
                    break;

                case TypeKind.Error:
                    Logger.Error<string>("Unhandled or incorrect type error for named type '{0}'",
                        symbolName);

                    return false;

                case TypeKind.Dynamic:
                case TypeKind.Pointer:
                    Logger.Error<string, TypeKind>(
                        "named type '{0}' is a {1} and not supported",
                        symbolName, 
                        nature);

                    return false;
            }

            if ( !_assemblySink.TryGetSunkValue( symbol.ContainingAssembly, out var dbAssembly ) )
                return false;

            if( !_nsSink.TryGetSunkValue( symbol.ContainingNamespace, out var dbNS ) )
                return false;

            var dbSymbol = DbContext.NamedTypes.FirstOrDefault(nt => nt.FullyQualifiedName == symbolName);

            bool isNew = dbSymbol == null;

            dbSymbol ??= new NamedType() { FullyQualifiedName = symbolName };

            if (isNew)
                DbContext.NamedTypes.Add(dbSymbol);

            dbSymbol.Synchronized = true;
            dbSymbol.Name = symbol.Name;
            dbSymbol.AssemblyID = dbAssembly!.ID;
            dbSymbol.NamespaceId = dbNS!.ID;
            dbSymbol.Accessibility = symbol.DeclaredAccessibility;
            dbSymbol.DeclarationModifier = symbol.GetDeclarationModifier();
            dbSymbol.Nature = nature;
            dbSymbol.InDocumentationScope = syntaxWalker.InDocumentationScope( symbol.ContainingAssembly );

            DbContext.SaveChanges();

            return true;
        }
    }
}
