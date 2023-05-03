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
    
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = new CancellationToken())
    {
        if (eventData.Context is null)
            return new ValueTask<InterceptionResult<int>>(result);

        if (!_options.PerformPostChangeUpdates)
        {
            var changeEvents = FinishEvents(eventData.Context.ChangeTracker);
            eventData.Context.AttachRange(changeEvents);
        }

        return new ValueTask<InterceptionResult<int>>(result);
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
        if (eventData.Context is null)
            return result;

        var changeEvents = FinishEvents(eventData.Context.ChangeTracker);

        if (!changeEvents.Any())
            return result;

        eventData.Context.AttachRange(changeEvents);
        await eventData.Context.SaveChangesAsync(cancellationToken);

        return result;
    }

    private IEnumerable<(EntityState State, EntityEntry Entity, ChangeEvent Event)> TrackEvents(ChangeTracker changeTracker)
    {
        // Scan for changes on the context, context null checks are done on public methods
        EntityEntry[] changes = changeTracker.Entries().ToArray();
        foreach (var entityEntry in changes)
        {
            if (entityEntry.Metadata.ClrType.BaseType == typeof(ChangeEventBase))
                continue;
            
            if (_options.ExclusionFilter(entityEntry.Entity.GetType()))
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

            yield return (entry.State, entry.Entity, entry.Event);
        }
    }
    
    private List<ChangeEvent> FinishEvents(ChangeTracker changeTracker)
    {
        var events = TrackEvents(changeTracker).ToArray();
        var changeEvents = new List<ChangeEvent>(events.Length);

        foreach (var entry in events)
        {
            switch (entry.State)
            {
                // For added events we can now get the state including generated columns including primary key(s)
                case EntityState.Added:
                    entry.Event.NewData = entry.Entity.GetNewState(_options);
                    entry.Event.SourceRowId = entry.Entity.PrimaryKey();
                    break;
            }

            entry.Event.Succeeded = true;
            entry.Event.CompletedOn = DateTimeOffset.UtcNow;

            changeEvents.Add(entry.Event);
        }

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