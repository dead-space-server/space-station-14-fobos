using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    [DbContext(typeof(SqliteServerDbContext))]
    [Migration("20260520000000_RoundGameModeHistoryDetails")]
    public partial class RoundGameModeHistoryDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "map_name",
                table: "round",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "start_player_count",
                table: "round",
                type: "INTEGER",
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
