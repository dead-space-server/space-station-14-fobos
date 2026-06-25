using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    [DbContext(typeof(SqliteServerDbContext))]
    [Migration("20260619000000_GamePresetConfig")]
    public partial class GamePresetConfig : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "game_preset_config",
                columns: table => new
                {
                    server_id = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    max_rdm_row = table.Column<int>(type: "INTEGER", nullable: false),
                    max_rdm_day = table.Column<int>(type: "INTEGER", nullable: false),
                    vote_duration_seconds = table.Column<int>(type: "INTEGER", nullable: false),
                    current_preset_index = table.Column<int>(type: "INTEGER", nullable: false),
                    active_preset_ids_json = table.Column<string>(type: "TEXT", nullable: false),
                    custom_presets_json = table.Column<string>(type: "TEXT", nullable: false),
                    disable_ooc_during_vote = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_preset_config", x => x.server_id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "game_preset_config");
        }
    }
}