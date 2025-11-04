using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IkeaDocuScan.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCounterPartyRelationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop CounterPartyRelation table if it exists
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CounterPartyRelation]') AND type in (N'U'))
                BEGIN
                    DROP TABLE [dbo].[CounterPartyRelation]
                END
            ");

            // Drop CounterPartyRelationType table if it exists
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CounterPartyRelationType]') AND type in (N'U'))
                BEGIN
                    DROP TABLE [dbo].[CounterPartyRelationType]
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Recreate CounterPartyRelationType table
            migrationBuilder.CreateTable(
                name: "CounterPartyRelationType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CounterP__3214EC27E77D97BE", x => x.Id);
                });

            // Recreate CounterPartyRelation table
            migrationBuilder.CreateTable(
                name: "CounterPartyRelation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentCounterPartyId = table.Column<int>(type: "int", nullable: false),
                    ChildCounterPartyId = table.Column<int>(type: "int", nullable: false),
                    RelationType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CounterP__3214EC274F1EBB10", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CounterPartyRelation_Child_CounterParty",
                        column: x => x.ChildCounterPartyId,
                        principalTable: "CounterParty",
                        principalColumn: "CounterPartyId");
                    table.ForeignKey(
                        name: "FK_CounterPartyRelation_Parent_CounterParty",
                        column: x => x.ParentCounterPartyId,
                        principalTable: "CounterParty",
                        principalColumn: "CounterPartyId");
                    table.ForeignKey(
                        name: "FK__CounterPa__Relat__5FB337D6",
                        column: x => x.RelationType,
                        principalTable: "CounterPartyRelationType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CounterPartyRelation_ChildCounterPartyId",
                table: "CounterPartyRelation",
                column: "ChildCounterPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_CounterPartyRelation_ParentCounterPartyId",
                table: "CounterPartyRelation",
                column: "ParentCounterPartyId");

            migrationBuilder.CreateIndex(
                name: "IX_CounterPartyRelation_RelationType",
                table: "CounterPartyRelation",
                column: "RelationType");
        }
    }
}
