using CareerHub.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;  
using Microsoft.Extensions.Configuration;
using System.IO;

namespace CareerHub.Api.Data;

public class CareerHubDbContext(DbContextOptions<CareerHubDbContext> options): DbContext(options)
{
    public DbSet<JobListing> JobListings => Set<JobListing>();//owns database connection and access to tables through DB<Set>
    public DbSet<Company> Companies => Set<Company>();

    public DbSet<Applicant> Applicants => Set<Applicant>();

    public DbSet<Application> Applications => Set<Application>();

    //temporary
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // This will temporarily spit out raw SQL statements into your terminal
        optionsBuilder.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JobListing>(entity =>
{
        entity.ToTable("job_listings");

        entity.HasKey(j => j.Id);

        entity.Property(j => j.Id).ValueGeneratedNever();

        entity.Property(j => j.Title).IsRequired().HasMaxLength(100);

        entity.Property(j => j.Description).IsRequired().HasMaxLength(1000);

        entity.Property(j => j.Location).IsRequired().HasMaxLength(100);

        entity.HasIndex(j => new
        {
            j.Title,
            j.CompanyId
        })
        .IsUnique();
    });
        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("companies");

            entity.HasKey(c => c.Id);

            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);

            entity.Property(c => c.Website).IsRequired().HasMaxLength(255);
        });

        modelBuilder.Entity<Applicant>(entity =>
        {
            entity.ToTable("applicants");

            entity.HasKey(a => a.Id);

            entity.Property(a => a.FullName).IsRequired().HasMaxLength(100);

            entity.Property(a => a.Email).IsRequired().HasMaxLength(255);

            entity.HasIndex(a => a.Email).IsUnique();//HasIndex - tells EF to create database index on Email column in table
        });

        //composite key
        modelBuilder.Entity<Application>().HasKey(ap => new
        {
            ap.JobListingId,
            ap.ApplicantId
        });

        //Company Relationship
        modelBuilder.Entity<JobListing>() //Targets Joblistings classs: I want to configure the joblistings database mapping rules
        .HasOne(j => j.Company) //Tells EF every Joblisting is connected to one company
        .WithMany(c => c.JobListings)// Tells EF on the other side 1 Company can own many job listings
        .HasForeignKey(j => j.CompanyId) 
        .OnDelete(DeleteBehavior.Restrict);//if you try to delete company that has active joblistigns the delete will be restricted

        //Application Relationships
        modelBuilder.Entity<Application>()
        .HasOne(ap => ap.JobListing)
        .WithMany(j => j.Applications)
        .HasForeignKey(ap => ap.JobListingId)
        .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Application>()
        .HasOne(ap => ap.Applicant)
        .WithMany(a => a.Applications)
        .HasForeignKey(ap => ap.ApplicantId)
        .OnDelete(DeleteBehavior.Restrict);

        var c1 = Guid.Parse("75ba7d3e-2b50-4841-860e-cbfb4e54e4df"); // TechCorp
        var c2 = Guid.Parse("2d5d8e24-9b16-4d2a-89a1-fbf22d4f5c92"); // FinanceFlow
        var c3 = Guid.Parse("a43fa893-7c3e-4b72-ac2b-923fca8565b3"); // HealthNet
        var c4 = Guid.Parse("b11c34ef-56d1-419a-9cb8-b2a1aefb23d4"); // EduBuild
        var c5 = Guid.Parse("e8fa4292-1a4b-4b11-bdc1-42cb9fa234fe"); // LogiRoute

        // Applicant IDs
        var applicantA = Guid.Parse("a1111111-1111-1111-1111-111111111111");
        var applicantB = Guid.Parse("b2222222-2222-2222-2222-222222222222");

        // Static date so EF Core doesn't throw a warning
        var staticDate = DateTime.Parse("2026-06-03").ToUniversalTime();

        // 2. SEED COMPANIES USING THE CLEAN VARIABLES
        modelBuilder.Entity<Company>().HasData(
            new Company { Id = c1, Name = "TechCorp", Website = "techcorp.com" },
            new Company { Id = c2, Name = "FinanceFlow", Website = "financeflow.com" },
            new Company { Id = c3, Name = "HealthNet", Website = "healthnet.com" },
            new Company { Id = c4, Name = "EduBuild", Website = "edubuild.com" },
            new Company { Id = c5, Name = "LogiRoute", Website = "logiroute.com" }
        );

        // 3. SEED JOB LISTINGS (Now c1 through c5 point to valid companies!)
        modelBuilder.Entity<JobListing>().HasData(
            new JobListing { Id = Guid.Parse("91111111-1111-1111-1111-111111111111"), CompanyId = c1, Title = "Backend Developer", Description = "C# Engineer needed", Location = "Remote", PostedAt = staticDate, IsActive = true },
            new JobListing { Id = Guid.Parse("92222222-2222-2222-2222-222222222222"), CompanyId = c2, Title = "Data Analyst", Description = "SQL expert needed", Location = "Cape Town", PostedAt = staticDate, IsActive = true },
            new JobListing { Id = Guid.Parse("93333333-3333-3333-3333-333333333333"), CompanyId = c3, Title = "DevOps Specialist", Description = "Docker expert", Location = "Remote", PostedAt = staticDate, IsActive = true },
            new JobListing { Id = Guid.Parse("94444444-4444-4444-4444-444444444444"), CompanyId = c4, Title = "Frontend Developer", Description = "React components", Location = "Johannesburg", PostedAt = staticDate, IsActive = true },
            new JobListing { Id = Guid.Parse("95555555-5555-5555-5555-555555555555"), CompanyId = c5, Title = "Cloud Architect", Description = "AWS infra design", Location = "Remote", PostedAt = staticDate, IsActive = true }
        );

        // Seed Test Applicants
        modelBuilder.Entity<Applicant>().HasData(
            new Applicant { Id = applicantA, FullName = "Applicant A", Email = "applicantA@test.com" },
            new Applicant { Id = applicantB, FullName = "Applicant B", Email = "applicantB@test.com" }
        );
    }



    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CareerHubDbContext>
    {
        public CareerHubDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<CareerHubDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            optionsBuilder.UseNpgsql(connectionString);

            return new CareerHubDbContext(optionsBuilder.Options);
        }
    }

}