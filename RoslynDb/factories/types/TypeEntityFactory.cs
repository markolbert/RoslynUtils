﻿using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public abstract class TypeEntityFactory<TSymbol, TEntity> : EntityFactory<TSymbol, TEntity>
        where TSymbol : class, ITypeSymbol
        where TEntity : TypeDb
    {
        protected TypeEntityFactory(
            SharpObjectType sharpObjType,
            IJ4JLogger logger
        )
            : base( sharpObjType, logger )
        {
        }

        protected override bool ValidateEntitySymbol( TSymbol symbol )
        {
            if( !base.ValidateEntitySymbol( symbol ) )
                return false;

            if( !Factories!.Get<AssemblyDb>( symbol, out _ ) )
            {
                Logger.Error<string>( "Couldn't find AssemblyDb entity in database for '{0}'",
                    Factories!.GetFullName( symbol ) );

                return false;
            }

            if (!Factories!.Get<NamespaceDb>(symbol, out _))
            {
                Logger.Error<string>("Couldn't find NamespaceDb entity in database for '{0}'",
                    Factories!.GetFullName(symbol));

                return false;
            }

            return true;
        }

        protected override bool ConfigureEntity( TSymbol symbol, TEntity newEntity )
        {
            if( !base.ConfigureEntity( symbol, newEntity ) )
                return false;

            newEntity.Accessibility = symbol.DeclaredAccessibility;
            newEntity.Nature = symbol.TypeKind;

            return true;
        }
    }
}