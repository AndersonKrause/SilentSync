using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SilentSync.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomOwnerAndProtectDeleteFlows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OwnerId",
                table: "Rooms",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_OwnerId",
                table: "Rooms",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Users_OwnerId",
                table: "Rooms",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Users_OwnerId",
                table: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_OwnerId",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Rooms");
        }
    }
}
