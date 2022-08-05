using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EntityFrameworkCore.ChangeEvents;

/// <summary>
/// Change event interceptor.
/// </summary>
internal class ChangeEventInterceptor : SaveChangesInterceptor
{
    private readonly ChangeEventOptions _options;
    private bool _handledFailures = false;

    private List<(EntityState State, EntityEntry Entity, ChangeEvent Event)> _entries = new();

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

        TrackEvents(eventData.Context.ChangeTracker);

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
        var changeEvents = FinishEvents();
        
        if (eventData.Context is null || !changeEvents.Any())
            return result;

        eventData.Context.AttachRange(changeEvents);
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
        if (eventData.Context is null || !_entries.Any() || _handledFailures)
            return;
        
        // Detach entities to avoid endless looping on save failure
        var entries = eventData.Context.ChangeTracker.Entries().ToList();
        foreach (var entityEntry in eventData.Context.ChangeTracker.Entries())
        {
            entityEntry.State = EntityState.Detached;
        }

        var changeEvents = FinishEvents(succeeded: false);
        
        eventData.Context.AttachRange(changeEvents);
        _handledFailures = true;
        await eventData.Context.SaveChangesAsync(cancellationToken);
        
        // Re-attach entities to the context
        eventData.Context.AttachRange(entries.Select(e => e.Entity));
    }

    private void TrackEvents(ChangeTracker changeTracker)
    {
        // Scan for changes on the context, context null checks are done on public methods
        changeTracker.DetectChanges();
        
        foreach (var entityEntry in changeTracker.Entries())
        {
            if (entityEntry.Metadata.ClrType.BaseType == typeof(ChangeEventBase))
                continue;

            (EntityState State, EntityEntry? Entity, ChangeEvent? Event) entry = entityEntry.State switch
            {
                EntityState.Added => (entityEntry.State, entityEntry, CreateAddedEvent(entityEntry)),
                EntityState.Modified => (entityEntry.State, entityEntry, CreateModifiedEvent(entityEntry)),
                EntityState.Deleted => (entityEntry.State, entityEntry, CreateDeletedEvent(entityEntry)),
                _ => (default, default, null)
            };
            
            if (entry.Entity is null || entry.Event is null)
                continue;
            
            _entries.Add(entry!);
        }
    }
    
    private List<ChangeEvent> FinishEvents(bool succeeded = true)
    {
        var changeEvents = new List<ChangeEvent>();

        foreach (var entry in _entries)
        {
            switch (entry.State)
            {
                // For added events we can now get the state including generated columns including primary key(s)
                case EntityState.Added:
                    entry.Event.NewData = entry.Entity.GetNewState(_options);
                    entry.Event.SourceRowId = entry.Entity.PrimaryKey();
                    break;
            }

            entry.Event.Succeeded = succeeded;
            entry.Event.CompletedOn = DateTimeOffset.UtcNow;
            
            changeEvents.Add(entry.Event);
        }

        _entries.Clear();
        return changeEvents;
    }

    private ChangeEvent CreateAddedEvent(EntityEntry entry)
    {
        return new()
        {
            ChangeType = nameof(EntityState.Added),
            SourceTableName = entry.TableName(),
            SourceRowId = entry.PrimaryKey(),
            OldData = null,
            NewData = entry.GetNewState(_options),
            Succeeded = true,
            StartedOn = DateTimeOffset.UtcNow,
            CompletedOn = DateTimeOffset.UtcNow,
            IsPublished = false,
            PublishedOn = null
        };
    }
    
    private ChangeEvent CreateModifiedEvent(EntityEntry entry)
    {
        return new()
        {
            ChangeType = nameof(EntityState.Modified),
            SourceTableName = entry.TableName(),
            SourceRowId = entry.PrimaryKey(),
            OldData = entry.GetOldState(_options),
            NewData = entry.GetNewState(_options),
            Succeeded = true,
            StartedOn = DateTimeOffset.UtcNow,
            CompletedOn = DateTimeOffset.UtcNow,
            IsPublished = false,
            PublishedOn = null
        };
    }
    
    private ChangeEvent CreateDeletedEvent(EntityEntry entry)
    {
        return new()
        {
            ChangeType = nameof(EntityState.Deleted),
            SourceTableName = entry.TableName(),
            SourceRowId = entry.PrimaryKey(),
            OldData = entry.GetOldState(_options),
            NewData = null,
            Succeeded = true,
            StartedOn = DateTimeOffset.UtcNow,
            CompletedOn = DateTimeOffset.UtcNow,
            IsPublished = false,
            PublishedOn = null
        };
    }
}