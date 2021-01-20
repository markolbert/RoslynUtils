using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using J4JSoftware.Logging;
using J4JSoftware.Roslyn;
using J4JSoftware.Utilities;
using Microsoft.CodeAnalysis;

namespace Tests.RoslynWalker
{
    public sealed class SyntaxWalkers : Nodes<ISyntaxWalker>, IActionProcessor<CompiledProject>
    {
        private readonly List<ISymbolSink> _sinks;
        private readonly ActionsContext _context;
        private readonly IJ4JLogger _logger;

        public SyntaxWalkers(
            ISymbolFullName symbolNamer,
            IDefaultSymbolSink defaultSymbolSink,
            IEnumerable<ISymbolSink> symbolSinks,
            WalkerContext context,
            Func<IJ4JLogger> loggerFactory
        )
        {
            _sinks = symbolSinks.ToList();
            _context = context;

            _logger = loggerFactory();
            _logger.SetLoggedType( this.GetType() );

            var node = AddIndependentNode( new AssemblyWalker( symbolNamer,
                defaultSymbolSink,
                context,
                loggerFactory(),
                GetSink<IAssemblySymbol>() ) );

            node = AddDependentNode( new NamespaceWalker( symbolNamer,
                defaultSymbolSink,
                context,
                loggerFactory(),
                GetSink<INamespaceSymbol>() ), node.Value );

            node = AddDependentNode(new TypeWalker(symbolNamer,
                defaultSymbolSink,
                context,
                loggerFactory(),
                GetSink<ITypeSymbol>()), node.Value);

            node = AddDependentNode(new MethodWalker(symbolNamer,
                defaultSymbolSink,
                context,
                loggerFactory(),
                GetSink<IMethodSymbol>()), node.Value);

            node = AddDependentNode(new PropertyWalker(symbolNamer,
                defaultSymbolSink,
                context,
                loggerFactory(),
                GetSink<IPropertySymbol>()), node.Value);

            node = AddDependentNode(new FieldWalker(symbolNamer,
                defaultSymbolSink,
                context,
                loggerFactory(),
                GetSink<IFieldSymbol>()), node.Value);

            node = AddDependentNode(new EventWalker(symbolNamer,
                defaultSymbolSink,
                context,
                loggerFactory(),
                GetSink<IEventSymbol>()), node.Value);

            node = AddDependentNode(new AttributeWalker(symbolNamer,
                defaultSymbolSink,
                context,
                loggerFactory(),
                GetSink<ISymbol>()), node.Value);
        }

        private ISymbolSink<TSymbol>? GetSink<TSymbol>()
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

            walkerNodes!.Reverse();

            var allOkay = true;

            foreach( var node in walkerNodes )
            {
                allOkay &= node.Process( projects );

                if( !allOkay && _context.StopOnFirstError )
                    break;
            }

            return allOkay;
        }
    }
}