using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessDashboard.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDebtAndSpecialPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDebt",
                table: "Sales",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "SpecialPrice",
                table: "SaleItems",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "Notifications",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDebt",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "SpecialPrice",
                table: "SaleItems");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Notifications");
        }
    }
}
