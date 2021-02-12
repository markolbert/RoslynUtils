#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'GeneralRoslyn' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public static class SymbolExtensions
    {
        public static DeclarationModifier GetDeclarationModifier( this ISymbol symbol )
        {
            var retVal = DeclarationModifier.None;

            if( symbol.IsAbstract ) retVal |= DeclarationModifier.Abstract;
            if( symbol.IsOverride ) retVal |= DeclarationModifier.Override;
            if( symbol.IsSealed ) retVal |= DeclarationModifier.Sealed;
            if( symbol.IsStatic ) retVal |= DeclarationModifier.Static;
            if( symbol.IsVirtual ) retVal |= DeclarationModifier.Virtual;

            if( symbol is IFieldSymbol fieldSymbol )
            {
                if( fieldSymbol.IsConst ) retVal |= DeclarationModifier.Const;
                if( fieldSymbol.IsReadOnly ) retVal |= DeclarationModifier.ReadOnly;
            }

            if( symbol is IPropertySymbol propSymbol )
            {
                if( propSymbol.IsWithEvents ) retVal |= DeclarationModifier.WithEvents;
                if( propSymbol.IsWriteOnly ) retVal |= DeclarationModifier.WriteOnly;
                if( propSymbol.IsReadOnly ) retVal |= DeclarationModifier.ReadOnly;
            }

            if( symbol is IMethodSymbol methodSymbol )
            {
                if( methodSymbol.IsAsync ) retVal |= DeclarationModifier.Async;
                if( methodSymbol.IsReadOnly ) retVal |= DeclarationModifier.ReadOnly;
            }

            return retVal;
        }

        public static ParametricTypeConstraint GetParametricTypeConstraint( this ITypeParameterSymbol typeParamSymbol )
        {
            var retVal = ParametricTypeConstraint.None;

            if( typeParamSymbol.HasConstructorConstraint ) retVal |= ParametricTypeConstraint.Constructor;
            if( typeParamSymbol.HasNotNullConstraint ) retVal |= ParametricTypeConstraint.NotNull;
            if( typeParamSymbol.HasReferenceTypeConstraint ) retVal |= ParametricTypeConstraint.Reference;
            if( typeParamSymbol.HasUnmanagedTypeConstraint ) retVal |= ParametricTypeConstraint.Unmanaged;
            if( typeParamSymbol.HasValueTypeConstraint ) retVal |= ParametricTypeConstraint.Value;

            return retVal;
        }
    }
}