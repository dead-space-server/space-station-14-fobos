using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Content.Server.Database.Migrations.Postgres
{
    [DbContext(typeof(PostgresServerDbContext))]
    [Migration("20260511120000_BackfillChatToggleAdminFlag")]
    public class BackfillChatToggleAdminFlag : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
INSERT INTO admin_rank_flag (flag, admin_rank_id)
SELECT 'CHATTOGGLE', server_flags.admin_rank_id
FROM admin_rank_flag AS server_flags
WHERE server_flags.flag = 'SERVER'
  AND NOT EXISTS (
      SELECT 1
      FROM admin_rank_flag AS chat_flags
      WHERE chat_flags.admin_rank_id = server_flags.admin_rank_id
        AND chat_flags.flag = 'CHATTOGGLE'
  );
""");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM admin_rank_flag WHERE flag = 'CHATTOGGLE';");
        }
    }
}
