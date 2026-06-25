using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Postgres
{
    /// <inheritdoc />
    [DbContext(typeof(PostgresServerDbContext))]
    [Migration("20260520000000_RoundGameModeHistoryDetails")]
    public partial class RoundGameModeHistoryDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "map_name",
                table: "round",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "start_player_count",
                table: "round",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "map_name",
                table: "round");

            migrationBuilder.DropColumn(
                name: "start_player_count",
                table: "round");
        }
    }
}
