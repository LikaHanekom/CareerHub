using CareerHub.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;  
using Microsoft.Extensions.Configuration;
using System.IO;

namespace CareerHub.Api.Data;

public class CareerHubDbContext(DbContextOptions<CareerHubDbContext> options): DbContext(options)
{
    public DbSet<JobListing> JobListings => Set<JobListing>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<JobListing>(entity =>
        {
            entity.ToTable("job_listings");

            entity.HasKey(j => j.Id);

            entity.Property(j => j.Id).ValueGeneratedNever();

            entity.Property(j => j.Title).IsRequired().HasMaxLength(100);

            entity.Property(j => j.Company).IsRequired().HasMaxLength(100);

            entity.Property(j => j.Description).IsRequired().HasMaxLength(1000);

            entity.Property(j => j.Location).IsRequired().HasMaxLength(100);

            entity.HasIndex(j => new
            {
                j.Title,
                j.Company
            })
            .IsUnique();
        });
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