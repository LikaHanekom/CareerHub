using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareerHub.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDatabaseCheckConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_applicants_ApplicantId",
                table: "Applications");

            migrationBuilder.DropForeignKey(
                name: "FK_Applications_job_listings_JobListingId",
                table: "Applications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Applications",
                table: "Applications");

            migrationBuilder.RenameTable(
                name: "Applications",
                newName: "applications");

            migrationBuilder.RenameIndex(
                name: "IX_Applications_ApplicantId",
                table: "applications",
                newName: "IX_applications_ApplicantId");

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "job_listings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SalaryMax",
                table: "job_listings",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SalaryMin",
                table: "job_listings",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_applications",
                table: "applications",
                columns: new[] { "JobListingId", "ApplicantId" });

            migrationBuilder.UpdateData(
                table: "job_listings",
                keyColumn: "Id",
                keyValue: new Guid("91111111-1111-1111-1111-111111111111"),
                columns: new[] { "ExpiresAt", "SalaryMax", "SalaryMin" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "job_listings",
                keyColumn: "Id",
                keyValue: new Guid("92222222-2222-2222-2222-222222222222"),
                columns: new[] { "ExpiresAt", "SalaryMax", "SalaryMin" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "job_listings",
                keyColumn: "Id",
                keyValue: new Guid("93333333-3333-3333-3333-333333333333"),
                columns: new[] { "ExpiresAt", "SalaryMax", "SalaryMin" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "job_listings",
                keyColumn: "Id",
                keyValue: new Guid("94444444-4444-4444-4444-444444444444"),
                columns: new[] { "ExpiresAt", "SalaryMax", "SalaryMin" },
                values: new object[] { null, null, null });

            migrationBuilder.UpdateData(
                table: "job_listings",
                keyColumn: "Id",
                keyValue: new Guid("95555555-5555-5555-5555-555555555555"),
                columns: new[] { "ExpiresAt", "SalaryMax", "SalaryMin" },
                values: new object[] { null, null, null });

            migrationBuilder.AddCheckConstraint(
                name: "ck_job_listings_expiry_date",
                table: "job_listings",
                sql: "\"ExpiresAt\" IS NULL OR \"ExpiresAt\" > \"PostedAt\"");

            migrationBuilder.AddCheckConstraint(
                name: "ck_job_listings_salary_range",
                table: "job_listings",
                sql: "(\"SalaryMin\" IS NULL OR \"SalaryMin\" > 0) AND (\"SalaryMin\" IS NULL OR \"SalaryMax\" IS NULL OR \"SalaryMax\" > \"SalaryMin\")");

            migrationBuilder.AddCheckConstraint(
                name: "ck_applications_submitted_at_no_future",
                table: "applications",
                sql: "\"SubmittedAt\" <= CURRENT_TIMESTAMP");

            migrationBuilder.AddForeignKey(
                name: "FK_applications_applicants_ApplicantId",
                table: "applications",
                column: "ApplicantId",
                principalTable: "applicants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_applications_job_listings_JobListingId",
                table: "applications",
                column: "JobListingId",
                principalTable: "job_listings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_applications_applicants_ApplicantId",
                table: "applications");

            migrationBuilder.DropForeignKey(
                name: "FK_applications_job_listings_JobListingId",
                table: "applications");

            migrationBuilder.DropCheckConstraint(
                name: "ck_job_listings_expiry_date",
                table: "job_listings");

            migrationBuilder.DropCheckConstraint(
                name: "ck_job_listings_salary_range",
                table: "job_listings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_applications",
                table: "applications");

            migrationBuilder.DropCheckConstraint(
                name: "ck_applications_submitted_at_no_future",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "job_listings");

            migrationBuilder.DropColumn(
                name: "SalaryMax",
                table: "job_listings");

            migrationBuilder.DropColumn(
                name: "SalaryMin",
                table: "job_listings");

            migrationBuilder.RenameTable(
                name: "applications",
                newName: "Applications");

            migrationBuilder.RenameIndex(
                name: "IX_applications_ApplicantId",
                table: "Applications",
                newName: "IX_Applications_ApplicantId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Applications",
                table: "Applications",
                columns: new[] { "JobListingId", "ApplicantId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_applicants_ApplicantId",
                table: "Applications",
                column: "ApplicantId",
                principalTable: "applicants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_job_listings_JobListingId",
                table: "Applications",
                column: "JobListingId",
                principalTable: "job_listings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
