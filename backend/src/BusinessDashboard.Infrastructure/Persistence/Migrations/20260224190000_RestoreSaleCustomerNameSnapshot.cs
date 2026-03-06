using BusinessDashboard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BusinessDashboard.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260224190000_RestoreSaleCustomerNameSnapshot")]
public partial class RestoreSaleCustomerNameSnapshot : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "CustomerName",
            table: "Sales",
            type: "character varying(200)",
            maxLength: 200,
            nullable: true);

        migrationBuilder.Sql("""
            UPDATE "Sales" AS s
            SET "CustomerName" = c."Name"
            FROM "Customers" AS c
            WHERE s."CustomerId" = c."Id"
              AND (s."CustomerName" IS NULL OR s."CustomerName" = '');
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "CustomerName",
            table: "Sales");
    }
}
