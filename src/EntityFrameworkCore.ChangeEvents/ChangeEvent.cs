using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.ChangeEvents;

public class ChangeEvent : ChangeEventBase
{
    
}

/// <summary>
/// Base change event.
/// </summary>
public abstract class ChangeEventBase
{
    /// <summary>
    /// Gets or sets the unique identifier of this <see cref="ChangeEventBase"/>.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Optionally, gets or sets the source row identifier.
    /// </summary>
    /// <remarks>
    /// When database generated identifiers are used this can be null on inserts.
    /// </remarks>
    public string? SourceRowId { get; set; }

    /// <summary>
    /// Gets or sets the source table name.
    /// </summary>
    public string SourceTableName { get; set; }

    /// <summary>
    /// Optionally, gets or sets the new data of the entity.
    /// </summary>
    /// <remarks>
    /// Not present when a new entity is tracked.
    /// </remarks>
    public string? OldData { get; set; }
    
    /// <summary>
    /// Optionally, gets or sets the new state of the entity.
    /// </summary>
    /// <remarks>
    /// Not present when a tracked entity is deleted.
    /// </remarks>
    public string? NewData { get; set; }

    /// <summary>
    /// Gets or sets the change type. Valid values come from <see cref="EntityState"/>
    /// </summary>
    public string ChangeType { get; set; }

    /// <summary>
    /// Gets or sets if the modification was successful. If the query failed this will be false.
    /// </summary>
    public bool Succeeded { get; set; }
    
    /// <summary>
    /// Gets or sets the start datetime of the change.
    /// </summary>
    public DateTimeOffset StartedOn { get; set; }
    
    /// <summary>
    /// Gets or sets the end datetime of the change.
    /// </summary>
    public DateTimeOffset? CompletedOn { get; set; }
    
    /// <summary>
    /// Gets or sets if the change is published. Usable for an transaction outbox for example.
    /// </summary>
    public bool IsPublished { get; set; }
    
    /// <summary>
    /// Gets or sets when the change was published. Usable for an transaction outbox for example.
    /// </summary>
    public DateTimeOffset? PublishedOn { get; set; }
}