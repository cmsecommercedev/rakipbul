using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RakipBul.Migrations
{
    /// <inheritdoc />
    public partial class AddVideoTotalView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VideoLikes");

            migrationBuilder.DropTable(
                name: "VideoStats");

            migrationBuilder.CreateTable(
                name: "VideoTotalView",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VideoId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalViews = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoTotalView", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VideoTotalView");

            migrationBuilder.CreateTable(
                name: "VideoLikes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    VideoId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoLikes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VideoStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VideoId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ViewCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VideoStats", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VideoLikes_VideoId_UserId",
                table: "VideoLikes",
                columns: new[] { "VideoId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VideoStats_VideoId",
                table: "VideoStats",
                column: "VideoId",
                unique: true);
        }
    }
}
