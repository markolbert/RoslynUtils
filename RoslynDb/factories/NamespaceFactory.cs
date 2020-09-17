﻿using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class NamespaceFactory : EntityFactory<INamespaceSymbol, NamespaceDb>
    {
        public NamespaceFactory( IJ4JLogger logger )
            : base( SharpObjectType.Namespace, logger )
        {
        }

        protected override bool GetEntitySymbol(ISymbol? symbol, out INamespaceSymbol? result )
        {
            result = null;

            if( symbol == null )
                return false;

            if (symbol is INamespaceSymbol nsSymbol)
                result = nsSymbol;

            if (symbol is IArrayTypeSymbol arraySymbol)
                result = arraySymbol.ElementType.ContainingNamespace;

            result ??= symbol.ContainingNamespace;

            return result != null;
        }

        protected override bool ValidateEntitySymbol(INamespaceSymbol symbol)
        {
            if (!base.ValidateEntitySymbol(symbol))
                return false;

            if (Factories!.Get<AssemblyDb>(symbol.ContainingAssembly, out _))
                return true;

            Logger.Error<string>("Couldn't find AssemblyDb entity in database for '{0}'",
                Factories!.GetFullName(symbol));

            return false;
        }

        protected override bool CreateNewEntity( INamespaceSymbol symbol, out NamespaceDb? result )
        {
            result = new NamespaceDb();

            return true;
        }
    }
}