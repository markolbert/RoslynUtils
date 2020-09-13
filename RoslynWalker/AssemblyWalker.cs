using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.Roslyn
{
    public class AssemblyWalker : SyntaxWalker<IAssemblySymbol>
    {
        private static readonly List<SyntaxKind> _ignoredNodeKinds = new List<SyntaxKind>();

        static AssemblyWalker()
        {
            _ignoredNodeKinds.Add( SyntaxKind.UsingDirective );
            _ignoredNodeKinds.Add( SyntaxKind.QualifiedName );
        }

        private readonly IInScopeAssemblyProcessor _inScopeProcessor;

        public AssemblyWalker( 
            ISymbolFullName symbolInfo,
            IDefaultSymbolSink defaultSymbolSink,
            IInScopeAssemblyProcessor inScopeProcessor,
            IJ4JLogger logger,
            ISymbolSink<IAssemblySymbol>? symbolSink = null
            ) 
            : base( symbolInfo, defaultSymbolSink, logger, symbolSink )
        {
            _inScopeProcessor = inScopeProcessor;
        }

        // override the Traverse() method to synchronize the project file based metadata
        // for assemblies that are within the scope of the documentation
        public override bool Process( IEnumerable<CompiledProject> compResults, bool stopOnFirstError = false )
        {
            if( !base.Process( compResults, stopOnFirstError ) )
                return false;

            if( !_inScopeProcessor.Initialize() )
                return false;

            return _inScopeProcessor.Synchronize( compResults );
        }

        protected override bool NodeReferencesSymbol( SyntaxNode node, CompiledFile context, out IAssemblySymbol? result )
        {
            result = null;

            // certain node types don't lead to places we need to process
            if( _ignoredNodeKinds.Any( nk => nk == node.Kind() ) )
                return false;

            //// SyntaxKind.CompilationUnit appears to represent the assembly level...but you can't
            //// retrieve its ISymbol from the node
            //if( node.IsKind( SyntaxKind.CompilationUnit ) )
            //{
            //    Logger.Information<string, SyntaxKind>( "{0}: found {1}", 
            //        context.Container.AssemblyName,
            //        SyntaxKind.CompilationUnit );

            //    result = context.Container.AssemblySymbol;

            //    return true;
            //}

            if( !context.GetSymbol<ISymbol>( node, out var otherSymbol ) )
                return false;

            var otherAssembly = otherSymbol!.ContainingAssembly;

            if( otherAssembly == null )
            {
                Logger.Verbose<string>( "Symbol {0} isn't contained in an Assembly", otherSymbol.ToDisplayString() );

                return false;
            }

            //if( InDocumentationScope( otherAssembly ) )
            //{
            //    Logger.Verbose<string>("Assembly for symbol {0} is in scope", otherSymbol.ToDisplayString());

            //    return false;
            //}

            result = otherAssembly;

            return true;
        }

        protected override bool GetChildNodesToVisit( SyntaxNode node, out List<SyntaxNode>? result )
        {
            result = null;

            // we're interested in traversing almost everything that's within scope
            // except for node types that we know don't lead any place interesting
            if( _ignoredNodeKinds.Any( nk => nk == node.Kind() ) )
                return false;

            result = node.ChildNodes()
                .Where(n => _ignoredNodeKinds.All(i => i != n.Kind()))
                .ToList();

            return true;
        }
    }
}
