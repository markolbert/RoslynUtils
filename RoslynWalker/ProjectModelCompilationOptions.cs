using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public class ProjectModelCompilationOptions
    {
        private int _warningLevel = 4;

        public ProjectModelCompilationOptions()
        {
            Suppress = new List<string>( new string[] { "CS1701", "CS1702" } );
        }

        public int WarningLevel
        {
            get => _warningLevel;
            set => _warningLevel = value < 0 ? 4 : value;
        }

        public List<string> Suppress { get; }
        public OptimizationLevel OptimizationLevel { get; set; } = OptimizationLevel.Release;
        public ReportDiagnostic DiagnosticLevel { get; set; } = ReportDiagnostic.Error;
        public Platform Platform { get; set; } = Platform.AnyCpu;

        public Dictionary<string, ReportDiagnostic> GetSuppressedDiagnostics() =>
            Suppress.ToDictionary( s => s, s => ReportDiagnostic.Suppress );
    }
}