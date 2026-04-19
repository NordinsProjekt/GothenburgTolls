using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore.Configurations;

internal class TollEventConfiguration : IEntityTypeConfiguration<TollEvent>
{
    public void Configure(EntityTypeBuilder<TollEvent> e)
    {
        e.HasKey(te => te.Id);
        e.Property(te => te.EventDateTime).IsRequired();

        e.HasIndex(te => te.DailyTollSummaryId);

        e.HasOne(te => te.Vehicle)
            .WithMany()
            .HasForeignKey(te => te.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(te => te.DailyTollSummaries)
            .WithMany("TollEvents")
            .HasForeignKey(te => te.DailyTollSummaryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
