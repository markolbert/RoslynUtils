using System;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public interface IEntityFactory
    {
        EntityFactories Factories { get; set; }

        bool CanProcess( ISymbol? symbol );

        bool CanCreate<T>()
            where T : ISharpObject;

        bool IsAssignableTo<T>()
            where T : ISharpObject;

        bool Get( ISymbol? symbol, out ISharpObject? result );
        bool Create( ISymbol? symbol, out ISharpObject? result );
    }

    public interface IEntityFactory<TEntity> : IEntityFactory
        where TEntity : class, ISharpObject
    {
        bool Get( ISymbol? symbol, out TEntity? result );
        bool Create( ISymbol? symbol, out TEntity? result );
    }
}