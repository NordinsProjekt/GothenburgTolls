using EFCore;
using EFCore.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UseCases.Extensions;

namespace AzureTimedFunctions;

public class Program
{
    public static void Main()
    {
        var host = new HostBuilder()
            .ConfigureFunctionsWorkerDefaults()
            .ConfigureServices((context, services) =>
            {
                string connectionString = context.Configuration.GetConnectionString("TollDb")
                    ?? throw new InvalidOperationException("Connection string 'TollDb' not found.");

                services.AddDbContextFactory<TollDbContext>(opt =>
                    opt.UseSqlServer(connectionString));

                services.AddEfRepositories();
                services.AddUseCases();
            })
            .Build();

        host.Run();
    }
}
