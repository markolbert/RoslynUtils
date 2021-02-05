namespace J4JSoftware.Roslyn
{
    public enum NodeSinkResult
    {
        Okay,
        AlreadyProcessed,
        IgnorableNode,
        InvalidNode,
        TerminalNode,
        UnsupportedNode
    }
}