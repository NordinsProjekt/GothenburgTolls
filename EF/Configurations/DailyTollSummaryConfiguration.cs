using Entities.Tolls;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore.Configurations;

internal class DailyTollSummaryConfiguration : IEntityTypeConfiguration<DailyTollSummary>
{
    public void Configure(EntityTypeBuilder<DailyTollSummary> e)
    {
        e.HasKey(dts => dts.Id);

        e.Property(dts => dts.ForDay).IsRequired();
        e.Property(dts => dts.Created).IsRequired();
        e.Property(dts => dts.Amount).HasPrecision(18, 2);
        e.Property(dts => dts.VehicleId).IsRequired();

        e.HasIndex(x => new { x.VehicleId, x.ForDay }).IsUnique();
        e.HasIndex(dts => dts.TollInvoiceId);

        e.HasOne(dts => dts.Vehicle)
         .WithMany()
         .HasForeignKey(dts => dts.VehicleId)
         .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(dts => dts.TollInvoice)
         .WithMany(dts => dts.TollSummary)
         .HasForeignKey(dts => dts.TollInvoiceId)
         .OnDelete(DeleteBehavior.SetNull);

        e.HasMany(dts => dts.TollEvents)
         .WithOne(dts => dts.DailyTollSummaries)
         .HasForeignKey(dts => dts.DailyTollSummaryId)
         .OnDelete(DeleteBehavior.SetNull);
    }
}
