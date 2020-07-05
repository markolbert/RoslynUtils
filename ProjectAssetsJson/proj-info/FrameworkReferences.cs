using System;
using System.Collections.Generic;
using System.Dynamic;
using J4JSoftware.Logging;

namespace J4JSoftware.Roslyn
{
    public class FrameworkReferences : FrameworkBase
    {
        public FrameworkReferences( 
            string text,
            ExpandoObject fwInfo,
            Func<IJ4JLogger> loggerFactory
            ) 
            : base( text, loggerFactory )
        {
            CreateProjectReferences( GetProperty<ExpandoObject>( fwInfo, "projectReferences", optional : true ) );
        }

        private void CreateProjectReferences( ExpandoObject refInfo )
        {
            foreach( var kvp in refInfo )
            {
                if( kvp.Value is ExpandoObject detail )
                    ProjectReferences.Add( new ProjectReference( kvp.Key, detail, LoggerFactory ) );
                else
                    LogAndThrow( "Project reference item is not an ExpandoObject", kvp.Key, typeof(ExpandoObject) );
            }
        }

        public List<ProjectReference> ProjectReferences { get; } = new List<ProjectReference>();
    }
}