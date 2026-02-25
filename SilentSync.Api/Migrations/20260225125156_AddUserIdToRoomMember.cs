using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SilentSync.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToRoomMember : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "RoomMembers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_RoomMembers_RoomId_UserId",
                table: "RoomMembers",
                columns: new[] { "RoomId", "UserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RoomMembers_RoomId_UserId",
                table: "RoomMembers");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "RoomMembers");
        }
    }
}
