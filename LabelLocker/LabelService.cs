using LabelLocker.Repositories;
using LabelLocker.Repositories.Entities;

namespace LabelLocker;

/// <summary>
/// Service for managing label reservations and releases.
/// </summary>
public class LabelService : ILabelService
{
    private readonly ILabelRepository _labelRepository;

    /// <summary>
    /// Initializes a new instance of the LabelService class.
    /// </summary>
    /// <param name="labelRepository">The repository used by this service.</param>
    public LabelService(ILabelRepository labelRepository)
    {
        _labelRepository = labelRepository;
    }
    
    /// <summary>
    /// Attempts to reserve a label.
    /// </summary>
    /// <param name="label">The name of the label to reserve.</param>
    /// <param name="clientRowVersion">The RowVersion of the label from the client, used for optimistic concurrency control.</param>
    /// <returns>True if the label was successfully reserved, false otherwise (e.g., if the label is already reserved or there's a RowVersion mismatch).</returns>
    public async Task<bool> ReserveLabelAsync(string label, byte[] clientRowVersion)
    {
        var labelEntity = await _labelRepository.FindLabelAsync(label);
        if (labelEntity == null)
        {
            labelEntity = new LabelEntity { Name = label, State = LabelState.Reserved, RowVersion = clientRowVersion };
            // Directly return the result of the update operation, which now includes saving changes.
            return await _labelRepository.UpdateLabelAsync(labelEntity);
        }

        if (!labelEntity.RowVersion.SequenceEqual(clientRowVersion))
        {
            return false; // Concurrency conflict
        }

        labelEntity.State = LabelState.Reserved;
        return await _labelRepository.UpdateLabelAsync(labelEntity);
    }
    
    /// <summary>
    /// Attempts to release a label, making it available again.
    /// </summary>
    /// <param name="label">The name of the label to release.</param>
    /// <param name="clientRowVersion">The RowVersion of the label from the client, used for optimistic concurrency control.</param>
    /// <returns>True if the label was successfully released or if the label does not exist, false otherwise (e.g., if there's a RowVersion mismatch).</returns>
    public async Task<bool> ReleaseLabelAsync(string label, byte[] clientRowVersion)
    {
        var labelEntity = await _labelRepository.FindLabelAsync(label);

        // If the label does not exist, it's considered a success because 
        // the label is effectively in an 'Available' state from a business logic perspective.
        if (labelEntity == null)
        {
            return true;
        }

        // Check for RowVersion mismatch to handle concurrency.
        if (!labelEntity.RowVersion.SequenceEqual(clientRowVersion))
        {
            // If the RowVersion doesn't match, return false to indicate a concurrency conflict.
            return false;
        }

        // Proceed to release the label if it's currently reserved.
        if (labelEntity.State == LabelState.Reserved)
        {
            labelEntity.State = LabelState.Available;
            // Use the repository to update the label's state and save the changes.
            return await _labelRepository.UpdateLabelAsync(labelEntity);
        }

        // If the label is not in a 'Reserved' state, no action is needed, and it's considered a success.
        return true;
    }
}
