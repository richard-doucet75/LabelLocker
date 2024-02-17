using LabelLocker.Repositories.Entities;

namespace LabelLocker.Repositories;

/// <summary>
/// Interface for repository operations related to labels.
/// </summary>
public interface ILabelRepository
{
    /// <summary>
    /// Finds a label by its name.
    /// </summary>
    /// <param name="label">The name of the label.</param>
    /// <returns>The found label entity or null if not found.</returns>
    Task<LabelEntity?> FindLabelAsync(string label);

    /// <summary>
    /// Updates a label entity in the repository. This includes adding new labels or updating existing ones.
    /// </summary>
    /// <param name="labelEntity">The label entity to update.</param>
    /// <returns>True if the operation was successful, false if there was a concurrency conflict.</returns>
    Task<bool> UpdateLabelAsync(LabelEntity labelEntity);
}


