using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Watchly.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddActorsDirectorPicture : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "actors",
                table: "titles",
                type: "character varying(3000)",
                maxLength: 3000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "director",
                table: "titles",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "localization_languages",
                table: "titles",
                type: "character varying(1500)",
                maxLength: 1500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "profile_picture_url",
                table: "asp_net_users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_adult",
                table: "titles",
                type: "boolean",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "actors",
                table: "titles");

            migrationBuilder.DropColumn(
                name: "director",
                table: "titles");

            migrationBuilder.DropColumn(
                name: "localization_languages",
                table: "titles");

            migrationBuilder.DropColumn(
                name: "profile_picture_url",
                table: "asp_net_users");

            migrationBuilder.DropColumn(
                name: "is_adult",
                table: "titles");
        }
    }
}
