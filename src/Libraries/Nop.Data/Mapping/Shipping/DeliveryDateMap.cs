using Nop.Core.Domain.Shipping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nop.Data.Mapping.Shipping
{
    public class DeliveryDateMap : NopEntityTypeConfiguration<DeliveryDate>
    {
        public override void Configure(EntityTypeBuilder<DeliveryDate> builder)
        {
            builder.ToTable("DeliveryDate");
            builder.HasKey(dd => dd.Id);
            builder.Property(dd => dd.Name).IsRequired().HasMaxLength(400);
            PostInitialize();
        }
    }
}