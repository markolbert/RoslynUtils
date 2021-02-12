using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Tests.RoslynWalker
{
    public class LineBlock
    {
        private readonly List<SourceLine> _lines = new();

        public LineBlock( SourceLine? srcLine )
        {
            SourceLine = srcLine;

            if( SourceLine != null )
                SourceLine.ChildBlock = this;
        }

        public SourceLine? SourceLine { get; }
        public ReadOnlyCollection<SourceLine> Lines => _lines.AsReadOnly();
        public SourceLine? CurrentLine => _lines.LastOrDefault();

        public void AddLine( string line ) => _lines.Add( new SourceLine( line, this ) );
    }
}