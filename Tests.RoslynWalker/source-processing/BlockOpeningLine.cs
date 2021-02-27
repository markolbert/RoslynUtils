namespace Tests.RoslynWalker
{
    public class BlockOpeningLine : StatementLine
    {
        public BlockOpeningLine( string line, LineBlock? parent )
            :base(line, LineType.BlockOpener, parent)
        {
            ChildBlock = new LineBlock( this );
        }

        public LineBlock ChildBlock { get; }
    }
}