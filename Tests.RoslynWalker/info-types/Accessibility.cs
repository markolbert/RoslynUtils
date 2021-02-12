namespace Tests.RoslynWalker
{
    public enum Accessibility
    {
        [AccessibilityText("public")]
        Public,

        [AccessibilityText("protected")]
        Protected,

        [AccessibilityText("private")]
        [AccessibilityText("")]
        Private,

        [AccessibilityText("internal")]
        Internal,
        
        [AccessibilityText("protected internal")]
        ProtectedInternal,

        Undefined
    }
}