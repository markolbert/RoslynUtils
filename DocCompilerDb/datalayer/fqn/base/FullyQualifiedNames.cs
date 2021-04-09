using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.DocCompiler
{
    public class FullyQualifiedNames : IFullyQualifiedNames
    {
        private readonly Dictionary<SyntaxKind, IFullyQualifiedName> _namers = new();
        private readonly IJ4JLogger? _logger;

        public FullyQualifiedNames(
            IEnumerable<IFullyQualifiedName> namers,
            IJ4JLogger? logger
        )
        {
            _logger = logger;
            _logger?.SetLoggedType(GetType());

            var temp = namers.ToList();

            foreach( var syntaxNamer in temp )
            {
                foreach( var supportedKind in syntaxNamer.SupportedKinds )
                {
                    if( _namers.ContainsKey( supportedKind ) )
                        _logger?.Error( "Skipping duplicate IFullyQualifiedName for {0}", supportedKind );
                    else _namers.Add( supportedKind, syntaxNamer );
                }
            }
        }

        public bool Supports( SyntaxNode node ) => _namers.ContainsKey( node.Kind() );

        public bool GetName( SyntaxNode node, out string? result )
        {
            var nodeKind = node.Kind();

            if( _namers.ContainsKey( nodeKind ) ) 
                return _namers[ nodeKind ].GetName( node, out result );

            _logger?.Error( "Unsupported SyntaxKind {0}", nodeKind );

            result = null;

            return false;
        }

        public bool GetFullyQualifiedName( SyntaxNode node, out string? result )
        {
            var nodeKind = node.Kind();

            if (_namers.ContainsKey(nodeKind))
                return _namers[nodeKind].GetFullyQualifiedName(node, out result);

            _logger?.Error("Unsupported SyntaxKind {0}", nodeKind);

            result = null;

            return false;
        }

        public bool GetIdentifierTokens( SyntaxNode node, out IEnumerable<SyntaxToken> result )
        {
            var nodeKind = node.Kind();

            if( _namers.ContainsKey( nodeKind ) )
                return _namers[ nodeKind ].GetIdentifierTokens( node, out result );

            _logger?.Error("Unsupported SyntaxKind {0}", nodeKind);

            result = Enumerable.Empty<SyntaxToken>();

            return false;
        }
    }
}
