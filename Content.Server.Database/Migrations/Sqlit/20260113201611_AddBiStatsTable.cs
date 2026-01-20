using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlit
{
    /// <inheritdoc />
    public partial class AddBiStatsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bi_stats",
                columns: table => new
                {
                    bi_stats_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    game_mode = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    winner = table.Column<byte>(type: "INTEGER", nullable: false),
                    date = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bi_stats", x => x.bi_stats_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bi_stats_date",
                table: "bi_stats",
                column: "date");

            migrationBuilder.CreateIndex(
                name: "IX_bi_stats_game_mode",
                table: "bi_stats",
                column: "game_mode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bi_stats");
        }
    }
}
