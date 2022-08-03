using System.Text.Json;
using Shouldly;
using Xunit;

namespace EntityFrameworkCore.ChangeEvents.Tests;

public class EntityEntryExtensionTests
{
    private readonly TestContext _context;
    private readonly ChangeEventOptions _options;

    public EntityEntryExtensionTests()
    {
        _context = new TestContext(new());
        _context.Database.EnsureCreated();

        _options = new();
    }
    
    [Fact]
    public void GivenEntityEntry_WhenGettingOldState_ShouldReturnSerialized()
    {
        // Given
        var entity = new TestEntity() { Name = "Test entity", IsActive = true };
        var entityEntry = _context.Entities.Add(entity);

        // When
        var state = entityEntry.GetOldState(_options);

        // Then
        var deserializedEntity = JsonSerializer.Deserialize<TestEntity>(state);
        deserializedEntity.Name.ShouldBe(entity.Name);
        deserializedEntity.IsActive.ShouldBe(entity.IsActive);
    }
    
    [Fact]
    public void GivenEntityEntry_WhenGettingOldState_ShouldNotSerializeKeys()
    {
        // Given
        var entity = new TestEntity() { Name = "Test entity", IsActive = true };
        var entityEntry = _context.Entities.Add(entity);
        
        // When
        _options.OmitPrimaryKeys = true;
        _options.OmitForeignKeys = true;
        var state = entityEntry.GetOldState(_options);

        // Then
        var deserializedEntity = JsonSerializer.Deserialize<TestEntity>(state);
        deserializedEntity.Id.ShouldBeNull();
    }
    
    [Fact]
    public void GivenEntityEntry_WhenGettingOldState_ShouldSerializeKeys()
    {
        // Given
        var entity = new TestEntity() { Name = "Test entity", IsActive = true };
        var entityEntry = _context.Entities.Add(entity);
        
        // When
        _options.OmitPrimaryKeys = false;
        _options.OmitForeignKeys = false;
        var state = entityEntry.GetOldState(_options);

        // Then
        var deserializedEntity = JsonSerializer.Deserialize<TestEntity>(state);
        deserializedEntity.Id.ShouldNotBeNull();
    }
    
    [Fact]
    public void GivenEntityEntry_WhenGettingNewState_ShouldReturnSerialized()
    {
        // Given
        var entity = new TestEntity() { Name = "Test entity", IsActive = true };
        var entityEntry = _context.Entities.Add(entity);

        // When
        var state = entityEntry.GetNewState(_options);

        // Then
        var deserializedEntity = JsonSerializer.Deserialize<TestEntity>(state);
        deserializedEntity.Name.ShouldBe(entity.Name);
        deserializedEntity.IsActive.ShouldBe(entity.IsActive);
    }

    [Fact]
    public void GivenEntityEntry_WhenGettingNewState_ShouldNotSerializeKeys()
    {
        // Given
        var entity = new TestEntity() { Name = "Test entity", IsActive = true };
        var entityEntry = _context.Entities.Add(entity);

        // When
        _options.OmitPrimaryKeys = true;
        _options.OmitForeignKeys = true;
        var state = entityEntry.GetOldState(_options);

        // Then
        var deserializedEntity = JsonSerializer.Deserialize<TestEntity>(state);
        deserializedEntity.Id.ShouldBeNull();
    }
    
    [Fact]
    public void GivenEntityEntry_WhenGettingNewState_ShouldSerializeKeys()
    {
        // Given
        var entity = new TestEntity() { Name = "Test entity", IsActive = true };
        var entityEntry = _context.Entities.Add(entity);

        // When
        _options.OmitPrimaryKeys = false;
        _options.OmitForeignKeys = false;
        var state = entityEntry.GetOldState(_options);

        // Then
        var deserializedEntity = JsonSerializer.Deserialize<TestEntity>(state);
        deserializedEntity.Id.ShouldNotBeNull();
    }
    
    [Fact]
    public void GivenEntityEntry_WhenGettingPrimaryKey_ShouldReturn()
    {
        // Given
        var entity = new TestEntity() { Name = "Test entity", IsActive = true };
        var entityEntry = _context.Entities.Add(entity);

        // When
        var state = entityEntry.PrimaryKey();

        // Then
        state.ShouldBe(entityEntry.Entity.Id.ToString());
    }
}