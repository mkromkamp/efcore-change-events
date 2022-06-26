using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.ChangeEvents.Tests;

public class TestContext : DbContext
{
    public TestContext(DbContextOptions<TestContext> options) 
        : base(options) { }

    public DbSet<TestEntity> Entities { get; set; }
    
    // Sqlite limitation, not needed when using other database providers
    public DbSet<ChangeEvent> ChangeEvents { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // optionsBuilder.UseSqlite("Filename=:memory:");
        optionsBuilder.UseInMemoryDatabase("TestDb");
        optionsBuilder.UseChangeEvents();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.AddChangeEvents<ChangeEvent>();

        modelBuilder.Entity<TestEntity>(cfg =>
        {
            cfg.HasKey(x => x.Id);
            
            // Max length to trigger save change failures
            cfg.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(16);
        });
    }
}