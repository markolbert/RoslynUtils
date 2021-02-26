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
using System.Text.RegularExpressions;
using Xunit.Sdk;

namespace Tests.RoslynWalker
{
    public class SourceLine
    {
        public SourceLine( string line, LineBlock parent )
        {
            Line = line;
            LineType = LineType.Statement;
            Parent = parent;
        }

        protected SourceLine( string line, LineType lineType, LineBlock? parent )
        {
            Line = line;
            LineType = lineType;
            Parent = parent;
        }

        public bool Parsed { get; internal set; }
        public string Line { get; }
        public LineType LineType { get; }
        public LineBlock? Parent { get; set; }
        public List<BaseInfo>? Elements { get; internal set; }
    }

    public class BlockOpeningLine : SourceLine
    {
        public BlockOpeningLine( string line, LineBlock? parent )
            :base(line, LineType.BlockOpener, parent)
        {
            ChildBlock = new LineBlock( this );
        }

        public LineBlock ChildBlock { get; }
    }

    public class BlockClosingLine : SourceLine
    {
        public BlockClosingLine( LineBlock? parent )
            : base( string.Empty, LineType.BlockCloser, parent )
        {
        }
    }
}
