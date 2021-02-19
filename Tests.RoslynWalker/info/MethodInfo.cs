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

using System;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable 8618

namespace Tests.RoslynWalker
{
    public class MethodInfo : ElementInfo, IArguments
    {
        public MethodInfo()
            : base( ElementNature.Method )
        {
        }

        public List<string> Arguments { get; } = new();
        public List<string> TypeArguments { get; } = new();
        public string ReturnType { get; set; }

        public override string FullName
        {
            get
            {
                var typeArgs = TypeArguments.Count > 0 ? $"<{string.Join( ", ", TypeArguments )}>" : string.Empty;
                var args = Arguments.Any() ? $"( {string.Join(", ", Arguments)} )" : "()";

                return $"{FullNameWithoutArguments}{typeArgs}{args}";
            }
        }

        //public static MethodInfo Create( SourceLine srcLine )
        //{
        //    var openParenLoc = srcLine.Line.IndexOf( "(", StringComparison.Ordinal );
        //    var lesserThanLoc = srcLine.Line.IndexOf( "<", StringComparison.Ordinal );

        //    var retVal = new MethodInfo( srcLine.ElementName!, srcLine.Accessibility );

        //    if( lesserThanLoc >= 0 )
        //        retVal.TypeArguments.AddRange( SourceText.GetTypeArgs( srcLine.Line[ lesserThanLoc.. ] ) );

        //    retVal.Arguments.AddRange( SourceText.GetArgs( srcLine.Line[ openParenLoc.. ] ) );

        //    return retVal;
        //}
    }
}