using Entities.Tolls;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EFCore.Configurations;

internal class TollEventConfiguration : IEntityTypeConfiguration<TollEvent>
{
    public void Configure(EntityTypeBuilder<TollEvent> e)
    {
        e.HasKey(te => te.Id);
        e.Property(te => te.EventDateTime)
            .IsRequired()
            .HasConversion(new ValueConverter<DateTimeOffset, DateTime>(
                v => v.UtcDateTime,
                v => new DateTimeOffset(DateTime.SpecifyKind(v, DateTimeKind.Utc))));
        e.Property(te => te.Zone).IsRequired().HasMaxLength(64);

        e.HasIndex(te => te.DailyTollSummaryId);

        e.HasOne(te => te.Vehicle)
            .WithMany()
            .HasForeignKey(te => te.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        e.HasOne(te => te.DailyTollSummaries)
            .WithMany(te => te.TollEvents)
            .HasForeignKey(te => te.DailyTollSummaryId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
