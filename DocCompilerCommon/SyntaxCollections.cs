#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'DocCompilerCommon' is free software: you can redistribute it
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

using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.DocCompiler
{
    public static class SyntaxCollections
    {
        public static SyntaxKind[] AccessKinds = new[]
        {
            SyntaxKind.PublicKeyword,
            SyntaxKind.PrivateKeyword,
            SyntaxKind.ProtectedKeyword,
            SyntaxKind.InternalKeyword
        };

        public static SyntaxKind[] DocumentedTypeKinds = new[]
        {
            SyntaxKind.ClassDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.RecordDeclaration,
            SyntaxKind.StructDeclaration
        };

        public static SyntaxKind[] TypeNodeKinds = new[]
        {
            SyntaxKind.GenericName,
            SyntaxKind.IdentifierName,
            SyntaxKind.PredefinedType,
            SyntaxKind.TupleType
        };

        public static SyntaxKind[] ArgumentModifiers = new[]
        {
            SyntaxKind.RefKeyword, 
            SyntaxKind.OutKeyword
        };

        public static SyntaxKind[] TypeAnalyzerKinds =
        {
            SyntaxKind.SimpleBaseType, 
            SyntaxKind.TypeConstraint,
            SyntaxKind.IdentifierName,
            SyntaxKind.PredefinedType,
            SyntaxKind.ArrayType,
            SyntaxKind.GenericName,
            SyntaxKind.TupleType
        };

        public static SyntaxKind[] TupleKinds =
        {
            SyntaxKind.TupleElement,
            SyntaxKind.TupleType
        };
    }
}