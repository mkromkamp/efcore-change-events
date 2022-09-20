using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shouldly;
using Xunit;

namespace EntityFrameworkCore.ChangeEvents.Tests;

public partial class ChangeEventInterceptorTests
{
    [Fact]
    public void GivenNullDbContext_WhenSavedChanges_ShouldReturn()
    {
        // Given
        var interceptionResult = InterceptionResult<int>.SuppressWithResult(1);
        var ctx = new DbContextEventData(null, null, null);

        // When
        _interceptor.SavingChanges(ctx, interceptionResult);
        var result = _interceptor.SavedChanges(new SaveChangesCompletedEventData(null, null, null, 0), interceptionResult.Result);

        // Then
        result.ShouldBe(interceptionResult.Result);
    }
    
    [Fact]
    public void GivenAdded_WhenSavedChanges_ShouldSetSucceeded()
    {
        // Given
        var testEntity = new TestEntity() { Name = "Test User", IsActive = true };
        
        // When
        _context.Entities.Add(testEntity);
        _context.SaveChanges();
        
        // Then
        _context.ChangeTracker.Entries<ChangeEvent>().First().Entity.Succeeded.ShouldBeTrue();
    }
    
    [Fact]
    public void GivenAdded_WhenSavedChanges_ShouldSetCompletedOn()
    {
        using var context = new TestContext(new(), new());
        
        // Given
        var testEntity = new TestEntity() { Name = "Test User", IsActive = true };
        
        // When
        context.Entities.Add(testEntity);
        context.SaveChanges();
        
        // Then
        context.ChangeTracker.Entries<ChangeEvent>().First().Entity.CompletedOn.ShouldNotBeNull();
    }
}