using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shouldly;
using Xunit;

namespace EntityFrameworkCore.ChangeEvents.Tests;

public partial class ChangeEventInterceptorTests
{
    [Fact]
    public void GivenNullDbContext_WhenSaveChangesFailed_ShouldReturn()
    {
        // Given
        var interceptionResult = InterceptionResult<int>.SuppressWithResult(1);
        var ctx = new DbContextEventData(null, null, null);

        // When
        _interceptor.SavingChanges(ctx, interceptionResult);
        _interceptor.SaveChangesFailed(new(null, null, null, null));

        // Then
        // Don´t do anything
        true.ShouldBeTrue();
    }
    
    [Fact]
    public void GivenAdded_WhenSaveChangesFailed_ShouldSetSucceeded()
    {
        // Given
        var testEntity = new TestEntity() { Name = null, IsActive = true };
        
        // When
        _context.Entities.Add(testEntity);
        try
        {
            _context.SaveChanges();
        }
        catch
        {
            // Expected
        }

        // Then
        _context.ChangeTracker.Entries<ChangeEvent>().First().Entity.Succeeded.ShouldBeFalse();
    }
    
    [Fact]
    public void GivenAdded_WhenSaveChangesFailed_ShouldSetCompletedOn()
    {
        // Given
        var testEntity = new TestEntity() { Name = null, IsActive = true };
        
        // When
        _context.Entities.Add(testEntity);
        try
        {
            _context.SaveChanges();
        }
        catch
        {
            // Expected
        }
        
        // Then
        _context.ChangeTracker.Entries<ChangeEvent>().First().Entity.CompletedOn.ShouldNotBeNull();
    }
}