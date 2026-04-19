using Microsoft.Extensions.DependencyInjection;
using UseCases.HelperClass;
using UseCases.Interfaces;
using UseCases.Services;

namespace UseCases.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        services.AddScoped<IVehicleService, VehicleService>();
        services.AddScoped<ITollEventService, TollEventService>();
        services.AddSingleton<ISwedishHolidayService, SwedishHolidayService>();
        services.AddTransient<TollCalculator>();

        return services;
    }
}
