namespace Tests.RoslynWalker
{
    public class BlockClosingLine : StatementLine
    {
        public BlockClosingLine( LineBlock? parent )
            : base( string.Empty, LineType.BlockCloser, parent )
        {
        }
    }
}