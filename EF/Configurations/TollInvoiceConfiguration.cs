using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EF.Configurations;

internal class TollInvoiceConfiguration : IEntityTypeConfiguration<TollInvoice>
{
    public void Configure(EntityTypeBuilder<TollInvoice> e)
    {
        e.HasKey(x => x.Id);

        e.Property(x => x.FromDay).IsRequired();
        e.Property(x => x.ToDay).IsRequired();

        e.Ignore(x => x.Sum);
    }
}
