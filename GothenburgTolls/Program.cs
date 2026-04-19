using EFCore;
using EFCore.Extensions;
using GothenburgTolls.Components;
using Microsoft.EntityFrameworkCore;
using UseCases.Extensions;

namespace GothenburgTolls;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        builder.Services.AddDbContextFactory<TollDbContext>(opt =>
            opt.UseSqlServer(builder.Configuration.GetConnectionString("TollDb")));
        builder.Services.AddEfRepositories();
        builder.Services.AddUseCases();

        var app = builder.Build();

        using (IServiceScope scope = app.Services.CreateScope())
        {
            TollDbContext db = scope.ServiceProvider.GetRequiredService<IDbContextFactory<TollDbContext>>().CreateDbContext();
            db.Database.Migrate();
        }

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        app.UseHttpsRedirection();

        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }
}
