using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace EntityFrameworkCore.ChangeEvents;

internal static class EntityEntryExtensions
{
    /// <summary>
    /// Get the new state.
    /// </summary>
    /// <param name="entry">The entry.</param>
    /// <param name="serializerOptions">The <see cref="JsonSerializerOptions"/>.</param>
    /// <returns>A string representing the new state in JSON format.</returns>
    public static string GetNewState(this EntityEntry entry, JsonSerializerOptions serializerOptions)
    {
        var result = new Dictionary<string, object?>();
        
        foreach (var entryProperty in entry.Properties)
        {
            // Ignore (foreign) keys 
            if (entryProperty.Metadata.IsKey() || entryProperty.Metadata.IsForeignKey())
            {
                continue;
            }

            result[entryProperty.Metadata.Name] = entryProperty.CurrentValue;
        }

        return JsonSerializer.Serialize(result, serializerOptions);
    }
    
    /// <summary>
    /// Get the old state. 
    /// </summary>
    /// <param name="entry">The entry.</param>
    /// <param name="serializerOptions">The <see cref="JsonSerializerOptions"/>.</param>
    /// <returns>A string representing the old state in JSON format.</returns>
    public static string GetOldState(this EntityEntry entry, JsonSerializerOptions serializerOptions)
    {
        var result = new Dictionary<string, object?>();
        
        foreach (var entryProperty in entry.Properties)
        {
            // Ignore (foreign) keys 
            if (entryProperty.Metadata.IsKey() || entryProperty.Metadata.IsForeignKey())
            {
                continue;
            }

            result[entryProperty.Metadata.Name] = entryProperty.OriginalValue;
        }

        return JsonSerializer.Serialize(result, serializerOptions);
    }

    /// <summary>
    /// Gets the, composite, primary key of the <see cref="EntityEntry"/>.
    /// </summary>
    /// <param name="entry">The entry.</param>
    /// <returns>Return the primary key. If the primary key is a composite key the value are concatenated by comma.</returns>
    public static string? PrimaryKey(this EntityEntry entry)
    {
        var key = entry.Metadata.FindPrimaryKey();
        if (key is null)
            return null;

        var values = new List<object>();
        foreach (var property in key.Properties)
        {
            var value = entry.Property(property.Name).CurrentValue;
            if (value != null)
            {
                values.Add(value);
            }
        }

        return string.Join(",", values);
    }

    /// <summary>
    /// Gets the name of the table (entity).
    /// </summary>
    /// <param name="entry"></param>
    /// <returns></returns>
    public static string TableName(this EntityEntry entry)
    {
        return entry.Metadata.ShortName();
    }
}