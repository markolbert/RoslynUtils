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
#pragma warning disable 8618

namespace J4JSoftware.DocCompiler
{
    public class StandaloneFile : IScanResults
    {
        public static bool ParseStandaloneFile( 
            string sourceFilePath, 
            DocNodeWalker nodeWalker, 
            IJ4JLogger? logger,
            out StandaloneFile? result )
        {
            result = null;

            if( !File.Exists( sourceFilePath ) )
            {
                logger?.Error<string>("Source file '{0}' does not exist", sourceFilePath);
                return false;
            }

            SyntaxNode? rootNode = null;

            try
            {
                using var fileStream = File.OpenRead( sourceFilePath );
                var srcText = SourceText.From( fileStream );

                var syntaxTree  = CSharpSyntaxTree.ParseText( srcText );
                rootNode = syntaxTree.GetRoot();

                nodeWalker.Visit(rootNode);
            }
            catch( Exception e )
            {
                logger?.Error<string>("Parsing failed, exception was '{0}'", e.Message  );

                return false;
            }

            result = new StandaloneFile
            {
                SourceFilePath = sourceFilePath,
                RootNode = rootNode,
                Usings = nodeWalker.Usings,
                Namespaces = nodeWalker.Namespaces,
                Classes = nodeWalker.Classes,
                Interfaces = nodeWalker.Interfaces,
                Structs = nodeWalker.Structs,
                Records = nodeWalker.Records
            };

            return true;
        }

        protected StandaloneFile()
        {
        }

        public string SourceFilePath { get; init; }
        public SyntaxNode RootNode { get; init; } 

        public List<UsingStatementSyntax> Usings { get; init; }
        public List<NamespaceDeclarationSyntax> Namespaces { get; init; }
        public List<ClassDeclarationSyntax> Classes { get; init; }
        public List<InterfaceDeclarationSyntax> Interfaces { get; init; }
        public List<StructDeclarationSyntax> Structs { get; init; }
        public List<RecordDeclarationSyntax> Records { get; init; }
    }
}
