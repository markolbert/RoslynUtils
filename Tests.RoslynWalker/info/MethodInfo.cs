#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'Tests.RoslynWalker' is free software: you can redistribute it
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

using System.Collections.Generic;
using System.Linq;

#pragma warning disable 8618

namespace Tests.RoslynWalker
{
    public class MethodInfo : ElementInfo, IArguments, IAttributable
    {
        public MethodInfo( MethodSource src )
            : base( ElementNature.Method, src )
        {
            TypeArguments = src.TypeArguments;

            Arguments = src.Arguments
                .Select( x => new ArgumentInfo( x ) )
                .ToList();

            Attributes = src.Attributes
                .Select( x => new AttributeInfo( x ) )
                .ToList();
            
            ReturnType = src.ReturnType;
        }

        public List<ArgumentInfo> Arguments { get; }
        public List<string> TypeArguments { get; }
        public string ReturnType { get; }
        public List<AttributeInfo> Attributes { get; }

        public override string FullName
        {
            get
            {
                var typeArgs = TypeArguments.Count > 0 ? $"<{string.Join( ", ", TypeArguments )}>" : string.Empty;
                var args = Arguments.Any() ? $"( {string.Join(", ", Arguments)} )" : "()";

                return $"{FullNameWithoutArguments}{typeArgs}{args}";
            }
        }
    }
}