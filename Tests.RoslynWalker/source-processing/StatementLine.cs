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

namespace Tests.RoslynWalker
{
    public class StatementLine
    {
        public StatementLine( string line, LineBlock parent )
        {
            Line = line;
            LineType = LineType.Statement;
            Parent = parent;
        }

        protected StatementLine( string line, LineType lineType, LineBlock? parent )
        {
            Line = line;
            LineType = lineType;
            Parent = parent;
        }

        public bool Parsed { get; private set; }
        public string Line { get; }
        public LineType LineType { get; }
        public LineBlock? Parent { get; set; }
        public List<BaseInfo>? Elements { get; private set; }

        public void Parse( ParserCollection parsers )
        {
            Elements = parsers.Parse( this );
            Parsed = true;
        }
    }
}
