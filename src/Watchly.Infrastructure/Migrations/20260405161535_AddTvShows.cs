using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Watchly.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTvShows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "created_by",
                table: "titles",
                type: "character varying(3000)",
                maxLength: 3000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "episode_run_time",
                table: "titles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "first_air_date",
                table: "titles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "in_production",
                table: "titles",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "last_air_date",
                table: "titles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "number_of_episodes",
                table: "titles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "number_of_seasons",
                table: "titles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "original_language",
                table: "titles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "original_name",
                table: "titles",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "titles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "type",
                table: "titles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "vote_average",
                table: "titles",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_tv_show",
                table: "title_spoken_languages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_tv_show",
                table: "title_production_companies",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_tv_show",
                table: "title_genres",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_tv_show",
                table: "keyword_titles",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_by",
                table: "titles");

            migrationBuilder.DropColumn(
                name: "episode_run_time",
                table: "titles");

            migrationBuilder.DropColumn(
                name: "first_air_date",
                table: "titles");

            migrationBuilder.DropColumn(
                name: "in_production",
                table: "titles");

            migrationBuilder.DropColumn(
                name: "last_air_date",
                table: "titles");

            migrationBuilder.DropColumn(
                name: "number_of_episodes",
                table: "titles");

            migrationBuilder.DropColumn(
                name: "number_of_seasons",
                table: "titles");

            migrationBuilder.DropColumn(
                name: "original_language",
                table: "titles");

            migrationBuilder.DropColumn(
                name: "original_name",
                table: "titles");

            migrationBuilder.DropColumn(
                name: "status",
                table: "titles");

            migrationBuilder.DropColumn(
                name: "type",
                table: "titles");

            migrationBuilder.DropColumn(
                name: "vote_average",
                table: "titles");

            migrationBuilder.DropColumn(
                name: "is_tv_show",
                table: "title_spoken_languages");

            migrationBuilder.DropColumn(
                name: "is_tv_show",
                table: "title_production_companies");

            migrationBuilder.DropColumn(
                name: "is_tv_show",
                table: "title_genres");

            migrationBuilder.DropColumn(
                name: "is_tv_show",
                table: "keyword_titles");
        }
    }
}
