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
    }
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