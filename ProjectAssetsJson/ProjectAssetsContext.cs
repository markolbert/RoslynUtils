using System;
using System.Dynamic;
using System.IO;

namespace J4JSoftware.Roslyn
{
    public class ProjectAssetsContext
    {
        public ExpandoObject RootContainer { get; set; }

        public string ProjectDirectory =>
            String.IsNullOrEmpty( ProjectPath ) ? null : Path.GetDirectoryName( ProjectPath );

        public string ProjectPath { get; set; }
        public string ProjectAssetsJsonPath { get; set; }
    }
}