using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CareerHub.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixedForeignKeysBaseline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "applicants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_applicants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "companies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Website = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "job_listings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    PostedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_listings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_job_listings_companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    JobListingId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApplicantId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => new { x.JobListingId, x.ApplicantId });
                    table.ForeignKey(
                        name: "FK_Applications_applicants_ApplicantId",
                        column: x => x.ApplicantId,
                        principalTable: "applicants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Applications_job_listings_JobListingId",
                        column: x => x.JobListingId,
                        principalTable: "job_listings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "applicants",
                columns: new[] { "Id", "Email", "FullName" },
                values: new object[,]
                {
                    { new Guid("a1111111-1111-1111-1111-111111111111"), "applicantA@test.com", "Applicant A" },
                    { new Guid("b2222222-2222-2222-2222-222222222222"), "applicantB@test.com", "Applicant B" }
                });

            migrationBuilder.InsertData(
                table: "companies",
                columns: new[] { "Id", "Name", "Website" },
                values: new object[,]
                {
                    { new Guid("2d5d8e24-9b16-4d2a-89a1-fbf22d4f5c92"), "FinanceFlow", "financeflow.com" },
                    { new Guid("75ba7d3e-2b50-4841-860e-cbfb4e54e4df"), "TechCorp", "techcorp.com" },
                    { new Guid("a43fa893-7c3e-4b72-ac2b-923fca8565b3"), "HealthNet", "healthnet.com" },
                    { new Guid("b11c34ef-56d1-419a-9cb8-b2a1aefb23d4"), "EduBuild", "edubuild.com" },
                    { new Guid("e8fa4292-1a4b-4b11-bdc1-42cb9fa234fe"), "LogiRoute", "logiroute.com" }
                });

            migrationBuilder.InsertData(
                table: "job_listings",
                columns: new[] { "Id", "CompanyId", "Description", "IsActive", "Location", "PostedAt", "Title", "Type" },
                values: new object[,]
                {
                    { new Guid("91111111-1111-1111-1111-111111111111"), new Guid("75ba7d3e-2b50-4841-860e-cbfb4e54e4df"), "C# Engineer needed", true, "Remote", new DateTime(2026, 6, 2, 22, 0, 0, 0, DateTimeKind.Utc), "Backend Developer", 0 },
                    { new Guid("92222222-2222-2222-2222-222222222222"), new Guid("2d5d8e24-9b16-4d2a-89a1-fbf22d4f5c92"), "SQL expert needed", true, "Cape Town", new DateTime(2026, 6, 2, 22, 0, 0, 0, DateTimeKind.Utc), "Data Analyst", 0 },
                    { new Guid("93333333-3333-3333-3333-333333333333"), new Guid("a43fa893-7c3e-4b72-ac2b-923fca8565b3"), "Docker expert", true, "Remote", new DateTime(2026, 6, 2, 22, 0, 0, 0, DateTimeKind.Utc), "DevOps Specialist", 0 },
                    { new Guid("94444444-4444-4444-4444-444444444444"), new Guid("b11c34ef-56d1-419a-9cb8-b2a1aefb23d4"), "React components", true, "Johannesburg", new DateTime(2026, 6, 2, 22, 0, 0, 0, DateTimeKind.Utc), "Frontend Developer", 0 },
                    { new Guid("95555555-5555-5555-5555-555555555555"), new Guid("e8fa4292-1a4b-4b11-bdc1-42cb9fa234fe"), "AWS infra design", true, "Remote", new DateTime(2026, 6, 2, 22, 0, 0, 0, DateTimeKind.Utc), "Cloud Architect", 0 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_applicants_Email",
                table: "applicants",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Applications_ApplicantId",
                table: "Applications",
                column: "ApplicantId");

            migrationBuilder.CreateIndex(
                name: "IX_job_listings_CompanyId",
                table: "job_listings",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_job_listings_Title_CompanyId",
                table: "job_listings",
                columns: new[] { "Title", "CompanyId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Applications");

            migrationBuilder.DropTable(
                name: "applicants");

            migrationBuilder.DropTable(
                name: "job_listings");

            migrationBuilder.DropTable(
                name: "companies");
        }
    }
}
