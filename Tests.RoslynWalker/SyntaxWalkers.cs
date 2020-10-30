using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using Microsoft.CodeAnalysis;

namespace Tests.RoslynWalker
{
    public sealed class SyntaxWalkers : TopologicalCollection<ISyntaxWalker>, IProcessorCollection<CompiledProject>
    {
        private readonly List<ISymbolSink> _sinks;
        private readonly ExecutionContext _context;
        private readonly IJ4JLogger _logger;

        public SyntaxWalkers(
            ISymbolFullName symbolNamer,
            IDefaultSymbolSink defaultSymbolSink,
            IEnumerable<ISymbolSink> symbolSinks,
            ExecutionContext context,
            Func<IJ4JLogger> loggerFactory
        )
        {
            _sinks = symbolSinks.ToList();
            _context = context;

            _logger = loggerFactory();
            _logger.SetLoggedType( this.GetType() );

            var node = AddValue( new AssemblyWalker( symbolNamer,
                defaultSymbolSink,
                context,
                loggerFactory(),
                GetSink<IAssemblySymbol>() ) );

            node = AddDependency( new NamespaceWalker( symbolNamer,
                defaultSymbolSink,
                context,
                loggerFactory(),
                GetSink<INamespaceSymbol>() ), node.Value );

            node = AddDependency(new TypeWalker(symbolNamer,
                defaultSymbolSink,
                context,
                loggerFactory(),
                GetSink<ITypeSymbol>()), node.Value);

            node = AddDependency(new MethodWalker(symbolNamer,
                defaultSymbolSink,
                context,
                loggerFactory(),
                GetSink<IMethodSymbol>()), node.Value);

            node = AddDependency(new PropertyWalker(symbolNamer,
                defaultSymbolSink,
                context,
                loggerFactory(),
                GetSink<IPropertySymbol>()), node.Value);

            node = AddDependency(new FieldWalker(symbolNamer,
                defaultSymbolSink,
                context,
                loggerFactory(),
                GetSink<IFieldSymbol>()), node.Value);

            node = AddDependency(new EventWalker(symbolNamer,
                defaultSymbolSink,
                context,
                loggerFactory(),
                GetSink<IEventSymbol>()), node.Value);

            node = AddDependency(new AttributeWalker(symbolNamer,
                defaultSymbolSink,
                context,
                loggerFactory(),
                GetSink<ISymbol>()), node.Value);
        }

        private ISymbolSink<TSymbol> GetSink<TSymbol>()
            where TSymbol : ISymbol =>
            _sinks.FirstOrDefault( x => x.SupportsSymbol( typeof(TSymbol) ) )
                as ISymbolSink<TSymbol>;

        public bool Process( IEnumerable<CompiledProject> projects )
        {
            var numRoots = GetRoots().Count;

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

            if( !Sort( out var walkerNodes, out var remainingEdges ) )
            {
                _logger.Error("Couldn't topologically sort ISyntaxWalkers"  );
                return false;
            }

            walkerNodes.Reverse();

            var allOkay = true;

            foreach( var node in walkerNodes! )
            {
                allOkay &= node.Value.Process( projects );

                if( !allOkay && _context.StopOnFirstError )
                    break;
            }

            return allOkay;
        }
    }
}