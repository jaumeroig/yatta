using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Yatta.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAnnualQuota : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnnualQuotas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    VacationDays = table.Column<int>(type: "INTEGER", nullable: false),
                    FreeChoiceDays = table.Column<int>(type: "INTEGER", nullable: false),
                    IntensiveDays = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnnualQuotas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnnualQuotas_Year",
                table: "AnnualQuotas",
                column: "Year",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnnualQuotas");
        }
    }
}
