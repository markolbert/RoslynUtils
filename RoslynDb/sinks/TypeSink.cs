using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Internal;

namespace J4JSoftware.Roslyn.Sinks
{
    public class TypeSink : RoslynDbSink<ITypeSymbol>
    {
        private readonly TypeSymbolContainer _symbols;
        private readonly List<string> _visited = new List<string>();

        public TypeSink(
            UniqueSymbols<ITypeSymbol> uniqueSymbols,
            Func<IJ4JLogger> loggerFactory,
            IProcessorCollection<ITypeSymbol>? processors = null )
            : base( uniqueSymbols, loggerFactory(), processors)
        {
            _symbols = new TypeSymbolContainer( loggerFactory() );
        }

        public override bool InitializeSink( ISyntaxWalker syntaxWalker, bool stopOnFirstError = false )
        {
            if( !base.InitializeSink( syntaxWalker, stopOnFirstError ) )
                return false;

            _symbols.Clear();

            return true;
        }

        public override bool FinalizeSink( ISyntaxWalker syntaxWalker )
        {
            if (_processors == null)
            {
                Logger.Error<Type>("No processors defined for {0}", this.GetType());
                return false;
            }

            return _processors.Process( _symbols, StopOnFirstError );
        }

        public override bool OutputSymbol( ISyntaxWalker syntaxWalker, ITypeSymbol symbol )
        {
            _visited.Clear();

            if( symbol is INamedTypeSymbol ntSymbol && ntSymbol.TypeKind == TypeKind.Interface )
                return AddInterface( ntSymbol );
            
            AddNonInterface( symbol, null );

            return true;
        }

        private bool AddInterface( INamedTypeSymbol symbol )
        {
            if ( SymbolIsDuplicate( symbol ) )
                return true;

            if( symbol.TypeKind != TypeKind.Interface )
            {
                Logger.Error<string>("Non-interface '{0}' submitted to AddInterface()", symbol.ToFullName());
                return false;
            }

            if ( symbol.BaseType != null )
            {
                Logger.Error<string>( "Interface '{0}' has a base type", symbol.ToFullName() );
                return false;
            }

            _symbols.AddConnection( symbol );

            // add any type parameters and type arguments
            foreach( var tpSymbol in symbol.TypeParameters )
            {
                AddNonInterface( tpSymbol, null );
            }

            foreach (var taSymbol in symbol.TypeArguments)
            {
                AddNonInterface(taSymbol, null);
            }

            return true;
        }

        private void AddNonInterface( ITypeSymbol symbol, ITypeSymbol? parentSymbol )
        {
            _symbols.AddConnection( symbol, parentSymbol );

            if (SymbolIsDuplicate(symbol))
                return;

            if ( symbol.BaseType != null )
                AddNonInterface( symbol.BaseType, symbol );

            if( !( symbol is INamedTypeSymbol ntSymbol ) )
                return;

            // add any interfaces
            foreach( var interfaceSymbol in ntSymbol.AllInterfaces )
            {
                AddInterface( interfaceSymbol );
            }

            // add any type parameters and type arguments
            foreach (var tpSymbol in ntSymbol.TypeParameters)
            {
                AddNonInterface(tpSymbol, ntSymbol);

                // add any interfaces
                foreach (var interfaceSymbol in tpSymbol.AllInterfaces)
                {
                    AddInterface(interfaceSymbol);
                }
            }

            foreach (var taSymbol in ntSymbol.TypeArguments)
            {
                AddNonInterface(taSymbol, symbol);

                // add any interfaces
                foreach (var interfaceSymbol in taSymbol.AllInterfaces)
                {
                    AddInterface(interfaceSymbol);
                }
            }
        }

        private bool SymbolIsDuplicate( ISymbol symbol )
        {
            // don't allow duplicate additions so we can avoid infinite loops
            var fullName = symbol.ToFullName();

            if (_visited.Any(x => x.Equals(fullName)))
                return true;

            _visited.Add(fullName);

            return false;
        }
    }
}
