using LabelLocker.EFCore;
using LabelLocker.Repositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace LabelLocker.Tests;

[Trait("Unit Test", "LabelService")]
public class LabelServiceTests
{
    private static LabelContext GetInMemoryLabelContext()
    {
        var options = new DbContextOptionsBuilder<LabelContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Use a unique name to ensure a fresh database for each test
            .Options;

        var context = new LabelContext(options);
        context.Database.EnsureCreated(); // Ensure the in-memory database is created
        return context;
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public async Task ReserveLabelAsync_NullOrWhitespaceName_ThrowsArgumentException(string labelName)
    {
        // Arrange
        await using var context = GetInMemoryLabelContext();
        var service = new LabelService(new LabelRepository(context));
        
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.ReserveLabelAsync(labelName, new byte[] { 1, 2, 3 }));
        Assert.Equal("Label name cannot be null or whitespace.", exception.Message);
    }
    
    [Fact]
    public async Task ReserveLabelAsync_LabelDoesNotExist_ShouldReserveSuccessfully()
    {
        // Arrange
        const string labelName = "uniqueLabel";
        await using var context = GetInMemoryLabelContext();
        var labelService = new LabelService(new LabelRepository(context));

        // Act
        var result = await labelService.ReserveLabelAsync(labelName, Array.Empty<byte>());

        // Assert
        Assert.True(result);
        var reservedLabel = await context.Labels.FirstOrDefaultAsync(l => l.Name == labelName);
        Assert.NotNull(reservedLabel);
        Assert.Equal(LabelState.Reserved, reservedLabel.State);
    }

    [Fact]
    public async Task ReserveLabelAsync_LabelExists_ShouldReturnFalse()
    {
        // Arrange
        const string labelName = "existingLabel";
        await using var context = GetInMemoryLabelContext();
        context.Labels.Add(new LabelEntity { Name = labelName, State = LabelState.Reserved });
        await context.SaveChangesAsync();

        var labelService = new LabelService(new LabelRepository(context));

        // Act
        var result = await labelService.ReserveLabelAsync(labelName, Array.Empty<byte>());

        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public async Task ReserveLabelAsync_LabelAlreadyReserved_ShouldReturnFalse()
    {
        await using var context = GetInMemoryLabelContext();
        var service = new LabelService(new LabelRepository(context));
        const string label = "testLabel";
        await context.Labels.AddAsync(new LabelEntity { Name = label, State = LabelState.Reserved });
        await context.SaveChangesAsync();

        // Attempt to reserve an already reserved label
        var result = await service.ReserveLabelAsync(label, new byte[] { });

        Assert.False(result);
    }

    [Fact]
    public async Task ReleaseLabelAsync_LabelDoesNotExist_ShouldReturnTrue()
    {
        await using var context = GetInMemoryLabelContext();
        var service = new LabelService(new LabelRepository(context));

        // Attempt to release a label that does not exist
        var result = await service.ReleaseLabelAsync("nonexistentLabel", new byte[] { });

        Assert.True(result);
    }

    [Fact]
    public async Task ReserveLabelAsync_WithConcurrencyConflict_ShouldReturnFalse()
    {
        await using var context = GetInMemoryLabelContext();
        var service = new LabelService(new LabelRepository(context));
        const string labelName = "ConcurrentLabel";
        var initialVersion = new byte[] { 1, 2, 3, 4 };
        var conflictingVersion = new byte[] { 5, 6, 7, 8 };

        // Simulate existing label with initial RowVersion
        await context.Labels.AddAsync(new LabelEntity { Name = labelName, State = LabelState.Available, RowVersion = initialVersion });
        await context.SaveChangesAsync();

        // Attempt to reserve with a conflicting RowVersion
        var result = await service.ReserveLabelAsync(labelName, conflictingVersion);

        Assert.False(result);
    }
    
    [Fact]
    public async Task ReserveLabelAsync_AfterFailedAttemptDueToConcurrency_ShouldSucceed()
    {
        await using var context = GetInMemoryLabelContext();
        var service = new LabelService(new LabelRepository(context));
        const string labelName = "RetryLabel";
        var initialVersion = new byte[] { 1, 2, 3, 4 };

        // Initial reservation attempt simulating existing label
        await context.Labels.AddAsync(new LabelEntity { Name = labelName, State = LabelState.Available, RowVersion = initialVersion });
        await context.SaveChangesAsync();

        // First attempt with mismatched RowVersion simulating concurrency conflict
        var failedAttempt = await service.ReserveLabelAsync(labelName, new byte[] { 5, 6, 7, 8 });

        Assert.False(failedAttempt); // Expecting failure due to RowVersion conflict

        // Simulating client fetching the latest version and retrying
        var latestLabel = await context.Labels.AsNoTracking().FirstAsync(l => l.Name == labelName);
        var successfulAttempt = await service.ReserveLabelAsync(labelName, latestLabel.RowVersion);

        Assert.True(successfulAttempt); // Expecting success on retry with correct RowVersion
    }
    
    [Fact]
    public async Task ReleaseLabelAsync_WithInvalidLabelName_ShouldReturnTrue()
    {
        await using var context = GetInMemoryLabelContext();
        var service = new LabelService(new LabelRepository(context));
        const string invalidLabelName = "invalid"; // Assuming future validation for empty or null names

        var result = await service.ReleaseLabelAsync(invalidLabelName, new byte[] { });

        // Assuming the method would return false or throw a specific exception for invalid names
        Assert.True(result);
    }
    
    [Fact]
    public async Task ReleaseLabelAsync_LabelAlreadyAvailable_ShouldReturnTrue()
    {
        await using var context = GetInMemoryLabelContext();
        var service = new LabelService(new LabelRepository(context));
        const string label = "availableLabel";
        await context.Labels.AddAsync(new LabelEntity { Name = label, State = LabelState.Available });
        await context.SaveChangesAsync();

        // Attempt to release an already available label
        var result = await service.ReleaseLabelAsync(label, new byte[] { });

        Assert.True(result);
    }

    [Fact]
    public async Task ReleaseLabelAsync_WithRowVersionMismatch_ShouldReturnFalse()
    {
        // Arrange
        const string labelName = "TestLabel";
        var initialRowVersion = new byte[] { 1, 2, 3, 4 };
        var mismatchRowVersion = new byte[] { 5, 6, 7, 8 }; // Simulate a client's outdated RowVersion
        
        await using var context = GetInMemoryLabelContext();
        // Seed the label with an initial state and RowVersion
        await context.Labels.AddAsync(new LabelEntity 
        { 
            Name = labelName, 
            State = LabelState.Reserved, 
            RowVersion = initialRowVersion 
        });
        await context.SaveChangesAsync();

        var labelService = new LabelService(new LabelRepository(context));

        // Act
        // Attempt to release the label with a RowVersion that mismatches the stored version
        var result = await labelService.ReleaseLabelAsync(labelName, mismatchRowVersion);

        // Assert
        Assert.False(result); // Expect the method to return false indicating a concurrency conflict

        // Optional: Verify the label state remains unchanged
        var labelEntity = await context.Labels.AsNoTracking().FirstOrDefaultAsync(l => l.Name == labelName);
        Assert.NotNull(labelEntity);
        Assert.Equal(LabelState.Reserved, labelEntity.State); // State should remain reserved
    }
}