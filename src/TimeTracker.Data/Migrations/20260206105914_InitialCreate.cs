using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Activities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Color = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Theme = table.Column<string>(type: "TEXT", nullable: false),
                    Notifications = table.Column<bool>(type: "INTEGER", nullable: false),
                    WorkdayTotalTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    Language = table.Column<string>(type: "TEXT", nullable: true),
                    MinimizeToTray = table.Column<bool>(type: "INTEGER", nullable: false),
                    NotificationIntervalMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    StartWithWindows = table.Column<bool>(type: "INTEGER", nullable: false),
                    RetentionPolicy = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    CustomRetentionDays = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 365)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TimeRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "TEXT", nullable: true),
                    ActivityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Telework = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Workdays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    DayType = table.Column<int>(type: "INTEGER", nullable: false),
                    TargetDuration = table.Column<TimeSpan>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workdays", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkdaySlots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    Telework = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkdaySlots", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_Name",
                table: "Activities",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimeRecords_Date_StartTime",
                table: "TimeRecords",
                columns: new[] { "Date", "StartTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Workdays_Date",
                table: "Workdays",
                column: "Date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkdaySlots_Date",
                table: "WorkdaySlots",
                column: "Date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Activities");

            migrationBuilder.DropTable(
                name: "AppSettings");

            migrationBuilder.DropTable(
                name: "TimeRecords");

            migrationBuilder.DropTable(
                name: "Workdays");

            migrationBuilder.DropTable(
                name: "WorkdaySlots");
        }
    }
}
