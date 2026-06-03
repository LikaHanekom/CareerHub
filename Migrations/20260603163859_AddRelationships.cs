using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareerHub.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_job_listings_Title_Company",
                table: "job_listings");

            migrationBuilder.DropColumn(
                name: "Company",
                table: "job_listings");

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyId",
                table: "job_listings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

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

            migrationBuilder.CreateIndex(
                name: "IX_job_listings_CompanyId",
                table: "job_listings",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_job_listings_Title_CompanyId",
                table: "job_listings",
                columns: new[] { "Title", "CompanyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_applicants_Email",
                table: "applicants",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Applications_ApplicantId",
                table: "Applications",
                column: "ApplicantId");

            migrationBuilder.AddForeignKey(
                name: "FK_job_listings_companies_CompanyId",
                table: "job_listings",
                column: "CompanyId",
                principalTable: "companies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_job_listings_companies_CompanyId",
                table: "job_listings");

            migrationBuilder.DropTable(
                name: "Applications");

            migrationBuilder.DropTable(
                name: "companies");

            migrationBuilder.DropTable(
                name: "applicants");

            migrationBuilder.DropIndex(
                name: "IX_job_listings_CompanyId",
                table: "job_listings");

            migrationBuilder.DropIndex(
                name: "IX_job_listings_Title_CompanyId",
                table: "job_listings");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "job_listings");

            migrationBuilder.AddColumn<string>(
                name: "Company",
                table: "job_listings",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_job_listings_Title_Company",
                table: "job_listings",
                columns: new[] { "Title", "Company" },
                unique: true);
        }
    }
}
