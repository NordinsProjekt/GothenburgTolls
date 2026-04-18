using Entities;
using Entities.Bases;
using Microsoft.EntityFrameworkCore;

namespace EF;

public class TollDbContext : DbContext
{

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TollDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<TollInvoice> TollInvoices { get; set; }
    public DbSet<TollEvent> TollEvents { get; set; }
    public DbSet<DailyTollSummary> DailyTollSummaries { get; set; }
}
