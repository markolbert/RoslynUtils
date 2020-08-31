using System.Collections.Generic;
using System.Linq;
using System.Text;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn.Deprecated
{
    public abstract class TypeProcessor<TResult> : BaseProcessorDb<ITypeSymbol, TResult>
        where TResult : class, ISymbol
    {
        protected TypeProcessor(
            RoslynDbContext dbContext,
            ISymbolNamer symbolInfo,
            IJ4JLogger logger
        )
            : base( dbContext, symbolInfo, logger )
        {
        }

        protected bool ValidateAssembly( ITypeSymbol symbol, out AssemblyDb? result )
        {
            result = null;

            if( symbol.ContainingAssembly == null )
                return false;

            if( !GetByFullyQualifiedName<AssemblyDb>( symbol.ContainingAssembly, out var dbResult ) )
                return false;

            result = dbResult;

            return true;
        }

        protected bool ValidateNamespace(ITypeSymbol ntSymbol, out NamespaceDb? result)
        {
            result = null;

            if (ntSymbol.ContainingNamespace == null)
                return false;

            if (!GetByFullyQualifiedName<NamespaceDb>(ntSymbol.ContainingNamespace, out var dbResult))
                return false;

            result = dbResult;

            return true;
        }
    }
}
