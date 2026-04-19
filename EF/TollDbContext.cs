using Entities;
using Entities.Bases;
using Microsoft.EntityFrameworkCore;

namespace EFCore;

public class TollDbContext(DbContextOptions<TollDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TollDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<TollInvoice> TollInvoices => Set<TollInvoice>();
    public DbSet<TollEvent> TollEvents => Set<TollEvent>();
    public DbSet<DailyTollSummary> DailyTollSummaries => Set<DailyTollSummary>();
}
