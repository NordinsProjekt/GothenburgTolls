using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EF.Configurations;

internal class TollEventConfiguration : IEntityTypeConfiguration<TollEvent>
{
    public void Configure(EntityTypeBuilder<TollEvent> e)
    {
        e.HasKey(e => e.Id);
        e.Property(e => e.EventDateTime).IsRequired();

        e.HasIndex(x => x.DailyTollSummaryId);

        e.HasOne(x => x.Vehicle)
            .WithMany()
            .HasForeignKey(x => x.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(x => x.DailyTollSummaries)
            .WithMany("TollEvents")
            .HasForeignKey(x => x.DailyTollSummaryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
