using LabelLocker.Repositories.Entities;

namespace LabelLocker.Repositories
{
    /// <summary>
    /// Interface for repository operations related to labels.
    /// </summary>
    public interface ILabelRepository
    {
        /// <summary>
        /// Finds a label by its name asynchronously.
        /// </summary>
        /// <param name="label">The name of the label.</param>
        /// <returns>A task that represents the asynchronous find operation. The task result contains the found LabelEntity or null if no label is found.</returns>
        Task<LabelEntity?> FindLabelAsync(string label);

        /// <summary>
        /// Adds or updates a label entity in the repository and returns the updated reservation token.
        /// If the entity does not exist, it is added; if it exists, it is updated.
        /// </summary>
        /// <param name="labelEntity">The label entity to add or update.</param>
        /// <returns>A task that represents the asynchronous add or update operation. The task result contains the updated reservation token if the operation was successful, or null if there was a concurrency conflict.</returns>
        Task<byte[]?> AddOrUpdateLabelAsync(LabelEntity labelEntity);
    }
}