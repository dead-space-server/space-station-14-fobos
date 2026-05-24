using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Postgres
{
    /// <inheritdoc />
    [DbContext(typeof(PostgresServerDbContext))]
    [Migration("20260524000001_AutoMapVotePoolState")]
    public partial class AutoMapVotePoolState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "small_played_maps",
                table: "auto_map_vote_config",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "medium_played_maps",
                table: "auto_map_vote_config",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "large_played_maps",
                table: "auto_map_vote_config",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "small_pool_queue_maps",
                table: "auto_map_vote_config",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "medium_pool_queue_maps",
                table: "auto_map_vote_config",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "large_pool_queue_maps",
                table: "auto_map_vote_config",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "large_pool_queue_maps",
                table: "auto_map_vote_config");

            migrationBuilder.DropColumn(
                name: "medium_pool_queue_maps",
                table: "auto_map_vote_config");

            migrationBuilder.DropColumn(
                name: "small_pool_queue_maps",
                table: "auto_map_vote_config");

            migrationBuilder.DropColumn(
                name: "large_played_maps",
                table: "auto_map_vote_config");

            migrationBuilder.DropColumn(
                name: "medium_played_maps",
                table: "auto_map_vote_config");

            migrationBuilder.DropColumn(
                name: "small_played_maps",
                table: "auto_map_vote_config");
        }
    }
}
