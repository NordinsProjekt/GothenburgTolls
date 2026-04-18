using Entities;
using Entities.Bases;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EF.Configurations;

internal class DailyTollSummaryConfiguration : IEntityTypeConfiguration<DailyTollSummary>
{
    public void Configure(EntityTypeBuilder<DailyTollSummary> e)
    {
        e.HasKey(x => x.Id);

        e.Property(x => x.ForDay).IsRequired();
        e.Property(x => x.Created).IsRequired();
        e.Property(x => x.Amount).HasPrecision(18, 2);
        e.Property(x => x.VehicleId).IsRequired();

        e.HasIndex(x => new { x.VehicleId, x.ForDay }).IsUnique();
        e.HasIndex(x => x.TollInvoiceId);

        e.HasOne<Vehicle>()
         .WithMany()
         .HasForeignKey(x => x.VehicleId)
         .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(x => x.TollInvoice)
         .WithMany(i => i.TollSummary)
         .HasForeignKey(x => x.TollInvoiceId)
         .OnDelete(DeleteBehavior.SetNull);

        e.HasMany(x => x.TollEvents)
         .WithOne(te => te.DailyTollSummaries)
         .HasForeignKey(te => te.DailyTollSummaryId)
         .OnDelete(DeleteBehavior.SetNull);
    }
}
