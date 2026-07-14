using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class UserIdLoginMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_id_login_migrations",
                columns: table => new
                {
                    old_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    new_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_id_login_migrations", x => new { x.old_user_id, x.new_user_id });
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_id_login_migrations_new_user_id",
                table: "user_id_login_migrations",
                column: "new_user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_id_login_migrations_old_user_id",
                table: "user_id_login_migrations",
                column: "old_user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_id_login_migrations");
        }
    }
}
