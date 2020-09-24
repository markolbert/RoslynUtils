using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using Microsoft.CodeAnalysis;

namespace Tests.RoslynWalker
{
    public sealed class SyntaxWalkers : TopologicallySortableCollection<ISyntaxWalker>, IProcessorCollection<CompiledProject>
    {
        private readonly List<ISymbolSink> _sinks;
        private readonly IJ4JLogger _logger;

        public SyntaxWalkers(
            ISymbolFullName symbolNamer,
            IDefaultSymbolSink defaultSymbolSink,
            IEnumerable<ISymbolSink> symbolSinks,
            Func<IJ4JLogger> loggerFactory
        )
        {
            _sinks = symbolSinks.ToList();

            _logger = loggerFactory();
            _logger.SetLoggedType( this.GetType() );

            var node = Add( new AssemblyWalker( symbolNamer,
                defaultSymbolSink,
                loggerFactory(),
                GetSink<IAssemblySymbol>() ) );

            node = Add( new NamespaceWalker( symbolNamer,
                defaultSymbolSink,
                loggerFactory(),
                GetSink<INamespaceSymbol>() ), node );

            node = Add(new TypeWalker(symbolNamer,
                defaultSymbolSink,
                loggerFactory(),
                GetSink<ITypeSymbol>()), node);
        }

        private ISymbolSink<TSymbol> GetSink<TSymbol>()
            where TSymbol : ISymbol =>
            _sinks.FirstOrDefault( x => x.SupportsSymbol( typeof(TSymbol) ) )
                as ISymbolSink<TSymbol>;

        public bool Initialize( ISyntaxWalker syntaxWalker ) => true;

        public bool Process( IEnumerable<CompiledProject> compResults, bool stopOnFirstError = false )
        {
            var numRoots = Edges().Count;

            switch( numRoots )
            {
                case 0:
                    _logger.Error("No initial ISyntaxWalker defined");
                    return false;

                case 1:
                    // no op; desired situation
                    break;

                default:
                    _logger.Error("Multiple initial ISyntaxWalkers ({0}) defined", numRoots);
                    return false;

            }

            if( !Sort( out var walkers, out var remainingEdges ) )
            {
                _logger.Error("Couldn't topologically sort ISyntaxWalkers"  );
                return false;
            }

            var allOkay = true;

            foreach( var walker in walkers! )
            {
                allOkay &= walker.Process( compResults, stopOnFirstError );

                if( !allOkay && stopOnFirstError )
                    break;
            }

            return allOkay;
        }
    }
}