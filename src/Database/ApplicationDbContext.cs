using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProjectMagpie.Models;

namespace Infrastructure.Database;

public interface IApplicationDbContext 
{
    DbSet<Thing> Things { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<Thing> Things => Set<Thing>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // optionsBuilder.UseSqlite("Data Source=projectmagpie.db");
        base.OnConfiguring(optionsBuilder);
    }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Thing>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        base.OnModelCreating(builder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        int result = await base.SaveChangesAsync(cancellationToken);
        return result;
    }

}