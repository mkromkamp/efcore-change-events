using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntityFrameworkCore.ChangeEvents;

public static class ModelBuilderExtensions
{
    public static ModelBuilder AddChangeEvents<TChangeEventBase>(this ModelBuilder builder, Action<EntityTypeBuilder<TChangeEventBase>>? entityTypeBuilder = null) 
        where TChangeEventBase : ChangeEventBase
    {
        builder.Entity<TChangeEventBase>(e =>
        {
            entityTypeBuilder?.Invoke(e);
            
            // Set the key after the entity type builder. This avoids overwriting of the key.
            e.HasKey(ce => ce.Id);
        });
        
        return builder;
    }
}