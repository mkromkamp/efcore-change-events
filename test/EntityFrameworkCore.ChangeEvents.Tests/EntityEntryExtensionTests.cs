using System.Linq;
using System.Text.Json;
using Shouldly;
using Xunit;

namespace EntityFrameworkCore.ChangeEvents.Tests;

public class EntityEntryExtensionTests
{
    private readonly TestContext _context;
    private readonly JsonSerializerOptions _serializerOptions;

    public EntityEntryExtensionTests()
    {
        _context = new TestContext(new());
        _context.Database.EnsureCreated();

        _serializerOptions = new();
    }
    
    [Fact]
    public void GivenEntityEntry_WhenGettingOldState_ShouldReturnSerialized()
    {
        // Given
        var entity = new TestEntity() { Name = "Test entity", IsActive = true };
        var entityEntry = _context.Entities.Add(entity);

        // When
        var state = entityEntry.GetOldState(_serializerOptions);

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
        var state = entityEntry.GetOldState(_serializerOptions);

        // Then
        var deserializedEntity = JsonSerializer.Deserialize<TestEntity>(state);
        deserializedEntity.Id.ShouldBeNull();
    }
    
    [Fact]
    public void GivenEntityEntry_WhenGettingNewState_ShouldReturnSerialized()
    {
        // Given
        var entity = new TestEntity() { Name = "Test entity", IsActive = true };
        var entityEntry = _context.Entities.Add(entity);

        // When
        var state = entityEntry.GetNewState(_serializerOptions);

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
        var state = entityEntry.GetOldState(_serializerOptions);

        // Then
        var deserializedEntity = JsonSerializer.Deserialize<TestEntity>(state);
        deserializedEntity.Id.ShouldBeNull();
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