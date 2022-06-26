using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EntityFrameworkCore.ChangeEvents;

/// <summary>
/// Change event interceptor.
/// </summary>
internal class ChangeEventInterceptor : SaveChangesInterceptor
{
    private readonly ChangeEventOptions _options;
    private List<ChangeEvent> _changeEvents = new();
    private bool _handledFailures = false;

    public ChangeEventInterceptor(ChangeEventOptions options)
    {
        _options = options;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        return SavingChangesAsync(eventData, result, CancellationToken.None)
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();
    }
    
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = new CancellationToken())
    {
        if (eventData.Context is null)
            return result;
        
        _changeEvents = CreateEvents(eventData.Context.ChangeTracker);
        await eventData.Context.AddRangeAsync(_changeEvents, cancellationToken);

        return result;
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        return SavedChangesAsync(eventData, result, CancellationToken.None)
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();
    }

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result,
        CancellationToken cancellationToken = new CancellationToken())
    {
        if (eventData.Context is null || !_changeEvents.Any())
            return result;
        
        foreach (var changeEvent in _changeEvents)
        {
            changeEvent.Succeeded = true;
            changeEvent.CompletedOn = DateTimeOffset.UtcNow;
        }
        
        eventData.Context.AttachRange(_changeEvents);
        await eventData.Context.SaveChangesAsync(cancellationToken);

        return result;
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        SaveChangesFailedAsync(eventData, CancellationToken.None)
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();
    }

    public override async Task SaveChangesFailedAsync(DbContextErrorEventData eventData,
        CancellationToken cancellationToken = new CancellationToken())
    {
        if (eventData.Context is null || !_changeEvents.Any() || _handledFailures)
            return;
        
        // Detach entities to avoid endless looping on save failure
        var entries = eventData.Context.ChangeTracker.Entries().ToList();
        foreach (var entityEntry in eventData.Context.ChangeTracker.Entries())
        {
            entityEntry.State = EntityState.Detached;
        }
        
        foreach (var changeEvent in _changeEvents)
        {
            changeEvent.Succeeded = false;
            changeEvent.CompletedOn = DateTimeOffset.UtcNow;
        }
        
        eventData.Context.AttachRange(_changeEvents);
        _handledFailures = true;
        await eventData.Context.SaveChangesAsync(cancellationToken);
        
        // Re-attach entities to the context
        eventData.Context.AttachRange(entries.Select(e => e.Entity));
    }

    private List<ChangeEvent> CreateEvents(ChangeTracker changeTracker)
    {
        var changeEvents = new List<ChangeEvent>();

        // Scan for changes on the context, context null checks are done on public methods
        changeTracker.DetectChanges();

        foreach (var entityEntry in changeTracker.Entries())
        {
            if (entityEntry.Metadata.ClrType.BaseType == typeof(ChangeEventBase))
                continue;
            
            var changeEvent = entityEntry.State switch
            {
                EntityState.Added => CreateAddedEvent(entityEntry),
                EntityState.Modified => CreateModifiedEvent(entityEntry),
                EntityState.Deleted => CreateDeletedEvent(entityEntry),
                _ => null
            };
            
            if (changeEvent is not null)
                changeEvents.Add(changeEvent);
        }

        return changeEvents;
    }

    private ChangeEvent CreateAddedEvent(EntityEntry entry)
    {
        return new()
        {
            ChangeType = entry.State.ToString(),
            SourceTableName = entry.Metadata.DisplayName(),
            SourceRowId = entry.PrimaryKey(),
            OldData = null,
            NewData = entry.GetNewState(_options.JsonSerializerOptions),
            Succeeded = false,
            StartedOn = DateTimeOffset.UtcNow,
            CompletedOn = null,
            IsPublished = false,
            PublishedOn = null
        };
    }
    
    private ChangeEvent CreateModifiedEvent(EntityEntry entry)
    {
        return new()
        {
            ChangeType = entry.State.ToString(),
            SourceTableName = entry.Metadata.DisplayName(),
            SourceRowId = entry.PrimaryKey(),
            OldData = entry.GetOldState(_options.JsonSerializerOptions),
            NewData = entry.GetNewState(_options.JsonSerializerOptions),
            Succeeded = false,
            StartedOn = DateTimeOffset.UtcNow,
            CompletedOn = null,
            IsPublished = false,
            PublishedOn = null
        };
    }
    
    private ChangeEvent CreateDeletedEvent(EntityEntry entry)
    {
        return new()
        {
            ChangeType = entry.State.ToString(),
            SourceTableName = entry.Metadata.DisplayName(),
            SourceRowId = entry.PrimaryKey(),
            OldData = entry.GetOldState(_options.JsonSerializerOptions),
            NewData = null,
            Succeeded = false,
            StartedOn = DateTimeOffset.UtcNow,
            CompletedOn = null,
            IsPublished = false,
            PublishedOn = null
        };
    }
}