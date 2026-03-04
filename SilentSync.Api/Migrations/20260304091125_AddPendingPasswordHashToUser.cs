using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SilentSync.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingPasswordHashToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LoginCodes_Email_Code",
                table: "LoginCodes");

            migrationBuilder.AddColumn<string>(
                name: "PendingPasswordHash",
                table: "Users",
                type: "character varying(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LoginCodes_Email_Code",
                table: "LoginCodes",
                columns: new[] { "Email", "Code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LoginCodes_Email_Code",
                table: "LoginCodes");

            migrationBuilder.DropColumn(
                name: "PendingPasswordHash",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_LoginCodes_Email_Code",
                table: "LoginCodes",
                columns: new[] { "Email", "Code" });
        }
    }
}
