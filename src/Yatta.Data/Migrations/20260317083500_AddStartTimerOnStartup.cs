using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yatta.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStartTimerOnStartup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "StartTimerOnStartup",
                table: "AppSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartTimerOnStartup",
                table: "AppSettings");
        }
    }
}
