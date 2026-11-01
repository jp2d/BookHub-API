using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CaseInsensitiveUniqueGenreName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_genres_Name",
                table: "genres");

            // Expression index (not expressible in the EF model): "Terror" and "TERROR" are the same genre.
            migrationBuilder.Sql("""CREATE UNIQUE INDEX "IX_genres_Name_ci" ON genres (lower("Name"));""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP INDEX "IX_genres_Name_ci";""");

            migrationBuilder.CreateIndex(
                name: "IX_genres_Name",
                table: "genres",
                column: "Name",
                unique: true);
        }
    }
}
