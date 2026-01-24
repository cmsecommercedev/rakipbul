using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RakipBul.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyTeamSquadImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "TeamSquadImages");

            migrationBuilder.DropColumn(
                name: "ImageType",
                table: "TeamSquadImages");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "TeamSquadImages");

            migrationBuilder.DropColumn(
                name: "LeagueId",
                table: "TeamSquadImages");

            migrationBuilder.DropColumn(
                name: "MatchId",
                table: "TeamSquadImages");

            migrationBuilder.DropColumn(
                name: "SeasonId",
                table: "TeamSquadImages");

            migrationBuilder.DropColumn(
                name: "TeamName",
                table: "TeamSquadImages");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TeamSquadImages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "TeamSquadImages",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageType",
                table: "TeamSquadImages",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "TeamSquadImages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LeagueId",
                table: "TeamSquadImages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MatchId",
                table: "TeamSquadImages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SeasonId",
                table: "TeamSquadImages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeamName",
                table: "TeamSquadImages",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TeamSquadImages",
                type: "datetime2",
                nullable: true);
        }
    }
}
