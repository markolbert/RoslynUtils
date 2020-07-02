namespace J4JSoftware.Roslyn
{
    public class ReferenceLoadResult
    {
        public string Assembly { get; set; } = string.Empty;
        public CompilationLoadStatus Status { get; set; }
    }
}