using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;

namespace J4JSoftware.Roslyn
{
    public interface IProjectLibrary : ILibraryInfo
    {
        string ProjectFilePath { get; }
        string ProjectDirectory { get; }
        List<TargetFramework> TargetFrameworks { get; }
        OutputType OutputType { get; }
        OutputKind OutputKind { get; }
        XDocument? Document { get; }
        XElement ProjectElement { get; }
        string? AssemblyName { get; }
        string? RootNamespace { get; }
        string? Authors { get; }
        string? Company { get; }
        string? Description { get; }
        string? Copyright { get; }
        NullableContextOptions NullableContextOptions { get; }
        List<string> ExcludedFiles { get; }
        List<string> SourceFiles { get; }
        Version DotNetVersion { get; }
        Version FileVersion { get; }
    }
}