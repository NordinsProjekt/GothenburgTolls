
using EF;
using EF.Extensions;
using Microsoft.EntityFrameworkCore;

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

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        app.UseHttpsRedirection();
        app.UseAntiforgery();

        app.UseAuthorization();

        app.MapPost("/TollEvent", (HttpContext httpContext) =>
        {

        })
        .WithName("PostTollEvent");

        app.Run();
    }
}
