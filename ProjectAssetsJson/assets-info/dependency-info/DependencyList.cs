using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using J4JSoftware.Logging;
using NuGet.Versioning;

namespace J4JSoftware.Roslyn
{
    public class DependencyList : DependencyInfoBase
    {
        public DependencyList(
            string text,
            ExpandoObject depListInfo,
            Func<IJ4JLogger> loggerFactory ) 
            : base( text, loggerFactory )
        {
            TargetType = GetEnum<ReferenceType>( depListInfo, "target" );

            var versionsText = GetProperty<string>(depListInfo,"version");

            // parse into individual version strings
            versionsText = versionsText.Replace("[", "")
                .Replace(")", "")
                .Replace(" ", "");

            var parts = versionsText.Split(',', StringSplitOptions.RemoveEmptyEntries);

            foreach (var verText in parts)
            {
                if( verText.ToSemanticVersion( out var version ) )
                    Versions.Add( version! );
                else throw new ArgumentException( $"Couldn't parse '{verText}' into a {typeof(SemanticVersion)}" );
            }
        }

        public ReferenceType TargetType { get; }
        public List<SemanticVersion> Versions { get; } = new List<SemanticVersion>();
    }
}