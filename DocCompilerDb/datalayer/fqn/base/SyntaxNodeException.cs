#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'DocCompilerDb' is free software: you can redistribute it
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

using System;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.DocCompiler
{
    public class SyntaxNodeException : Exception
    {
        public SyntaxNodeException( 
            string? preamble = null, 
            SyntaxKind? requiredKind = null, 
            SyntaxKind? providedKind = null 
            )
        {
            Preamble = preamble;
            RequiredKind = requiredKind;
            ProvidedKind = providedKind;
        }

        public string? Preamble { get; }
        public SyntaxKind? RequiredKind { get; }
        public SyntaxKind? ProvidedKind { get; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            if( RequiredKind.HasValue )
                sb.Append( $": required a {RequiredKind.Value}" );

            if( ProvidedKind.HasValue )
            {
                if( sb.Length > 0 )
                    sb.Append( ", " );

                sb.Append( $"given a {ProvidedKind.Value}" );
            }

            if( sb.Length > 0 )
                sb.Insert( 0, ": " );

            sb.Insert( 0, "SyntaxNodeException" );

            return sb.ToString();
        }
    }
}