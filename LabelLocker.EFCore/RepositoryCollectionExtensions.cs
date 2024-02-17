using LabelLocker.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LabelLocker.EFCore;

public static class RepositoryCollectionExtensions
{
    /// <summary>
    /// Adds repositories related to Label Management to the specified IServiceCollection.
    /// Allows for flexible database provider configuration.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="configureDbContext">A delegate to configure the DbContext with a database provider.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IServiceCollection AddLabelManagementEntityFrameworkRepositories(
        this IServiceCollection services, 
        Action<DbContextOptionsBuilder> configureDbContext)
    {
        // Register the DbContext with flexible database provider configuration
        services.AddDbContext<LabelContext>(configureDbContext);

        // Register the LabelRepository as an implementation of ILabelRepository
        services.AddScoped<ILabelRepository, LabelRepository>();

        return services;
    }
}