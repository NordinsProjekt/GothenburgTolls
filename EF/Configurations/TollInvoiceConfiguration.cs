using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore.Configurations;

internal class TollInvoiceConfiguration : IEntityTypeConfiguration<TollInvoice>
{
    public void Configure(EntityTypeBuilder<TollInvoice> e)
    {
        e.HasKey(ti => ti.Id);

        e.Property(ti => ti.FromDay).IsRequired();
        e.Property(ti => ti.ToDay).IsRequired();

        e.Ignore(ti => ti.Sum);
    }
}
