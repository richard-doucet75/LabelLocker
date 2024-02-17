namespace LabelLocker;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Label Management services to the specified IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IServiceCollection AddLabelManagementServices(this IServiceCollection services)
    {
        // Register the LabelService as an implementation of ILabelService
        services.AddScoped<ILabelService, LabelService>();

        return services;
    }
}
