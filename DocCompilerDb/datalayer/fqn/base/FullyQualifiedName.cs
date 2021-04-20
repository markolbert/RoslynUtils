using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.DocCompiler
{
    public abstract class FullyQualifiedName : IFullyQualifiedNodeName
    {
        private readonly List<SyntaxKind> _supportedKinds;
        
        protected FullyQualifiedName(
            IJ4JLogger? logger,
            params SyntaxKind[] syntaxKinds
        )
        {
            Logger = logger;
            Logger?.SetLoggedType( GetType() );

            _supportedKinds = syntaxKinds.ToList();

            if (_supportedKinds.Count == 0)
                Logger?.Error("No supported SyntaxKinds defined");
        }

        protected IJ4JLogger? Logger { get; }

        public ReadOnlyCollection<SyntaxKind> SupportedKinds => _supportedKinds.AsReadOnly();

        public virtual bool GetName( SyntaxNode node, out string? result )
        {
            result = null;

            return _supportedKinds.Any( node.IsKind );
        }

        public virtual bool GetFullyQualifiedName( SyntaxNode node, out string? result )
        {
            result = null;

            return _supportedKinds.Any(node.IsKind);
        }

        public virtual bool GetIdentifierTokens(SyntaxNode node, out IEnumerable<IIdentifier> result)
        {
            result = Enumerable.Empty<IIdentifier>();

            return _supportedKinds.Any(node.IsKind);
        }
    }
}