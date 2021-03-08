﻿#region license

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
using J4JSoftware.Roslyn;

namespace Tests.RoslynWalker
{
    public class StatementLine
    {
        public StatementLine( string line, BlockLine parent )
        {
            Line = line;
            LineType = LineType.Statement;
            Parent = parent;
        }

        protected StatementLine( string line, LineType lineType, BlockLine? parent )
        {
            Line = line;
            LineType = lineType;
            Parent = parent;
        }

        public bool Parsed { get; private set; }
        public string Line { get; }
        public LineType LineType { get; }
        public BlockLine? Parent { get; set; }
        public List<AttributeInfo>? Attributes { get; private set; }
        public List<BaseInfo>? Elements { get; private set; }

        public IEnumerable<BaseInfo> Parse( ParserCollection parsers )
        {
            var parsed = parsers.Parse( this );

            Elements = parsed?.Elements;
            Attributes = parsed?.Attributes;

            Parsed = true;

            return Elements ?? Enumerable.Empty<BaseInfo>();
        }
    }
}
