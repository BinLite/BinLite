using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BinLite.Migrations
{
    /// <inheritdoc />
    public partial class RenameAndConsistentTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Location",
                table: "Location");

            migrationBuilder.RenameTable(
                name: "Location",
                newName: "Container");

            migrationBuilder.RenameIndex(
                name: "IX_Location_Number",
                table: "Container",
                newName: "IX_Container_Number");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Container",
                table: "Container",
                column: "ContainerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Container",
                table: "Container");

            migrationBuilder.RenameTable(
                name: "Container",
                newName: "Location");

            migrationBuilder.RenameIndex(
                name: "IX_Container_Number",
                table: "Location",
                newName: "IX_Location_Number");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Location",
                table: "Location",
                column: "ContainerId");
        }
    }
}
