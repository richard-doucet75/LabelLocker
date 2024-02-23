using LabelLocker.EFCore;
using LabelLocker.Repositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace LabelLocker.UnitTests;

[Trait("Category", "Unit Test")]
[Trait("Category", "Integration Test")]
public sealed class LabelServiceTests : IDisposable
{
    private readonly ILabelService _service;
    private readonly LabelRepository _repository;
    private readonly LabelContext _labelContext;

    public LabelServiceTests()
    {


        _labelContext = new LabelContext(new DbContextOptionsBuilder<LabelContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options);
        
        _repository = new LabelRepository(_labelContext);  

        _service = new LabelService(_repository);
    }
    
    public void Dispose()
    {
        _labelContext.Database.EnsureDeleted();
        _labelContext.Dispose();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public async Task ReserveLabelAsync_NullOrWhitespaceName_ReturnsFailureResult(string labelName)
    {
        var result = await _service.ReserveLabelAsync(labelName);
        
        Assert.False(result.Success);
        Assert.Equal("Label name cannot be null or whitespace.", result.ErrorMessage);
    }

    [Fact]
    public async Task ReserveLabelAsync_LabelDoesNotExist_ShouldReserveSuccessfully()
    {
        const string labelName = "unique-label";

        var result = await _service.ReserveLabelAsync(labelName);

        Assert.True(result.Success);
        var reservedLabel = await _repository.FindLabelAsync(labelName);
        Assert.NotNull(reservedLabel);
        Assert.Equal(LabelState.Reserved, reservedLabel.State);
    }

    [Fact]
    public async Task ReserveLabelAsync_AlreadyReservedLabel_ShouldReturnFailureResult()
    {
        // Arrange
        const string labelName = "reserved-label";

        // Seed the database with a reserved label
        
        await _repository.AddOrUpdateLabelAsync(new LabelEntity
        {
            Name = labelName,
            State = LabelState.Reserved
            // Assuming ReservationToken is set during the reservation
        });

        
        // Act
        // Attempt to reserve the already reserved label again
        var result = await _service.ReserveLabelAsync(labelName);

        // Assert
        // Expecting failure due to the label already being reserved
        Assert.False(result.Success);
        // Optionally check for a specific error message if your implementation provides one

        // Verify the label's state remains Reserved
        var labelEntity = await _repository.FindLabelAsync(labelName);
        Assert.NotNull(labelEntity);
        Assert.Equal(LabelState.Reserved, labelEntity.State);
    }

    [Fact]
    public async Task ReserveLabelAsync_AvailableExistingLabel_ShouldReserveSuccessfully()
    {
        // Arrange
        const string labelName = "available-label";

        // Seed the database with an available label
        await _repository.AddOrUpdateLabelAsync(new LabelEntity 
        { 
            Name = labelName, 
            State = LabelState.Available,
        });

        // Act
        // Attempt to reserve the already available label
        var result = await _service.ReserveLabelAsync(labelName);

        // Assert
        // The operation should succeed, indicating the label has been reserved successfully
        Assert.True(result.Success);
        Assert.NotNull(result.ReservationToken); // Ensure a new reservation token is provided

        // Optionally, verify the label's state has changed to Reserved
        var labelEntity = await _repository.FindLabelAsync(labelName);
        Assert.NotNull(labelEntity);
        Assert.Equal(LabelState.Reserved, labelEntity.State); // State should be updated to Reserved
    }
    
    [Fact]
    public async Task ReleaseLabelAsync_LabelDoesNotExist_ShouldReturnSuccessResult()
    {
        var result = await _service.ReleaseLabelAsync("nonexistent-label", new byte[] { });

        Assert.True(result.Success);
    }

    [Fact]
    public async Task ReleaseLabelAsync_WithReservationTokenMismatch_ShouldReturnFailureResult()
    {
        const string labelName = "test-label";
        await _repository.AddOrUpdateLabelAsync(
            new LabelEntity
            {
                Name = labelName,
                State = LabelState.Reserved,
            });
        

        var mismatchReservationToken = new byte[] { 5, 6, 7, 8 };

        var result = await _service.ReleaseLabelAsync(labelName, mismatchReservationToken);

        // Expected to return false due to ReservationToken mismatch indicating a concurrency conflict.
        Assert.False(result.Success); 
    }
    
    [Fact]
    public async Task ReleaseLabelAsync_AvailableExistingLabel_ShouldReturnSuccessWithoutStateChange()
    {
        // Arrange
        const string labelName = "already-available-label";
        

        // Seed the database with an available label including a reservation token
        var initialReservationToken = await _repository.AddOrUpdateLabelAsync(
            new LabelEntity
            {
                Name = labelName,
                State = LabelState.Available, // The label is already in an available state
            });
        
        // Act
        // Attempt to release the already available label
        var result = await _service.ReleaseLabelAsync(labelName, initialReservationToken!);

        // Assert
        // The operation should return true, indicating success, but the label's state should remain unchanged
        Assert.True(result.Success);

        // Optionally, verify the label's state remains Available
        var labelEntity = await _repository.FindLabelAsync(labelName);
        Assert.NotNull(labelEntity);
        Assert.Equal(LabelState.Available, labelEntity.State); // State should still be Available
    }
    
    [Fact]
    public async Task ReleaseLabelAsync_ReservedLabelWithCorrectReservationToken_ShouldReturnSuccessAndMakeLabelAvailable()
    {
        // Arrange
        const string labelName = "reserved-label";

        // Seed the database with a label that is already reserved and has a known reservation token
        var correctReservationToken = await _repository.AddOrUpdateLabelAsync(
            new LabelEntity
            {
                Name = labelName,
                State = LabelState.Reserved,
            });

        // Act
        // Attempt to release the reserved label using the correct reservation token
        var result = await _service.ReleaseLabelAsync(labelName, correctReservationToken!);

        // Assert
        // The operation should return true, indicating the release was successful
        Assert.True(result.Success);

        // Optionally, verify the label's state has changed to Available
        var labelEntity = await _repository.FindLabelAsync(labelName);
        Assert.NotNull(labelEntity);
        Assert.Equal(LabelState.Available, labelEntity.State); // State should be updated to Available
    }
    
    [Fact]
    public async Task ReserveLabelAsync_ShouldTreatLabelsCaseInsensitively()
    {
        const string labelName = "test-label";
        const string labelNameDifferentCase = "Test-Label";

        // Reserve the label with one casing
        var reservationResult = await _service.ReserveLabelAsync(labelName);
        Assert.True(reservationResult.Success);

        // Try to reserve the same label with different casing
        var reservationResultDifferentCase = await _service.ReserveLabelAsync(labelNameDifferentCase);
        Assert.False(reservationResultDifferentCase.Success, "Label was reserved again with different case, which should not happen if treated case-insensitively.");
    }

    [Fact]
    public async Task ReleaseLabelAsync_ShouldTreatLabelsCaseInsensitively()
    {
        const string labelName = "another-label";
        // Reserve the label first
        var reservationResult = await _service.ReserveLabelAsync(labelName);
        Assert.True(reservationResult.Success);

        // Attempt to release with different casing
        const string labelNameDifferentCase = "Another-Label";
        var releaseResult = await _service.ReleaseLabelAsync(labelNameDifferentCase, reservationResult.ReservationToken!);

        Assert.True(releaseResult.Success, "Failed to release label with different case, which should succeed if treated case-insensitively.");
    }

    [Fact]
    public async Task CaseInsensitivity_AcrossReserveAndReleaseOperations()
    {
        const string labelName = "MixedCaseLabel";
        // Reserve the label with mixed casing
        var reservationResult = await _service.ReserveLabelAsync(labelName);
        Assert.True(reservationResult.Success);

        // Convert label name to lower case for release
        var labelNameLowerCase = labelName.ToLowerInvariant();
        var releaseResult = await _service.ReleaseLabelAsync(labelNameLowerCase, reservationResult.ReservationToken!);

        Assert.True(releaseResult.Success, "The label could not be released with different case, indicating case sensitivity issues.");
    }
}
