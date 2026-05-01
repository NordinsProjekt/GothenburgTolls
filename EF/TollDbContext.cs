using Entities.Bases;
using Entities.Tolls;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EFCore;

public class TollDbContext(DbContextOptions<TollDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TollDbContext).Assembly);
        base.OnModelCreating(modelBuilder);

        // SQLite has no native datetimeoffset type and cannot translate comparisons/ordering on it.
        // Use the built-in binary converter so DateTimeOffset values are stored as sortable longs.
        if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
        {
            var converter = new DateTimeOffsetToBinaryConverter();

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTimeOffset) || property.ClrType == typeof(DateTimeOffset?))
                    {
                        property.SetValueConverter(converter);
                    }
                }
            }
        }
    }

    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<TollInvoice> TollInvoices => Set<TollInvoice>();
    public DbSet<TollEvent> TollEvents => Set<TollEvent>();
    public DbSet<DailyTollSummary> DailyTollSummaries => Set<DailyTollSummary>();
}
