using Entities.Tolls;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore.Configurations;

internal class TollInvoiceConfiguration : IEntityTypeConfiguration<TollInvoice>
{
    public void Configure(EntityTypeBuilder<TollInvoice> e)
    {
        e.HasKey(ti => ti.Id);

        e.Property(ti => ti.VehicleId).IsRequired();
        e.Property(ti => ti.Year).IsRequired();
        e.Property(ti => ti.Month).IsRequired();
        e.Property(ti => ti.Created).IsRequired();

        e.Ignore(ti => ti.Sum);

        e.HasIndex(ti => new { ti.VehicleId, ti.Year, ti.Month }).IsUnique();

        e.HasOne(ti => ti.Vehicle)
         .WithMany()
         .HasForeignKey(ti => ti.VehicleId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
