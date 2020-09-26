﻿using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public static class SymbolExtensions
    {
        public static DeclarationModifier GetDeclarationModifier(this ISymbol symbol)
        {
            var retVal = DeclarationModifier.None;

            if (symbol.IsAbstract) retVal |= DeclarationModifier.Abstract;
            if (symbol.IsOverride) retVal |= DeclarationModifier.Override;
            if (symbol.IsSealed) retVal |= DeclarationModifier.Sealed;
            if (symbol.IsStatic) retVal |= DeclarationModifier.Static;
            if (symbol.IsVirtual) retVal |= DeclarationModifier.Virtual;

            if (symbol is IFieldSymbol fieldSymbol)
            {
                if (fieldSymbol.IsConst) retVal |= DeclarationModifier.Const;
                if (fieldSymbol.IsReadOnly) retVal |= DeclarationModifier.ReadOnly;
            }

            if (symbol is IPropertySymbol propSymbol)
            {
                if (propSymbol.IsWithEvents) retVal |= DeclarationModifier.WithEvents;
                if (propSymbol.IsWriteOnly) retVal |= DeclarationModifier.WriteOnly;
                if (propSymbol.IsReadOnly) retVal |= DeclarationModifier.ReadOnly;
            }

            if (symbol is IMethodSymbol methodSymbol)
            {
                if (methodSymbol.IsAsync) retVal |= DeclarationModifier.Async;
                if (methodSymbol.IsReadOnly) retVal |= DeclarationModifier.ReadOnly;
            }

            return retVal;
        }

        public static ParametricTypeConstraint GetParametricTypeConstraint(this ITypeParameterSymbol typeParamSymbol)
        {
            var retVal = ParametricTypeConstraint.None;

            if (typeParamSymbol.HasConstructorConstraint) retVal |= ParametricTypeConstraint.Constructor;
            if (typeParamSymbol.HasNotNullConstraint) retVal |= ParametricTypeConstraint.NotNull;
            if (typeParamSymbol.HasReferenceTypeConstraint) retVal |= ParametricTypeConstraint.Reference;
            if (typeParamSymbol.HasUnmanagedTypeConstraint) retVal |= ParametricTypeConstraint.Unmanaged;
            if (typeParamSymbol.HasValueTypeConstraint) retVal |= ParametricTypeConstraint.Value;

            return retVal;
        }
    }
}