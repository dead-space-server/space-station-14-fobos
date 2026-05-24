using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    [DbContext(typeof(SqliteServerDbContext))]
    [Migration("20260524000000_AutoMapVoteConfig")]
    public partial class AutoMapVoteConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "auto_map_vote_config",
                columns: table => new
                {
                    server_id = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    small_max_players = table.Column<int>(type: "INTEGER", nullable: false),
                    medium_max_players = table.Column<int>(type: "INTEGER", nullable: false),
                    large_max_players = table.Column<int>(type: "INTEGER", nullable: false),
                    small_maps = table.Column<string>(type: "TEXT", nullable: false),
                    medium_maps = table.Column<string>(type: "TEXT", nullable: false),
                    large_maps = table.Column<string>(type: "TEXT", nullable: false),
                    blacklist_maps = table.Column<string>(type: "TEXT", nullable: false),
                    vote_duration_seconds = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auto_map_vote_config", x => x.server_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "auto_map_vote_config");
        }
    }
}
