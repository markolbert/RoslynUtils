#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'GeneralRoslyn' is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public License as
// published by the Free Software Foundation, either version 3 of the License,
// or (at your option) any later version.
// 
// This library or program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// General Public License for more details.
// 
// You should have received a copy of the GNU General Public License along with
// this library or program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Buildalyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.Roslyn
{
    public class CompiledProject : IEnumerable<CompiledFile>
    {
        private readonly List<CompiledFile> _results = new();

        public CompiledProject(
            IAnalyzerResults buildResults,
            Project project,
            CSharpCompilation compilation
        )
        {
            AssemblySymbol = compilation.Assembly;
            AssemblyName = compilation.AssemblyName ?? string.Empty;
            ProjectFile = project.FilePath ?? "***undefined csproj file***";

            TargetFrameworksText = buildResults.Aggregate(
                new StringBuilder(),
                ( sb, ar ) =>
                {
                    if( sb.Length > 0 )
                        sb.Append( ";" );

                    sb.Append( ar.TargetFramework.ToString() );

                    return sb;
                }, sb => sb.ToString() );

            var buildResult = buildResults.First();

            RootNamespace = buildResult.GetProperty( "RootNamespace" ) ?? string.Empty;
            Authors = buildResult.GetProperty( "Authors" ) ?? string.Empty;
            Company = buildResult.GetProperty( "Company" ) ?? string.Empty;
            Description = buildResult.GetProperty( "Description" ) ?? string.Empty;
            Copyright = buildResult.GetProperty( "Copyright" ) ?? string.Empty;
            FileVersionText = buildResult.GetProperty( "Version" ) ?? "1.0.0.0";
            AssemblyVersionText = buildResult.GetProperty( "AssemblyVersion" ) ?? string.Empty;
            PackageVersionText = buildResult.GetProperty( "PackageVersion" ) ?? "1.0.0";

            foreach( var tree in compilation.SyntaxTrees )
            {
                if( !tree.TryGetRoot( out var rootNode ) )
                    throw new ArgumentException( $"Couldn't get root of {typeof(SyntaxTree)}" );

                try
                {
                    _results.Add( new CompiledFile( rootNode, compilation.GetSemanticModel( tree ), this ) );
                }
                catch( Exception e )
                {
                    throw new InvalidOperationException(
                        $"Couldn't get {typeof(SemanticModel)} for {typeof(SyntaxTree)}. Exception message was {e.Message}" );
                }
            }
        }

        public IAssemblySymbol AssemblySymbol { get; }
        public string AssemblyName { get; }
        public string ProjectFile { get; }
        public string RootNamespace { get; }
        public string Authors { get; }
        public string Company { get; }
        public string Description { get; }
        public string Copyright { get; }
        public string AssemblyVersionText { get; }
        public string FileVersionText { get; }
        public string PackageVersionText { get; }
        public string TargetFrameworksText { get; }

        public ReadOnlyCollection<CompiledFile> Results => _results.AsReadOnly();

        public IEnumerator<CompiledFile> GetEnumerator()
        {
            foreach( var result in _results ) yield return result;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}