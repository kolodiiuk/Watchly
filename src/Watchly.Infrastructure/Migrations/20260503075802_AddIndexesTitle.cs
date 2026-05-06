using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Watchly.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_title_spoken_languages_spoken_language_id",
                table: "title_spoken_languages");

            migrationBuilder.DropIndex(
                name: "ix_title_spoken_languages_title_id",
                table: "title_spoken_languages");

            migrationBuilder.DropIndex(
                name: "ix_title_production_companies_production_company_id",
                table: "title_production_companies");

            migrationBuilder.DropIndex(
                name: "ix_title_production_companies_title_id",
                table: "title_production_companies");

            migrationBuilder.DropIndex(
                name: "ix_title_genres_genre_id",
                table: "title_genres");

            migrationBuilder.DropIndex(
                name: "ix_title_genres_title_id",
                table: "title_genres");

            migrationBuilder.DropIndex(
                name: "ix_keyword_titles_keyword_id",
                table: "keyword_titles");

            migrationBuilder.DropIndex(
                name: "ix_keyword_titles_title_id",
                table: "keyword_titles");

            migrationBuilder.CreateIndex(
                name: "IX_title_spoken_languages_spoken_language_id_title_id",
                table: "title_spoken_languages",
                columns: new[] { "spoken_language_id", "title_id" });

            migrationBuilder.CreateIndex(
                name: "IX_title_spoken_languages_title_id_spoken_language_id",
                table: "title_spoken_languages",
                columns: new[] { "title_id", "spoken_language_id" });

            migrationBuilder.CreateIndex(
                name: "IX_title_production_companies_production_company_id_title_id",
                table: "title_production_companies",
                columns: new[] { "production_company_id", "title_id" });

            migrationBuilder.CreateIndex(
                name: "IX_title_production_companies_title_id_production_company_id",
                table: "title_production_companies",
                columns: new[] { "title_id", "production_company_id" });

            migrationBuilder.CreateIndex(
                name: "IX_title_genres_genre_id_title_id",
                table: "title_genres",
                columns: new[] { "genre_id", "title_id" });

            migrationBuilder.CreateIndex(
                name: "IX_title_genres_title_id_genre_id",
                table: "title_genres",
                columns: new[] { "title_id", "genre_id" });
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
            migrationBuilder.CreateIndex(
                name: "IX_keywords_name",
                table: "keywords",
                column: "name")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "IX_keyword_titles_keyword_id_title_id",
                table: "keyword_titles",
                columns: new[] { "keyword_id", "title_id" });

            migrationBuilder.CreateIndex(
                name: "IX_keyword_titles_title_id_keyword_id",
                table: "keyword_titles",
                columns: new[] { "title_id", "keyword_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_title_spoken_languages_spoken_language_id_title_id",
                table: "title_spoken_languages");

            migrationBuilder.DropIndex(
                name: "IX_title_spoken_languages_title_id_spoken_language_id",
                table: "title_spoken_languages");

            migrationBuilder.DropIndex(
                name: "IX_title_production_companies_production_company_id_title_id",
                table: "title_production_companies");

            migrationBuilder.DropIndex(
                name: "IX_title_production_companies_title_id_production_company_id",
                table: "title_production_companies");

            migrationBuilder.DropIndex(
                name: "IX_title_genres_genre_id_title_id",
                table: "title_genres");

            migrationBuilder.DropIndex(
                name: "IX_title_genres_title_id_genre_id",
                table: "title_genres");

            migrationBuilder.DropIndex(
                name: "IX_keywords_name",
                table: "keywords");

            migrationBuilder.DropIndex(
                name: "IX_keyword_titles_keyword_id_title_id",
                table: "keyword_titles");

            migrationBuilder.DropIndex(
                name: "IX_keyword_titles_title_id_keyword_id",
                table: "keyword_titles");

            migrationBuilder.CreateIndex(
                name: "ix_title_spoken_languages_spoken_language_id",
                table: "title_spoken_languages",
                column: "spoken_language_id");

            migrationBuilder.CreateIndex(
                name: "ix_title_spoken_languages_title_id",
                table: "title_spoken_languages",
                column: "title_id");

            migrationBuilder.CreateIndex(
                name: "ix_title_production_companies_production_company_id",
                table: "title_production_companies",
                column: "production_company_id");

            migrationBuilder.CreateIndex(
                name: "ix_title_production_companies_title_id",
                table: "title_production_companies",
                column: "title_id");

            migrationBuilder.CreateIndex(
                name: "ix_title_genres_genre_id",
                table: "title_genres",
                column: "genre_id");

            migrationBuilder.CreateIndex(
                name: "ix_title_genres_title_id",
                table: "title_genres",
                column: "title_id");

            migrationBuilder.CreateIndex(
                name: "ix_keyword_titles_keyword_id",
                table: "keyword_titles",
                column: "keyword_id");

            migrationBuilder.CreateIndex(
                name: "ix_keyword_titles_title_id",
                table: "keyword_titles",
                column: "title_id");
        }
    }
}
