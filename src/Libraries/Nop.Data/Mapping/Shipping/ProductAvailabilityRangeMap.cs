using Nop.Core.Domain.Shipping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nop.Data.Mapping.Shipping
{
    public class ProductAvailabilityRangeMap : NopEntityTypeConfiguration<ProductAvailabilityRange>
    {
        public override void Configure(EntityTypeBuilder<ProductAvailabilityRange> builder)
        {
            builder.ToTable("ProductAvailabilityRange");
            builder.HasKey(range => range.Id);
            builder.Property(range => range.Name).IsRequired().HasMaxLength(400);
            PostInitialize();
        }
    }
}