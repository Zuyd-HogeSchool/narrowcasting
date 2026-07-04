using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Narrowcasting.Data.Migrations
{
    /// <inheritdoc />
    public partial class Identitycodegenrator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "ROLE-ADMIN-0001", null, "Admin", "ADMIN" },
                    { "ROLE-EMP-0001", null, "Employee", "EMPLOYEE" }
                });

            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Algemene wachtkamer begane grond", "Wachtkamer" },
                    { 2, "Radiologie & beeldvorming", "Radiologie" },
                    { 3, "Ziekenhuisapotheek", "Apotheek" }
                });

            migrationBuilder.InsertData(
                table: "Screens",
                columns: new[] { "Id", "DepartmentId", "IsActive", "IsStaffScreen", "Location", "Name" },
                values: new object[,]
                {
                    { 1, 1, true, false, "Wachtkamer Begane Grond", "Scherm Wachtkamer A" },
                    { 2, 2, true, false, "Radiologie Gang", "Scherm Radiologie" },
                    { 3, 3, true, false, "Apotheek Balie", "Scherm Apotheek" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "ROLE-ADMIN-0001");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "ROLE-EMP-0001");

            migrationBuilder.DeleteData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Screens",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
