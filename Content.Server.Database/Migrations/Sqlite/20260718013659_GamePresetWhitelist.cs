using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class GamePresetWhitelist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "max_rdm_day",
                table: "game_preset_config");

            migrationBuilder.AddColumn<bool>(
                name: "prevent_repeat_mode",
                table: "game_preset_config",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "check_player_limit",
                table: "game_preset_config",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "whitelist_modes_json",
                table: "game_preset_config",
                type: "TEXT",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "whitelist_modes_json",
                table: "game_preset_config");

            migrationBuilder.DropColumn(
                name: "check_player_limit",
                table: "game_preset_config");

            migrationBuilder.DropColumn(
                name: "prevent_repeat_mode",
                table: "game_preset_config");

            migrationBuilder.AddColumn<int>(
                name: "max_rdm_day",
                table: "game_preset_config",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
