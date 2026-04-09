using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yatta.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeRecordLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Link",
                table: "TimeRecords",
                type: "TEXT",
                maxLength: 2048,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Link",
                table: "TimeRecords");
        }
    }
}
