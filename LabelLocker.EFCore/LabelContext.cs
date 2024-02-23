using LabelLocker.Repositories.Entities;
using Microsoft.EntityFrameworkCore;

namespace LabelLocker.EFCore;

/// <summary>
/// Represents the database context used for accessing and managing label entities.
/// </summary>
public class LabelContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LabelContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by the DbContext.</param>
    public LabelContext(DbContextOptions<LabelContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the DbSet for <see cref="LabelEntity"/> entities.
    /// </summary>
    public DbSet<LabelEntity> Labels { get; set; } = null!;

    // If you have overridden any DbContext methods like OnModelCreating, you can document those here as well.
    /// <summary>
    /// Configures the schema needed for the label management context.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<LabelEntity>()
            .HasIndex(e => e.Name)
            .IsUnique();
    }
}
