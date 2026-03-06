using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SilentSync.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_RoomMembers_UserId",
                table: "RoomMembers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_RoomMembers_Users_UserId",
                table: "RoomMembers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RoomMembers_Users_UserId",
                table: "RoomMembers");

            migrationBuilder.DropIndex(
                name: "IX_RoomMembers_UserId",
                table: "RoomMembers");
        }
    }
}
