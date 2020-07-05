﻿using System;
using System.Dynamic;
using J4JSoftware.Logging;

namespace J4JSoftware.Roslyn
{
    public class ProjectReference : ConfigurationBase
    {
        public ProjectReference( 
            string text,
            ExpandoObject refInfo,
            Func<IJ4JLogger> loggerFactory 
            ) 
            : base( loggerFactory )
        {
            ProjectName = text;
            ProjectPath = GetProperty<string>( refInfo, "projectPath" );
        }

        public string ProjectName { get; }
        public string ProjectPath { get; }
    }
}