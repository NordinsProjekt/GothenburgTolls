using EFCore;
using EFCore.Extensions;
using Microsoft.EntityFrameworkCore;
using UseCases.Dtos;
using UseCases.Extensions;
using UseCases.Interfaces;

namespace LogVehicleAPI;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddAuthorization();

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        builder.Services.AddDbContextFactory<TollDbContext>(opt =>
            opt.UseSqlServer(builder.Configuration.GetConnectionString("TollDb")));
        builder.Services.AddEfRepositories();
        builder.Services.AddUseCases();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapPost("/TollEvent", async (
                VehiclePassageDto dto,
                ITollEventService tollEventService,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    var tollEvent = await tollEventService.RegisterAsync(dto, cancellationToken);
                    return Results.Created($"/TollEvent/{tollEvent.Id}", new { tollEvent.Id });
                }
                catch (ArgumentNullException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
                catch (ArgumentException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }
                catch (DbUpdateException ex)
                {
                    var logger = app.Services.GetRequiredService<ILoggerFactory>()
                        .CreateLogger("TollEventEndpoint");
                    logger.LogError(ex, "Database error while registering toll event");
                    return Results.Conflict(new { error = "A database conflict occurred." });
                }
            })
            .WithName("PostTollEvent");

        if (app.Environment.IsDevelopment())
        {
            app.MapPost("/seed", async (
                    ITollEventService tollEventService,
                    CancellationToken cancellationToken) =>
                {
                    var created = new List<object>();
                    foreach (VehiclePassageDto dto in SeedData.Passages)
                    {
                        var tollEvent = await tollEventService.RegisterAsync(dto, cancellationToken);
                        created.Add(new { tollEvent.Id, dto.RegistrationNumber, dto.EventDateTime, dto.Zone, dto.VehicleType });
                    }

                    return Results.Ok(new { count = created.Count, events = created });
                })
                .WithName("SeedTestData");
        }

        app.Run();
    }
}
