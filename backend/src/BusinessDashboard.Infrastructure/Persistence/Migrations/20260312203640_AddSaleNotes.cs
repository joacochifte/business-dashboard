using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessDashboard.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSaleNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Sales",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Sales");
        }
    }
}
