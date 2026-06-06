using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Watchly.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWatchCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "watch_count",
                table: "user_content_activities",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "watch_count",
                table: "user_content_activities");
        }
    }
}
