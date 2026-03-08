using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Watchly.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWatchTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "title_id",
                table: "votes",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "episode_id",
                table: "votes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                table: "votes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "content_type",
                table: "titles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "title_id",
                table: "comments",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "episode_id",
                table: "comments",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "comments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "user_content_activities",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    content_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    content_type = table.Column<int>(type: "integer", nullable: false),
                    activity_type = table.Column<int>(type: "integer", nullable: false),
                    watched_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_content_activities", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_content_activities_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "user_title_progresses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_title_progresses", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_title_progresses_asp_net_users_user_id",
                        column: x => x.user_id,
                        principalTable: "asp_net_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_user_title_progresses_titles_title_id",
                        column: x => x.title_id,
                        principalTable: "titles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_votes_episode_id",
                table: "votes",
                column: "episode_id");

            migrationBuilder.CreateIndex(
                name: "IX_votes_user_id",
                table: "votes",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_content_activities_user_id",
                table: "user_content_activities",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_title_progresses_title_id",
                table: "user_title_progresses",
                column: "title_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_title_progresses_user_id",
                table: "user_title_progresses",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "FK_votes_asp_net_users_user_id",
                table: "votes",
                column: "user_id",
                principalTable: "asp_net_users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_votes_episodes_episode_id",
                table: "votes",
                column: "episode_id",
                principalTable: "episodes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_votes_asp_net_users_user_id",
                table: "votes");

            migrationBuilder.DropForeignKey(
                name: "fk_votes_episodes_episode_id",
                table: "votes");

            migrationBuilder.DropTable(
                name: "user_content_activities");

            migrationBuilder.DropTable(
                name: "user_title_progresses");

            migrationBuilder.DropIndex(
                name: "ix_votes_episode_id",
                table: "votes");

            migrationBuilder.DropIndex(
                name: "IX_votes_user_id",
                table: "votes");

            migrationBuilder.DropColumn(
                name: "episode_id",
                table: "votes");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "votes");

            migrationBuilder.DropColumn(
                name: "content_type",
                table: "titles");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "comments");

            migrationBuilder.AlterColumn<int>(
                name: "title_id",
                table: "votes",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "title_id",
                table: "comments",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "episode_id",
                table: "comments",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
