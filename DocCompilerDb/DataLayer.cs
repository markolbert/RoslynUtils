#region license

// Copyright 2021 Mark A. Olbert
// 
// This library or program 'DocCompilerDb' is free software: you can redistribute it
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
using System.Linq;
using J4JSoftware.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace J4JSoftware.DocCompiler
{
    public class DataLayer : IDataLayer
    {
        private readonly DocDbContext _dbContext;
        private readonly IJ4JLogger? _logger;

        public DataLayer(
            DocDbContext dbContext,
            IJ4JLogger? logger
        )
        {
            _dbContext = dbContext;

            _logger = logger;
            _logger?.SetLoggedType( GetType() );
        }

        public void Deprecate()
        {
            _dbContext.Deprecate();
        }

        public void SaveChanges() => _dbContext.SaveChanges();

        public void UpdateAssembly( IProjectInfo projInfo )
        {
            var assemblyDb = _dbContext.Assemblies
                .FirstOrDefault( x => x.AssemblyName == projInfo.AssemblyName );

            if( assemblyDb == null )
            {
                assemblyDb = new Assembly { AssemblyName = projInfo.AssemblyName };

                _dbContext.Assemblies.Add( assemblyDb );
            }
            else assemblyDb.Deprecated = false;

            assemblyDb.ProjectDirectory = projInfo.ProjectDirectory;
            assemblyDb.Timestamp = DateTime.Now;

            assemblyDb.RootNamespace = projInfo.RootNamespace;
            assemblyDb.TargetFrameworks = projInfo.TargetFrameworks;
            
            assemblyDb.Authors = projInfo.Authors;
            assemblyDb.Company = projInfo.Company;
            assemblyDb.Copyright = projInfo.Copyright;
            assemblyDb.Description = projInfo.Description;
            
            assemblyDb.PackageDescription = projInfo.PackageDescription;
            assemblyDb.PackageLicense = projInfo.PackageLicense;
            
            assemblyDb.RepositoryUrl = projInfo.RepositoryUrl;
            assemblyDb.RepositoryType = projInfo.RepositoryType;
            
            assemblyDb.AssemblyVersion = projInfo.AssemblyVersion;
            assemblyDb.FileVersion = projInfo.FileVersion;
            assemblyDb.Version = projInfo.Version;

            _dbContext.SaveChanges();
        }

        public bool UpdateCodeFile( IScannedFile scannedFile )
        {
            var assemblyDb = _dbContext.Assemblies
                .FirstOrDefault( x =>
                    x.AssemblyName == scannedFile.BelongsTo.AssemblyName );

            if( assemblyDb == null )
            {
                _logger?.Error<string>("File '{0}' references an Assembly not in the database", scannedFile.SourceFilePath  );
                return false;
            }

            var codeFileDb = _dbContext.CodeFiles
                .FirstOrDefault( x => x.FullPath == scannedFile.SourceFilePath );

            if( codeFileDb == null )
            {
                codeFileDb = new CodeFile
                {
                    AssemblyID = assemblyDb.ID,
                    FullPath = scannedFile.SourceFilePath
                };

                _dbContext.CodeFiles.Add( codeFileDb );
            }
            else
            {
                codeFileDb.Deprecated = false;
                codeFileDb.Assembly = assemblyDb;
            }

            _dbContext.SaveChanges();

            return true;
        }

        public bool UpdateNamespace( SyntaxNode node )
        {
            if( node.Kind() != SyntaxKind.NamespaceDeclaration )
            {
                _logger?.Error("Supplied SyntaxNode is not a NamespaceDeclaration"  );
                return false;
            }

            if( node.Parent == null )
            {
                _logger?.Error("Supplied NamespaceDeclaration node has no parent"  );
                return false;
            }

            // grab all the IdentifierToken child tokens as they contain the dotted
            // elements of the namespace's name
            var qualifiedName = node.ChildNodes()
                .FirstOrDefault( x => x.Kind() == SyntaxKind.QualifiedName );

            if( qualifiedName == null )
            {
                _logger?.Error("Supplied NamespaceDeclaration node has no QualifiedName"  );
                return false;
            }

            var identifierNodes = qualifiedName.DescendantNodes()
                .Where( x => x.Kind() == SyntaxKind.IdentifierName );

            var name = string.Join( ".", identifierNodes );

            var nsDb = _dbContext.Namespaces
                .FirstOrDefault( x => x.Name == name );

            // we don't try to update the namespace's container, as that may not be defined yet
            // if it's a class
            if( nsDb == null )
            {
                nsDb = new Namespace { Name = name };
                _dbContext.Namespaces.Add( nsDb );
            }
            else nsDb.Deprecated = false;

            _dbContext.SaveChanges();

            return true;
        }
    }
}