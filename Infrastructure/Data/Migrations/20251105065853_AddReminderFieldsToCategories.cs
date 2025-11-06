using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MisFinanzas.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReminderFieldsToCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DayOfMonth",
                table: "Categories",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "EstimatedAmount",
                table: "Categories",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFixedExpense",
                table: "Categories",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false),
                    NotificationDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsRead = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('now')")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_Notifications_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CategoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
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
                values: new object[] { "ec3f33ab-fc35-42b5-a806-8392905ac0f1", "5cd9f734-fa52-4fec-a743-25dd3de23011" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CategoryId",
                table: "Notifications",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_DueDate",
                table: "Notifications",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropColumn(
                name: "DayOfMonth",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "EstimatedAmount",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "IsFixedExpense",
                table: "Categories");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-550e8400-e29b-41d4-a716-446655440000",
                columns: new[] { "ConcurrencyStamp", "SecurityStamp" },
                values: new object[] { "3c36277d-fef5-4b4c-a991-22b0db55fd1b", "524e6bed-4463-4b24-817d-6bd201afc36d" });
        }
    }
}
