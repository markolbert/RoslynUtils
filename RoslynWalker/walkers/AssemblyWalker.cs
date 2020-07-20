using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace J4JSoftware.Roslyn.walkers
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
        private readonly Func<IJ4JLogger> _loggerFactory;

        public AssemblyWalker( 
            IEnumerable<ISymbolSink> symbolSinks, 
            IInScopeAssemblyProcessor inScopeProcessor,
            IDefaultSymbolSink defaultSymbolSink,
            Func<IJ4JLogger> loggerFactory 
            ) 
            : base( symbolSinks, defaultSymbolSink, loggerFactory() )
        {
            _inScopeProcessor = inScopeProcessor;
            _loggerFactory = loggerFactory;
        }

        // override the Traverse() method to synchronize the project file based metadata
        // for assemblies that are within the scope of the documentation
        public override bool Traverse( List<CompilationResults> compResults )
        {
            if( !base.Traverse( compResults ) )
                return false;

            if( !_inScopeProcessor.Initialize() )
                return false;

            var inScopeLibs = compResults
                .Select( cr => new ProjectLibrary( 
                    cr.ProjectModel.ProjectFile!, 
                    _loggerFactory ) )
                .ToList();

            if( !_inScopeProcessor.Synchronize( inScopeLibs) )
                return false;

            return _inScopeProcessor.Cleanup();
        }

        protected override bool ProcessNode( SyntaxNode node, CompilationResult context, out IAssemblySymbol? result )
        {
            result = null;

            // certain node types don't lead to places we need to process
            if( _ignoredNodeKinds.Any( nk => nk == node.Kind() ) )
                return false;

            // SyntaxKind.CompilationUnit appears to represent the assembly level...but you can't
            // retrieve its ISymbol from the node
            if( node.IsKind( SyntaxKind.CompilationUnit ) )
            {
                Logger.Information<string, SyntaxKind>( "{0}: found {1}", 
                    context.Container.ProjectModel.ProjectName!,
                    SyntaxKind.CompilationUnit );

                result = context.Container.AssemblySymbol;

                return true;
            }

            if( !context.GetSymbol<ISymbol>( node, out var otherSymbol ) )
            {
                Logger.Verbose<string, SyntaxKind>( "{0}: no ISymbol found for node of kind {1}",
                    context.Container.ProjectModel.ProjectName!,
                    node.Kind() );

                return false;
            }

            var otherAssembly = otherSymbol!.ContainingAssembly;

            if( otherAssembly == null )
            {
                Logger.Verbose<string>( "Symbol {0} isn't contained in an Assembly", otherSymbol.ToDisplayString() );

                return false;
            }

            if( AssemblyInScope( otherAssembly ) )
            {
                Logger.Verbose<string>("Assembly for symbol {0} is in scope", otherSymbol.ToDisplayString());

                return false;
            }

            if( ProcessedSymbols.Any( ps => SymbolEqualityComparer.Default.Equals( ps, otherAssembly ) ) )
            {
                Logger.Verbose<IAssemblySymbol, string>("Assembly '{0}' for symbol {1} was already processed",
                    otherAssembly,
                    otherSymbol.ToDisplayString());

                return false;
            }

            result = otherAssembly;

            Logger.Information<string, string>( "{0}: found new out-of-scope assembly {1}",
                context.Container.ProjectModel.ProjectName!,
                otherAssembly.ToDisplayString() );

            return result != null;
        }

        protected override bool GetTraversableChildren( SyntaxNode node, out List<SyntaxNode>? result )
        {
            result = null;

            // we're interested in traversing almost everything that's within scope
            // except for node types that we know don't lead any place interesting
            if( _ignoredNodeKinds.Any( nk => nk == node.Kind() ) )
                return false;

            //switch( node )
            //{
            //    // some TypeOfExpressionSyntax nodes don't have child nodes containing Type information
            //    // -- which we want -- but they all have a Type property
            //    case TypeOfExpressionSyntax toeNode:
            //        result = new List<SyntaxNode>();
            //        result.Add( toeNode.Type );

            //        break;

            //    default:
            //        result = node.ChildNodes()
            //            .Where( n => _ignoredNodeKinds.All( i => i != n.Kind() ) )
            //            .ToList();

            //        break;
            //}

            result = node.ChildNodes()
                .Where(n => _ignoredNodeKinds.All(i => i != n.Kind()))
                .ToList();

            return true;
        }
    }
}
