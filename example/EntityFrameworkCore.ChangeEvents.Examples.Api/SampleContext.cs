using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.ChangeEvents.Examples.Api;

public class SampleContext : DbContext
{
    public SampleContext(DbContextOptions<SampleContext> options) 
        : base(options)
    {
    }

    public DbSet<WeatherForecast> WeatherForecasts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Add the change event interceptor, customise the json serializer.
        optionsBuilder.UseChangeEvents(options =>
        {
            options.JsonSerializerOptions.Converters.Insert(0, new JsonStringEnumConverter());
        });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Add change event models, add custom index on IsPublished.
        modelBuilder.AddChangeEvents<ChangeEvent>(e => 
            e.HasIndex(x => x.IsPublished));

        modelBuilder.Entity<WeatherForecast>(e =>
        {
            e.HasKey(x => x.Id);
            e.Ignore(x => x.EntryId);
        });
    }
}