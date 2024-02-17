using LabelLocker.Repositories;
using LabelLocker.Repositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace LabelLocker.EFCore;

/// <summary>
/// Repository for performing operations on label entities within the database.
/// </summary>
public class LabelRepository : ILabelRepository
{
    private readonly LabelContext _context;

    /// <summary>
    /// Initializes a new instance of the LabelRepository class.
    /// </summary>
    /// <param name="context">The database context to be used for label operations.</param>
    public LabelRepository(LabelContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Finds a label by its name asynchronously.
    /// </summary>
    /// <param name="label">The name of the label to find.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the found LabelEntity or null if no label is found.</returns>
    public async Task<LabelEntity?> FindLabelAsync(string label)
    {
        return await _context.Labels.AsNoTracking().FirstOrDefaultAsync(l => l.Name == label);
    }

    /// <summary>
    /// Updates a label entity in the database asynchronously. If the entity does not exist, it is added.
    /// </summary>
    /// <param name="labelEntity">The label entity to update or add.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is true if the update or addition was successful, false if a concurrency conflict occurred.</returns>
    public async Task<bool> UpdateLabelAsync(LabelEntity labelEntity)
    {
        var existingEntity = await _context.Labels.FirstOrDefaultAsync(l => l.Id == labelEntity.Id);
        if (existingEntity != null)
        {
            _context.Entry(existingEntity).CurrentValues.SetValues(labelEntity);
            _context.Entry(existingEntity).State = EntityState.Modified;
        }
        else
        {
            _context.Labels.Add(labelEntity);
        }

        try
        {
            await _context.SaveChangesAsync();
            return true; // Changes saved successfully.
        }
        catch (DbUpdateConcurrencyException)
        {
            return false; // Concurrency conflict detected.
        }
    }
}
