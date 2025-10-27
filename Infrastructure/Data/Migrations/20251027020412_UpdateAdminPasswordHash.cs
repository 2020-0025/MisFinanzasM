using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MisFinanzas.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdminPasswordHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-550e8400-e29b-41d4-a716-446655440000",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "3c36277d-fef5-4b4c-a991-22b0db55fd1b", "AQAAAAIAAYagAAAAEFtEOmQZoktPuyAmR2lQn+NXQ7SqGeemT34tl3d2FFIltnPO6o587scWhKK3G9PmSw==", "524e6bed-4463-4b24-817d-6bd201afc36d" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: "admin-550e8400-e29b-41d4-a716-446655440000",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "e0741497-d2b7-42da-81e3-3dd5cab6f5f2", "Admin123", "2566f19f-10fb-4bba-bf33-5e512ac55c37" });
        }
    }
}
