namespace LabelLocker;

/// <summary>
/// Defines operations for managing label reservations and releases.
/// </summary>
public interface ILabelService
{
    /// <summary>
    /// Attempts to reserve a label with the specified name.
    /// </summary>
    /// <param name="label">The name of the label to reserve.</param>
    /// <param name="clientRowVersion">The current row version of the label for concurrency control.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether the label was successfully reserved.</returns>
    Task<bool> ReserveLabelAsync(string label, byte[] clientRowVersion);

    /// <summary>
    /// Attempts to release a previously reserved label, making it available again.
    /// </summary>
    /// <param name="label">The name of the label to release.</param>
    /// <param name="clientRowVersion">The current row version of the label for concurrency control.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean value indicating whether the label was successfully released.</returns>
    Task<bool> ReleaseLabelAsync(string label, byte[] clientRowVersion);
}