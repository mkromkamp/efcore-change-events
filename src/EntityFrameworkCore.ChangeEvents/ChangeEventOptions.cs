using System.Text.Json;

namespace EntityFrameworkCore.ChangeEvents;

public class ChangeEventOptions
{
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
}