# EntityFrameworkCore.ChangeEvents

[![MIT License](https://img.shields.io/apm/l/atomic-design-ui.svg?)](https://github.com/mkromkamp/efcore-change-events/blob/master/LICENSEs)
![GitHub release (latest SemVer)](https://img.shields.io/github/v/release/mkromkamp/efcore-change-events?sort=semver)
![GitHub branch checks state](https://img.shields.io/github/checks-status/mkromkamp/efcore-change-events/main)
![GitHub issues](https://img.shields.io/github/issues/mkromkamp/efcore-change-events)

Automatic change events for entity framework core.

## Features

- Configure once, no way to forget to track changes
- Tracks old and new state
- Tracks success and failure of `SaveChanges()`
- Database provider agnostic

## Installation

Install with dotnet cli

Add package
```bash
  dotnet add package EntityFrameworkCore.ChangeEvents
```

After setup (see below), remember to run and apply migrations
``` bash
dotnet ef migrations add ChangeEvents
dotnet ef database update
```

## Usage/Examples

Minimal setup
```csharp
public class SampleContext : DbContext
{
    public SampleContext(DbContextOptions<SampleContext> options) 
        : base(options)
    {
    }

    public DbSet<WeatherForecast> WeatherForecasts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Add the change event interceptor.
        optionsBuilder.UseChangeEvents();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Add change event model.
        modelBuilder.AddChangeEvents<ChangeEvent>();
    }
}
```

Customize JSON serializer
```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    // Add the change event interceptor, customize JSON serializer
    optionsBuilder.UseChangeEvents(options => 
        options.JsonSerializerOptions.Converters.Insert(0, new JsonStringEnumConverter()));
}
```

Customize Entity framework model
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Add change event model, create index on IsPublished
    modelBuilder.AddChangeEvents<ChangeEvent>(e => 
        e.HasIndex(x => x.IsPublished));
}
```

## Run sample project locally

Clone the project
``` bash
  git@github.com:mkromkamp/efcore-change-events.git 
```

Navigate to the example project
```bash
  cd example/EntityFrameworkCore.ChangeEvents.Examples.Api
```

Bring up docker-compose stack
```bash
  docker-compose pull
  docker-compose up -d --build
```

Apply migrations
```bash
  dotnet ef database update
```

Start the example project
```bash
  dotnet run
```

[Navigate to the swagger documentation](https://localhost:5001/swagger)

## License

[MIT](https://choosealicense.com/licenses/mit/)

