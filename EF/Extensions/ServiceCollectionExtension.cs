using EF.Repositories;
using Entities.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace EF.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddEfRepositories(this IServiceCollection services)
    {
        services.AddScoped<IVehicleRepository, VehicleRepository>();
        services.AddScoped<ITollEventRepository, TollEventRepository>();
        services.AddScoped<IDailyTollSummaryRepository, DailyTollSummaryRepository>();
        services.AddScoped<ITollInvoiceRepository, TollInvoiceRepository>();

        return services;
    }
}
