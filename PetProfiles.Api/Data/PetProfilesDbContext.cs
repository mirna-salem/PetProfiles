using Microsoft.EntityFrameworkCore;
using PetProfiles.Api.Models;

namespace PetProfiles.Api.Data;

public class PetProfilesDbContext : DbContext
{
    public PetProfilesDbContext(DbContextOptions<PetProfilesDbContext> options) : base(options)
    {
    }

    public DbSet<PetProfile> PetProfiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PetProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Breed).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Age).IsRequired();
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
        });
    }
} 