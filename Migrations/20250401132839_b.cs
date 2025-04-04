using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scribe.Migrations
{
    /// <inheritdoc />
    public partial class b : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AllocationHistory_ADUsers_ADUsersId",
                table: "AllocationHistory");

            migrationBuilder.AlterColumn<int>(
                name: "ADUsersId",
                table: "AllocationHistory",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "GroupId",
                table: "AllocationHistory",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AllocationHistory_GroupId",
                table: "AllocationHistory",
                column: "GroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_AllocationHistory_ADUsers_ADUsersId",
                table: "AllocationHistory",
                column: "ADUsersId",
                principalTable: "ADUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AllocationHistory_Group_GroupId",
                table: "AllocationHistory",
                column: "GroupId",
                principalTable: "Group",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AllocationHistory_ADUsers_ADUsersId",
                table: "AllocationHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_AllocationHistory_Group_GroupId",
                table: "AllocationHistory");

            migrationBuilder.DropIndex(
                name: "IX_AllocationHistory_GroupId",
                table: "AllocationHistory");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "AllocationHistory");

            migrationBuilder.AlterColumn<int>(
                name: "ADUsersId",
                table: "AllocationHistory",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AllocationHistory_ADUsers_ADUsersId",
                table: "AllocationHistory",
                column: "ADUsersId",
                principalTable: "ADUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
