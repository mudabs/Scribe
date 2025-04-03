using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scribe.Migrations
{
    /// <inheritdoc />
    public partial class d : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "SerialNumbers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SerialNumbers_GroupId",
                table: "SerialNumbers",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_SerialNumbers_Group_GroupId",
                table: "SerialNumbers",
                column: "GroupId",
                principalTable: "Group",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SerialNumbers_Group_GroupId",
                table: "SerialNumbers");

            migrationBuilder.DropIndex(
                name: "IX_SerialNumbers_GroupId",
                table: "SerialNumbers");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "SerialNumbers");
        }
    }
}
