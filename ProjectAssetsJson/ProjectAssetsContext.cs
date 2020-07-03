using System;
using System.Dynamic;
using System.IO;

namespace J4JSoftware.Roslyn
{
    public class ProjectAssetsContext
    {
        public ExpandoObject RootContainer { get; set; } = new ExpandoObject();

        public string? ProjectDirectory =>
            string.IsNullOrEmpty( ProjectPath ) ? null : Path.GetDirectoryName( ProjectPath );

        public string ProjectPath { get; set; } = string.Empty;
        public string ProjectAssetsJsonPath { get; set; } = string.Empty;
    }
}