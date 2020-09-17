using System;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public interface IEntityFactory
    {
        bool Initialized { get; }

        EntityFactories Factories { get; }

        SharpObjectType SharpObjectType { get; }
        Type EntityType { get; }
        Type SymbolType { get; }

        bool CanProcess( ISymbol? symbol );

        bool CanCreate<T>()
            where T : ISharpObject;

        bool IsAssignableTo<T>()
            where T : ISharpObject;

        bool InDatabase( ISymbol? symbol );

        bool Get( ISymbol? symbol, out ISharpObject? result );
        bool Create( ISymbol? symbol, out ISharpObject? result );
    }

    internal interface IEntityFactoryInternal
    {
        void SetFactories( EntityFactories factories );
    }

    public interface IEntityFactory<TEntity> : IEntityFactory
        where TEntity : class, ISharpObject
    {
        bool Get( ISymbol? symbol, out TEntity? result );
        bool Create( ISymbol? symbol, out TEntity? result );
    }
}