using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RakipBul.Migrations
{
    /// <inheritdoc />
    public partial class AddNameAndImageToFavourites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TeamImageUrl",
                table: "FavouriteTeams",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeamName",
                table: "FavouriteTeams",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlayerImageUrl",
                table: "FavouritePlayers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlayerName",
                table: "FavouritePlayers",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TeamImageUrl",
                table: "FavouriteTeams");

            migrationBuilder.DropColumn(
                name: "TeamName",
                table: "FavouriteTeams");

            migrationBuilder.DropColumn(
                name: "PlayerImageUrl",
                table: "FavouritePlayers");

            migrationBuilder.DropColumn(
                name: "PlayerName",
                table: "FavouritePlayers");
        }
    }
}
