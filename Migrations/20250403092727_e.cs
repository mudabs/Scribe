using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scribe.Migrations
{
    /// <inheritdoc />
    public partial class e : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "User",
                table: "SerialNumbers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "User",
                table: "SerialNumbers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
