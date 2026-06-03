using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CareerHub.Api.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialJobsAndCompanies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "companies",
                columns: new[] { "Id", "Name", "Website" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "TechCorp", "techcorp.com" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "FinanceFlow", "financeflow.com" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "HealthNet", "healthnet.com" },
                    { new Guid("44444444-4444-4444-4444-444444444444"), "EduBuild", "edubuild.com" },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "LogiRoute", "logiroute.com" }
                });

            migrationBuilder.InsertData(
                table: "job_listings",
                columns: new[] { "Id", "CompanyId", "Description", "IsActive", "Location", "PostedAt", "Title", "Type" },
                values: new object[,]
                {
                    { new Guid("91111111-1111-1111-1111-111111111111"), new Guid("11111111-1111-1111-1111-111111111111"), "C# Engineer needed", true, "Remote", new DateTime(2026, 6, 2, 22, 0, 0, 0, DateTimeKind.Utc), "Backend Developer", 0 },
                    { new Guid("92222222-2222-2222-2222-222222222222"), new Guid("22222222-2222-2222-2222-222222222222"), "SQL expert needed", true, "Cape Town", new DateTime(2026, 6, 2, 22, 0, 0, 0, DateTimeKind.Utc), "Data Analyst", 0 },
                    { new Guid("93333333-3333-3333-3333-333333333333"), new Guid("33333333-3333-3333-3333-333333333333"), "Docker expert", true, "Remote", new DateTime(2026, 6, 2, 22, 0, 0, 0, DateTimeKind.Utc), "DevOps Specialist", 0 },
                    { new Guid("94444444-4444-4444-4444-444444444444"), new Guid("44444444-4444-4444-4444-444444444444"), "React components", true, "Johannesburg", new DateTime(2026, 6, 2, 22, 0, 0, 0, DateTimeKind.Utc), "Frontend Developer", 0 },
                    { new Guid("95555555-5555-5555-5555-555555555555"), new Guid("55555555-5555-5555-5555-555555555555"), "AWS infra design", true, "Remote", new DateTime(2026, 6, 2, 22, 0, 0, 0, DateTimeKind.Utc), "Cloud Architect", 0 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "job_listings",
                keyColumn: "Id",
                keyValue: new Guid("91111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "job_listings",
                keyColumn: "Id",
                keyValue: new Guid("92222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "job_listings",
                keyColumn: "Id",
                keyValue: new Guid("93333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "job_listings",
                keyColumn: "Id",
                keyValue: new Guid("94444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "job_listings",
                keyColumn: "Id",
                keyValue: new Guid("95555555-5555-5555-5555-555555555555"));

            migrationBuilder.DeleteData(
                table: "companies",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "companies",
                keyColumn: "Id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "companies",
                keyColumn: "Id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "companies",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "companies",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"));
        }
    }
}
