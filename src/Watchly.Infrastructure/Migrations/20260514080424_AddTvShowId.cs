using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Watchly.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTvShowId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "tv_show_id",
                table: "episodes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_episodes_tv_show_id",
                table: "episodes",
                column: "tv_show_id");

            migrationBuilder.AddForeignKey(
                name: "fk_episodes_titles_tv_show_id",
                table: "episodes",
                column: "tv_show_id",
                principalTable: "titles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_episodes_titles_tv_show_id",
                table: "episodes");

            migrationBuilder.DropIndex(
                name: "ix_episodes_tv_show_id",
                table: "episodes");

            migrationBuilder.DropColumn(
                name: "tv_show_id",
                table: "episodes");
        }
    }
}
