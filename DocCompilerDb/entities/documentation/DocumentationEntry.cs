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
using J4JSoftware.EFCoreUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace J4JSoftware.DocCompiler
{
    [EntityConfiguration(typeof(DocumentationEntryConfigurator))]
    public class DocumentationEntry
    {
        public int ID { get; set; }
        public DateTime Timestamp { get; set; }
        public DocumentationEntryType EntryType { get;set; }
        public string Text { get; set; }
        public int Index { get;set; }

        public int AuthorID { get; set; }
        public Author Author { get; set; }

        public int DocumentationID { get; set; }
        public Documentation Documentation { get; set; }
    }

    internal class DocumentationEntryConfigurator : EntityConfigurator<DocumentationEntry>
    {
        protected override void Configure( EntityTypeBuilder<DocumentationEntry> builder )
        {
            builder.Property( x => x.Timestamp )
                .HasDefaultValue( DateTime.Now );

            builder.Property( x => x.EntryType )
                .HasConversion<string>();

            builder.HasIndex( x => x.Index )
                .IsUnique();

            builder.HasOne( x => x.Author )
                .WithMany( x => x.Entries )
                .HasForeignKey( x => x.AuthorID )
                .HasPrincipalKey( x => x.ID );

            builder.HasOne( x => x.Documentation )
                .WithMany( x => x.Entries )
                .HasForeignKey( x => x.DocumentationID )
                .HasPrincipalKey( x => x.ID );
        }
    }
}