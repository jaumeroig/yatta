using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveWorkdaySlots : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkdaySlots");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WorkdaySlots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Date = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    Telework = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkdaySlots", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkdaySlots_Date",
                table: "WorkdaySlots",
                column: "Date");
        }
    }
}
