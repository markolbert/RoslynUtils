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
using System.Collections.ObjectModel;
using System.Linq;

namespace Tests.RoslynWalker
{
    public class LineBlock
    {
        private readonly List<SourceLine> _lines = new();

        public LineBlock( BlockOpeningLine? srcLine )
        {
            ParentLine = srcLine;
        }

        public BlockOpeningLine? ParentLine { get; }
        public ReadOnlyCollection<SourceLine> Lines => _lines.AsReadOnly();
        public SourceLine? CurrentLine => _lines.LastOrDefault();

        public void AddStatement( string text ) => _lines.Add( new SourceLine( text, this ) );
        public void AddBlockOpener( string text ) => _lines.Add( new BlockOpeningLine( text, this ) );
        public void AddBlockCloser() => _lines.Add( new BlockClosingLine( this ) );
    }
}