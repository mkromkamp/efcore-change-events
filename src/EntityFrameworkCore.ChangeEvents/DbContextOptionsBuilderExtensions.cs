using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.ChangeEvents;

public static class DbContextOptionsBuilderExtensions
{
    public static DbContextOptionsBuilder UseChangeEvents(this DbContextOptionsBuilder builder, Action<ChangeEventOptions>? optionsFactory = null)
    {
        var options = new ChangeEventOptions();
        optionsFactory?.Invoke(options);

        builder.AddInterceptors(new ChangeEventInterceptor(options));

        return builder;
    }
    
    public static DbContextOptionsBuilder UseChangeEvents(this DbContextOptionsBuilder builder, ChangeEventOptions options)
    {
        if (options is null) throw new ArgumentNullException(nameof(options));
        
        builder.AddInterceptors(new ChangeEventInterceptor(options));

        return builder;
    }
}