using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yatta.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceMinutes5WithMinutes15 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE AppSettings
                SET ReminderInterval = 'Minutes15', NotificationIntervalMinutes = 15
                WHERE ReminderInterval = 'Minutes5';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE AppSettings
                SET ReminderInterval = 'Minutes5', NotificationIntervalMinutes = 5
                WHERE ReminderInterval = 'Minutes15';
                """);
        }
    }
}
