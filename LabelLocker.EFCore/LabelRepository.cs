using LabelLocker.Repositories;
using LabelLocker.Repositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace LabelLocker.EFCore;

/// <summary>
/// Represents a repository for managing label entities using Entity Framework Core.
/// </summary>
public class LabelRepository : ILabelRepository
{
    private readonly LabelContext _context;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="LabelRepository"/> class.
    /// </summary>
    /// <param name="context">The database context to be used for label operations.</param>
    public LabelRepository(LabelContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Asynchronously finds a label by its name.
    /// </summary>
    /// <param name="label">The name of the label to find.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the found <see cref="LabelEntity"/> or null if no label is found.</returns>

    public async Task<LabelEntity?> FindLabelAsync(string label)
    {
        return await _context
            .Labels
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Name == label);
    }

    /// <summary>
    /// Asynchronously adds a new label or updates an existing label in the database.
    /// </summary>
    /// <param name="labelEntity">The label entity to add or update.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the reservation token of the label if the operation is successful; null if a concurrency conflict occurs.</returns>
    public async Task<byte[]?> AddOrUpdateLabelAsync(LabelEntity labelEntity)
    {
        var existingEntity =
            await _context
                .Labels
                .FirstOrDefaultAsync(l => l.Name == labelEntity.Name);

        if (existingEntity == null)
        {
            _context.Labels.Add(labelEntity);
        }
        else
        {
            _context.Entry(existingEntity).CurrentValues.SetValues(labelEntity);
            
            // Ensure state is explicitly updated.
            existingEntity.State = labelEntity.State; 
        }

        try
        {
            await _context.SaveChangesAsync();
            return (existingEntity ?? labelEntity).ReservationToken;
        }
        catch (DbUpdateConcurrencyException)
        {
            // Indicate a concurrency conflict.
            return null; 
        }
    }
}
