using Microsoft.EntityFrameworkCore;

namespace ImageGenApp.Models;

public class AppDbContext : DbContext
{
    public DbSet<AppSettings> Settings { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public AppDbContext() : base(new DbContextOptionsBuilder<AppDbContext>().UseSqlite($"Data Source={GetDatabasePath()}").Options)
    {
    }

    private static string GetDatabasePath()
    {
        var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ImageGenApp", "settings.db");
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        return dbPath;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure AppSettings as a singleton
        modelBuilder.Entity<AppSettings>()
            .HasIndex(s => s.Id)
            .IsUnique();
    }
}
