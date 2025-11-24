using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RakipBul.Migrations
{
    /// <inheritdoc />
    public partial class CreatePanoramaEntriesTable22 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SeasonId",
                table: "PanoramaEntries",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SeasonId",
                table: "PanoramaEntries");
        }
    }
}
