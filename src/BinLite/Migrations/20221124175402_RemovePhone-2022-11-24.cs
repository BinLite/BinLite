using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BinLite.Migrations
{
    /// <inheritdoc />
    public partial class RemovePhone20221124 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "User");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "User",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
