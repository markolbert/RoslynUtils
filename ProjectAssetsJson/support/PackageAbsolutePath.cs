namespace J4JSoftware.Roslyn
{
    public class PackageAbsolutePath
    {
        public TargetFramework? TargetFramework { get; set; }
        public string DllPath { get; set; } = string.Empty;
        public bool IsVirtual => DllPath != null && DllPath == "_._";
    }
}