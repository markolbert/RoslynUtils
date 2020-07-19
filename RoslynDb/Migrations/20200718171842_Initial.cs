using Microsoft.EntityFrameworkCore.Migrations;

namespace J4JSoftware.Roslyn.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InScopeInfo",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RootNamespace = table.Column<string>(nullable: false),
                    Authors = table.Column<string>(nullable: false),
                    Company = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: false),
                    Copyright = table.Column<string>(nullable: false),
                    AssemblyID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InScopeInfo", x => x.ID);
                    table.UniqueConstraint("AK_InScopeInfo_AssemblyID", x => x.AssemblyID);
                });

            migrationBuilder.CreateTable(
                name: "Assemblies",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false),
                    Synchronized = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    FullyQualifiedName = table.Column<string>(nullable: false),
                    DotNetVersionText = table.Column<string>(nullable: false),
                    FileVersionText = table.Column<string>(nullable: false),
                    PackageVersionText = table.Column<string>(nullable: false),
                    FrameworkName = table.Column<string>(nullable: false),
                    FrameworkVersion = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assemblies", x => x.ID);
                    table.UniqueConstraint("AK_Assemblies_FullyQualifiedName", x => x.FullyQualifiedName);
                    table.ForeignKey(
                        name: "FK_Assemblies_InScopeInfo_ID",
                        column: x => x.ID,
                        principalTable: "InScopeInfo",
                        principalColumn: "AssemblyID",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Assemblies");

            migrationBuilder.DropTable(
                name: "InScopeInfo");
        }
    }
}
