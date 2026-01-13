using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RakipBul.Migrations
{
    /// <inheritdoc />
    public partial class CreatePanoramaEntriesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PanoramaEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    YoutubeEmbedLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlayerId = table.Column<int>(type: "int", nullable: true),
                    PlayerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlayerImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PlayerPosition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TeamId = table.Column<int>(type: "int", nullable: true),
                    TeamName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TeamImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LeagueId = table.Column<int>(type: "int", nullable: true),
                    LeagueName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProvinceName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PanoramaEntries", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PanoramaEntries");
        }
    }
}
