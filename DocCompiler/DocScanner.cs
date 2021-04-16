#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'DocCompiler' is free software: you can redistribute it
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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace J4JSoftware.DocCompiler
{
    public class DocScanner : CSharpSyntaxWalker, IDocScanner
    {
        private readonly IScannedFileFactory _scannerFactory;
        private readonly List<IScannedFile> _scanResults = new();
        private readonly List<IProjectInfo> _projects = new();

        public DocScanner(
            IScannedFileFactory scannerFactory
        )
        {
            _scannerFactory = scannerFactory;
        }

        public bool Scan( string fileToScan )
        {
            _scanResults.Clear();
            _projects.Clear();

            if( !_scannerFactory.Create( fileToScan, out var scannedFiles ) )
                return false;

            _scanResults.AddRange( scannedFiles! );

            _projects.AddRange( _scanResults.Select( x => x.BelongsTo )
                .Distinct( ProjectInfo.ProjectInfoComparer ) );

            return true;
        }

        public ReadOnlyCollection<IScannedFile> ScannedFiles => _scanResults.AsReadOnly();
        public ReadOnlyCollection<IProjectInfo> Projects => _projects.AsReadOnly();

        public List<UsingStatementSyntax> Usings => _scanResults.SelectMany( x => x.Usings ).ToList();
        public List<NamespaceDeclarationSyntax> Namespaces => _scanResults.SelectMany( x => x.Namespaces ).ToList();
        public List<ClassDeclarationSyntax> Classes => _scanResults.SelectMany( x => x.Classes ).ToList();
        public List<InterfaceDeclarationSyntax> Interfaces => _scanResults.SelectMany( x => x.Interfaces ).ToList();
        public List<StructDeclarationSyntax> Structs => _scanResults.SelectMany( x => x.Structs ).ToList();
        public List<RecordDeclarationSyntax> Records => _scanResults.SelectMany( x => x.Records ).ToList();
    }
}