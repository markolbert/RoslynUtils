using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

#pragma warning disable 8618

namespace J4JSoftware.DocCompiler
{
    public class ScannedFile : IScannedFile
    {
        internal ScannedFile()
        {
        }

        public IProjectInfo BelongsTo { get; init; }
        public string SourceFilePath { get; init; }
        public SyntaxNode RootNode { get; init; }

        public List<UsingStatementSyntax> Usings { get; } = new();
        public List<NamespaceDeclarationSyntax> Namespaces { get; } = new();
        public List<ClassDeclarationSyntax> Classes { get; } = new();
        public List<InterfaceDeclarationSyntax> Interfaces { get; } = new();
        public List<StructDeclarationSyntax> Structs { get; } = new();
        public List<RecordDeclarationSyntax> Records { get; } = new();
    }
}
