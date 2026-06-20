using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yatta.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReminderInterval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReminderInterval",
                table: "AppSettings",
                type: "TEXT",
                nullable: false,
                defaultValue: "Hours2");

            // Migrate existing NotificationIntervalMinutes values to the new ReminderInterval preset.
            // Known presets map to their enum; any other value is treated as Custom so the user's
            // custom minutes are preserved in NotificationIntervalMinutes.
            migrationBuilder.Sql(@"
UPDATE AppSettings
SET ReminderInterval = CASE
    WHEN NotificationIntervalMinutes = 5 THEN 'Minutes5'
    WHEN NotificationIntervalMinutes = 10 THEN 'Minutes10'
    WHEN NotificationIntervalMinutes = 30 THEN 'Minutes30'
    WHEN NotificationIntervalMinutes = 60 THEN 'Hour1'
    WHEN NotificationIntervalMinutes = 120 THEN 'Hours2'
    ELSE 'Custom'
END;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReminderInterval",
                table: "AppSettings");
        }
    }
}
