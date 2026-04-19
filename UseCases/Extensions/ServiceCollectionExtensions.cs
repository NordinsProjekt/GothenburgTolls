using Microsoft.Extensions.DependencyInjection;
using UseCases.Interfaces;
using UseCases.Services;

namespace UseCases.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddScoped<IVehicleService, VehicleService>();
        services.AddScoped<ITollEventService, TollEventService>();

        return services;
    }
}
