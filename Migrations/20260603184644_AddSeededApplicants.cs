using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CareerHub.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSeededApplicants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "applicants",
                columns: new[] { "Id", "Email", "FullName" },
                values: new object[,]
                {
                    { new Guid("a1111111-1111-1111-1111-111111111111"), "applicantA@test.com", "Applicant A" },
                    { new Guid("b2222222-2222-2222-2222-222222222222"), "applicantB@test.com", "Applicant B" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "applicants",
                keyColumn: "Id",
                keyValue: new Guid("a1111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "applicants",
                keyColumn: "Id",
                keyValue: new Guid("b2222222-2222-2222-2222-222222222222"));
        }
    }
}
