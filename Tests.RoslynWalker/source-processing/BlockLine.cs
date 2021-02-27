using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Tests.RoslynWalker
{
    public class BlockLine : StatementLine
    {
        private readonly List<StatementLine> _children = new();

        public BlockLine( string line, BlockLine? parent )
            :base(line, LineType.BlockOpener, parent)
        {
        }

        public ReadOnlyCollection<StatementLine> Children => _children.AsReadOnly();

        public StatementLine AddStatement(string text)
        {
            var retVal = new StatementLine(text, this);
            _children.Add(retVal);

            return retVal;
        }

        public BlockLine AddBlockOpener(string text)
        {
            var retVal = new BlockLine(text, this);
            _children.Add(retVal);

            return retVal;
        }
    }
}