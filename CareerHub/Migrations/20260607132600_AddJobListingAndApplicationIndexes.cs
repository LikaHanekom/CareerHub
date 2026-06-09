using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace CareerHub.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddJobListingAndApplicationIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_job_listings_CompanyId",
                table: "job_listings");

            migrationBuilder.DropIndex(
                name: "IX_applications_ApplicantId",
                table: "applications");

            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "job_listings",
                type: "tsvector",
                nullable: true,
                computedColumnSql: "to_tsvector('english', coalesce(\"Title\", '') || ' ' || coalesce(\"Description\", ''))",
                stored: true);

            migrationBuilder.CreateIndex(
                name: "ix_job_listings_company_id_status",
                table: "job_listings",
                columns: new[] { "CompanyId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "ix_job_listings_search_vector",
                table: "job_listings",
                column: "SearchVector")
                .Annotation("Npgsql:IndexMethod", "GIN");

            migrationBuilder.CreateIndex(
                name: "ix_job_listings_status_expires_at",
                table: "job_listings",
                columns: new[] { "IsActive", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "ix_applications_applicant_id_job_listing_id",
                table: "applications",
                columns: new[] { "ApplicantId", "JobListingId" });

            migrationBuilder.CreateIndex(
                name: "ix_applications_job_listing_id",
                table: "applications",
                column: "JobListingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_job_listings_company_id_status",
                table: "job_listings");

            migrationBuilder.DropIndex(
                name: "ix_job_listings_search_vector",
                table: "job_listings");

            migrationBuilder.DropIndex(
                name: "ix_job_listings_status_expires_at",
                table: "job_listings");

            migrationBuilder.DropIndex(
                name: "ix_applications_applicant_id_job_listing_id",
                table: "applications");

            migrationBuilder.DropIndex(
                name: "ix_applications_job_listing_id",
                table: "applications");

            migrationBuilder.DropColumn(
                name: "SearchVector",
                table: "job_listings");

            migrationBuilder.CreateIndex(
                name: "IX_job_listings_CompanyId",
                table: "job_listings",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_applications_ApplicantId",
                table: "applications",
                column: "ApplicantId");
        }
    }
}
