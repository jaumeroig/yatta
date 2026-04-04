using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yatta.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultWorkingDaysMask : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DefaultWorkingDaysMask",
                table: "AppSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 31);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultWorkingDaysMask",
                table: "AppSettings");
        }
    }
}
