using Entities;
using Entities.Bases;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore.Configurations;

internal class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> e)
    {
        e.ToTable("Vehicles");
        e.HasKey(v => v.Id);

        e.Property(v => v.RegistrationNumber)
            .HasMaxLength(12)
            .IsRequired();

        e.HasIndex(v => v.RegistrationNumber).IsUnique();

        e.HasDiscriminator<string>("VehicleType")
            .HasValue<Car>(nameof(Car))
            .HasValue<Motorbike>(nameof(Motorbike));
    }
}
