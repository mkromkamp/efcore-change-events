using System.Text.Json;

namespace EntityFrameworkCore.ChangeEvents;

public class ChangeEventOptions
{
    /// <summary>
    /// Gets or set the <see cref="JsonSerializer"/> used to serialize change data.
    /// </summary>
    public JsonSerializerOptions JsonSerializerOptions { get; set; } = new();
}