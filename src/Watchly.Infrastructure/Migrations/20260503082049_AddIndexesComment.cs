using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Watchly.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_comments_episode_id",
                table: "comments");

            migrationBuilder.DropIndex(
                name: "ix_comments_title_id",
                table: "comments");

            migrationBuilder.CreateIndex(
                name: "ix_comments_episode_id",
                table: "comments",
                column: "episode_id");

            migrationBuilder.CreateIndex(
                name: "ix_comments_title_id",
                table: "comments",
                column: "title_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_comments_episode_id",
                table: "comments");

            migrationBuilder.DropIndex(
                name: "ix_comments_title_id",
                table: "comments");

            migrationBuilder.CreateIndex(
                name: "ix_comments_episode_id",
                table: "comments",
                column: "episode_id");

            migrationBuilder.CreateIndex(
                name: "ix_comments_title_id",
                table: "comments",
                column: "title_id");
        }
    }
}
