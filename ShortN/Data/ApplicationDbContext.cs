using Microsoft.EntityFrameworkCore;
using ShortN.Models;

namespace ShortN.Data;

public class ApplicationDbContext : DbContext
{
    public DbSet<UrlEntry> UrlEntries { get; set; } = null!;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UrlEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LongUrl).IsRequired();
            entity.Property(e => e.ShortCode).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // This is just a fallback, the main configuration is in Program.cs
            optionsBuilder.UseNpgsql("Server=localhost;Port=5432;Database=shortn;User Id=postgres;Password=egecagdas;");
        }
    }
}