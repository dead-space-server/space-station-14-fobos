using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class AddWhitelistModes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "game_preset_config",
                columns: table => new
                {
                    server_id = table.Column<string>(maxLength: 128, nullable: false),
                    enabled = table.Column<bool>(nullable: false),
                    max_rdm_row = table.Column<int>(nullable: false),
                    vote_duration_seconds = table.Column<int>(nullable: false, defaultValue: 30),
                    current_preset_index = table.Column<int>(nullable: false),
                    active_preset_ids_json = table.Column<string>(nullable: false),
                    custom_presets_json = table.Column<string>(nullable: false),
                    disable_ooc_during_vote = table.Column<bool>(nullable: false, defaultValue: false),
                    prevent_repeat_mode = table.Column<bool>(nullable: false, defaultValue: false),
                    check_player_limit = table.Column<bool>(nullable: false, defaultValue: false),
                    whitelist_modes_json = table.Column<string>(nullable: false, defaultValue: "[]")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_preset_config", x => x.server_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "game_preset_config");
        }
    }
}
