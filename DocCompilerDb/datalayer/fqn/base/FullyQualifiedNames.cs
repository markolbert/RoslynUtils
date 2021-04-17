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
            result = null;

            var nodeKind = node.Kind();

            // tuples are handled differently because they derive from/depend on other types
            return nodeKind switch
            {
                SyntaxKind.TupleType => GetTupleTypeName( node, out result ),
                SyntaxKind.TupleElement => GetTupleElementName( node, out result ),
                _ => _namers.ContainsKey( nodeKind ) switch
                {
                    true => _namers[ nodeKind ].GetName( node, out result ),
                    _ => unsupported()
                }
            };

            bool unsupported()
            {
                _logger?.Error( "Unsupported SyntaxKind {0}", nodeKind );
                return false;
            }
        }

        public bool GetFullyQualifiedName( SyntaxNode node, out string? result )
        {
            result = null;

            var nodeKind = node.Kind();

            // tuples are handled differently because they derive from/depend on other types
            return nodeKind switch
            {
                SyntaxKind.TupleType => GetTupleTypeFullyQualifiedName( node, out result ),
                SyntaxKind.TupleElement => GetTupleElementFullyQualifiedName( node, out result ),
                _ => _namers.ContainsKey( nodeKind ) switch
                {
                    true => _namers[ nodeKind ].GetName( node, out result ),
                    _ => unsupported()
                }
            };

            bool unsupported()
            {
                _logger?.Error( "Unsupported SyntaxKind {0}", nodeKind );
                return false;
            }
        }

        public bool GetIdentifierTokens( SyntaxNode node, out IEnumerable<IIdentifier> result )
        {
            var nodeKind = node.Kind();

            if( _namers.ContainsKey( nodeKind ) )
                return _namers[ nodeKind ].GetIdentifierTokens( node, out result );

            _logger?.Error("Unsupported SyntaxKind {0}", nodeKind);

            result = Enumerable.Empty<BasicIdentifier>();

            return false;
        }

        private bool GetTupleTypeName( SyntaxNode node, out string? result )
        {
            result = null;

            if( !node.IsKind( SyntaxKind.TupleType ) )
            {
                _logger?.Error("SyntaxNode is not a TupleType");
                return false;
            }

            var sb = new StringBuilder();

            foreach( var elementNode in node.ChildNodes()
                .Where( x => x.IsKind( SyntaxKind.TupleElement ) ) )
            {
                if( !GetTupleElementName( elementNode, out var elementName ) )
                    return false;

                if( sb.Length > 0 )
                    sb.Append( ", " );

                sb.Append( elementName );
            }

            result = $"({sb.ToString()})";

            return true;
        }

        private bool GetTupleTypeFullyQualifiedName( SyntaxNode node, out string? result )
        {
            result = null;

            if( !node.IsKind( SyntaxKind.TupleType ) )
            {
                _logger?.Error("SyntaxNode is not a TupleType");
                return false;
            }

            var sb = new StringBuilder();

            foreach( var elementNode in node.ChildNodes()
                .Where( x => x.IsKind( SyntaxKind.TupleElement ) ) )
            {
                if( !GetTupleElementFullyQualifiedName( elementNode, out var elementName ) )
                    return false;

                if( sb.Length > 0 )
                    sb.Append( ", " );

                sb.Append( elementName );
            }

            result = $"({sb.ToString()})";

            return true;
        }

        private bool GetTupleElementName( SyntaxNode node, out string? result )
        {
            result = null;

            if( !GetTypeNodeFromTupleElement( node, out var typeNode ) )
                return false;

            if( typeNode == null )
            {
                _logger?.Error( "Could not find type node within TupleElement node" );
                return false;
            }

            if( !GetName( typeNode, out var typeName ) )
            {
                _logger?.Error("Could not get name for type node");
                return false;
            }

            var idToken = node.ChildTokens().First( x => x.IsKind( SyntaxKind.IdentifierToken ) );

            result = $"{typeName} {idToken.Text}";

            return true;
        }

        private bool GetTypeNodeFromTupleElement( SyntaxNode node, out SyntaxNode? result )
        {
            result = null;

            if( !node.IsKind( SyntaxKind.TupleElement ) )
            {
                _logger?.Error( "SyntaxNode is a {0}, not a SyntaxKind.TupleElement", node.Kind() );
                return false;
            }

            result = node.ChildNodes()
                .FirstOrDefault( x => SyntaxCollections.TypeNodeKinds.Any( x.IsKind ) );

            if( result == null )
                _logger?.Error( "Could not find type node within TupleElement node" );

            return result != null;
        }

        private bool GetTupleElementFullyQualifiedName( SyntaxNode node, out string? result )
        {
            result = null;

            if( !GetTypeNodeFromTupleElement( node, out var typeNode ) )
                return false;

            if( typeNode == null )
            {
                _logger?.Error( "Could not find type node within TupleElement node" );
                return false;
            }

            if( !GetFullyQualifiedName( typeNode, out var typeName ) )
            {
                _logger?.Error("Could not get fully-qualified name for type node");
                return false;
            }

            var idToken = node.ChildTokens().First( x => x.IsKind( SyntaxKind.IdentifierToken ) );

            result = $"{typeName} {idToken.Text}";

            return true;
        }
    }
}
