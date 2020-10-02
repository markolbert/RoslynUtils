﻿using System.Collections.Generic;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.Roslyn
{
    public class MethodWalker : SyntaxWalker<IMethodSymbol>
    {
        private static readonly List<SyntaxKind> _ignoredNodeKinds = new List<SyntaxKind>();

        static MethodWalker()
        {
            _ignoredNodeKinds.Add( SyntaxKind.UsingDirective );
            _ignoredNodeKinds.Add( SyntaxKind.QualifiedName );
            _ignoredNodeKinds.Add(SyntaxKind.SimpleLambdaExpression);
            _ignoredNodeKinds.Add( SyntaxKind.ParenthesizedLambdaExpression );
            _ignoredNodeKinds.Add( SyntaxKind.Attribute );
            _ignoredNodeKinds.Add(SyntaxKind.GetAccessorDeclaration);
            _ignoredNodeKinds.Add(SyntaxKind.SetAccessorDeclaration);
        }

        public MethodWalker(
            ISymbolFullName symbolInfo,
            IDefaultSymbolSink defaultSymbolSink,
            ExecutionContext context,
            IJ4JLogger logger,
            ISymbolSink<IMethodSymbol>? symbolSink = null
        )
            : base( "Method walking", symbolInfo, defaultSymbolSink, context, logger, symbolSink )
        {
        }

        protected override bool NodeReferencesSymbol( SyntaxNode node, 
            CompiledFile compiledFile,
            out IMethodSymbol? result )
        {
            result = null;

            // certain node types don't lead to places we need to process
            if( _ignoredNodeKinds.Any( nk => nk == node.Kind() ) )
                return false;

            if( !compiledFile.GetSymbol<IMethodSymbol>( node, out var symbol ) )
                return false;

            if( !Context.InDocumentationScope( symbol!.ContainingAssembly ) )
                return false;

            result = symbol;

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
                .Where( n => _ignoredNodeKinds.All( i => i != n.Kind() ) )
                .ToList();

            return true;
        }
    }
}
