using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RakipBul.Migrations
{
    /// <inheritdoc />
    public partial class AddEmbedCodeToVideoTotalViewghjgh : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UserDeviceToken");

            migrationBuilder.AddColumn<string>(
                name: "MacId",
                table: "UserDeviceToken",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MacId",
                table: "UserDeviceToken");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "UserDeviceToken",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
