using Nop.Core.Domain.Shipping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nop.Data.Mapping.Shipping
{
    public class WarehouseMap : NopEntityTypeConfiguration<Warehouse>
    {
        public override void Configure(EntityTypeBuilder<Warehouse> builder)
        {
            builder.ToTable("Warehouse");
            builder.HasKey(wh => wh.Id);
            builder.Property(wh => wh.Name).IsRequired().HasMaxLength(400);
            PostInitialize();
        }
    }
}