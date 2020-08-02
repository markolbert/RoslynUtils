using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public static class SymbolExtensions
    {
        public static DeclarationModifier GetDeclarationModifier(this ISymbol symbol)
        {
            var retVal = DeclarationModifier.None;

            if (symbol == null) return retVal;

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

        public static GenericConstraint GetGenericConstraints(this ITypeParameterSymbol typeParamSymbol)
        {
            var retVal = GenericConstraint.None;
            if (typeParamSymbol == null) return retVal;

            if (typeParamSymbol.HasConstructorConstraint) retVal |= GenericConstraint.Constructor;
            if (typeParamSymbol.HasNotNullConstraint) retVal |= GenericConstraint.NotNull;
            if (typeParamSymbol.HasReferenceTypeConstraint) retVal |= GenericConstraint.Reference;
            if (typeParamSymbol.HasUnmanagedTypeConstraint) retVal |= GenericConstraint.Unmanaged;
            if (typeParamSymbol.HasValueTypeConstraint) retVal |= GenericConstraint.Value;

            return retVal;
        }
    }
}
