using System.Text.Json;

namespace EntityFrameworkCore.ChangeEvents;

public class ChangeEventOptions
{
    /// <summary>
    /// Gets or sets the exclusion filter.
    /// Can be used to exclude certain types from being tracked.
    /// </summary>
    public Func<Type, bool> ExclusionFilter { get; set; } = _ => false; 

    /// <summary>
    /// Gets or set the <see cref="JsonSerializer"/> used to serialize change data.
    /// </summary>
    public JsonSerializerOptions JsonSerializerOptions { get; set; } = new();

    /// <summary>
    /// Gets or sets if primary keys should be omitted from change events.
    /// </summary>
    public bool OmitPrimaryKeys { get; set; } = false;
    
    /// <summary>
    /// Gets or sets if foreign keys should be omitted from change events.
    /// </summary>
    public bool OmitForeignKeys { get; set; } = false;

    /// <summary>
    /// Perform post change updates.
    /// </summary>
    /// <remarks>
    /// When enabled database generated identifiers and data will be added to change events.
    /// </remarks>
    public bool PerformPostChangeUpdates { get; set; } = false;
}