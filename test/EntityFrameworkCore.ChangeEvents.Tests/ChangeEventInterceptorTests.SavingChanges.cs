using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shouldly;
using Xunit;

namespace EntityFrameworkCore.ChangeEvents.Tests;

public partial class ChangeEventInterceptorTests
{
    [Fact]
    public void GivenNullDbContext_WhenSavingChanges_ShouldReturn()
    {
        // Given
        var interceptionResult = new InterceptionResult<int>();
        var ctx = new DbContextEventData(null, null, null);

        // When
        var result = _interceptor.SavingChanges(ctx, interceptionResult);

        // Then
        result.ShouldBe(interceptionResult);
    }
    
    [Fact]
    public void GivenAdded_WhenSavingChanges_ShouldAddChangeEvent()
    {
        // Given
        var testEntity = new TestEntity() { Name = "Test User", IsActive = true };
        
        // When
        _context.Entities.Add(testEntity);
        _context.SaveChanges();

        // Then
        _context.ChangeTracker.Entries<ChangeEvent>().Any().ShouldBeTrue();
    }
    
    [Fact]
    public void GivenAdded_WhenSavingChanges_ShouldSetSourceTable()
    {
        // Given
        var testEntity = new TestEntity() { Name = "Test User", IsActive = true };

        // When
        _context.Entities.Add(testEntity);
        _context.SaveChanges();

        // Then
        _context.ChangeTracker.Entries<ChangeEvent>().First().Entity.SourceTableName.ShouldBe(nameof(TestEntity));
    }
    
    [Fact]
    public void GivenAdded_WhenSavingChanges_ShouldSetChangeType()
    {
        // Given
        var testEntity = new TestEntity() { Name = "Test User", IsActive = true };

        // When
        _context.Entities.Add(testEntity);
        _context.SaveChanges();

        // Then
        _context.ChangeTracker.Entries<ChangeEvent>().First().Entity.ChangeType.ShouldBe(EntityState.Added.ToString());
    }
    
    [Fact]
    public void GivenModified_WhenSavingChanges_ShouldSetChangeType()
    {
        // Given
        var testEntity = new TestEntity() { Name = "Test User", IsActive = true };
        _context.Entities.Add(testEntity);
        _context.SaveChanges();

        // When
        testEntity.Name = "Other test user";
        _context.Attach(testEntity);
        _context.SaveChanges();

        // Then
        _context.ChangeTracker.Entries<ChangeEvent>().Last().Entity.ChangeType.ShouldBe(EntityState.Modified.ToString());
    }
    
    [Fact]
    public void GivenDeleted_WhenSavingChanges_ShouldSetChangeType()
    {
        // Given
        var testEntity = new TestEntity() { Name = "Test User", IsActive = true };
        _context.Entities.Add(testEntity);
        _context.SaveChanges();

        // When
        _context.Remove(testEntity);
        _context.SaveChanges();

        // Then
        _context.ChangeTracker.Entries<ChangeEvent>().First().Entity.ChangeType.ShouldBe(EntityState.Deleted.ToString());
    }
    
    [Fact]
    public void GivenModified_WhenSavingChanges_ShouldSetSourceRowId()
    {
        // Given
        var testEntity = new TestEntity() { Name = "Test User", IsActive = true };
        _context.Entities.Add(testEntity);
        _context.SaveChanges();

        // When
        testEntity.Name = "Other test user";
        _context.Attach(testEntity);
        _context.SaveChanges();

        // Then
        _context.ChangeTracker.Entries<ChangeEvent>().First().Entity.SourceRowId.ShouldBe(testEntity.Id.ToString());
    }
    
    [Fact]
    public void GivenRemoved_WhenSavingChanges_ShouldSetSourceRowId()
    {
        // Given
        var testEntity = new TestEntity() { Name = "Test User", IsActive = true };
        _context.Entities.Add(testEntity);
        _context.SaveChanges();

        // When
        _context.Remove(testEntity);
        _context.SaveChanges();

        // Then
        _context.ChangeTracker.Entries<ChangeEvent>().First().Entity.SourceRowId.ShouldBe(testEntity.Id.ToString());
    }

    [Fact]
    public void GivenAdded_WhenSavingChanges_ShouldNotSetOldData()
    {
        // Given
        var testEntity = new TestEntity() { Name = "Test User", IsActive = true };
        
        // When
        _context.Entities.Add(testEntity);
        _context.SaveChanges();

        // Then
        _context.ChangeTracker.Entries<ChangeEvent>().First().Entity.OldData.ShouldBeNull();
    }
    
    [Fact]
    public void GivenModified_WhenSavingChanges_ShouldSetOldData()
    {
        // Given
        var testEntity = new TestEntity() { Name = "Test User", IsActive = true };
        _context.Entities.Add(testEntity);
        _context.SaveChanges();

        // When
        testEntity.Name = "Other test user";
        _context.Attach(testEntity);
        _context.SaveChanges();

        // Then
        _context.ChangeTracker.Entries<ChangeEvent>().Last().Entity.OldData.ShouldNotBeNull();
    }
    
    [Fact]
    public void GivenDeleted_WhenSavingChanges_ShouldSetOldData()
    {
        // Given
        var testEntity = new TestEntity() { Name = "Test User", IsActive = true };
        _context.Entities.Add(testEntity);
        _context.SaveChanges();

        // When
        _context.Remove(testEntity);
        _context.SaveChanges();

        // Then
        _context.ChangeTracker.Entries<ChangeEvent>().First().Entity.OldData.ShouldNotBeNull();
    }
    
    [Fact]
    public void GivenAdded_WhenSavingChanges_ShouldSetNewData()
    {
        // Given
        var testEntity = new TestEntity() { Name = "Test User", IsActive = true };

        // When
        _context.Entities.Add(testEntity);
        _context.SaveChanges();

        // Then
        _context.ChangeTracker.Entries<ChangeEvent>().First().Entity.NewData.ShouldNotBeNull();
    }
    
    [Fact]
    public void GivenModified_WhenSavingChanges_ShouldSetNewData()
    {
        // Given
        var testEntity = new TestEntity() { Name = "Test User", IsActive = true };
        _context.Entities.Add(testEntity);
        _context.SaveChanges();

        // When
        testEntity.Name = "Other test user";
        _context.Attach(testEntity);
        _context.SaveChanges();

        // Then
        _context.ChangeTracker.Entries<ChangeEvent>().First().Entity.NewData.ShouldNotBeNull();
    }
    
    [Fact]
    public void GivenDeleted_WhenSavingChanges_ShouldNotSetNewData()
    {
        // Given
        var testEntity = new TestEntity() { Name = "Test User", IsActive = true };
        _context.Entities.Add(testEntity);
        _context.SaveChanges();

        // When
        _context.Remove(testEntity);
        _context.SaveChanges();

        // Then
        _context.ChangeTracker.Entries<ChangeEvent>().First().Entity.NewData.ShouldBeNull();
    }
}