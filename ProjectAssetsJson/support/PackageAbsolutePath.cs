namespace J4JSoftware.Roslyn.ProjectAssets
{
    public class PackageAbsolutePath
    {
        public PackageAbsolutePath(
            TargetFramework targetFW,
            string dllPath )
        {
            TargetFramework = targetFW;
            DllPath = dllPath;
        }
        public TargetFramework TargetFramework { get; }
        public string DllPath { get; }

        public bool IsVirtual => DllPath == "_._";
    }
}