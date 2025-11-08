using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MisFinanzas.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLoansTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Loans",
                columns: table => new
                {
                    LoanId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    PrincipalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InstallmentAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NumberOfInstallments = table.Column<int>(type: "INTEGER", nullable: false),
                    DueDay = table.Column<int>(type: "INTEGER", nullable: false),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')"),
                    Icon = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false, defaultValue: "🏦"),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    InstallmentsPaid = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Loans", x => x.LoanId);
                    table.CheckConstraint("CK_Loan_DueDay", "DueDay >= 1 AND DueDay <= 31");
                    table.CheckConstraint("CK_Loan_InstallmentAmount", "InstallmentAmount > 0");
                    table.CheckConstraint("CK_Loan_InstallmentsPaid", "InstallmentsPaid >= 0");
                    table.CheckConstraint("CK_Loan_NumberOfInstallments", "NumberOfInstallments >= 1 AND NumberOfInstallments <= 1000");
                    table.CheckConstraint("CK_Loan_PrincipalAmount", "PrincipalAmount > 0");
                    table.ForeignKey(
                        name: "FK_Loans_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Loans_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-550e8400-e29b-41d4-a716-446655440000",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "408b15dc-ed03-4521-ab5b-3ff37fd1fb27", "bee75674-bd29-4b0e-93bb-b42241b7a002" });

            migrationBuilder.CreateIndex(
                name: "IX_Loans_CategoryId",
                table: "Loans",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Loans_UserId",
                table: "Loans",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Loans_UserId_IsActive",
                table: "Loans",
                columns: new[] { "UserId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Loans");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-550e8400-e29b-41d4-a716-446655440000",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "ec3f33ab-fc35-42b5-a806-8392905ac0f1", "5cd9f734-fa52-4fec-a743-25dd3de23011" });
        }
    }
}
