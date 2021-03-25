using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace J4JSoftware.DocCompiler
{
    public class ParsedSourceFile
    {
        public string SourceFilePath { get; internal set; } = string.Empty;
        public bool IsParsed { get; internal set; }
        public SyntaxNode? RootNode { get; internal set; } 

        public List<UsingStatementSyntax> Usings { get; } = new();
        public List<NamespaceDeclarationSyntax> Namespaces { get; } = new();
        public List<ClassDeclarationSyntax> Classes { get; } = new();
        public List<InterfaceDeclarationSyntax> Interfaces { get; } = new();
        public List<StructDeclarationSyntax> Structs { get; } = new();
        public List<RecordDeclarationSyntax> Records { get; } = new();
    }
}
