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
using System.Collections.Generic;
using System.Linq;
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
#pragma warning disable 8618

namespace J4JSoftware.DocCompiler
{
    [EntityConfiguration(typeof(CodeFileConfigurator))]
    public class CodeFile
    {
        //private readonly DocDbContext? _dbContext;

        //public CodeFile()
        //{
        //}

        //private CodeFile( DocDbContext dbContext )
        //{
        //    _dbContext = dbContext;
        //}

        public int ID { get; set; }
        public string FullPath { get; set; }
        public int AssemblyID { get; set; }
        public Assembly Assembly { get; set; }

        public List<NamespaceContext> GetNamespaceContext( DocDbContext dbContext, List<NamespaceContext>? retVal = null )
        {
            retVal ??= new List<NamespaceContext>();

            // load outer namespaces if that wasn't done and we have a DocDBContext
            // we can use to do so
            if( OuterNamespaces == null )
                dbContext.Entry( this )
                    .Collection( x => x.OuterNamespaces )
                    .Load();

            foreach( var curNS in OuterNamespaces! )
            {
                if( retVal.All( x => !x.Label.Equals( curNS.Name, StringComparison.Ordinal ) ) )
                    retVal.Add( new NamespaceContext(curNS) );
            }

            return retVal;
        }

        // OuterNamespaces are always the result of using statements 
        // within the outermost level of a source code file (i.e., outside
        // any namespace declarations)
        public ICollection<Namespace>? OuterNamespaces { get; set; }

        public ICollection<DocumentedType> DocumentedTypes { get; set; }
    }

    internal class CodeFileConfigurator : EntityConfigurator<CodeFile>
    {
        protected override void Configure( EntityTypeBuilder<CodeFile> builder )
        {
            builder.HasIndex( x => x.FullPath )
                .IsUnique();

            builder.HasOne( x => x.Assembly )
                .WithMany( x => x.CodeFiles )
                .HasForeignKey( x => x.AssemblyID )
                .HasPrincipalKey( x => x.ID );

            builder.Property( x => x.FullPath )
                .UseOsCollation();
        }
    }
}