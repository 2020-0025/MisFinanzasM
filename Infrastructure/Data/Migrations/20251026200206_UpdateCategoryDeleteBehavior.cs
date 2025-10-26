using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MisFinanzas.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCategoryDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExpensesIncomes_Categories_CategoryId",
                table: "ExpensesIncomes");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-550e8400-e29b-41d4-a716-446655440000",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "e0741497-d2b7-42da-81e3-3dd5cab6f5f2", "2566f19f-10fb-4bba-bf33-5e512ac55c37" });

            migrationBuilder.AddForeignKey(
                name: "FK_ExpensesIncomes_Categories_CategoryId",
                table: "ExpensesIncomes",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ExpensesIncomes_Categories_CategoryId",
                table: "ExpensesIncomes");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-550e8400-e29b-41d4-a716-446655440000",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "2f46c249-a361-4f21-ac8f-77fb87dc4a69", "041fbd77-e20c-44a6-a25b-c33eff6d7611" });

            migrationBuilder.AddForeignKey(
                name: "FK_ExpensesIncomes_Categories_CategoryId",
                table: "ExpensesIncomes",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
