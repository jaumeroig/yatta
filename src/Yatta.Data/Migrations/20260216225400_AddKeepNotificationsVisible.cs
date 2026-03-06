using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yatta.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddKeepNotificationsVisible : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "KeepNotificationsVisible",
                table: "AppSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KeepNotificationsVisible",
                table: "AppSettings");
        }
    }
}
